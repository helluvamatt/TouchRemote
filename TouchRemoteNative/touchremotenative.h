#pragma once

#include <windows.h>

#ifdef TOUCHREMOTENATIVE_EXPORTS
#define TOUCHREMOTENATIVE_API __declspec(dllexport)
#else
#define TOUCHREMOTENATIVE_API __declspec(dllimport)
#endif

TOUCHREMOTENATIVE_API void __cdecl SendKeyCombo(WORD dwVirtualKey, BOOL bAlt, BOOL bCtrl, BOOL bShift, BOOL bMeta);

TOUCHREMOTENATIVE_API void __cdecl SendKey(WORD dwVirtualKey, BOOL bUp);

TOUCHREMOTENATIVE_API void __cdecl SendMouseMove(LONG dX, LONG dY);

TOUCHREMOTENATIVE_API void __cdecl SendMouseScroll(DWORD wheelDelta);

TOUCHREMOTENATIVE_API void __cdecl SendMouseClickLeft();

TOUCHREMOTENATIVE_API void __cdecl SendMouseClickMiddle();

TOUCHREMOTENATIVE_API void __cdecl SendMouseClickRight();
