using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoControl : MonoBehaviour
{	
    public int duration = 0;
    public int force = 0;
    public int vibration_intensity;
    public int vibration_duration;

    private ArduinoBasic arduinoBasic;
    
    void Start()
    {
        arduinoBasic = this.GetComponent<ArduinoBasic>();
    }

    void Update()
    {	
     	if(Input.GetKeyDown(KeyCode.Space)){
     		Debug.Log("space is pressed");
     		Fire();
     	}
    }

    public void SetDuration(int duration){
        if (duration < 0 || duration > 4000)
            return;
    	this.duration = duration;
    }

    public void SetForce(int force){
        if (force < 0 || force > 255)
            return;
    	this.force = force;
    }


    public int GetDuration(){
        return duration;
    }

    public int GetForce(){
        return force;
    }

    public void SendConfig(int f, int d){

        arduinoBasic.ArduinoWrite(string.Format("c{0:d4}{1:d4}", f, d));
    }

    public void Fire(){
        arduinoBasic.ArduinoWrite("f0");
    }
    public void Fire1(){
        arduinoBasic.ArduinoWrite("f1");
    }

    public void Vibe()
    {
        arduinoBasic.ArduinoWrite("v");
    }

    public void SendForce(int f) {
        arduinoBasic.ArduinoWrite(string.Format("s{0:d3}", f));
    }

    public void Open() {
        arduinoBasic.ArduinoWrite("y");
    }

    public void Close() {
        arduinoBasic.ArduinoWrite("n");
    }
}
