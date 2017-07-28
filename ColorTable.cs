#pragma once

/****************************************************************************************************
 FileName:		Form1.h
	09-13-15	Works with video cam loop						
	10-02-15:	Works beautifully with LED Copntroller DMA USB sending black and white silhouette
	03-29-16:	Sends three color image with handshaking, in 64 byte packets.
	03-30-16:	Works beautifully.
	04-07-16:	Worked on Panel class implementation and initialization
	04-14-16:	Got all four orientions working on 16x32 panels
*****************************************************************************************************/

#include <Windows.h>	//Definitions for various common and not so common types like DWORD, PCHAR, HANDLE, etc.
#include <setupapi.h>	//From Windows Server 2003 R2 Platform SDK. 
#include <Winusb.h>		//Winusb.h comes as a part of the Windows Driver Kit (WDK) build 6001.18002 (and presumably later versions).
#include <Dbt.h>		//Need this for definitions of WM_DEVICECHANGE messages

#include "opencv2/core/core.hpp"  
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/video/background_segm.hpp"
#include "opencv2/highgui/highgui.hpp"
// #include "opencv2/contrib/contrib.hpp"


#include <stdio.h>

// using namespace std; // DO NOT USE!!!!
using namespace cv;


#include "stdafx.h"

using namespace System;


#define MAGENTA	0b11000011
#define PURPLE	0b10000101
#define CYAN	0b00011100
#define LIME	0b01011000
#define YELLOW	0b11100000
#define ORANGE	0b11011000

#define RED		0b11000000
#define GREEN	0b00111000
#define BLUE	0b00000111

#define PINK	0b10011011
#define LAVENDER 0b10001011

#define TURQUOISE 0b00100100


#define GRAY	0b10101101  

#define DARKGRAY 0b01011011

#define WHITE	0b11111111

#define MAXCOLOR 15
byte colorWheel[MAXCOLOR] = { MAGENTA, PURPLE, CYAN, LIME, YELLOW, ORANGE, RED, GREEN, BLUE, PINK, LAVENDER, TURQUOISE, GRAY, DARKGRAY, WHITE };

const unsigned char gammaTable[256] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5,
5, 6, 6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 9, 10, 10, 10, 11, 11, 11, 12, 12, 13, 13, 14,
14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20, 21, 21, 22, 22, 23, 23, 24, 25,
25, 26, 27, 27, 28, 29, 29, 30, 31, 31, 32, 33, 34, 34, 35, 36, 37, 37, 38, 39, 40,
41, 42, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 52, 53, 54, 55, 56, 57, 59, 60,
61, 62, 63, 64, 65, 66, 67, 68, 69, 71, 72, 73, 74, 75, 77, 78, 79, 80, 82, 83, 84,
85, 87, 88, 89, 91, 92, 93, 95, 96, 98, 99, 100, 102, 103, 105, 106, 108, 109, 111,
112, 114, 115, 117, 119, 120, 122, 123, 125, 127, 128, 130, 132, 133, 135, 137, 138,
140, 142, 144, 145, 147, 149, 151, 153, 155, 156, 158, 160, 162, 164, 166, 168, 170,
172, 174, 176, 178, 180, 182, 184, 186, 188, 190, 192, 194, 197, 199, 201, 203, 205,
207, 210, 212, 214, 216, 219, 221, 223, 226, 228, 230, 233, 235, 237, 240, 242, 245,
247, 250, 252, 255 };


#define TABLE_LENGTH 256
const long RGBtable[TABLE_LENGTH] = {
	0xff0000, 0xfc0300, 0xf90600, 0xf60900,
	0xf30c00, 0xf00f00, 0xed1200, 0xea1500,
	0xe71800, 0xe41b00, 0xe11e00, 0xde2100,
	0xdb2400, 0xd82700, 0xd52a00, 0xd22d00,
	0xcf3000, 0xcc3300, 0xc93600, 0xc63900,
	0xc33c00, 0xc03f00, 0xbd4200, 0xba4500,
	0xb74800, 0xb44b00, 0xb14e00, 0xae5100,
	0xab5400, 0xa85700, 0xa55a00, 0xa25d00,
	0x9f6000, 0x9c6300, 0x996600, 0x966900,
	0x936c00, 0x906f00, 0x8d7200, 0x8a7500,
	0x877800, 0x847b00, 0x817e00, 0x7e8100,
	0x7b8400, 0x788700, 0x758a00, 0x728d00,
	0x6f9000, 0x6c9300, 0x699600, 0x669900,
	0x639c00, 0x609f00, 0x5da200, 0x5aa500,
	0x57a800, 0x54ab00, 0x51ae00, 0x4eb100,
	0x4bb400, 0x48b700, 0x45ba00, 0x42bd00,
	0x3fc000, 0x3cc300, 0x39c600, 0x36c900,
	0x33cc00, 0x30cf00, 0x2dd200, 0x2ad500,
	0x27d800, 0x24db00, 0x21de00, 0x1ee100,
	0x1be400, 0x18e700, 0x15ea00, 0x12ed00,
	0x0ff000, 0x0cf300, 0x09f600, 0x06f900,
	0x03fc00,
	0x00ff00, 0x00fc03, 0x00f906, 0x00f609,
	0x00f30c, 0x00f00f, 0x00ed12, 0x00ea15,
	0x00e718, 0x00e41b, 0x00e11e, 0x00de21,
	0x00db24, 0x00d827, 0x00d52a, 0x00d22d,
	0x00cf30, 0x00cc33, 0x00c936, 0x00c639,
	0x00c33c, 0x00c03f, 0x00bd42, 0x00ba45,
	0x00b748, 0x00b44b, 0x00b14e, 0x00ae51,
	0x00ab54, 0x00a857, 0x00a55a, 0x00a25d,
	0x009f60, 0x009c63, 0x009966, 0x009669,
	0x00936c, 0x00906f, 0x008d72, 0x008a75,
	0x008778, 0x00847b, 0x00817e, 0x007e81,
	0x007b84, 0x007887, 0x00758a, 0x00728d,
	0x006f90, 0x006c93, 0x006996, 0x006699,
	0x00639c, 0x00609f, 0x005da2, 0x005aa5,
	0x0057a8, 0x0054ab, 0x0051ae, 0x004eb1,
	0x004bb4, 0x0048b7, 0x0045ba, 0x0042bd,
	0x003fc0, 0x003cc3, 0x0039c6, 0x0036c9,
	0x0033cc, 0x0030cf, 0x002dd2, 0x002ad5,
	0x0027d8, 0x0024db, 0x0021de, 0x001ee1,
	0x001be4, 0x0018e7, 0x0015ea, 0x0012ed,
	0x000ff0, 0x000cf3, 0x0009f6, 0x0006f9,
	0x0003fc,
	0x0000ff, 0x0300fc, 0x0600f9, 0x0900f6,
	0x0c00f3, 0x0f00f0, 0x1200ed, 0x1500ea,
	0x1800e7, 0x1b00e4, 0x1e00e1, 0x2100de,
	0x2400db, 0x2700d8, 0x2a00d5, 0x2d00d2,
	0x3000cf, 0x3300cc, 0x3600c9, 0x3900c6,
	0x3c00c3, 0x3f00c0, 0x4200bd, 0x4500ba,
	0x4800b7, 0x4b00b4, 0x4e00b1, 0x5100ae,
	0x5400ab, 0x5700a8, 0x5a00a5, 0x5d00a2,
	0x60009f, 0x63009c, 0x660099, 0x690096,
	0x6c0093, 0x6f0090, 0x72008d, 0x75008a,
	0x780087, 0x7b0084, 0x7e0081, 0x81007e,
	0x84007b, 0x870078, 0x8a0075, 0x8d0072,
	0x90006f, 0x93006c, 0x960069, 0x990066,
	0x9c0063, 0x9f0060, 0xa2005d, 0xa5005a,
	0xa80057, 0xab0054, 0xae0051, 0xb1004e,
	0xb4004b, 0xb70048, 0xba0045, 0xbd0042,
	0xc0003f, 0xc3003c, 0xc60039, 0xc90036,
	0xcc0033, 0xcf0030, 0xd2002d, 0xd5002a,
	0xd80027, 0xdb0024, 0xde0021, 0xe1001e,
	0xe4001b, 0xe70018, 0xea0015, 0xed0012,
	0xf0000f, 0xf3000c, 0xf60009, 0xf90006,
	0xfc0003, 0xff0000
};

