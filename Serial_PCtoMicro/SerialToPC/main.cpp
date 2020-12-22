//------------------------------------------------------------------------------------
//
//	Code: PC to uController Serial (main.cpp)
//	Authors: Zachary Garrard, Justin Cox, Casey Wood
//	Date: 3/10/2016
//
//------------------------------------------------------------------------------------

#pragma warning(disable:4996)
#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <Windows.h>
#include "Serial.h"
#include <string>
#include <iostream>
#include <fstream>
#include <bitset>

#define G0 0x4730
#define G1 0x4731
#define G4 0x4734
#define M2 0x4D32
#define M4 0x4D34
#define M5 0x4D35
#define S0 0x5330
#define S1 0x5331
#define S2 0x5332
#define S3 0x5333
#define S4 0x5334
#define S5 0x5335
#define S6 0x5336
#define S7 0x5337
 
COMSTAT comStat;
DWORD   dwErrors;

void readUart(void);
void splitShort(uint16_t short2Split, uint8_t &msb, uint8_t &lsb, uint8_t buffer[]);
bool getRowData(FILE *inFile, uint16_t xCoor[], uint16_t yCoor[], uint16_t gCode[], uint16_t pauseP[], uint16_t size);
void sendRowData(uint16_t xCoor[], uint16_t yCoor[], uint16_t gCode[], uint16_t pauseP[], uint16_t size, int jay, uint16_t rows);
void clearErrs();
bool checkResend();

//------------------------------------------------------------------------------------
//	FUNCTION MAIN
//------------------------------------------------------------------------------------

CSerial serial;


int main(){

	//*************************
	//	Serial Vars
	//*************************
	int port = 5;//3; 
	int baudRate = 115200;//9600; //115200
	int dispType = 0;
	int nBytesSent = 0;
	
	std::cout << "Please insert your UART COM port number: ";
	std::cin >> port;
	
	std::cout << "Please insert your UART baud rate (default is 115200): ";
	std::cin >> baudRate;

	uint8_t buffer[2];

	uint8_t shortMSB; 
	uint8_t shortLSB;
	//*************************
	//	File IO Vars
	//*************************
	FILE *fp;
	
	uint16_t xInit = 0;
	uint16_t yInit = 0;
	uint16_t pInit = 0;
	uint16_t g00 = G0;
	uint16_t g4 = G4;
	
	uint8_t garbage[100] = {0};
	uint8_t chG;

	uint16_t *xCoors;
	uint16_t *yCoors;
	uint16_t *gCodes;
	uint16_t *pauseP;

	uint16_t sizeCol = 0;
	uint16_t sizeRow = 0;

	
	//-----------------------------------
	//		GET GCODE LOCATION
	//-----------------------------------
	
	//define G-Code location path string and file stream
	std::string gcodeloc;
	std::ifstream gcloc("C:\\PLED\\PLEDpath.txt");
	
	//get G-Code Location
	std::getline(gcloc, gcodeloc);
	
	//close file stream
	gcloc.close();	
	
	//-----------------------------------
	//		PARSER CODE
	//-----------------------------------
	
	//convert string to const char
	const char * gpath = gcodeloc.c_str();
	fp = fopen(gpath, "r");

	if(!fp){
		printf("Error opening file! \n");
		return 0;
	}

	//Get Size of image for arrays
	fscanf(fp, "size %hd,%hd", &sizeCol, &sizeRow);
	chG = fgetc(fp);
	//printf("SizeX is: %hd and SizeY is: %hd \n", sizeX, sizeY);

	//Dynamically set size of arrays
	xCoors = new uint16_t [sizeCol];
	yCoors = new uint16_t [sizeCol];
	pauseP = new uint16_t [sizeCol];
	gCodes = new uint16_t [(sizeCol * 5)];

	//Get initial position
	fscanf(fp, "G00 X%hd Y%hd", &xInit, &yInit);
	chG = fgetc(fp);
	
	fscanf(fp, "G04 P0.%hd", &pInit);
	chG = fgetc(fp);

	//Open Serial Port
	if (!serial.Open(port, baudRate)){
		printf("Error opening COM port! \n");
		return 0;
	}

	
	for(int j = 0; j < sizeRow + 1; j++)
	{
		if(getRowData(fp, xCoors, yCoors, gCodes, pauseP, sizeCol)){
			//for(int i = 0; i < sizeCol; i++){
				//printf("X:%hd Y:%hd \n", xCoors[i], yCoors[i]);
				//printf("P value: %hd \n", pauseP[i]);
			//}

			//----------------------------
			//Send inital values
			//----------------------------


			if(j == 0)
			{	
				//	ZZ sizeCol, sizeRow
				buffer[0] = 'Z';
				buffer[1] = 'Z';
				serial.SendData(buffer, 2);
				splitShort(sizeCol, shortMSB, shortLSB, buffer);
				serial.SendData(buffer, 2);
				splitShort(sizeRow, shortMSB, shortLSB, buffer);
				serial.SendData(buffer, 2);
				//	G0, Xinit, Yinit
				splitShort(g00, shortMSB, shortLSB, buffer);
				serial.SendData(buffer, 2);
				splitShort(xInit, shortMSB, shortLSB, buffer);
				serial.SendData(buffer, 2);
				splitShort(yInit, shortMSB, shortLSB, buffer);
				serial.SendData(buffer, 2);
				//	G4, Pinit
				splitShort(g4, shortMSB, shortLSB, buffer);
				//serial.SendData(buffer, 2);
				splitShort(pInit, shortMSB, shortLSB, buffer);
				//serial.SendData(buffer, 2);
			}

			//----------------------------
			//Send row 
			//----------------------------
			sendRowData(xCoors, yCoors, gCodes, pauseP, sizeCol,j,sizeRow);
			//check to see if the row was recieved correctly, if not resend, if so, wait for the GO-ahead to send the next row
			while (checkResend())
			{
				//resend row
				sendRowData(xCoors, yCoors, gCodes, pauseP, sizeCol, j, sizeRow);
			}

			//Read GO to send next row
			readUart();
			for (int j = 0; j < 240000000; j++)
			{
				int m = 0;
			}

		}
		else{
			buffer[0] = 'M';
			buffer[1] = '2';
			serial.SendData(buffer, 2);
			clearErrs();
			buffer[0] = 'R';
			buffer[1] = 'D';
			serial.SendData(buffer, 2);
		}

	}//End of for()

	serial.Close();

	delete [] xCoors;
	delete [] yCoors;
	delete [] gCodes;
	delete [] pauseP;

	return 0;
}

