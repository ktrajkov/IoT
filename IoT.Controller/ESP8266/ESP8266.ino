

#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>

#include <ESP8266HTTPClient.h>
#include <ESP8266httpUpdate.h>

#include <WiFiUdp.h>
#include <WebSocketsClient.h>
#include <ArduinoOTA.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include <DS18B20Sensor.h>
#include <Hash.h>
#include <ArduinoJson.h>
#include <EEPROM.h>


#define SERIAL Serial

#pragma region Network settings 
#ifdef _DEBUG
#define SERVER_DOMAIN "192.168.2.150"
#define SERVER_PORT 52821
#else
#define SERVER_DOMAIN "iot-1.apphb.com"
#define SERVER_PORT 80
#endif
#define WEB_SOCKET_SERVER_PATH "/iot"
#define ACCESS_TOKEN "Arduino1"

WebSocketsClient webSocket;
IPAddress ip(192, 168, 2, 151);
IPAddress gateway(192, 168, 2, 1);
IPAddress subnet(255, 255, 255, 0);
IPAddress dns(46, 40, 72, 9);

const char* ssid = "linksys";
const char* password = "kaluhckua";
const char* otapass = "AOTA0887";

#pragma endregion

#pragma region Pin definitions

//13 for sonoff
#define ESP_BUILTIN_LED  2
#define ESP_RELAY 12

const int SENSOR_PINS[] = { 4,5 };

#pragma endregion

#pragma region Sensors
const int NUMBER_OF_SENSORS = sizeof(SENSOR_PINS) / sizeof(int);
Sensor sensors[NUMBER_OF_SENSORS];
float currentTemps[NUMBER_OF_SENSORS];

#pragma endregion

#pragma region Timers

const unsigned long ReadTempsInterval = 2000;

// Declaring the variables holding the timer values for each LED.
unsigned long ReadTempsTimer;

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
	ArduinoOTA.setPort(8266);
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
	webSocket.begin(SERVER_DOMAIN, SERVER_PORT, WEB_SOCKET_SERVER_PATH + (String)"?access_token=" + (String)ACCESS_TOKEN);
	// event handler
	webSocket.onEvent(WebSocketEvent);
	// try ever 5000 again if connection has failed
	webSocket.setReconnectInterval(10000);
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
	Serial.println(SERVER_DOMAIN);

#pragma endregion
}

void loop() {
	ArduinoOTA.handle();
	webSocket.loop();
	//digitalWrite(ESP_BUILTIN_LED, !digitalRead(ESP_BUILTIN_LED));
	if ((millis() - ReadTempsTimer) >= ReadTempsInterval)
	{
		ReadTemps();
		SendTemps();
		ReadTempsTimer = millis();
	}

}

void WebSocketEvent(WStype_t type, uint8_t * payload, size_t length)
{
	switch (type)
	{
	case WStype_DISCONNECTED:
		SERIAL.printf("[WSc] Disconnected!\n");
		break;
	case WStype_CONNECTED:
		SERIAL.println(ArduinoOTA.getHostname());
		SERIAL.printf("[WSc] Connected to url: %s\n", payload);
		break;
	case WStype_TEXT:
	{
		SERIAL.printf("[WSc] get text: %s\n", payload);
		StaticJsonBuffer<400> jsonBuffer;
		JsonObject& root = jsonBuffer.parseObject(payload);
		JsonObject& data = root["data"];

		//	[WSc] get text : {"messageType":0, "data" :{"chanel":"relay", "value": "OFF"}}
		String chanel = data["chanel"];
		String  value = data["value"];

		if (chanel == "relay")
		{
			SetRelay(value);
		}
		else if (chanel == "firmwareupdate" && value == "1")
		{
			FirmwareUpdate();
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

void FirmwareUpdate()
{
	SERIAL.println("UpdateFirmware");
	t_httpUpdate_return ret = ESPhttpUpdate.update(SERVER_DOMAIN, SERVER_PORT, "/firmware/update?clientId=Arduino1&currentVersion=test");

	switch (ret) {
	case HTTP_UPDATE_FAILED:
		SERIAL.printf("HTTP_UPDATE_FAILD Error (%d): %s", ESPhttpUpdate.getLastError(), ESPhttpUpdate.getLastErrorString().c_str());
		break;

	case HTTP_UPDATE_NO_UPDATES:
		SERIAL.println("HTTP_UPDATE_NO_UPDATES");
		break;

	case HTTP_UPDATE_OK:
		SERIAL.println("HTTP_UPDATE_OK");
		break;
	}
}

void SetRelay(String value)
{
	if (value == "ON")
	{
		digitalWrite(ESP_BUILTIN_LED, LOW);
		Log("relay: true");
	}
	else if (value == "OFF")
	{
		digitalWrite(ESP_BUILTIN_LED, HIGH);
		Log("relay: false");
	}
}


void Log(String log)
{
	StaticJsonBuffer<400> jsonBuffer;

	//nead refactor
	String message = "{\"MethodName\":\"Log\"}";
	JsonObject& root = jsonBuffer.parseObject(message);
	JsonArray& arguments = root.createNestedArray("Arguments");
	arguments.add(log);
	String result;
	root.printTo(result);
	SERIAL.println(result);
	webSocket.sendTXT(result);

}