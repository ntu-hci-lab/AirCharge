using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace IMADA_Force_Measure
{
	public class FinalPreload
	{
		static string ComPort = "COM8";
		static int BaudRate = 115200;
		static string PreloadString = "";
		const int ArduinoRepeatedTestTime = 5;
		int[] angleList = new int[1] { 90 };  //���q����
		int EncoderTick30Degree = 200;   //���30�׸g�L��tick��
		public void Start()
		{
			SerialPort Arduino_SerialPort = new SerialPort(ComPort);
			Arduino_SerialPort.BaudRate = BaudRate;
			Arduino_SerialPort.DataBits = 8;
			Arduino_SerialPort.StopBits = StopBits.One;
			Arduino_SerialPort.Parity = Parity.None;
			try
			{
				Arduino_SerialPort.Open();
				Thread.Sleep(1000);
				Console.WriteLine("Set Up done");
			}
			catch (Exception e)
			{
				Console.WriteLine("Error Cannot Open Valve!");
				return;
			}
			string JetString = "c15000500";
			
			Thread.Sleep(10000);
			
			foreach(int angle in angleList)
			{
				var PreloadTime_CSV_Recorder = File.CreateText($"Angle" + angle + "\\TotalPreloadTime_" + string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now) + ".csv");
				// PreloadString = "w0" + string.Format("{0:000}", EncoderTick30Degree * angle / 30) + "255";
				string SetForceString = "c"+ String.Format("{0:0000}", 100)+"1000";
				for (int i = 0; i < ArduinoRepeatedTestTime; ++i)
				{
					//var CSV_Recorder = File.CreateText($"Angle" + angle + $"\\PreloadTime_{i}_" + string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now) + ".csv");
					long StopTime = 20000;
					Stopwatch ResponseTime = new Stopwatch();
					Stopwatch LastArduinoSendTime = new Stopwatch();
					Arduino_SerialPort.Write(JetString);
					Console.WriteLine(JetString);
					Thread.Sleep(100);
					Arduino_SerialPort.Write("y");
					Console.WriteLine("y");
					Thread.Sleep(2000);
					Arduino_SerialPort.Write(SetForceString);
					Console.WriteLine(SetForceString);
					Thread.Sleep(100);

					LastArduinoSendTime.Restart();
					ResponseTime.Start();
					Arduino_SerialPort.Write(PreloadString);
					Arduino_SerialPort.Write("r");
					Console.WriteLine(PreloadString);

					bool GetResponse = false;
					while (ResponseTime.ElapsedMilliseconds < StopTime)
					{
						LastArduinoSendTime.Restart();
						while (LastArduinoSendTime.ElapsedMilliseconds < 2)
						{
							string s = Arduino_SerialPort.ReadExisting();
							if (s.Contains('D'))
							{
								double ElaspedTimeSeconds = ResponseTime.ElapsedTicks / (double)TimeSpan.TicksPerSecond;
								//CSV_Recorder.WriteLine($"{ElaspedTimeSeconds.ToString("0.0000")}");
								GetResponse = true;
								break;
							}
						}
						if (GetResponse && StopTime == 20000)
						{
							PreloadTime_CSV_Recorder.WriteLine($"{(ResponseTime.ElapsedTicks / (double)TimeSpan.TicksPerSecond).ToString()}");
							StopTime = ResponseTime.ElapsedMilliseconds + 100;
						}
					}
					//CSV_Recorder.Flush();
					//CSV_Recorder.Close();
					Console.WriteLine("Done");
					Console.WriteLine();		
				}
				PreloadTime_CSV_Recorder.Flush();
				PreloadTime_CSV_Recorder.Close();
			}
		}
	}
}