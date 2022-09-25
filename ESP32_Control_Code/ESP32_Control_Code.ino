#include <ESP32Servo.h>
Servo myservo;
Servo myservo2;
const int PWM_Pins[] = {32};//, 33, 25, 26, 27, 13, 02, 05
const int RegulatorCount = sizeof(PWM_Pins) / sizeof(int);
  
const int ValvePins[] = {23, 22, }; //21, 19 , 18, 17, 16, 04
const int ValveCount = sizeof(ValvePins) / sizeof(int);

const int PWM_Freq = 1000; //PWM Frequency: Higer Freq, Lower Response Time 
const int PWM_Step = 12;  //PWM Step: 12-bit, [0-4095] (0: 0V, 4095: 3.3V)
int air_duration = 20;

void InitPWM()
{
  for (int i = 0; i < RegulatorCount; ++i)
  {
    ledcSetup(i, PWM_Freq, PWM_Step);  //Clear PWM Signal
    ledcAttachPin(PWM_Pins[i], i);
    ledcWrite(i, 0);  //PWM Output: 0V
  }
}

void InitValve()
{
  for (int i = 0; i < ValveCount; ++i)
    pinMode(ValvePins[i], OUTPUT);
}

//ID: 0 - ValveCount
void SetValveOnOff(int ID, bool OnOff)
{
  assert (ID >= 0 && ID < ValveCount);
  digitalWrite(ValvePins[ID], OnOff);
}

//ID: 0 - RegulatorCount
//Step: 0 - (2^PWM_Step - 1)
void SetRegulatorByPWM(int ID, int Step)
{
  assert (ID >= 0 && ID < RegulatorCount);
  assert (Step >= 0 && Step < (1 << PWM_Step));
  ledcWrite(ID, Step);
}

//ID: 0 - RegulatorCount
//Voltage: 0 - 10V
//void SetRegulatorByVoltage(int ID, float Voltage)
//{
//  assert (ID >= 0 && ID < RegulatorCount);
//  assert (Voltage >= 0 && Voltage <= 10);
//  float PWM_Voltage = Voltage / 3;
//  float Duty_Cycle = PWM_Voltage / 3.3f;
//  Duty_Cycle = (Duty_Cycle >= 1) ? 1 : Duty_Cycle;
//  int Step = (int)(Duty_Cycle * ((1 << PWM_Step) - 1));
//  SetRegulatorByPWM(ID, Step); 
//}

void setup() 
{
  Serial.begin(115200);
  InitPWM();
  InitValve();
//  digitalWrite(ValvePins[2], HIGH);
//  digitalWrite(ValvePins[3], HIGH);
//  myservo.attach(14);
//  myservo.write(0);
//  myservo2.attach(16); 
//  myservo2.write(0);
}

void loop() 
{
  
  if(Serial.available()>0){
    char cmd = Serial.read();
    if(cmd == 'f'){
      String ID = "";
      char id;
      while(true){
        if(Serial.available()){
          id = Serial.read();
          break;
        }
      }
      ID += id;
      fireStart(ID.toInt());
     }
    // ex: C05001000 PWM500噴1秒
    else if(cmd == 'c'){
      int pressure = parseData(); // [0-4095]
      air_duration = parseData(); // [0-9999]
      SetRegulatorByPWM(0,pressure);
    }
    // ex: s081000 1秒噴8次
    else if(cmd == 's'){
      int freq = parseFreq();
      int duration = parseData();
      
      while(duration > 0)
      {
        switchFire(freq);
        duration -= 1000/freq;
      }
    }
    else if (cmd == 'y'){
      myservo.write(0);
      myservo2.write(0);
      }
    else if (cmd == 'n'){
      myservo.write(180);
      myservo2.write(180);
    }
    

  }
//  static int ValveID = 0, RegulatorID = 0;
//  static int VoltageStep = 0;
//  
//  SetValveOnOff(ValveID, HIGH);
//  SetRegulatorByPWM(RegulatorID, VoltageStep);
//  Serial.printf("Valve %d is ON, Regulator %d is set to %d\n", ValveID, RegulatorID, VoltageStep);
//  delay(1000);
//  
//  SetValveOnOff(ValveID, LOW);
//  Serial.printf("Valve %d is OFF, Regulator %d is set to %d\n", ValveID, RegulatorID, VoltageStep);
//  
//  ValveID = ++ValveID % ValveCount;
//  RegulatorID = ++RegulatorID % RegulatorCount;
//  VoltageStep = (VoltageStep + 100) % ((1 << PWM_Step) - 1);
//  
//  delay(1000); 
}
int parseFreq(){
  String num = "";
  char c;
   for(int i = 0; i < 2; i++){
      while(true){
        if(Serial.available()){
          c = Serial.read();
          break;
        }
      }
      num += c;
   }
   return num.toInt();
}

int parseData(){
  String num = "";
  char c;
   for(int i = 0; i < 4; i++){
      while(true){
        if(Serial.available()){
          c = Serial.read();
          break;
        }
      }
      num += c;
   }
   return num.toInt();
}
void fireStart(int ID){
  Serial.printf("fire ID %d\n", ID);
  SetValveOnOff(ID,HIGH);
  delay(air_duration);
  SetValveOnOff(ID,LOW);
}
void switchFire(int freq){
  static int ValveID = 0;
  air_duration = 1000/freq;
  fireStart(ValveID);
  ValveID = ++ValveID % ValveCount;
}
