using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using Valve.VR;
//Sources: https://github.com/BOLL7708/OpenVRInputTest
namespace OpenVRInputTest
{
    class Program
    {
        static ulong mActionSetHandle;
        static VRActiveActionSet_t[] mActionSetArray;

        // # items are referencing this list of actions: https://github.com/ValveSoftware/openvr/wiki/SteamVR-Input#getting-started
        static void Main(string[] args)
        {
            SerialPort arduinoStream;
            arduinoStream = new SerialPort("COM3", 115200); //指定連接埠、鮑率並實例化SerialPort
            arduinoStream.ReadTimeout = 10;
            try
            {
                arduinoStream.Open(); //開啟SerialPort連線
                Console.WriteLine("SerialPort開啟連接");
                ArduinoWrite(arduinoStream, "c30000050");
            }
            catch (System.Exception e)
            {
                Console.WriteLine("SerialPort連接失敗");
                Console.WriteLine(e.Message);
            }

            // Initializing connection to OpenVR
            var error = EVRInitError.None;
            OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background); // Had this as overlay before to get it working, but changing it back is now fine?
            Thread workerThread = new Thread(() => Worker(arduinoStream));
            if (error != EVRInitError.None)
                Utils.PrintError($"OpenVR initialization errored: {Enum.GetName(typeof(EVRInitError), error)}");
            else
            {
                Utils.PrintInfo("OpenVR initialized successfully.");

                // Load app manifest, I think this is needed for the application to show up in the input bindings at all
                Utils.PrintVerbose("Loading app.vrmanifest");
                var appError = OpenVR.Applications.AddApplicationManifest(Path.GetFullPath("./app.vrmanifest"), false);
                if (appError != EVRApplicationError.None)
                    Utils.PrintError($"Failed to load Application Manifest: {Enum.GetName(typeof(EVRApplicationError), appError)}");
                else 
                    Utils.PrintInfo("Application manifest loaded successfully.");

                // #3 Load action manifest
                Utils.PrintVerbose("Loading actions.json");
                var ioErr = OpenVR.Input.SetActionManifestPath(Path.GetFullPath("./actions.json"));
                if (ioErr != EVRInputError.None) 
                    Utils.PrintError($"Failed to load Action Manifest: {Enum.GetName(typeof(EVRInputError), ioErr)}");
                else 
                    Utils.PrintInfo("Action Manifest loaded successfully.");

                // #4 Get action handles
                Utils.PrintVerbose("Getting action handles");
                
                
                // #5 Get action set handle
                Utils.PrintVerbose("Getting action set handle");
                var errorAS = OpenVR.Input.GetActionSetHandle("/actions/default", ref mActionSetHandle);
                if (errorAS != EVRInputError.None) 
                    Utils.PrintError($"GetActionSetHandle Error: {Enum.GetName(typeof(EVRInputError), errorAS)}");
                Utils.PrintDebug($"Action Set Handle default: {mActionSetHandle}");

                // Starting worker
                Utils.PrintDebug("Starting worker thread.");
                if (!workerThread.IsAlive) 
                    workerThread.Start();
                else 
                    Utils.PrintError("Could not start worker thread.");
            }
            Console.ReadLine();
            workerThread.Abort();
            OpenVR.Shutdown();
        }

        public static void Worker(SerialPort arduinoStream)
        {
            long lastRecord = getTime();
            int bulletCount = 0;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Thread.CurrentThread.IsBackground = true;
            while (true)
            {
                var vrEvent = new VREvent_t();
                try
                {
                    while (OpenVR.System.PollNextEvent(ref vrEvent, Utils.SizeOf(vrEvent)))
                    {
                        var pid = vrEvent.data.process.pid;
                        if (vrEvent.eventType == (uint)EVREventType.VREvent_Input_HapticVibration && vrEvent.data.hapticVibration.fAmplitude > 0)
                        {
                            ETrackedControllerRole DeviceType = 
                                OpenVR.System.GetControllerRoleForTrackedDeviceIndex(pid);

                            if (DeviceType.Equals(ETrackedControllerRole.LeftHand))
                                Console.WriteLine($"Received Vibration Events: LeftController");
                            else if (DeviceType.Equals(ETrackedControllerRole.RightHand))
                            {
                                //Console.WriteLine("接收到右手把震動 震幅({0}) 頻率({1}) 時長({2})", vrEvent.data.hapticVibration.fAmplitude, vrEvent.data.hapticVibration.fFrequency, vrEvent.data.hapticVibration.fDurationSeconds);
                                float amplitude = vrEvent.data.hapticVibration.fAmplitude;
                                float frequency = vrEvent.data.hapticVibration.fFrequency;
                                float duration = vrEvent.data.hapticVibration.fDurationSeconds;
                                if (duration < -0.99f && duration > -1.01f)
                                {
                                    bulletCount += 1;
                                    Console.WriteLine("{0}", bulletCount);
                                    ArduinoWrite(arduinoStream, "f");
                                }
                            }
                            else
                                Console.WriteLine($"Received Vibration Events: {DeviceType.ToString()}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.PrintWarning($"Could not get events: {e.Message}");
                }

            }
        }

        private static void ArduinoWrite(SerialPort arduinoStream, string message)
        {
            if (arduinoStream.IsOpen)
            {
                try
                {
                    arduinoStream.Write(message);
                }
                catch (System.Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static long getTime()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}