long RGBsortedTable[TABLE_LENGTH] = {
	0x0000ff, 0x0003fc, 0x0006f9, 0x0009f6, 0x000cf3, 0x000ff0, 0x0012ed, 0x0015ea,
	0x0018e7, 0x001be4, 0x001ee1, 0x0021de, 0x0024db, 0x0027d8, 0x002ad5, 0x002dd2,
	0x0030cf, 0x0033cc, 0x0036c9, 0x0039c6, 0x003cc3, 0x003fc0, 0x0042bd, 0x0045ba,
	0x0048b7, 0x004bb4, 0x004eb1, 0x0051ae, 0x0054ab, 0x0057a8, 0x005aa5, 0x005da2,
	0x00609f, 0x00639c, 0x006699, 0x006996, 0x006c93, 0x006f90, 0x00728d, 0x00758a,
	0x007887, 0x007b84, 0x007e81, 0x00817e, 0x00847b, 0x008778, 0x008a75, 0x008d72,
	0x00906f, 0x00936c, 0x009669, 0x009966, 0x009c63, 0x009f60, 0x00a25d, 0x00a55a,
	0x00a857, 0x00ab54, 0x00ae51, 0x00b14e, 0x00b44b, 0x00b748, 0x00ba45, 0x00bd42,
	0x00c03f, 0x00c33c, 0x00c639, 0x00c936, 0x00cc33, 0x00cf30, 0x00d22d, 0x00d52a,
	0x00d827, 0x00db24, 0x00de21, 0x00e11e, 0x00e41b, 0x00e718, 0x00ea15, 0x00ed12,
	0x00f00f, 0x00f30c, 0x00f609, 0x00f906, 0x00fc03, 0x00ff00, 0x0300fc, 0x03fc00,
	0x0600f9, 0x06f900, 0x0900f6, 0x09f600, 0x0c00f3, 0x0cf300, 0x0f00f0, 0x0ff000,
	0x1200ed, 0x12ed00, 0x1500ea, 0x15ea00, 0x1800e7, 0x18e700, 0x1b00e4, 0x1be400,
	0x1e00e1, 0x1ee100, 0x2100de, 0x21de00, 0x2400db, 0x24db00, 0x2700d8, 0x27d800,
	0x2a00d5, 0x2ad500, 0x2d00d2, 0x2dd200, 0x3000cf, 0x30cf00, 0x3300cc, 0x33cc00,
	0x3600c9, 0x36c900, 0x3900c6, 0x39c600, 0x3c00c3, 0x3cc300, 0x3f00c0, 0x3fc000,
	0x4200bd, 0x42bd00, 0x4500ba, 0x45ba00, 0x4800b7, 0x48b700, 0x4b00b4, 0x4bb400,
	0x4e00b1, 0x4eb100, 0x5100ae, 0x51ae00, 0x5400ab, 0x54ab00, 0x5700a8, 0x57a800,
	0x5a00a5, 0x5aa500, 0x5d00a2, 0x5da200, 0x60009f, 0x609f00, 0x63009c, 0x639c00,
	0x660099, 0x669900, 0x690096, 0x699600, 0x6c0093, 0x6c9300, 0x6f0090, 0x6f9000,
	0x72008d, 0x728d00, 0x75008a, 0x758a00, 0x780087, 0x788700, 0x7b0084, 0x7b8400,
	0x7e0081, 0x7e8100, 0x81007e, 0x817e00, 0x84007b, 0x847b00, 0x870078, 0x877800,
	0x8a0075, 0x8a7500, 0x8d0072, 0x8d7200, 0x90006f, 0x906f00, 0x93006c, 0x936c00,
	0x960069, 0x966900, 0x990066, 0x996600, 0x9c0063, 0x9c6300, 0x9f0060, 0x9f6000,
	0xa2005d, 0xa25d00, 0xa5005a, 0xa55a00, 0xa80057, 0xa85700, 0xab0054, 0xab5400,
	0xae0051, 0xae5100, 0xb1004e, 0xb14e00, 0xb4004b, 0xb44b00, 0xb70048, 0xb74800,
	0xba0045, 0xba4500, 0xbd0042, 0xbd4200, 0xc0003f, 0xc03f00, 0xc3003c, 0xc33c00,
	0xc60039, 0xc63900, 0xc90036, 0xc93600, 0xcc0033, 0xcc3300, 0xcf0030, 0xcf3000,
	0xd2002d, 0xd22d00, 0xd5002a, 0xd52a00, 0xd80027, 0xd82700, 0xdb0024, 0xdb2400,
	0xde0021, 0xde2100, 0xe1001e, 0xe11e00, 0xe4001b, 0xe41b00, 0xe70018, 0xe71800,
	0xea0015, 0xea1500, 0xed0012, 0xed1200, 0xf0000f, 0xf00f00, 0xf3000c, 0xf30c00,
	0xf60009, 0xf60900, 0xf90006, 0xf90600, 0xfc0003, 0xfc0300, 0xff0000, 0xffffff};



Mat fgMaskMOG; // fg mask fg mask generated by MOG method
Ptr<BackgroundSubtractor> pMOG; // MOG Background subtractor

const byte STX = (byte) '>';
const byte DLE = (byte) '/';
const byte ETX = (byte) '\r';

ref class MyClass {
public:
	int m_i;
};

union {
	Byte arrByte[4];
	UInt16 shortInteger;
	UInt32 lngInteger;
} convertToInteger;

const char* keys =
{
	"{c |camera   |true    | use camera or not}"
	"{fn|file_name|tree.avi | movie file             }"
};


//Modify this value to match the VID and PID in your USB device descriptor.
//Use the formatting: "Vid_xxxx&Pid_xxxx" where xxxx is a 16-bit hexadecimal number.
#define MY_DEVICE_ID  "Vid_04d8&Pid_0052"	//Microchip High bandwidth WinUSB Demo.  Change this number (along with the 
											//corresponding VID/PID in the microcontroller firmware, and in the driver 
											//installation .INF file) before moving the design into production.


namespace HighBandwidthWinUSB {

	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;

#pragma region DLL Imports
	using namespace System::Threading;	
	using namespace System::Runtime::InteropServices;  // Need this to support "unmanaged" code.

	#ifdef UNICODE
	#define	Seeifdef	Unicode
	#else
	#define Seeifdef	Ansi
	#endif


	

	//Returns a HDEVINFO type for a device information set (WinUSB devices in
	//our case).  We will need the HDEVINFO as in input parameter for calling many of
	//the other SetupDixxx() functions.
	[DllImport("setupapi.dll" , CharSet = CharSet::Seeifdef, EntryPoint="SetupDiGetClassDevs", CallingConvention=CallingConvention::Winapi)]		
	extern "C" HDEVINFO  SetupDiGetClassDevsUM(
		LPGUID  ClassGuid,					//Input: Supply the class GUID here. 
		PCTSTR  Enumerator,					//Input: Use NULL here, not important for our purposes
		HWND  hwndParent,					//Input: Use NULL here, not important for our purposes
		DWORD  Flags);						//Input: Flags describing what kind of filtering to use.

