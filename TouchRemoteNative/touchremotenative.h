#pragma once

#include <windows.h>

#ifdef TOUCHREMOTENATIVE_EXPORTS
#define TOUCHREMOTENATIVE_API __declspec(dllexport)
#else
#define TOUCHREMOTENATIVE_API __declspec(dllimport)
#endif

TOUCHREMOTENATIVE_API void __cdecl SendKeyCombo(WORD dwVirtualKey, BOOL bAlt, BOOL bCtrl, BOOL bShift, BOOL bMeta);

TOUCHREMOTENATIVE_API void __cdecl SendKey(WORD dwVirtualKey, BOOL bUp);
