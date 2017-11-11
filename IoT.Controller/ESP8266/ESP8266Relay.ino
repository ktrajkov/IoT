#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <WiFiUdp.h>
#include <WebSocketsClient.h>
#include <ArduinoOTA.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include <DS18B20Sensor.h>
#include <Hash.h>
#include <ArduinoJson.h>

#define _DEBUG
#define SERIAL Serial

#pragma region Network settings 
#ifdef _DEBUG
#define WEB_SOCKET_SERVER_IP "192.168.2.150"
#define WEB_SOCKET_SERVER_PORT 52821
#else
#define WEB_SOCKET_SERVER_IP "iot-1.apphb.com"
#define WEB_SOCKET_SERVER_PORT 80
#endif
#define WEB_SOCKET_SERVER_PATH "/iot"
#define CLIENT_ID "Arduino1"

WebSocketsClient webSocket;

IPAddress ip(192, 168, 2, 152);
IPAddress gateway(192, 168, 2, 1);
IPAddress subnet(255, 255, 255, 0);
IPAddress dns(46, 40, 72, 9);

const char* ssid = "linksys";
const char* password = "kaluhckua";
const char* otapass = "AOTA0887";

#pragma endregion

#pragma region Pin definitions

#define ESP_BUILTIN_LED  13
#define ESP_RELAY 12

const int SENSOR_PINS[] = {  };

#pragma endregion

#pragma region Sensors
const int NUMBER_OF_SENSORS = sizeof(SENSOR_PINS) / sizeof(int);
Sensor sensors[NUMBER_OF_SENSORS];
float currentTemps[NUMBER_OF_SENSORS];

#pragma endregion

void setup() {

#pragma region Setup Wifi

	WiFi.config(ip, gateway, subnet, dns);
	WiFi.mode(WIFI_STA);
	WiFi.begin(ssid, password);
	while (WiFi.waitForConnectResult() != WL_CONNECTED)
	{
		Serial.println("Connection Failed! Rebooting...");
		delay(5000);
		ESP.restart();
	}

#pragma endregion

#pragma region Setup OTA
	// Port defaults to 8266	
	ArduinoOTA.setPort(8266);

	// No authentication by default
	ArduinoOTA.setPassword(otapass);

	ArduinoOTA.onStart([]() {
		Serial.println("Start");
	});
	ArduinoOTA.onEnd([]() {
		Serial.println("\nEnd");
	});
	ArduinoOTA.onProgress([](unsigned int progress, unsigned int total) {
		Serial.printf("Progress: %u%%\r", (progress / (total / 100)));
	});
	ArduinoOTA.onError([](ota_error_t error) {
		Serial.printf("Error[%u]: ", error);
		if (error == OTA_AUTH_ERROR) Serial.println("Auth Failed");
		else if (error == OTA_BEGIN_ERROR) Serial.println("Begin Failed");
		else if (error == OTA_CONNECT_ERROR) Serial.println("Connect Failed");
		else if (error == OTA_RECEIVE_ERROR) Serial.println("Receive Failed");
		else if (error == OTA_END_ERROR) Serial.println("End Failed");
	});
	ArduinoOTA.begin();
#pragma endregion

#pragma region Setup WebSocketClient
	// server address, port and URL
	webSocket.begin(WEB_SOCKET_SERVER_IP, WEB_SOCKET_SERVER_PORT, WEB_SOCKET_SERVER_PATH);
	// event handler
	webSocket.onEvent(WebSocketEvent);
	// try ever 5000 again if connection has failed
	webSocket.setReconnectInterval(5000);
#pragma endregion

#pragma region Setup Serial ports

	SERIAL.begin(115200);
	SERIAL.println("Booting");

#pragma endregion

#pragma region Setup Pins

	pinMode(ESP_BUILTIN_LED, OUTPUT);
	pinMode(ESP_RELAY, OUTPUT);

#pragma endregion

#pragma region Setup Sensors

	for (size_t i = 0; i < NUMBER_OF_SENSORS; i++)
	{
		sensors[i] = Sensor(SENSOR_PINS[i]);
	}

#pragma endregion

#pragma region Print setup state

	Serial.println("Ready");
	Serial.print("IP address: ");
	Serial.println(WiFi.localIP());
	Serial.println(WiFi.gatewayIP());
	Serial.println(WiFi.subnetMask());
	Serial.println(WiFi.dnsIP());
	Serial.print("WS Server: ");
	Serial.println(WEB_SOCKET_SERVER_IP);

#pragma endregion
}

void loop() {
	digitalWrite(ESP_BUILTIN_LED, !digitalRead(ESP_BUILTIN_LED));

	ArduinoOTA.handle();
	delay(2000);
	//ReadTemps();
	//SendTemps();
	webSocket.loop();
}

void WebSocketEvent(WStype_t type, uint8_t * payload, size_t length)
{
	switch (type)
	{
	case WStype_DISCONNECTED:
		SERIAL.printf("[WSc] Disconnected!\n");
		break;
	case WStype_CONNECTED:
		SERIAL.printf("[WSc] Connected to url: %s\n", payload);
		break;
	case WStype_TEXT:
	{
		SERIAL.printf("[WSc] get text: %s\n", payload);
		StaticJsonBuffer<400> jsonBuffer;
		JsonObject& root = jsonBuffer.parseObject(payload);

		String result;
		root.printTo(result);
		SERIAL.print("result: ");
		SERIAL.println(result);


		//	[WSc] get text : {"messageType":0, "data" : "OFF"}
		String  status = root["data"];
		if (status == "ON")
		{
			//digitalWrite(ESP_RELAY, true);
			SERIAL.println("relay: true");
		}
		else if (status == "OFF")
		{
			//digitalWrite(ESP_RELAY, false);
			SERIAL.println("relay: false");
		}
	}
	break;
	case WStype_BIN:
		SERIAL.printf("[WSc] get binary length: %u\n", length);
		hexdump(payload, length);
		// send data to server
		// webSocket.sendBIN(payload, length);
		break;
	case WStype_ERROR:
		SERIAL.printf("[WSc] ERROR: %s\n", payload);
		break;
	}
}

void SendTemps()
{
	StaticJsonBuffer<400> jsonBuffer;

	//nead refactor
	String message = "{\"MethodName\":\"UpdateTemps\"}";
	JsonObject& root = jsonBuffer.parseObject(message);
	JsonArray& arguments = root.createNestedArray("Arguments");
	JsonObject& argument = arguments.createNestedObject();
	argument["ClientId"] = CLIENT_ID;
	JsonArray& tempArray = argument.createNestedArray("Temps");

	for (int i = 0; i < NUMBER_OF_SENSORS; i++)
	{
		JsonObject& chanelInside = tempArray.createNestedObject();
		chanelInside["Id"] = i;
		chanelInside["Value"] = currentTemps[i];
	}
	String result;
	root.printTo(result);
	SERIAL.println(result);
	webSocket.sendTXT(result);
}

void  ReadTemps()
{
	for (int i = 0; i < NUMBER_OF_SENSORS; i++)
	{
		currentTemps[i] = sensors[i].GetCurrentTemp();
	}
}