	//Gives us "PSP_DEVICE_INTERFACE_DATA" which contains the Interface specific GUID (different
	//from class GUID).  We need the interface GUID to get the device path.
	[DllImport("setupapi.dll" , CharSet = CharSet::Seeifdef, EntryPoint="SetupDiEnumDeviceInterfaces", CallingConvention=CallingConvention::Winapi)]				
	extern "C" WINSETUPAPI BOOL WINAPI  SetupDiEnumDeviceInterfacesUM(
		HDEVINFO  DeviceInfoSet,			//Input: Give it the HDEVINFO we got from SetupDiGetClassDevs()
		PSP_DEVINFO_DATA  DeviceInfoData,	//Input (optional)
		LPGUID  InterfaceClassGuid,			//Input 
		DWORD  MemberIndex,					//Input: "Index" of the device you are interested in getting the path for.
		PSP_DEVICE_INTERFACE_DATA  DeviceInterfaceData);//Output: This function fills in an "SP_DEVICE_INTERFACE_DATA" structure.

	//SetupDiDestroyDeviceInfoList() frees up memory by destroying a DeviceInfoList
	[DllImport("setupapi.dll" , CharSet = CharSet::Seeifdef, EntryPoint="SetupDiDestroyDeviceInfoList", CallingConvention=CallingConvention::Winapi)]
	extern "C" WINSETUPAPI BOOL WINAPI  SetupDiDestroyDeviceInfoListUM(			
		HDEVINFO  DeviceInfoSet);			//Input: Give it a handle to a device info list to deallocate from RAM.

	//SetupDiEnumDeviceInfo() fills in an "SP_DEVINFO_DATA" structure, which we need for SetupDiGetDeviceRegistryProperty()
	[DllImport("setupapi.dll" , CharSet = CharSet::Seeifdef, EntryPoint="SetupDiEnumDeviceInfo", CallingConvention=CallingConvention::Winapi)]
	extern "C" WINSETUPAPI BOOL WINAPI  SetupDiEnumDeviceInfoUM(
		HDEVINFO  DeviceInfoSet,
		DWORD  MemberIndex,
		PSP_DEVINFO_DATA  DeviceInfoData);

	//SetupDiGetDeviceRegistryProperty() gives us the hardware ID, which we use to check to see if it has matching VID/PID
	[DllImport("setupapi.dll" , CharSet = CharSet::Seeifdef, EntryPoint="SetupDiGetDeviceRegistryProperty", CallingConvention=CallingConvention::Winapi)]
	extern "C"	WINSETUPAPI BOOL WINAPI  SetupDiGetDeviceRegistryPropertyUM(
		HDEVINFO  DeviceInfoSet,
		PSP_DEVINFO_DATA  DeviceInfoData,
		DWORD  Property,
		PDWORD  PropertyRegDataType,
		PBYTE  PropertyBuffer,   
		DWORD  PropertyBufferSize,  
		PDWORD  RequiredSize);

	//SetupDiGetDeviceInterfaceDetail() gives us a device path, which is needed before CreateFile() can be used.
	[DllImport("setupapi.dll" , CharSet = CharSet::Seeifdef, EntryPoint="SetupDiGetDeviceInterfaceDetail", CallingConvention=CallingConvention::Winapi)]
	extern "C" BOOL SetupDiGetDeviceInterfaceDetailUM(
		HDEVINFO DeviceInfoSet,										//Input: Wants HDEVINFO which can be obtained from SetupDiGetClassDevs()
		PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData,				//Input: Pointer to an structure which defines the device interface.  
		PSP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData,	//Output: Pointer to a strucutre, which will contain the device path.
		DWORD DeviceInterfaceDetailDataSize,						//Input: Number of bytes to retrieve.
		PDWORD RequiredSize,										//Output (optional): Te number of bytes needed to hold the entire struct 
		PSP_DEVINFO_DATA DeviceInfoData);							//Output

	//WinUsb_Initialize() needs to be called before the application can begin sending/receiving data with the USB device.
	[DllImport("winusb.dll" , CharSet = CharSet::Seeifdef, EntryPoint="WinUsb_Initialize", CallingConvention=CallingConvention::Winapi)]
	extern "C" BOOL WinUsb_Initialize(
		HANDLE	DeviceHandle,
		PWINUSB_INTERFACE_HANDLE InterfaceHandle);

	//WinUsb_WritePipe() is the basic function used to write data to the USB device (sends data to OUT endpoints on the device)
	[DllImport("winusb.dll" , CharSet = CharSet::Seeifdef, EntryPoint="WinUsb_WritePipe", CallingConvention=CallingConvention::Winapi)]
	extern "C" BOOL WinUsb_WritePipe(
		WINUSB_INTERFACE_HANDLE InterfaceHandle,
		UCHAR PipeID,
		PUCHAR Buffer,
		ULONG BufferLength,
		PULONG LengthTransferred,
		LPOVERLAPPED Overlapped);

	//WinUsb_ReadPipe() is the basic function used to read data from the USB device (polls for and obtains data from
	//IN endpoints on the device)
	[DllImport("winusb.dll" , CharSet = CharSet::Seeifdef, EntryPoint="WinUsb_ReadPipe", CallingConvention=CallingConvention::Winapi)]
	extern "C" BOOL WinUsb_ReadPipe(
		WINUSB_INTERFACE_HANDLE InterfaceHandle,
		UCHAR PipeID,
		PUCHAR Buffer,
		ULONG BufferLength,
		PULONG LengthTransferred,
		LPOVERLAPPED Overlapped);

	//WinUsb_SetPipePolicy() can be used to configure the behavior of the WinUSB use of the specified endpoint
	[DllImport("winusb.dll" , CharSet = CharSet::Seeifdef, EntryPoint="WinUsb_SetPipePolicy", CallingConvention=CallingConvention::Winapi)]
	extern "C" BOOL WinUsb_SetPipePolicy(
		WINUSB_INTERFACE_HANDLE  InterfaceHandle,
		UCHAR  PipeID,
		ULONG  PolicyType,
		ULONG  ValueLength,
		PVOID  Value);

	//WinUsb_Free() is used to free up resources/close the handle that was returned when calling WinUsb_Initialize()
	[DllImport("winusb.dll" , CharSet = CharSet::Seeifdef, EntryPoint="WinUsb_Free", CallingConvention=CallingConvention::Winapi)]
	extern "C" BOOL WinUsb_Free(WINUSB_INTERFACE_HANDLE InterfaceHandle);

	//WinUsb_FlushPipe() is used to discard any data that may be "cached in a pipe".
	[DllImport("winusb.dll" , CharSet = CharSet::Seeifdef, EntryPoint="WinUsb_Free", CallingConvention=CallingConvention::Winapi)]
	extern "C" BOOL WinUsb_FlushPipe(WINUSB_INTERFACE_HANDLE  InterfaceHandle, UCHAR  PipeID);

	//Note: WinUSB supports quite a few more functions that aren't being used in this application, and aren't
	//shown here.  See the WinUSB client support routines documentation in MSDN (found in the WDK documentation
	//relating to WinUSB).

