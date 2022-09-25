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
		static int ValveID = 0; // 0 or 1
		static string JetString = "f"; //first motor stop & clutch detach
		static string SetForceString = "c00000000";//pressure and duration
		// static string PreloadString = "t090255";
		// static string ClutchAttachString = "y";
		const int ArduinoRepeatedTestTime = 20;
		const double StartResponseThreshold = 3f;

		//�j�Y
		string PwmToPressureList = "PwmToAirPressure4096.csv";   //���w������
		int EnsurePreloadBufferTime = 2000;  //���F�����w���שһݮɶ�
		// int EncoderTick30Degree = 200;   //���?30�׸g�L��tick��
		long TimeoutMS = 5000;
		int[] angleList = new int[1] { 90 };  //���q����
		//static string PwmToAirPressurePath = "E:\ShihChin\LoadCell\IMADA_Force_Measure_1217\IMADA_Force_Measure\IMADA_Force_Measure\bin\Debug\netcoreapp3.1\PwmToAirPressure.csv";
		public void Start()
		{
			ManagementObjectCollection ManObjReturn;
			ManagementObjectSearcher ManObjSearch;
			ManObjSearch = new ManagementObjectSearcher("Select * from Win32_SerialPort");
			ManObjReturn = ManObjSearch.Get();
			SerialPort IMADA_SerialPort = null;
			SerialPort IMADA_SerialPort_2 = null;
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

			var path = Directory.GetCurrentDirectory()+ "\\" + PwmToPressureList;

			StreamReader reader = new StreamReader(File.OpenRead(path));
			List<int> PwmList = new List<int>();
			while (!reader.EndOfStream)
			{
				var line = reader.ReadLine();
				PwmList.Add(int.Parse(line.Split(" ")[0]));
			}
			Thread.Sleep(1000);
			foreach (int pwm in PwmList)
			{
				/*
				400		1.0 bar	0.10 MPa
				470		1.2 bar	0.12 MPa
				550		1.4 bar	0.14 MPa
				650		1.6 bar	0.16 MPa
				730		1.8 bar	0.18 MPa
				830		2.0 bar	0.20 MPa
				910		2.2 bar	0.22 MPa
				1020	2.4 bar	0.24 MPa
				1120	2.6 bar	0.26 MPa
				1220	2.8 bar	0.28 MPa
				1300	3.0 bar	0.30 MPa
				1400	3.2 bar	0.32 MPa
				1450	3.4 bar	0.34 MPa
				1550	3.6 bar	0.36 MPa
				1650	3.8 bar	0.38 MPa
				1750	4.0 bar	0.40 MPa
				1850	4.2 bar	0.42 MPa
				1950	4.4 bar	0.44 MPa
				2050	4.6 bar	0.46 MPa
				2100	4.8 bar	0.48 MPa
				2200	5.0 bar	0.50 MPa
				2300	5.2 bar	0.52 MPa
				2400	5.4 bar	0.54 MPa
				2500	5.6 bar	0.56 MPa
				2600	5.8 bar	0.58 MPa
				2700	6.0 bar	0.60 MPa
				2800	6.2 bar	0.62 MPa
				2900	6.4 bar	0.64 MPa
				3000	6.6 bar	0.66 MPa
				3100	6.8 bar	0.68 MPa
				3200	7.0 bar	0.70 MPa
				*/
				//調整量測範圍
				if (pwm <= 800 || pwm > 2200)
					continue;
				
				SetForceString = "c"+ String.Format("{0:0000}", pwm) + "0300";
				
				//for (int angle = 30; angle <= 90; angle += 30)
				foreach(int angle in angleList)
				{
					//if (pwm == 135 && angle < 60)
					//continue;
					// PreloadString = "t0" + string.Format("{0:000}", EncoderTick30Degree * angle / 30) + "255";
					var ResponseTime_CSV_Recorder = File.CreateText($"Angle"+ angle + "\\Force" + pwm + "TotalResponseTime_" + string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now) + ".csv");
					var MaximumMagnitude_CSV_Recorder = File.CreateText($"Angle" + angle + "\\Force" + pwm + "MaximumMagnitude_" + string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now) + ".csv");
					for (int i = 0; i < ArduinoRepeatedTestTime; ++i)
					{
						//Thread.Sleep(2000);
						Console.WriteLine("Pwm: {0} Count: {1}",pwm,i+1);
						String restoreForce = "c12000300";
						Arduino_SerialPort.Write(restoreForce);
						Console.WriteLine(restoreForce);
						Thread.Sleep(100);
						
						ValveID = ++ValveID % 2;
						Arduino_SerialPort.Write(JetString + ValveID.ToString());
						Console.WriteLine(JetString + ValveID.ToString());
						Thread.Sleep(520);
						ValveID = ++ValveID % 2;

						// Arduino_SerialPort.Write(ClutchAttachString);
						// Console.WriteLine(ClutchAttachString);
						// Thread.Sleep(100);
						
						Arduino_SerialPort.Write(SetForceString);
						Console.WriteLine(SetForceString);
						Thread.Sleep(100);

						// Arduino_SerialPort.Write(PreloadString);
						// Console.WriteLine(PreloadString);
						// Arduino_SerialPort.Write("r");
						//Thread.Sleep(EnsurePreloadBufferTime);

						var CSV_Recorder = File.CreateText($"Angle" + angle + "\\Force" + pwm +$"ResponseTime_{i}_" + string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now) + ".csv");
						long StopTime = TimeoutMS;
						Stopwatch ResponseTime = new Stopwatch();
						Stopwatch LastIMADASendTime = new Stopwatch();
						double InitValue = double.PositiveInfinity, NowForceValueDouble = double.PositiveInfinity;
						double MaximumMagnitude = -100f;
						LastIMADASendTime.Restart();
						ResponseTime.Start();
						Arduino_SerialPort.Write(JetString + ValveID.ToString());
						Console.WriteLine(JetString + ValveID.ToString());
						IMADA_SerialPort.WriteLine("XAR");
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
							if (Math.Abs(InitValue - NowForceValueDouble) > StartResponseThreshold && StopTime == TimeoutMS)
							{
								ResponseTime_CSV_Recorder.WriteLine($"{(ResponseTime.ElapsedTicks / (double)TimeSpan.TicksPerSecond).ToString()}");
								StopTime = ResponseTime.ElapsedMilliseconds + 1000;  //Record More (1000ms)
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
					//Reset pressure
					// Arduino_SerialPort.Write("c00000000");
					// Console.WriteLine("c00000000");
					Thread.Sleep(100);
				}
			}
		}
	}
}