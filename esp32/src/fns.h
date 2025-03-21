#ifndef FNS_h
#define FNS_h

#define IN1 26
#define IN2 0
#define IN3 2
#define IN4 13
#define ENA 25
#define ENB 14
const int pwmChannel1 = 0;
const int pwmChannel2 = 1; 
const int freq = 8000; 
const int resolution = 8;   
enum Direction {FORWARD, BACKWARD,RIGHT,LEFT, STOP};

void setupMotors();
void moveRobot(Direction dir, int speed);



#endif