bool getRowData(FILE *inFile, uint16_t xCoor[], uint16_t yCoor[], uint16_t gCode[], uint16_t pauseP[], uint16_t size){
	uint8_t firstChar;
	uint8_t secondChar;
	uint8_t thirdChar;
	uint16_t checker;
	const char* input[3];
	int ascii;
	uint8_t awesome=0;
	uint8_t temp;
	int zephyr=0;
	int remainder = 0;
	uint8_t chG;
	uint32_t iPix = 0;
	uint32_t counter = 1;
	long binary = 0, a = 1;
	uint16_t *b;
	char *byteptr = (char *)gCode;

	for(int i = 0; i < (size * 5); i++){
		
		firstChar = fgetc(inFile);
		//printf("Character in question: %c \n",firstChar);
		
		switch(firstChar){
			case 'G':
				secondChar = fgetc(inFile);
				thirdChar = fgetc(inFile);
				if(thirdChar == '1'){
					fscanf(inFile, " X%hd Y%hd", &xCoor[iPix], &yCoor[iPix]);
					//printf("X and Y: %hd %hd \n",xCoor[iPix], yCoor[iPix]);
					chG = fgetc(inFile);
					gCode[i] = G1;
				}
				else if(thirdChar == '4'){
					fscanf(inFile, " P0.0%hd", &pauseP[iPix]);
					chG = fgetc(inFile);
					gCode[i] = G4;
				}
				else{
					printf("Unkown Gcode with 'G' \n");
				}
				break;
			case 'S':
				/*temp = fgetc(inFile);
				temp = temp - '0';
				zephyr = zephyr + temp*100;
				temp = fgetc(inFile);
				temp = temp - '0';
				zephyr = zephyr + temp*10;
				temp = fgetc(inFile);
				temp = temp - '0';
				zephyr = zephyr + temp;
				secondChar = zephyr;
				//memcpy(gCode, &secondChar, 1);*/
				secondChar = 'S';
				//memcpy(byteptr+1, &secondChar, 1);
				fscanf(inFile, "%hd", &byteptr[2 * i]);
				memcpy(&byteptr[2*i]+1, &secondChar, 1);
				
				
						
					/*default:
						printf("Unkown S code \n");
						break;*/
				//} //end of switch
				chG = fgetc(inFile);
				break;
			case 'M':
				secondChar = fgetc(inFile);
				thirdChar = fgetc(inFile);

				if(thirdChar == '4'){
					gCode[i] = M4;
				}
				else if(thirdChar == '5'){
					gCode[i] = M5;
				}
				else if(thirdChar == '2'){
					return false;
				}
				else {
					printf("Unknown M code \n");
				}
				chG = fgetc(inFile);
				break;
			default:
				printf("Unknown line from file \n");
				break;
		} //end of switch

		if(counter == 5){
			iPix++;
			counter = 0;
		}

		counter++;
	}// end of for

	return true;
}

void clearErrs()
{
	ClearCommError(serial.m_hIDComDev, &dwErrors, &comStat);
	while ((comStat.fCtsHold))
	{
		//wait until the CTS line says we can send
		ClearCommError(serial.m_hIDComDev, &dwErrors, &comStat);
	}
	return;
}

