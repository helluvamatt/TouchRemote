#include "touchremotenative.h"

void SendKeyCombo(WORD dwVirtualKey, BOOL bAlt, BOOL bCtrl, BOOL bShift, BOOL bMeta)
{
	/* Greetings, future developer. Don't mess around in here! Not sending a corresponding KEYEVENTF_KEYUP can break the OS UX */
	if (bAlt) SendKey(VK_LMENU, FALSE);
	if (bCtrl) SendKey(VK_LCONTROL, FALSE);
	if (bShift) SendKey(VK_LSHIFT, FALSE);
	if (bMeta) SendKey(VK_LWIN, FALSE);
	SendKey(dwVirtualKey, FALSE);
	SendKey(dwVirtualKey, TRUE);
	if (bMeta) SendKey(VK_LWIN, TRUE);
	if (bShift) SendKey(VK_LSHIFT, TRUE);
	if (bCtrl) SendKey(VK_LCONTROL, TRUE);
	if (bAlt) SendKey(VK_LMENU, TRUE);
}

void SendKey(WORD wVirtualKey, BOOL bUp)
{
	INPUT input;
	input.type = INPUT_KEYBOARD;
	input.ki.dwFlags = bUp ? KEYEVENTF_KEYUP : 0;
	input.ki.wVk = wVirtualKey;
	SendInput(1, &input, sizeof(INPUT));
}

void SendMouseMove(LONG dX, LONG dY)
{
	INPUT input;
	input.type = INPUT_MOUSE;
	input.mi.dwFlags = MOUSEEVENTF_MOVE;
	input.mi.dx = dX;
	input.mi.dy = dY;
	input.mi.time = 0;
	SendInput(1, &input, sizeof(INPUT));
}

void SendMouseScroll(DWORD wheelDelta)
{
	INPUT input;
	input.type = INPUT_MOUSE;
	input.mi.dwFlags = MOUSEEVENTF_WHEEL;
	input.mi.mouseData = wheelDelta;
	input.mi.time = 0;
	SendInput(1, &input, sizeof(INPUT));
}

void SendMouse(DWORD flags)
{
	INPUT input;
	input.type = INPUT_MOUSE;
	input.mi.dwFlags = flags;
	input.mi.time = 0;
	SendInput(1, &input, sizeof(INPUT));
}

void SendMouseClickLeft()
{
	SendMouse(MOUSEEVENTF_LEFTDOWN);
	SendMouse(MOUSEEVENTF_LEFTUP);
}

void SendMouseClickMiddle()
{
	SendMouse(MOUSEEVENTF_MIDDLEDOWN);
	SendMouse(MOUSEEVENTF_MIDDLEUP);
}

void SendMouseClickRight()
{
	SendMouse(MOUSEEVENTF_RIGHTDOWN);
	SendMouse(MOUSEEVENTF_RIGHTUP);
}
