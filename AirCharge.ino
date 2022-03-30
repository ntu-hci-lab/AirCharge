#include <util/atomic.h> // For the ATOMIC_BLOCK macro

#define ENCA 2 // YELLOW
#define ENCB 3 // WHITE
#define PWM 5
#define IN2 9
#define IN1 10
#define FORCE 6
#define VALVE 8
#define CLUTCH 4


volatile int posi = 0;// specify posi as volatile: https://www.arduino.cc/reference/en/language/variables/variable-scope-qualifiers/volatile/
volatile int m_speed = 0;
volatile int dir = 1;
int pressure = 20;
int air_duration = 100;
long prevT = 0;
float eprev = 0;
float eintegral = 0;
int target_angle = 0;
int pinA = 11; // Connected to CLK on KY-040
int pinB =12; // Connected to DT on KY-040
int encoderPosCount = 0;
int pinALast;
int aVal;
boolean bCW;
boolean serialWrite = false;
boolean writeAngle = false;
int initialPosCount;

void setup() {
  Serial.begin(115200);

  pinMode(ENCA,INPUT);
  pinMode(ENCB,INPUT);
  pinMode (pinA,INPUT);
  pinMode (pinB,INPUT);
  pinALast = digitalRead(pinA);
  attachInterrupt(digitalPinToInterrupt(ENCA),readEncoder,RISING);
  
  pinMode(PWM,OUTPUT);
  pinMode(IN1,OUTPUT);
  pinMode(IN2,OUTPUT);
  pinMode(FORCE, OUTPUT);
  pinMode(VALVE, OUTPUT);
  //pinMode(CLUTCH, OUTPUT);
  
  Serial.println("target pos");
  analogWrite(FORCE, 0);
}

void loop() {  
    //analogWrite(FORCE, 20);
    //clutchStop();
    //int m_speed = 0;
   if(Serial.available()>0){
    char cmd = Serial.read();
    if(cmd == 'f'){
      m_speed = 0;
      initialPosCount = encoderPosCount;
      clutchStop();
      fireStart();
    }
    else if(cmd == 'y'){
      clutchStart();
    }
    else if(cmd == 'n'){
      clutchStop();
    }
    else if(cmd == 'r'){
      dir = 1;
      m_speed = 255;
    }
    else if(cmd == 's'){
      m_speed = 0;
    }
    else if(cmd == 'b'){
      dir = -1;
      m_speed = 255;
    }
    else if(cmd == 'c'){
      pressure = parseData();
      air_duration = parseTime();
      //Serial.println(pressure);
      analogWrite(FORCE, pressure);
    }
    else if(cmd == 't'){
      //0015 for 30 degree
      //0030 for 60 degree
      //0055 for 90 degree
      target_angle = parseTime();
      m_speed = parseData();
      posi = 0;
      dir = -1;
      //Serial.println("motor command");
    }
    else if(cmd == 'w'){
      target_angle = parseTime();
      m_speed = parseData();
      posi = 0;
      dir = -1;
      serialWrite = true;
    }
    else if(cmd == 'a'){
      writeAngle = !writeAngle;
      //initialPosCount = encoderPosCount;
      Serial.println(writeAngle);
    }
  }
  aVal = digitalRead(pinA);
  if (aVal != pinALast){
    if (digitalRead(pinB) != aVal){
      encoderPosCount ++;
      bCW = true;
    }else {// Otherwise B changed first and we're moving CCW
      bCW = false;
      encoderPosCount--;
    }
    //Serial.print ("Rotated: ");
    if (bCW){
      //Serial.println ("clockwise");
    }else{
      //Serial.println("counterclockwise");
    }
    //Serial.print("Encoder Position: ");
    //Serial.println(encoderPosCount);
    if(writeAngle){
      String msg = "EncoderRead:" + String(encoderPosCount - initialPosCount) + "!";
      char* cString = (char*) malloc(sizeof(char)*(msg.length() + 1));
      msg.toCharArray(cString, msg.length() + 1);
      Serial.write(cString);
    }
  }
  pinALast = aVal;
  // set target position
  int target = 75;
  //int target = 250*sin(prevT/1e6);

  
  // motor direction
      //int dir = 1;
      // signal the motor
      setMotor(dir, m_speed,PWM,IN1,IN2);



  Serial.println(posi);
  //.Serial.println(target);


}

int parseData(){
  String num = "";
  char c;
   for(int i = 0; i < 3; i++){
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

int parseTime(){
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

void setMotor(int dir, int pwmVal, int pwm, int in1, int in2){
  analogWrite(pwm,pwmVal);
  if(dir == 1 && posi < target_angle){
    digitalWrite(in1,HIGH);
    digitalWrite(in2,LOW);
  }
  else if(dir == -1 && -posi < target_angle){
    //Serial.println(posi);
    digitalWrite(in1,LOW);
    digitalWrite(in2,HIGH);
  }
  else{
    if(serialWrite){
      Serial.write("Done");
      serialWrite = false;
    }
    digitalWrite(in1,LOW);
    digitalWrite(in2,LOW);
  }  
  //Serial.println(posi);
}

void readEncoder(){
  int b = digitalRead(ENCB);
  if(b > 0){
    posi++;
  }
  else{
    posi--;
  }
}

void fireStart(){
  Serial.println("fire");
  digitalWrite(VALVE, HIGH);
  delay(air_duration);
  digitalWrite(VALVE, LOW);
}

void clutchStart(){
  Serial.println("connect");
  digitalWrite(CLUTCH, HIGH);

}

void clutchStop(){
  Serial.println("disconnect");

  digitalWrite(CLUTCH, LOW);
}
