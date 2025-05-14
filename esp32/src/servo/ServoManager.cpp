#include "ServoManager.h"


ServoManager::ServoManager() : pwm(), initialized(false)  {

};


bool ServoManager::setup() {
    pwm.begin();
    pwm.setPWMFreq(SERVO_FREQ);
    initialized = true;
    return initialized;
};


void ServoManager::setServoAngle(uint8_t channel, int angle) {
    angle = constrain(angle, 0, 180);
    int pulse = map(angle, 0, 180, SERVO_MIN, SERVO_MAX);
    pwm.setPWM(channel, 0, pulse);
};


void ServoManager::moveServo(ServoID id, int angle) {
    setServoAngle(static_cast<uint8_t>(id), angle);
};


void ServoManager::moveServo(ServoID id, Position position) {
    switch (id) {
        case NECK:
            neckMovements(position);
            break;

        default:
            // Optional: log or ignore
            break;
    }
};


/**
 * Neck Movements Diagram
 *
 *   .------R1-----.-------R2------.
 *   |             |               |
 *   L             C               R
 *  180           105              0
 */
void ServoManager::neckMovements(Position position) {
    switch (position) {

        case LEFT:
            setServoAngle(SERVO_CONFIGS[NECK].channel, 180);
            break;

        case CENTER:
            setServoAngle(SERVO_CONFIGS[NECK].channel, 105);
            break;

        case RIGHT:
            setServoAngle(SERVO_CONFIGS[NECK].channel, 0);
            break;

        default:
            setServoAngle(SERVO_CONFIGS[NECK].channel, 105);
            break;
    };
};
