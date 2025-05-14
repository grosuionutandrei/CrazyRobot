#pragma once

#include <Wire.h>
#include <Adafruit_PWMServoDriver.h>

#define SERVO_MIN  100
#define SERVO_MAX  580
#define SERVO_FREQ 60
#define SERVO_COUNT 7


enum ServoID : uint8_t {
    NECK = 0,
    LEYE,
    REYE,
    HEAD,
    LHAND,
    RHAND,
};

enum Position {
    LEFT,
    RIGHT,
    CENTER,
    UP,
    DOWN
};

struct ServoConfig {
    int channel;
    int initialAngle;
    int minAngle;
    int maxAngle;
    const char* name;
};

const ServoConfig SERVO_CONFIGS[SERVO_COUNT] = {
    // ID             CHA   INIT  MIN  MAX   NAME
    { /* NECK */       0,   90,   0,  180,  "Neck" },
    { /* HEAD */       1,   45,  10,  170,  "Head" },
    { /* LEYE */       2,   90,  30,  150,  "LeftEye" },
    { /* REYE */       3,   90,   0,  180,  "RightEye" },
    { /* LHAND */      4,   90,   0,  180,  "LeftHand" },
    { /* RHAND */      5,   50,   0,  100,  "RightHand" }
};

