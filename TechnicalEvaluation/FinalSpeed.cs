using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace IMADA_Force_Measure
{
	public class FinalSpeed
	{
		static string ComPort = "COM7";
		static int BaudRate = 115200;
		static string PreloadString = "";
		static string JetString = "f";
		static string ClutchAttachString = "y";
		static string SetForceString = "c0200100";
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
			StreamReader reader = new StreamReader(File.OpenRead(@"E:\ShihChin\LoadCell\IMADA_Force_Measure_1217\IMADA_Force_Measure\IMADA_Force_Measure\bin\Debug\netcoreapp3.1\PwmToAirPressure.csv"));
			List<int> PwmList = new List<int>();
			while (!reader.EndOfStream)
			{
				var line = reader.ReadLine();
				PwmList.Add(int.Parse(line.Split("\t")[0]));
			}
			Arduino_SerialPort.Write("a");
			foreach (int pwm in PwmList)
			{
				SetForceString = "c" + String.Format("{0:000}", pwm) + "1000";
				Arduino_SerialPort.Write(SetForceString);
				for (int angle = 30; angle <= 90; angle += 30)
				{
					int signalCount;
					if(angle == 30)
                    {
						signalCount = 2;
                    }
					else if(angle == 60)
                    {
						signalCount = 5;
                    }
                    else
                    {
						signalCount = 7;
                    }
					PreloadString = "t0" + Convert.ToInt32(25 * angle / 30) + "0255";
					var Speed_CSV_Recorder = File.CreateText($"Angle" + angle + "\\TotalAngularSpeed.csv");
					for (int i = 0; i < ArduinoRepeatedTestTime; ++i)
					{
						Arduino_SerialPort.Write(PreloadString);
						//Wait until preload OK
						Thread.Sleep(0);
						var CSV_Recorder = File.CreateText($"Angle" + angle + $"\\AngularSpeed_{i}.csv");
						long StopTime = long.MaxValue;
						Stopwatch ResponseTime = new Stopwatch();
						Stopwatch LastArduinoSendTime = new Stopwatch();
						LastArduinoSendTime.Restart();
						ResponseTime.Start();
						Arduino_SerialPort.Write(JetString);
						int GetResponse = 0;
						while (ResponseTime.ElapsedMilliseconds < StopTime)
						{
							List<double> TimeStamp = new List<double>();
							LastArduinoSendTime.Restart();
							while (LastArduinoSendTime.ElapsedMilliseconds < 2)
							{
								string s = Arduino_SerialPort.ReadExisting();
								if (s.Contains('E'))
								{
									double ElaspedTimeSeconds = ResponseTime.ElapsedTicks / (double)TimeSpan.TicksPerSecond;
									int Ecount = s.Split('!').Length - 1;
									for (int getStrNum = 0; getStrNum < Ecount; getStrNum++)
                                    {
										CSV_Recorder.WriteLine(s.Split(':')[1+getStrNum].Split('!')[0] + $", {ElaspedTimeSeconds.ToString("0.0000")}");
										TimeStamp.Add(ElaspedTimeSeconds);
										GetResponse++;
									}
									break;
								}
							}
							if (GetResponse == signalCount && StopTime == long.MaxValue)
							{
								string toWrite = "";
								foreach(double timestamp in TimeStamp)
                                {
									toWrite += timestamp + ",";
                                }
								Speed_CSV_Recorder.WriteLine(toWrite);
								StopTime = ResponseTime.ElapsedMilliseconds + 100;
							}
						}
						Arduino_SerialPort.Write(ClutchAttachString);
						CSV_Recorder.Flush();
						CSV_Recorder.Close();
						Console.WriteLine("Done");
						Console.WriteLine();
						//Wait until jet end
						Thread.Sleep(0);
					}
					Speed_CSV_Recorder.Flush();
					Speed_CSV_Recorder.Close();
				}
			}
		}
	}
}