	//Need this function for receiving all of the WM_DEVICECHANGE messages.  See MSDN documentation for
	//description of what this function does/how to use it. Note: name is remapped "RegisterDeviceNotificationUM" to
	//avoid possible build error conflicts.
	[DllImport("user32.dll" , CharSet = CharSet::Seeifdef, EntryPoint="RegisterDeviceNotification", CallingConvention=CallingConvention::Winapi)]					
	extern "C" HDEVNOTIFY WINAPI RegisterDeviceNotificationUM(
		HANDLE hRecipient,
		LPVOID NotificationFilter,
		DWORD Flags);

#pragma endregion

//  Variables that need to have wide scope.
	BOOL AttachedState = FALSE;							//Need to keep track of the USB device attachment status for proper plug and play operation.
	PSP_DEVICE_INTERFACE_DETAIL_DATA DetailedInterfaceDataStructure = new SP_DEVICE_INTERFACE_DETAIL_DATA;	//Global
	HANDLE MyDeviceHandle = INVALID_HANDLE_VALUE;		//First need to get the Device handle
	WINUSB_INTERFACE_HANDLE MyWinUSBInterfaceHandle;	//And then can call WinUsb_Initialize() to get the interface handle
														//which is needed for doing other operations with the device (like
				

														//reading and writing to the USB device).

#define MATRIXWIDTH 128
#define MATRIXHEIGHT 64

#define NUMPANELS 4
#define PANELROWS 32
#define PANELCOLS 32
#define NUMCHANNELS 3
#define PANELSIZE (PANELROWS*PANELCOLS*NUMCHANNELS)
#define MATRIXROWSIZE (MATRIXWIDTH * NUMCHANNELS)
#define MATRIXSIZE (MATRIXHEIGHT*MATRIXWIDTH*NUMCHANNELS)
#define MAXDATABYTES (PANELROWS*PANELCOLS*NUMCHANNELS )

#define  MAXPACKET (MAXDATABYTES * 4)

	