void sendRowData(uint16_t xCoor[], uint16_t yCoor[], uint16_t gCode[], uint16_t pauseP[], uint16_t size, int jay, uint16_t rows){
	uint8_t shortMSB;
	uint8_t shortLSB;
	uint8_t buffer[2];

	uint32_t nBytesSent = 0;

	uint32_t iPix = 0;

	for(int i = 0; i < (size * 5); i = i + 5){
		//Order to send
		/*for (int j = 0; j < 12000000; j++)
		{
			int m = 0;
		}*/
		//	Gcode[i], X[iPix], Y[iPix]  <--- G00 X#,Y#
		clearErrs();
		splitShort(gCode[i], shortMSB, shortLSB, buffer);
		serial.SendData(buffer, 2);
		clearErrs();
		splitShort(xCoor[iPix], shortMSB, shortLSB, buffer);
		serial.SendData(buffer, 2);
		clearErrs();
		splitShort(yCoor[iPix], shortMSB, shortLSB, buffer);
		serial.SendData(buffer, 2);
		//  Gcode[i+1]	<--- S#
		clearErrs();
		splitShort(gCode[i+1], shortMSB, shortLSB, buffer);
		serial.SendData(buffer, 2);
		//  Gcode[i+2]	<--- M04
		clearErrs();
		splitShort(gCode[i+2], shortMSB, shortLSB, buffer);
		//serial.SendData(buffer, 2);
		//	Gcode[i+3], P[iPix]	<--- G04 P#
		clearErrs();
		splitShort(gCode[i+3], shortMSB, shortLSB, buffer);
		//serial.SendData(buffer, 2);
		clearErrs();
		splitShort(pauseP[iPix], shortMSB, shortLSB, buffer);
		//serial.SendData(buffer, 2);
		//	Gcode[i+4]	<--- M05
		clearErrs();
		splitShort(gCode[i+4], shortMSB, shortLSB, buffer);
		//serial.SendData(buffer, 2);

		iPix++;
		//if (iPix == 275)
		//{
		//	buffer[0] = 0;
		//}
	}
	
	//	RD when row has been sent
	clearErrs();
	if (jay != rows)//only send on all but the last row
	{
		buffer[0] = 'R';
		buffer[1] = 'D';
		serial.SendData(buffer, 2);
	}

	//nBytesSent = serial.SendData(buffer, SIZE);

}

void splitShort(uint16_t short2Split, uint8_t &msb, uint8_t &lsb, uint8_t buffer[]){
	msb = short2Split >> 8;
	lsb = short2Split & 0x00FF;

	buffer[0] = msb;
	buffer[1] = lsb;
}

void readUart(void){

	int nBytesRead = 0;
	int curT = 0; 
	int oldT = 0;
	uint8_t Gflag = 0;
	uint8_t Oflag = 0;

	char readBuffer[1];
	
	while(Gflag == 0 && Oflag == 0)
	{
		curT = GetTickCount();

		if (curT - oldT > 5)
		{
			nBytesRead = serial.ReadData(readBuffer, sizeof(readBuffer));
	
			if (nBytesRead > 0)
			{

				for(int i = 0; i < nBytesRead; i++){
					if(readBuffer[i] == 'G'){
						Gflag = 1;
					}
					else if(readBuffer[i] == 'O'){
						Oflag = 1;
					}
					printf("%c", readBuffer[i]);
				}

			}

			oldT = curT;
		}

	}
	for (int j = 0; j < 50; j++)
	{
		serial.ReadData(readBuffer, sizeof(readBuffer));
	}
	readBuffer[0] = '\0';

}

bool checkResend(){
	//RS=resend MB=muy bueno
	int nBytesRead = 0;
	int curT = 0;
	int oldT = 0;
	uint8_t Rflag = 0;
	uint8_t Sflag = 0;
	uint8_t Mflag = 0;
	uint8_t Bflag = 0;
	bool RS = 0;
	bool MB = 0;

	char readBuffer[1];

	while (RS==false && MB==false)
	{
		curT = GetTickCount();

		if (curT - oldT > 5)
		{
			nBytesRead = serial.ReadData(readBuffer, sizeof(readBuffer));

			if (nBytesRead > 0)
			{

				for (int i = 0; i < nBytesRead; i++){
					if (readBuffer[i] == 'R'){
						Rflag = 1;
					}
					else if (readBuffer[i] == 'S'){
						Sflag = 1;
					}
					else if (readBuffer[i] == 'M'){
						Mflag = 1;
					}
					else if (readBuffer[i] == 'B'){
						Bflag = 1;
					}
					printf("%c", readBuffer[i]);
				}

			}

			oldT = curT;
		}
		if (Rflag == 1 && Sflag == 1)
			RS = true;
		if (Mflag == 1 && Bflag == 1)
			MB = true;
	}
	for (int j = 0; j < 50; j++)
	{
		serial.ReadData(readBuffer, sizeof(readBuffer));
	}
	readBuffer[0] = '\0';
	if (Mflag == 1 && Bflag == 1)
		return false;
	else 
		return true;

}