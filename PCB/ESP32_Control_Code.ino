const int PWM_Pins[] = {32, 33, 25, 26, 27, 13, 02, 05};
const int RegulatorCount = sizeof(PWM_Pins) / sizeof(int);

const int ValvePins[] = {23, 22, 21, 19, 18, 17, 16, 04};
const int ValveCount = sizeof(ValvePins) / sizeof(int);

const int PWM_Freq = 1000; //PWM Frequency: Higer Freq, Lower Response Time 
const int PWM_Step = 12;  //PWM Step: 12-bit, [0-4095] (0: 0V, 4095: 3.3V)

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
void SetRegulatorByVoltage(int ID, float Voltage)
{
  assert (ID >= 0 && ID < RegulatorCount);
  assert (Voltage >= 0 && Voltage <= 10);
  float PWM_Voltage = Voltage / 3;
  float Duty_Cycle = PWM_Voltage / 3.3f;
  Duty_Cycle = (Duty_Cycle >= 1) ? 1 : Duty_Cycle;
  int Step = (int)(Duty_Cycle * ((1 << PWM_Step) - 1));
  SetRegulatorByPWM(ID, Step); 
}

void setup() 
{
  Serial.begin(115200);
  InitPWM();
  InitValve();
}

void loop() 
{
  static int ValveID = 0, RegulatorID = 0;
  static int VoltageStep = 0;
  
  SetValveOnOff(ValveID, HIGH);
  SetRegulatorByPWM(RegulatorID, VoltageStep);
  Serial.printf("Valve %d is ON, Regulator %d is set to %d\n", ValveID, RegulatorID, VoltageStep);
  delay(1000);
  
  SetValveOnOff(ValveID, LOW);
  Serial.printf("Valve %d is OFF, Regulator %d is set to %d\n", ValveID, RegulatorID, VoltageStep);
  
  ValveID = ++ValveID % ValveCount;
  RegulatorID = ++RegulatorID % RegulatorCount;
  VoltageStep = (VoltageStep + 100) % ((1 << PWM_Step) - 1);
  
  delay(1000); 
}
