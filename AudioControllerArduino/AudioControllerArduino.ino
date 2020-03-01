#include "SPI.h"
#include "Adafruit_GFX.h"
#include "Adafruit_ILI9341.h"

// For the Adafruit shield, these are the default.
#define TFT_DC 9
#define TFT_CS 10

#define PLAYBUTTON 6
#define CLK 2
#define DT 1

int presentState;
int previousState;

// Use hardware SPI (on Uno, #13, #12, #11) and the above for CS/DC
Adafruit_ILI9341 tft = Adafruit_ILI9341(TFT_CS, TFT_DC);
void setup()
{
  Serial.begin(200000);
  tft.begin();
  tft.fillScreen(ILI9341_BLACK);
  pinMode(PLAYBUTTON, INPUT);
  pinMode(CLK, INPUT);
  pinMode(DT, INPUT);
  previousState = digitalRead(CLK);
}

int lastTime = 0;

bool playing = false;
int32_t timeInSong = 0;
int32_t totalTime = 0;
void loop(void)
{
  int dt = millis() - lastTime;
  if (Serial.available() > 0) {
    byte buff[1];
    Serial.readBytes(buff, 1);
    switch (buff[0]) {
      case 0x00: //Image p(uint16_t[240*240] pixels)
        loadBitmap();
        break;
      case 0x01: //Play p(int32 time,int32 totaltime)
        playing = true;
        loadTimeSettings();
        break;
      case 0x02: //Pause p(int32 time,int32 totalTime)
        playing = false;
        loadTimeSettings();
        break;
      case 0x03: //SetTime p(int32 time,int32 totalTime)
        loadTimeSettings();
        break;
    }
  }
  if (playing)
    timeInSong += dt;
  tft.setCursor(0, 280);
  tft.setTextColor(ILI9341_WHITE);
  tft.setTextSize(2);
  tft.println(timeInSong / 1000);
  tft.println(totalTime / 1000);

  int pressed = digitalRead(PLAYBUTTON);
  if (pressed == LOW) {
    Serial.write(0x01);
  }
  doEncoders();
  lastTime = millis();
}

void loadTimeSettings() {
  byte params[8];
  Serial.readBytes(params, 8);
  timeInSong = 0;
  timeInSong =  (uint32_t) params[3] << 24;
  timeInSong |= (uint32_t) params[2] << 16;
  timeInSong |= (uint32_t) params[1] << 8;
  timeInSong |= (uint32_t) params[0];
  totalTime = 0;
  totalTime =  (uint32_t) params[7] << 24;
  totalTime |= (uint32_t) params[6] << 16;
  totalTime |= (uint32_t) params[5] << 8;
  totalTime |= (uint32_t) params[4];
}

void loadBitmap() {
  int height = tft.height();
  int width = tft.width();
  for (int y = 0; y < height; y++) {
    for (int x = 0; x < width; x++) {
      byte pixel[2];
      Serial.readBytes(pixel, 2);
      tft.drawPixel(x, y, (uint16_t)(pixel[1] << 8 | pixel[0]));
    }
  }
}

void doEncoders() {
  presentState = digitalRead(CLK);
  if (presentState != previousState)
  {
    if (digitalRead(DT) != presentState)
    {
      Serial.write(0x05);
      Serial.write('\n');
    }
    else {
      Serial.write(0x06);
      Serial.write('\n');
    }
  }
  previousState = presentState;
}
