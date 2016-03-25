//*****************************************************************************
//
// debug.h -
//
// Copyright (c) 2013-2015 Texas Instruments Incorporated.  All rights reserved.
// Software License Agreement
// 
// Texas Instruments (TI) is supplying this software for use solely and
// exclusively on TI's microcontroller products. The software is owned by
// TI and/or its suppliers, and is protected under applicable copyright
// laws. You may not combine this software with "viral" open-source
// software in order to form a larger program.
// 
// THIS SOFTWARE IS PROVIDED "AS IS" AND WITH ALL FAULTS.
// NO WARRANTIES, WHETHER EXPRESS, IMPLIED OR STATUTORY, INCLUDING, BUT
// NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE APPLY TO THIS SOFTWARE. TI SHALL NOT, UNDER ANY
// CIRCUMSTANCES, BE LIABLE FOR SPECIAL, INCIDENTAL, OR CONSEQUENTIAL
// DAMAGES, FOR ANY REASON WHATSOEVER.
// 
// This is part of revision 2.1.1.71 of the DK-TM4C129X Firmware Package.
//
//*****************************************************************************

#ifndef __DEBUG_H__
#define __DEBUG_H__

#ifdef DEBUG_OUTPUT
//#include "driverlib/uart.h"
#include "utils/uartstdio.h"
#define DebugPrintf(...)        UARTprintf(__VA_ARGS__)
#else
#define DebugPrintf(...)
#endif
#else
#define DebugPrintf(...)

#endif
