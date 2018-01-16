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

