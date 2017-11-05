/*
* WebSocketClient.ino
*
*  Created on: 24.05.2015
*
*/

#include "Arduino.h"
#include <OneWire.h>
#include <DallasTemperature.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <WebSocketsClient.h>
#include <ArduinoJson.h>

#include <DS18B20Sensor.h>
#include <Hash.h>

#define NumberOfSensors 2

int sensorPins[] = { 4,5 };

ESP8266WiFiMulti WiFiMulti;
WebSocketsClient webSocket;

Sensor sensors[NumberOfSensors];



#define USE_SERIAL Serial

void webSocketEvent(WStype_t type, uint8_t * payload, size_t length) {

	switch (type) {
	case WStype_DISCONNECTED:
		USE_SERIAL.printf("[WSc] Disconnected!\n");
		break;
	case WStype_CONNECTED: {
		USE_SERIAL.printf("[WSc] Connected to url: %s\n", payload);

		// send message to server when Connected
		webSocket.sendTXT("Connected");
	}
						   break;
	case WStype_TEXT:
		USE_SERIAL.printf("[WSc] get text: %s\n", payload);

		// send message to server
		// webSocket.sendTXT("message here");
		break;
	case WStype_BIN:
		USE_SERIAL.printf("[WSc] get binary length: %u\n", length);
		hexdump(payload, length);

		// send data to server
		// webSocket.sendBIN(payload, length);
		break;
	}

}

void setup() {
	for (size_t i = 0; i < NumberOfSensors; i++)
	{
		sensors[i] = Sensor(sensorPins[i]);
	}
	// USE_SERIAL.begin(921600);
	USE_SERIAL.begin(115200);

	//Serial.setDebugOutput(true);
	USE_SERIAL.setDebugOutput(true);



	for (uint8_t t = 4; t > 0; t--) {
		USE_SERIAL.printf("[SETUP] BOOT WAIT %d...\n", t);
		USE_SERIAL.flush();
		delay(1000);
	}

	WiFiMulti.addAP("linksys", "kaluhckua");

	//WiFi.disconnect();
	while (WiFiMulti.run() != WL_CONNECTED) {
		delay(100);
	}

	// server address, port and URL
	webSocket.begin("192.168.2.108", 45458, "/iot");

	// event handler
	webSocket.onEvent(webSocketEvent);	

	// try ever 5000 again if connection has failed
	webSocket.setReconnectInterval(5000);
}

void loop() {
	sendTemps();	
	delay(2000);
	webSocket.loop();
}

void sendTemps() {

	StaticJsonBuffer<200> jsonBuffer;

	String message = "{Client:{Username:\"chrome\"}}";
	JsonObject& root = jsonBuffer.parseObject(message);
	JsonArray& widgets = root.createNestedArray("Widgets");

	JsonObject& chanelInside = widgets.createNestedObject();
	chanelInside["Chanel"] = "Inside";
	chanelInside["Value"] = sensors[0].GetCurrentTemp();

	JsonObject& chanelOutside = widgets.createNestedObject();
	chanelOutside["Chanel"] = "Outside";
	chanelOutside["Value"] = sensors[1].GetCurrentTemp();

	String result;
	root.printTo(result);
	USE_SERIAL.println(result);
	webSocket.sendTXT(result);
}