	public ref class Panel {
#define HORIZONTAL 0
#define VERTICAL 1
#define HORIZONTAL_FLIPPED 2
#define VERTICAL_FLIPPED 3


	public:
		Panel();		
		void setOrientation(int index,  int orientation, int row, int col);
		void setOutData(byte *ptrInData);
		Byte getOutData(int index);

	public:
		int orientation;
		int ZeroCol, ZeroRow, PanelNumber;
		array< Byte, 1 >^ arrOutData = gcnew array< Byte, 1 >(PANELROWS*PANELCOLS*NUMCHANNELS);
	};	

	
	
	
	public ref class Form1 : public System::Windows::Forms::Form
	{
		array< Panel^ >^ arrPanel = gcnew array< Panel^ >(NUMPANELS);

	public:
		Form1(void)
		{
			int i;
			InitializeComponent();

			 // const array<int, 2>^ arrPanelInit = gcnew const array<int, 2>{ {HORIZONTAL, 8, 32}, { HORIZONTAL, 24, 32}, { HORIZONTAL, 8, 64}, { HORIZONTAL, 24, 64}};
			const array<int, 2>^ arrPanelInit = gcnew const array<int, 2>{ {HORIZONTAL, 0, 32}, { HORIZONTAL, 32, 32 }, { HORIZONTAL, 0, 64 }, { HORIZONTAL, 32, 64 }};
			 // const array<int, 2>^ arrPanelInit = gcnew const array<int, 2>{ {VERTICAL, 0, 64}, {VERTICAL, 0, 48}, { VERTICAL, 32, 64}, { VERTICAL, 32, 48}};
			 // const array<int, 2>^ arrPanelInit = gcnew const array<int, 2>{ { HORIZONTAL_FLIPPED, 24, 64 }, { HORIZONTAL_FLIPPED, 8, 64 }, { HORIZONTAL_FLIPPED, 24, 32 }, { HORIZONTAL_FLIPPED, 8, 32 }};
			// const array<int, 2>^ arrPanelInit = gcnew const array<int, 2>{ {VERTICAL_FLIPPED, 32, 48}, { VERTICAL_FLIPPED, 32, 64 }, { VERTICAL_FLIPPED, 0, 48 }, { VERTICAL_FLIPPED, 0, 64 }};
			
			for (i = 0; i < NUMPANELS; i++) {
				arrPanel[i] = gcnew Panel;
				arrPanel[i]->setOrientation(i, arrPanelInit[i, 0], arrPanelInit[i, 1], arrPanelInit[i, 2]);
			}

			//Globally Unique Identifier (GUID). Windows uses GUIDs to identify things.  
			GUID InterfaceClassGuid = { 0xa5dcbf10, 0x6530, 0x11d2, 0x90, 0x1F, 0x00, 0xC0, 0x4F, 0xB9, 0x51, 0xED }; //Globally Unique Identifier (GUID) for USB peripheral devices
																													  //Register for WM_DEVICECHANGE notifications:
			DEV_BROADCAST_DEVICEINTERFACE MyDeviceBroadcastHeader;// = new DEV_BROADCAST_HDR;
			MyDeviceBroadcastHeader.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
			MyDeviceBroadcastHeader.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
			MyDeviceBroadcastHeader.dbcc_reserved = 0;	//Reserved says not to use...
			MyDeviceBroadcastHeader.dbcc_classguid = InterfaceClassGuid;
			RegisterDeviceNotificationUM((HANDLE)this->Handle, &MyDeviceBroadcastHeader, DEVICE_NOTIFY_WINDOW_HANDLE);

			//Now perform an initial start up check of the device state (attached or not attached), since we would not have
			//received a WM_DEVICECHANGE notification.
			if (CheckIfPresentAndGetUSBDevicePath())	//Check and make sure at least one device with matching VID/PID is attached
			{
				//We now have the proper device path, and we can finally open a device handle to the device.
				//WinUSB requires the device handle to be opened with the FILE_FLAG_OVERLAPPED attribute.
				MyDeviceHandle = CreateFile((DetailedInterfaceDataStructure->DevicePath), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, 0);
				DWORD ErrorStatus = GetLastError();
				if (ErrorStatus == ERROR_SUCCESS)
				{
					//Now get the WinUSB interface handle by calling WinUsb_Initialize() and providing the device handle.
					BOOL BoolStatus = WinUsb_Initialize(MyDeviceHandle, &MyWinUSBInterfaceHandle);
					if (BoolStatus == TRUE)
					{
						AttachedState = TRUE;
						//SendMultiOUT_btn->Enabled = TRUE;
						//BulkOut_btn->Enabled = TRUE;
						Bandwidth_lbl->Enabled = TRUE;
						ElapsedTimer_lbl->Enabled = TRUE;
						DataXfer_chkbx->Enabled = TRUE;
						StatusBox_txtbx->Text = "Device Found: AttachedState = TRUE";
					}
				}
			}
			else	//Device must not be connected (or not programmed with correct firmware)
			{
				AttachedState = FALSE;
				//SendMultiOUT_btn->Enabled = FALSE;
				//BulkOut_btn->Enabled = FALSE;
				Bandwidth_lbl->Enabled = FALSE;
				ElapsedTimer_lbl->Enabled = FALSE;
				DataXfer_chkbx->Enabled = FALSE;
				Bandwidth_txtbx->Text = "";
				ElapsedTime_txtbx->Text = "";
				StatusBox_txtbx->Text = "Device Not Detected: Verify Connection/Correct Firmware";
			}
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~Form1()
		{
			//Recommend explicitly closing these handles when exiting the application.  Failure to do so may contribute to
			//reduced system stability.
			if (AttachedState == TRUE)
			{
				WinUsb_Free(MyWinUSBInterfaceHandle);
				CloseHandle(MyDeviceHandle);
			}

			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TextBox^  StatusBox_txtbx;
	protected:
	private: System::Windows::Forms::Label^  StatusBox_lbl;
	private: System::Windows::Forms::CheckBox^  DataXfer_chkbx;
	private: System::Windows::Forms::Label^  Bandwidth_lbl;
	private: System::Windows::Forms::TextBox^  Bandwidth_txtbx;
	private: System::Windows::Forms::Label^  ElapsedTimer_lbl;
	private: System::Windows::Forms::TextBox^  ElapsedTime_txtbx;
	//private: System::Windows::Forms::Button^  BulkOut_btn;
	//private: System::Windows::Forms::Button^  SendMultiOUT_btn;
	private: System::Windows::Forms::Button^  btnCamLoop;
	//private: System::Windows::Forms::Button^  btnCamLoop2;

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->StatusBox_txtbx = (gcnew System::Windows::Forms::TextBox());
			this->StatusBox_lbl = (gcnew System::Windows::Forms::Label());
			this->DataXfer_chkbx = (gcnew System::Windows::Forms::CheckBox());
			this->Bandwidth_lbl = (gcnew System::Windows::Forms::Label());
			this->Bandwidth_txtbx = (gcnew System::Windows::Forms::TextBox());
			this->ElapsedTimer_lbl = (gcnew System::Windows::Forms::Label());
			this->ElapsedTime_txtbx = (gcnew System::Windows::Forms::TextBox());
			//this->BulkOut_btn = (gcnew System::Windows::Forms::Button());
			//this->SendMultiOUT_btn = (gcnew System::Windows::Forms::Button());
			this->btnCamLoop = (gcnew System::Windows::Forms::Button());
			//this->btnCamLoop2 = (gcnew System::Windows::Forms::Button());
			this->SuspendLayout();
			// 
			// StatusBox_txtbx
			// 
			this->StatusBox_txtbx->BackColor = System::Drawing::SystemColors::Window;
			this->StatusBox_txtbx->Location = System::Drawing::Point(12, 12);
			this->StatusBox_txtbx->Name = L"StatusBox_txtbx";
			this->StatusBox_txtbx->ReadOnly = true;
			this->StatusBox_txtbx->Size = System::Drawing::Size(298, 20);
			this->StatusBox_txtbx->TabIndex = 16;
			// 
			// StatusBox_lbl
			// 
			this->StatusBox_lbl->AutoSize = true;
			this->StatusBox_lbl->Location = System::Drawing::Point(316, 15);
			this->StatusBox_lbl->Name = L"StatusBox_lbl";
			this->StatusBox_lbl->Size = System::Drawing::Size(37, 13);
			this->StatusBox_lbl->TabIndex = 17;
			this->StatusBox_lbl->Text = L"Status";
			// 
			// DataXfer_chkbx
			// 
			this->DataXfer_chkbx->AutoCheck = false;
			this->DataXfer_chkbx->AutoSize = true;
			this->DataXfer_chkbx->Enabled = false;
			this->DataXfer_chkbx->Location = System::Drawing::Point(12, 123);
			this->DataXfer_chkbx->Name = L"DataXfer_chkbx";
			this->DataXfer_chkbx->Size = System::Drawing::Size(147, 17);
			this->DataXfer_chkbx->TabIndex = 18;
			this->DataXfer_chkbx->Text = L"Data Transfer In Progress";
			this->DataXfer_chkbx->UseVisualStyleBackColor = true;
			// 
			// Bandwidth_lbl
			// 
			this->Bandwidth_lbl->AutoSize = true;
			this->Bandwidth_lbl->Enabled = false;
			this->Bandwidth_lbl->Location = System::Drawing::Point(99, 176);
			this->Bandwidth_lbl->Name = L"Bandwidth_lbl";
			this->Bandwidth_lbl->Size = System::Drawing::Size(102, 13);
			this->Bandwidth_lbl->TabIndex = 22;
			this->Bandwidth_lbl->Text = L"Bandwidth (Bytes/s)";
			// 
			// Bandwidth_txtbx
			// 
			this->Bandwidth_txtbx->BackColor = System::Drawing::SystemColors::Window;
			this->Bandwidth_txtbx->Location = System::Drawing::Point(12, 173);
			this->Bandwidth_txtbx->Name = L"Bandwidth_txtbx";
			this->Bandwidth_txtbx->ReadOnly = true;
			this->Bandwidth_txtbx->Size = System::Drawing::Size(81, 20);
			this->Bandwidth_txtbx->TabIndex = 21;
			// 
			// ElapsedTimer_lbl
			// 
			this->ElapsedTimer_lbl->AutoSize = true;
			this->ElapsedTimer_lbl->Enabled = false;
			this->ElapsedTimer_lbl->Location = System::Drawing::Point(99, 149);
			this->ElapsedTimer_lbl->Name = L"ElapsedTimer_lbl";
			this->ElapsedTimer_lbl->Size = System::Drawing::Size(93, 13);
			this->ElapsedTimer_lbl->TabIndex = 20;
			this->ElapsedTimer_lbl->Text = L"Elapsed Time (ms)";
			// 
			// ElapsedTime_txtbx
			// 
			this->ElapsedTime_txtbx->BackColor = System::Drawing::SystemColors::Window;
			this->ElapsedTime_txtbx->Location = System::Drawing::Point(12, 146);
			this->ElapsedTime_txtbx->Name = L"ElapsedTime_txtbx";
			this->ElapsedTime_txtbx->ReadOnly = true;
			this->ElapsedTime_txtbx->Size = System::Drawing::Size(81, 20);
			this->ElapsedTime_txtbx->TabIndex = 19;
			// btnCamLoop
			// 
			this->btnCamLoop->Location = System::Drawing::Point(232, 108);
			this->btnCamLoop->Name = L"btnCamLoop";
			this->btnCamLoop->Size = System::Drawing::Size(172, 54);
			this->btnCamLoop->TabIndex = 25;
			this->btnCamLoop->Text = L"Camera Loop";
			this->btnCamLoop->UseVisualStyleBackColor = true;
			this->btnCamLoop->Click += gcnew System::EventHandler(this, &Form1::btnCamLoop_Click);
			// 
			// btnCamLoop2
			// 
			/*
			this->btnCamLoop2->Location = System::Drawing::Point(232, 214);
			this->btnCamLoop2->Name = L"btnCamLoop2";
			this->btnCamLoop2->Size = System::Drawing::Size(172, 54);
			this->btnCamLoop2->TabIndex = 26;
			this->btnCamLoop2->Text = L"Camera Loop 2";
			this->btnCamLoop2->UseVisualStyleBackColor = true;
			this->btnCamLoop2->Click += gcnew System::EventHandler(this, &Form1::btnCamLoop2_Click);
			*/
			// 
			// Form1
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(564, 407);
			//this->Controls->Add(this->btnCamLoop2);
			this->Controls->Add(this->btnCamLoop);
			//this->Controls->Add(this->SendMultiOUT_btn);
			//this->Controls->Add(this->BulkOut_btn);
			this->Controls->Add(this->Bandwidth_lbl);
			this->Controls->Add(this->Bandwidth_txtbx);
			this->Controls->Add(this->ElapsedTimer_lbl);
			this->Controls->Add(this->ElapsedTime_txtbx);
			this->Controls->Add(this->DataXfer_chkbx);
			this->Controls->Add(this->StatusBox_lbl);
			this->Controls->Add(this->StatusBox_txtbx);
			this->Name = L"Form1";
			this->Text = L"LED Animator HBW Version";
			this->Load += gcnew System::EventHandler(this, &Form1::Form1_Load);
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
		BOOL	CheckIfPresentAndGetUSBDevicePath(void)
		{
			//Globally Unique Identifier (GUID). Windows uses GUIDs to identify things.  This GUID needs to match
			//the GUID that is used in the .INF file used to install the WinUSB driver onto the system.
			//The INF file creates a register entry which associates a GUID with the WinUSB device.  In order for
			//a user mode application (such as this one) to find the USB device on the bus, it needs to known the
			//correct GUID that got put into the registry.
			GUID InterfaceClassGuid = { 0x58D07210, 0x27C1, 0x11DD, 0xBD, 0x0B, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 };

			HDEVINFO DeviceInfoTable = INVALID_HANDLE_VALUE;
			PSP_DEVICE_INTERFACE_DATA InterfaceDataStructure = new SP_DEVICE_INTERFACE_DATA;
			//		PSP_DEVICE_INTERFACE_DETAIL_DATA DetailedInterfaceDataStructure = new SP_DEVICE_INTERFACE_DETAIL_DATA;	//Global
			SP_DEVINFO_DATA DevInfoData;

			DWORD InterfaceIndex = 0;
			DWORD StatusLastError = 0;
			DWORD dwRegType;
			DWORD dwRegSize;
			DWORD StructureSize = 0;
			PBYTE PropertyValueBuffer;
			bool MatchFound = false;
			DWORD ErrorStatus;
			BOOL BoolStatus = FALSE;
			DWORD LoopCounter = 0;

			System::String^ DeviceIDToFind = MY_DEVICE_ID;

			//First populate a list of plugged in devices (by specifying "DIGCF_PRESENT"), which are of the specified class GUID. 
			DeviceInfoTable = SetupDiGetClassDevsUM(&InterfaceClassGuid, NULL, NULL, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

			//Now look through the list we just populated.  We are trying to see if any of them match our device. 
			while (true)
			{
				InterfaceDataStructure->cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
				if (SetupDiEnumDeviceInterfacesUM(DeviceInfoTable, NULL, &InterfaceClassGuid, InterfaceIndex, InterfaceDataStructure))
				{
					ErrorStatus = GetLastError();
					if (ErrorStatus == ERROR_NO_MORE_ITEMS)	//Did we reach the end of the list of matching devices in the DeviceInfoTable?
					{	//Cound not find the device.  Must not have been attached.
						SetupDiDestroyDeviceInfoListUM(DeviceInfoTable);	//Clean up the old structure we no longer need.
						return FALSE;
					}
				}
				else	//Else some other kind of unknown error ocurred...
				{
					ErrorStatus = GetLastError();
					SetupDiDestroyDeviceInfoListUM(DeviceInfoTable);	//Clean up the old structure we no longer need.
					return FALSE;
				}

				//Now retrieve the hardware ID from the registry.  The hardware ID contains the VID and PID, which we will then 
				//check to see if it is the correct device or not.

				//Initialize an appropriate SP_DEVINFO_DATA structure.  We need this structure for SetupDiGetDeviceRegistryProperty().
				DevInfoData.cbSize = sizeof(SP_DEVINFO_DATA);
				SetupDiEnumDeviceInfoUM(DeviceInfoTable, InterfaceIndex, &DevInfoData);

				//First query for the size of the hardware ID, so we can know how big a buffer to allocate for the data.
				SetupDiGetDeviceRegistryPropertyUM(DeviceInfoTable, &DevInfoData, SPDRP_HARDWAREID, &dwRegType, NULL, 0, &dwRegSize);

				//Allocate a buffer for the hardware ID.
				PropertyValueBuffer = (BYTE *)malloc(dwRegSize);
				if (PropertyValueBuffer == NULL)	//if null, error, couldn't allocate enough memory
				{	//Can't really recover from this situation, just exit instead.
					SetupDiDestroyDeviceInfoListUM(DeviceInfoTable);	//Clean up the old structure we no longer need.
					return FALSE;
				}

				//Retrieve the hardware IDs for the current device we are looking at.  PropertyValueBuffer gets filled with a 
				//REG_MULTI_SZ (array of null terminated strings).  To find a device, we only care about the very first string in the
				//buffer, which will be the "device ID".  The device ID is a string which contains the VID and PID, in the example 
				//format "Vid_04d8&Pid_003f".
				SetupDiGetDeviceRegistryPropertyUM(DeviceInfoTable, &DevInfoData, SPDRP_HARDWAREID, &dwRegType, PropertyValueBuffer, dwRegSize, NULL);

				//Now check if the first string in the hardware ID matches the device ID of my USB device.
#ifdef UNICODE
				System::String^ DeviceIDFromRegistry = gcnew System::String((wchar_t *)PropertyValueBuffer);
#else
				String^ DeviceIDFromRegistry = gcnew String((char *)PropertyValueBuffer);
#endif

				free(PropertyValueBuffer);		//No longer need the PropertyValueBuffer, free the memory to prevent potential memory leaks

												//Convert both strings to lower case.  This makes the code more robust/portable accross OS Versions
				DeviceIDFromRegistry = DeviceIDFromRegistry->ToLowerInvariant();
				DeviceIDToFind = DeviceIDToFind->ToLowerInvariant();
				//Now check if the hardware ID we are looking at contains the correct VID/PID
				MatchFound = DeviceIDFromRegistry->Contains(DeviceIDToFind);
				if (MatchFound == true)
				{
					//Device must have been found.  Open WinUSB interface handle now.  In order to do this, we will need the actual device path first.
					//We can get the path by calling SetupDiGetDeviceInterfaceDetail(), however, we have to call this function twice:  The first
					//time to get the size of the required structure/buffer to hold the detailed interface data, then a second time to actually 
					//get the structure (after we have allocated enough memory for the structure.)
					DetailedInterfaceDataStructure->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
					//First call populates "StructureSize" with the correct value
					SetupDiGetDeviceInterfaceDetailUM(DeviceInfoTable, InterfaceDataStructure, NULL, NULL, &StructureSize, NULL);
					DetailedInterfaceDataStructure = (PSP_DEVICE_INTERFACE_DETAIL_DATA)(malloc(StructureSize));		//Allocate enough memory
					if (DetailedInterfaceDataStructure == NULL)	//if null, error, couldn't allocate enough memory
					{	//Can't really recover from this situation, just exit instead.
						SetupDiDestroyDeviceInfoListUM(DeviceInfoTable);	//Clean up the old structure we no longer need.
						return FALSE;
					}
					DetailedInterfaceDataStructure->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
					//Now call SetupDiGetDeviceInterfaceDetail() a second time to receive the goods.  
					SetupDiGetDeviceInterfaceDetailUM(DeviceInfoTable, InterfaceDataStructure, DetailedInterfaceDataStructure, StructureSize, NULL, NULL);

					//We now have the proper device path, and we can finally open a device handle to the device.
					//WinUSB requires the device handle to be opened with the FILE_FLAG_OVERLAPPED attribute.
					SetupDiDestroyDeviceInfoListUM(DeviceInfoTable);	//Clean up the old structure we no longer need.
					return TRUE;
				}

				InterfaceIndex++;
				//Keep looping until we either find a device with matching VID and PID, or until we run out of devices to check.
				//However, just in case some unexpected error occurs, keep track of the number of loops executed.
				//If the number of loops exceeds a very large number, exit anyway, to prevent inadvertent infinite looping.
				LoopCounter++;
				if (LoopCounter == 10000000)	//Surely there aren't more than 10 million devices attached to any forseeable PC...
				{
					return FALSE;
				}

			}//end of while(true)
		}

	protected: virtual void WndProc(Message% m) override {
		//This is a callback function that gets called when a Windows message is received by the form.
		// Listen for Windows messages.  We will receive various different types of messages, but the ones we really want to use are the WM_DEVICECHANGE messages.
		if (m.Msg == WM_DEVICECHANGE)
		{
			if (((int)m.WParam == DBT_DEVICEARRIVAL) || ((int)m.WParam == DBT_DEVICEREMOVEPENDING) || ((int)m.WParam == DBT_DEVICEREMOVECOMPLETE) || ((int)m.WParam == DBT_CONFIGCHANGED))
			{

				//WM_DEVICECHANGE messages by themselves are quite generic, and can be caused by a number of different
				//sources, not just your USB hardware device.  Therefore, must check to find out if any changes relavant
				//to your device (with known VID/PID) took place before doing any kind of opening or closing of endpoints.
				//(the message could have been totally unrelated to your application/USB device)

				if (CheckIfPresentAndGetUSBDevicePath())	//Check and make sure at least one device with matching VID/PID is attached
				{
					if (AttachedState == FALSE)
					{
						//We now have the proper device path, and we can finally open a device handle to the device.
						//WinUSB requires the device handle to be opened with the FILE_FLAG_OVERLAPPED attribute.
						MyDeviceHandle = CreateFile((DetailedInterfaceDataStructure->DevicePath), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, 0);
						DWORD ErrorStatus = GetLastError();
						if (ErrorStatus == ERROR_SUCCESS)
						{
							//Now get the WinUSB interface handle by calling WinUsb_Initialize() and providing the device handle.
							BOOL BoolStatus = WinUsb_Initialize(MyDeviceHandle, &MyWinUSBInterfaceHandle);
							if (BoolStatus == TRUE)
							{
								AttachedState = TRUE;
								//SendMultiOUT_btn->Enabled = TRUE;
								//BulkOut_btn->Enabled = TRUE;
								Bandwidth_lbl->Enabled = TRUE;
								ElapsedTimer_lbl->Enabled = TRUE;
								DataXfer_chkbx->Enabled = TRUE;
								StatusBox_txtbx->Text = "Device Found: AttachedState = TRUE";
							}
						}
					}
				}
				else	//Device must not be connected (or not programmed with correct firmware)
				{
					if (AttachedState == TRUE)		//If it is currently set to TRUE, that means the device was just now disconnected, and the open handles become invalid.
					{
						WinUsb_Free(MyWinUSBInterfaceHandle);
						CloseHandle(MyDeviceHandle);
					}
					AttachedState = FALSE;
					//SendMultiOUT_btn->Enabled = FALSE;
					//BulkOut_btn->Enabled = FALSE;
					Bandwidth_lbl->Enabled = FALSE;
					ElapsedTimer_lbl->Enabled = FALSE;
					DataXfer_chkbx->Enabled = FALSE;
					Bandwidth_txtbx->Text = "";
					ElapsedTime_txtbx->Text = "";
					StatusBox_txtbx->Text = "Device Not Detected: Verify Connection/Correct Firmware";
				}


			}
		}

		Form::WndProc(m);
	}


	private: System::Void Form1_Load(System::Object^  sender, System::EventArgs^  e) {
	}

			 UInt32 getLongInteger(byte b0, byte b1, byte b2, byte b3)
			 {
				 convertToInteger.arrByte[0] = b0;
				 convertToInteger.arrByte[1] = b1;
				 convertToInteger.arrByte[2] = b2;
				 convertToInteger.arrByte[3] = b3;
				 return (convertToInteger.lngInteger);
			 }

			 UInt16 getShortInteger(byte b0, byte b1)
			 {
				 convertToInteger.arrByte[0] = b0;
				 convertToInteger.arrByte[1] = b1;
				 return (convertToInteger.shortInteger);
			 }

			 bool insertByte(byte dataByte, byte *ptrBuffer, UInt16& index) {
				 if (index >= MAXPACKET) return (FALSE);
				 if (dataByte == STX || dataByte == DLE || dataByte == ETX)
					 ptrBuffer[index++] = DLE;
				 if (index >= MAXPACKET) return (FALSE);
				 ptrBuffer[index++] = dataByte;
				 return (TRUE);
			 }

			 UInt16 BuildPacket(byte command, byte subcommand, byte *ptrData, UInt16 dataLength, byte *ptrPacket) {

				 if (dataLength <= MAXDATABYTES) {
					 UInt16 packetIndex = 1;
					 ptrPacket[0] = STX;
					 insertByte(command, ptrPacket, packetIndex);
					 insertByte(subcommand, ptrPacket, packetIndex);

					 convertToInteger.shortInteger = dataLength;
					 insertByte(convertToInteger.arrByte[0], ptrPacket, packetIndex);
					 insertByte(convertToInteger.arrByte[1], ptrPacket, packetIndex);

					 for (UInt16 dataIndex = 0; dataIndex < dataLength; dataIndex++) {
						 insertByte(ptrData[dataIndex], ptrPacket, packetIndex);
					 }

					 ptrPacket[packetIndex++] = ETX;

					 return (packetIndex);
				 }
				 else return (0);
			 }



	private: System::Void btnCamLoop2_Click(System::Object^  sender, System::EventArgs^  e) {
		;
	}

	private: System::Void btnCamLoop_Click(System::Object^  sender, System::EventArgs^  e) {
#define PACKETSIZE 64

		byte outPacket[MAXPACKET]; // Allocate a memory buffer which will contain data to send to the USB device	
		byte inPacket[MAXPACKET]; // Allocate a memory buffer which will receive data from USB device	
		UInt16 packetLength;
		
		Mat ColorImage(MATRIXHEIGHT, MATRIXWIDTH, CV_8UC3);
		
		
		VideoCapture cap;

		cap.open(0);

		if (!cap.isOpened()) {
			; // TODO
		}

		namedWindow("Color", WINDOW_NORMAL);
		Mat3b img;
		byte imageByte;
		unsigned int imageInt;


		for (;;) {

			cap >> img;
			flip(img, img, 1);
			if (img.empty()) break;

			resize(img, ColorImage, ColorImage.size(), MATRIXHEIGHT, MATRIXWIDTH, CV_INTER_AREA);
			imshow("Color", ColorImage);

			int numChan = ColorImage.channels();			
			#define CAMDATASIZE (MATRIXHEIGHT * MATRIXWIDTH * 3)
			byte *camData = new byte[CAMDATASIZE];  // you will have to delete[] that later		
			std::memcpy(camData, ColorImage.data, CAMDATASIZE*sizeof(byte));

			#define BRIGHTNESS 64
			byte imageData[CAMDATASIZE];
			for (int i = 0; i < CAMDATASIZE; i++) {
				imageByte = camData[i];
				imageByte = gammaTable[imageByte];
				imageInt = (unsigned int)imageByte;
				imageInt = (imageInt * BRIGHTNESS) / 255;
				imageByte = (byte)imageInt;
				imageData[i] = imageByte;
			}

			if (NUMCHANNELS == 1) {
				byte matrix[MATRIXSIZE];
				int blue, green, red;
				int imageIndex = 0;
				for (int i = 0; i < MATRIXSIZE; i++) {
					red = imageData[imageIndex++] & 0b11000000;
					green = (imageData[imageIndex++] & 0b11100000) >> 2;
					blue = (imageData[imageIndex++] & 0b11100000) >> 5;
					matrix[i] = red | green | blue;
				}
				for (int i = 0; i < NUMPANELS; i++)
					Form1::arrPanel[i]->setOutData(matrix);
			}
			else for (int i = 0; i < NUMPANELS; i++)
				Form1::arrPanel[i]->setOutData(imageData);

			ULONG BytesWritten, BytesRead;
			byte *packetPtr;
			byte outData[PANELSIZE];

			
			for (int j = 0; j < NUMPANELS; j++) {							
				for (int i = 0; i < PANELSIZE; i++) outData[i] = Form1::arrPanel[j]->getOutData(i);				
				packetLength = BuildPacket(222, j, outData, PANELSIZE, outPacket);
				packetPtr = outPacket;
				do {
					if (WinUsb_WritePipe(MyWinUSBInterfaceHandle, 0x01, packetPtr, 64, &BytesWritten, NULL)) {
						ReadFile(MyWinUSBInterfaceHandle, inPacket, 65, &BytesRead, 0);
					}
					else break;
					packetPtr = packetPtr + 64;
				} while (packetPtr < &outPacket[packetLength]);				
			}			

			char ch = (char) waitKey(30);  

		}  // End of endless for loop

	}  // End of btnCamLoop_Click()  

			 byte getColorTableIndex(long *tablePtr, long tableVal) {
				 long upper, lower;
				 int upperIndex, lowerIndex, diffIndex;

				 if (tableVal >= (tablePtr[TABLE_LENGTH - 1])) return (TABLE_LENGTH - 1);
				 if (tableVal <= (tablePtr[0])) return (0);

				 upperIndex = TABLE_LENGTH - 1;
				 lowerIndex = upperIndex / 2;

				 do {
					 upper = tablePtr[upperIndex];
					 lower = tablePtr[lowerIndex];
					 if (tableVal == upper) return ((byte)upperIndex);
					 else if (tableVal == lower) return ((byte)lowerIndex);

					 diffIndex = abs(upperIndex - lowerIndex);
					 if (diffIndex > 1) {
						 if (tableVal < lower) {
							 upperIndex = lowerIndex;
							 lowerIndex = upperIndex - (diffIndex / 2);
						 }
						 else if (tableVal > upper) {
							 lowerIndex = upperIndex;
							 upperIndex = lowerIndex + (diffIndex / 2);
						 }
						 else upperIndex = upperIndex - (diffIndex / 2);
						 if (upperIndex >= (TABLE_LENGTH - 1)) upperIndex = TABLE_LENGTH - 1;
						 if (lowerIndex < 0) lowerIndex = 0;
					 }
				 } while (diffIndex > 1);

				 if (abs(tableVal - upper) < abs(tableVal - lower)) return (byte)upperIndex;
				 else return (byte)lowerIndex;
			 }

			 byte convertRGBtoColorByte(byte Red, byte Green, byte Blue) {
				 long remainder, RedVal, GreenVal, BlueVal;
				 long RGBval;
				 byte RGBindex;

				 RedVal = Red / 3;
				 remainder = Red % 3;
				 if (remainder > 1) RedVal = RedVal + 3;

				 GreenVal = Green / 3;
				 remainder = Green % 3;
				 if (remainder > 1) GreenVal = GreenVal + 3;

				 BlueVal = Blue / 3;
				 remainder = Blue % 3;
				 if (remainder > 1) BlueVal = BlueVal + 3;

				 BlueVal = (BlueVal << 16) & 0xFF0000;
				 GreenVal = (GreenVal << 8) & 0x00FF00;
				 RedVal = RedVal & 0x0000FF;

				 RGBval = BlueVal + GreenVal + RedVal;
				 RGBindex = getColorTableIndex(RGBsortedTable, RGBval);
				 return(RGBindex);
			 }

			 void swap(long *first, long *second)
			 {
				 long temp;

				 temp = *first;
				 *first = *second;
				 *second = temp;
			 }

			 void sort(long *arrData, long *arrSort) {
				 int i, j;

				 for (i = 0; i < TABLE_LENGTH; i++) {
					 arrSort[i] = arrData[i];
				 }

				 for (i = 0; i < TABLE_LENGTH - 1; i++) {
					 for (j = i + 1; j < TABLE_LENGTH; j++) {
						 if (arrSort[j] < arrSort[i])
							 swap(&arrSort[j], &arrSort[i]);
					 }
				 }
			 }


	}; // END public ref class Form1


	Panel::Panel() {
		int i, j, k;
		ZeroCol = 0;
		ZeroRow = 0;
		orientation = HORIZONTAL;
				
		for (i = 0; i < PANELSIZE; i++)
			arrOutData[i] = 0x00;
	}

	void Panel::setOrientation(int index, int orientation, int row, int col) {
		this->ZeroRow = row;
		this->ZeroCol = col;
		this->orientation = orientation;
		this->PanelNumber = index;
	}


	Byte Panel::getOutData(int index) {
		return (this->arrOutData[index]);
	}

	void Panel::setOutData(byte *ptrInData) {
		int offset, matrixRow, matrixCol, chan, colOffset, rowOffset, i, panelCol, panelRow;
		byte dataByte;		

		i = 0;

		if (orientation == HORIZONTAL) {						
			colOffset = ZeroCol * NUMCHANNELS;						
			for (matrixRow = ZeroRow; matrixRow < ZeroRow + PANELROWS; matrixRow++) {
				offset = (MATRIXROWSIZE * matrixRow) + colOffset;
				for (panelCol = 0; panelCol < PANELCOLS; panelCol++) {
					offset = offset + NUMCHANNELS;
					for (chan = NUMCHANNELS-1; chan >= 0; chan--)
						this->arrOutData[i++] = ptrInData[offset + chan];						
				}
			}
		}				
		else if (orientation == VERTICAL) {
			rowOffset = ZeroRow * MATRIXROWSIZE;
			for (matrixCol = ZeroCol + PANELROWS - 1; matrixCol >= ZeroCol; matrixCol--) {
				offset = rowOffset + (matrixCol * NUMCHANNELS) + NUMCHANNELS;
				for (panelCol = 0; panelCol < PANELCOLS; panelCol++) {					
					for (chan = NUMCHANNELS-1; chan >= 0; chan--)
						this->arrOutData[i++] = ptrInData[offset + chan];
					offset = offset + MATRIXROWSIZE;
				}
			}
		}
		else if (orientation == HORIZONTAL_FLIPPED) {
			colOffset = ZeroCol * NUMCHANNELS;
			for (matrixRow = ZeroRow + PANELROWS - 1; matrixRow >= ZeroRow; matrixRow--) {
				offset = (MATRIXROWSIZE * matrixRow) + MATRIXROWSIZE + colOffset;
				for (panelCol = 0; panelCol < PANELCOLS; panelCol++) {					
					for (chan = NUMCHANNELS-1; chan >= 0; chan--)
						this->arrOutData[i++] = ptrInData[offset + chan];
					offset = offset - NUMCHANNELS;
				}
			}
		}
		else {
			rowOffset = ZeroRow * MATRIXROWSIZE;
			for (matrixCol = ZeroCol; matrixCol < ZeroCol + PANELROWS; matrixCol++) {				
				for (panelCol = PANELCOLS - 1; panelCol >= 0; panelCol--) {
					offset = rowOffset + (MATRIXROWSIZE * panelCol) + (matrixCol * NUMCHANNELS) + NUMCHANNELS;
					for (chan = NUMCHANNELS-1; chan >= 0; chan--)
						this->arrOutData[i++] = ptrInData[offset + chan];
					offset = offset - MATRIXROWSIZE;
				}
			}
		}


	} // End setOutData()



}  // END namespace HighBandwidthWinUSB



/*
else if (orientation == VERTICAL) {
rowOffset = (ZeroRow + PANELCOLS) * MATRIXROWSIZE;
for (matrixCol = ZeroCol; matrixCol < ZeroCol + PANELROWS; matrixCol++) {
offset = rowOffset + (matrixCol * NUMCHANNELS) + NUMCHANNELS;
for (j = 0; j < PANELCOLS; j++) {
for (chan = 2; chan >= 0; chan--)
this->arrOutData[i++] = ptrInData[offset + chan];
offset = offset - MATRIXROWSIZE;
}
}
}
*/