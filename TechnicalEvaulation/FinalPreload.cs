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
		static string ComPort = "COM7";
		static int BaudRate = 115200;
		static string PreloadString = "";
		const int ArduinoRepeatedTestTime = 3;
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
			Arduino_SerialPort.Write("c0201000");
			Thread.Sleep(10000);
			Console.WriteLine("c0201000");
			for (int angle = 15; angle <= 75; angle += 15)
			{
				var PreloadTime_CSV_Recorder = File.CreateText($"Angle" + angle + "\\TotalPreloadTime.csv");
				PreloadString = "w0"+Convert.ToInt32(25 * angle / 3)+"255";
				for (int i = 0; i < ArduinoRepeatedTestTime; ++i)
				{
					var CSV_Recorder = File.CreateText($"Angle" + angle + $"\\PreloadTime_{i}.csv");
					long StopTime = 20000;
					Stopwatch ResponseTime = new Stopwatch();
					Stopwatch LastArduinoSendTime = new Stopwatch();
					Arduino_SerialPort.Write("f");
					Console.WriteLine("f");
					Thread.Sleep(100);
					Arduino_SerialPort.Write("y");
					Console.WriteLine("y");
					Thread.Sleep(2000);
					LastArduinoSendTime.Restart();
					ResponseTime.Start();
					Arduino_SerialPort.Write(PreloadString);
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
								CSV_Recorder.WriteLine($"{ElaspedTimeSeconds.ToString("0.0000")}");
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
					CSV_Recorder.Flush();
					CSV_Recorder.Close();
					Console.WriteLine("Done");
					Console.WriteLine();		
				}
				PreloadTime_CSV_Recorder.Flush();
				PreloadTime_CSV_Recorder.Close();
			}
		}
	}
}