using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoBasic : MonoBehaviour {
    private SerialPort arduinoStream;
    private SerialPort arduinoVib;
    public string port;
    public string portVib;
    private Thread readThread;
    private Thread readVibThread;
    public string readMessage;
    bool isNewMessage;

    void Start () {
        if (port != "") {
            arduinoStream = new SerialPort (port, 115200);
            arduinoStream.ReadTimeout = 10;
            try {
                arduinoStream.Open ();
                readThread = new Thread (new ThreadStart (ArduinoRead)); 
                readThread.Start ();
                Debug.Log ("SerialPort start connection");
            } catch (System.Exception e){
                Debug.Log ("SerialPort failed to connect");
                Debug.Log(e.Message);
            }
        }
        if (portVib != "")
        {
            arduinoVib = new SerialPort(portVib, 115200);
            arduinoVib.ReadTimeout = 10;
            try
            {
                arduinoVib.Open();
                Debug.Log("SerialPortVib Open");
            }
            catch(System.Exception e)
            {
                Debug.Log("SerialPort Vib Fail To Open");
                Debug.Log(e);
            }
        }
    }
    void Update () {
        if (isNewMessage) {
            Debug.Log (readMessage);
        }
        isNewMessage = false;
    }
    private void ArduinoRead () {
        while (arduinoStream.IsOpen) {
            try {
                readMessage = arduinoStream.ReadLine();
                isNewMessage = true;
             
            } catch (System.Exception e) {
                Debug.LogWarning (e.Message);
            }
        }
    }
    private void ArduinoReadVib()
    {
        while (arduinoVib.IsOpen)
        {
            try
            {

                readMessage = arduinoVib.ReadLine();
                isNewMessage = true;

            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }
    public void ArduinoWrite (string message) {
        Debug.Log (message);
      
        if (arduinoStream.IsOpen)
        {
            try
            {
                arduinoStream.Write(message);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }

    public void ArduinoWriteVib (string message)
    {
        if (arduinoVib.IsOpen)
        {
            try
            {
                arduinoVib.Write(message);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }
    void OnApplicationQuit () {
        if (arduinoStream != null) {
            if (arduinoStream.IsOpen) {
                arduinoStream.Close ();
            }
        }
        if (arduinoVib != null)
        {
            if (arduinoVib.IsOpen)
            {
                arduinoVib.Close();
            }
        }
    }

}