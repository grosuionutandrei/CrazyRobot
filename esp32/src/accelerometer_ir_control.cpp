#include <Wire.h>
#include <Arduino.h>
// MMA8452Q I2C address
#define MMA8452Q_ADDR 0x1D

// IR sensor and motor control pins
const int sensorPin = 25;
const int motorPinINA = 26;
const int motorPinINB = 14;

void setup() {
  Serial.begin(115200);

  // Configure pin modes
  pinMode(sensorPin, INPUT);
  pinMode(motorPinINA, OUTPUT);
  pinMode(motorPinINB, OUTPUT);

  Wire.begin(); // I2C init: SDA = GPIO21, SCL = GPIO22

  // --- Initialize MMA8452Q ---
  // Put MMA8452Q in standby mode
  Wire.beginTransmission(MMA8452Q_ADDR);
  Wire.write(0x2A); // CTRL_REG1
  Wire.write(0x00); // Standby mode
  Wire.endTransmission();

  // Set accelerometer range to ±2g
  Wire.beginTransmission(MMA8452Q_ADDR);
  Wire.write(0x0E); // XYZ_DATA_CFG
  Wire.write(0x00); // ±2g range
  Wire.endTransmission();

  // Set MMA8452Q to active mode
  Wire.beginTransmission(MMA8452Q_ADDR);
  Wire.write(0x2A); // CTRL_REG1
  Wire.write(0x01); // Active mode
  Wire.endTransmission();

  delay(100);
  Serial.println("✅ MMA8452Q initialized.");
}

void loop() {
  int sensorValue = digitalRead(sensorPin);
  int16_t xAccl = 0, yAccl = 0, zAccl = 0;

  // --- Read 7 bytes from MMA8452Q starting at register 0x00 ---
  Wire.beginTransmission(MMA8452Q_ADDR);
  Wire.write(0x00); // Start reading from status register
  Wire.endTransmission(false);
  Wire.requestFrom(MMA8452Q_ADDR, 7);

  if (Wire.available() == 7) {
    Wire.read(); // Status byte (not used here)
    int16_t x = (Wire.read() << 8) | Wire.read();
    int16_t y = (Wire.read() << 8) | Wire.read();
    int16_t z = (Wire.read() << 8) | Wire.read();

    // Convert raw values (12-bit left aligned)
    xAccl = x >> 4;
    yAccl = y >> 4;
    zAccl = z >> 4;

    // Apply sign correction
    if (xAccl > 2047) xAccl -= 4096;
    if (yAccl > 2047) yAccl -= 4096;
    if (zAccl > 2047) zAccl -= 4096;
  }
  
  // --- Output to Serial Monitor ---
  Serial.print("[IR] ");
  Serial.print(sensorValue == LOW ? "Flat surface" : "Irregularity");
  Serial.print(" | Z: ");
  Serial.print(zAccl);
  Serial.println(" (approx. ~1000 = 1g)");

  // --- Control motor logic ---
  // If surface is flat AND device is stable (Z acceleration > threshold)
  if (sensorValue == LOW && zAccl > 700) {
    Serial.println("✅ Motor running");
    digitalWrite(motorPinINA, HIGH);
    digitalWrite(motorPinINB, LOW);
  } else {
    // Either IR detects hole/edge or Z indicates instability
    Serial.println("⛔ Motor stopped (IR or Z condition)");
    digitalWrite(motorPinINA, LOW);
    digitalWrite(motorPinINB, LOW);
  }

  delay(200); // Short delay before next loop
}
