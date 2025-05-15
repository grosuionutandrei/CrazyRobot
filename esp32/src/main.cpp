#include "mqtt/mqtt.h"
#include <ArduinoJson.h>
#include "servo/ServoManager.h"

ServoManager servoManager;

void reader();

void setup() {

    Serial.begin(115200);

    if (servoManager.setup()) {
        Serial.println("Servo Setup Successfull");
    } else {
        Serial.println("Servo Setup Failed.");
    };
}

void loop() {

    reader();
}

// Only here for debug purposes
void reader() {
    if (Serial.available() > 0) {
        int angle = Serial.parseInt();
        if (angle <= 0) return;
        if (angle >= 0 && angle <= 180) {
            servoManager.moveServo(LEYE, angle);
            Serial.print("Moving servo to: ");
            Serial.println(angle);
        } else {
            Serial.println("Invalid angle. Please enter a value between 0 and 180.");
        }
    }

    delay(500);
}
