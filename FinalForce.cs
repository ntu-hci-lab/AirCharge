using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace IMADA_Force_Measure
{
	public class FinalForce
	{
		static string ComPort = "COM7";
		static int BaudRate = 115200;
		static string JetString = "f"; //first motor stop & clutch detach
		static string SetForceString = "c0200100";//and duration
		static string PreloadString = "t090255";
		static string ClutchAttachString = "y";
		const int ArduinoRepeatedTestTime = 20;
		const double StartResponseThreshold = 3f;
		public void Start()
		{
			ManagementObjectCollection ManObjReturn;
			ManagementObjectSearcher ManObjSearch;
			ManObjSearch = new ManagementObjectSearcher("Select * from Win32_SerialPort");
			ManObjReturn = ManObjSearch.Get();
			SerialPort IMADA_SerialPort = null;
			Console.WriteLine(ManObjReturn);
			foreach (ManagementObject ManObj in ManObjReturn)
			{
				if (!ManObj["Name"].ToString().Contains("IMADA"))
					continue;
				string IMADA_COM_Port = ManObj["DeviceID"].ToString();
				Console.WriteLine(IMADA_COM_Port + "\n" + ManObj["Name"].ToString());
				IMADA_SerialPort = new SerialPort(IMADA_COM_Port);
				IMADA_SerialPort.BaudRate = 115200;
				IMADA_SerialPort.DataBits = 8;
				IMADA_SerialPort.StopBits = StopBits.One;
				IMADA_SerialPort.Parity = Parity.None;
				IMADA_SerialPort.NewLine = "\r";
				IMADA_SerialPort.ReadBufferSize = 102400;
				IMADA_SerialPort.WriteBufferSize = 102400;
				IMADA_SerialPort.ReadTimeout = 2000;
				IMADA_SerialPort.WriteTimeout = 2000;
				IMADA_SerialPort.Handshake = Handshake.None;
				IMADA_SerialPort.RtsEnable = true;
				IMADA_SerialPort.DtrEnable = true;
				IMADA_SerialPort.Open();
				Console.WriteLine("Find IMADA!");
			}
			if (IMADA_SerialPort == null)
				return;
			//Console.Write("Enter Arduino Device Com Name: ");
			//string Arduino_COM_Port = Console.ReadLine().Trim();
			//Console.Write("Enter Arduino Device Com BaudRate: ");
			//int Arduino_COM_Port_Baudrate = int.Parse(Console.ReadLine());
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
			Thread.Sleep(1000);
			foreach (int pwm in PwmList)
			{
				if (pwm < 135)
					continue;
				
				SetForceString = "c"+ String.Format("{0:000}", pwm)+"1000";
				for (int angle = 15; angle <= 75; angle += 15)
				{
					if (pwm == 135 && angle < 60)
						continue;
					PreloadString = "t0" + Convert.ToInt32(25 * angle / 3) + "255";
					var ResponseTime_CSV_Recorder = File.CreateText($"Angle"+ angle + "\\Force" + pwm + "TotalResponseTime.csv");
					var MaximumMagnitude_CSV_Recorder = File.CreateText($"Angle" + angle + "\\Force" + pwm + "MaximumMagnitude.csv");
					for (int i = 0; i < ArduinoRepeatedTestTime; ++i)
					{
						Arduino_SerialPort.Write("c0501000");
						Thread.Sleep(100);
						Arduino_SerialPort.Write(JetString);
						Console.WriteLine(JetString);
						Thread.Sleep(100);
						Arduino_SerialPort.Write(ClutchAttachString);
						Console.WriteLine(ClutchAttachString);
						Thread.Sleep(2000);
						Arduino_SerialPort.Write(SetForceString);
						Console.WriteLine(SetForceString);
						Thread.Sleep(100);
						Arduino_SerialPort.Write(PreloadString);
						Console.WriteLine(PreloadString);
						Thread.Sleep(3000);
						var CSV_Recorder = File.CreateText($"Angle" + angle + "\\Force" + pwm +$"ResponseTime_{i}.csv");
						long StopTime = 20000;
						Stopwatch ResponseTime = new Stopwatch();
						Stopwatch LastIMADASendTime = new Stopwatch();
						LastIMADASendTime.Restart();
						ResponseTime.Start();
						Arduino_SerialPort.Write(JetString);
						IMADA_SerialPort.WriteLine("XAR");
						double InitValue = double.PositiveInfinity, NowForceValueDouble = double.PositiveInfinity;
						double MaximumMagnitude = -100f;
						while (ResponseTime.ElapsedMilliseconds < StopTime)
						{
							LastIMADASendTime.Restart();
							IMADA_SerialPort.WriteLine("XAR");
							while (LastIMADASendTime.ElapsedMilliseconds < 2)
							{
								string s = IMADA_SerialPort.ReadExisting();
								//Console.WriteLine(s);
								if (s.Contains('r'))
								{
									double ElaspedTimeSeconds = ResponseTime.ElapsedTicks / (double)TimeSpan.TicksPerSecond;
									string ForceValueString = s.Split('r')[1].Substring(0, 6);
									CSV_Recorder.WriteLine($"{ElaspedTimeSeconds.ToString("0.0000")},{ForceValueString}");
									NowForceValueDouble = double.Parse(ForceValueString);
									if (NowForceValueDouble > MaximumMagnitude)
										MaximumMagnitude = NowForceValueDouble;
									if (InitValue == double.PositiveInfinity)
										InitValue = NowForceValueDouble;
									break;
								}
							}
							if (Math.Abs(InitValue - NowForceValueDouble) > StartResponseThreshold && StopTime == 20000)
							{
								ResponseTime_CSV_Recorder.WriteLine($"{(ResponseTime.ElapsedTicks / (double)TimeSpan.TicksPerSecond).ToString()}");
								StopTime = ResponseTime.ElapsedMilliseconds + 1000;  //Record More (100ms)
							}
						}
						MaximumMagnitude_CSV_Recorder.WriteLine(MaximumMagnitude);
						CSV_Recorder.Flush();
						CSV_Recorder.Close();
						Console.WriteLine("Done");
						Console.WriteLine();
					}
					ResponseTime_CSV_Recorder.Flush();
					ResponseTime_CSV_Recorder.Close();
					MaximumMagnitude_CSV_Recorder.Flush();
					MaximumMagnitude_CSV_Recorder.Close();
				}
			}
		}
	}
}