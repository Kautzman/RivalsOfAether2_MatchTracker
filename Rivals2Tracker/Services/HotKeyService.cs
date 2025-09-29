using Rivals2Tracker.Data;
using Rivals2Tracker.HotkeyHandler;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace Rivals2Tracker.Services
{
    public static class HotKeyService
    {
        private const int HOTKEY_ID = 1;

        public static void RegisterHotKey(uint modifierCode, uint keyCode)
        {
            HotKeyManager.RegisterHotKey(GlobalData.MainWindowHandle, HOTKEY_ID, modifierCode, keyCode);
        }

        public static void UnregisterHotkey()
        {
            HotKeyManager.UnregisterHotKey(GlobalData.MainWindowHandle, HOTKEY_ID);
        }

        // Might not actually use this, but we'll see.
        public static void RegisterNewHotKey(uint modifierCode, uint keyCode)
        {
            HotKeyManager.UnregisterHotKey(GlobalData.MainWindowHandle, HOTKEY_ID);
            HotKeyManager.RegisterHotKey(GlobalData.MainWindowHandle, HOTKEY_ID, modifierCode, keyCode);
        }

        public static void RegisterNewHotKey(ModifierKeys modifierCode, Key keyCode)
        {
            HotKeyManager.UnregisterHotKey(GlobalData.MainWindowHandle, HOTKEY_ID);
            HotKeyManager.RegisterHotKey(GlobalData.MainWindowHandle, HOTKEY_ID, ConvertModifierFlagsToUint(modifierCode), ConvertKeyCodeToUint(keyCode));
            GlobalData.HotKeyCode = ConvertKeyCodeToUint(keyCode);
            GlobalData.ModifierCode = ConvertModifierFlagsToUint(modifierCode);
        }

        public static uint ConvertModifierFlagsToUint(ModifierKeys modifiers)
        {
            uint mod = 0;

            if (modifiers.HasFlag(ModifierKeys.Alt)) mod |= HotKeyManager.MOD_ALT;
            if (modifiers.HasFlag(ModifierKeys.Control)) mod |= HotKeyManager.MOD_CONTROL;
            if (modifiers.HasFlag(ModifierKeys.Shift)) mod |= HotKeyManager.MOD_SHIFT;
            if (modifiers.HasFlag(ModifierKeys.Windows)) mod |= HotKeyManager.MOD_WIN;

            return mod;
        }

        public static uint ConvertKeyCodeToUint(Key keyCode)
        {
            uint keyCodeUint = (uint)KeyInterop.VirtualKeyFromKey(keyCode);
            return keyCodeUint;
        }

        public static string GetReadableShortcut(ModifierKeys modifiers, Key key)
        {
            string mods = "";
            if (modifiers.HasFlag(ModifierKeys.Control)) mods += "Ctrl+";
            if (modifiers.HasFlag(ModifierKeys.Alt)) mods += "Alt+";
            if (modifiers.HasFlag(ModifierKeys.Shift)) mods += "Shift+";
            if (modifiers.HasFlag(ModifierKeys.Windows)) mods += "Win+";

            string keyName = new KeyConverter().ConvertToString(key);

            return mods + keyName;
        }

        public static string GetReadableShortcutFromUints(uint modifiersValue, uint keyValue)
        {
            ModifierKeys modifiers = (ModifierKeys)modifiersValue;
            Key key = KeyInterop.KeyFromVirtualKey((int)keyValue);

            return GetReadableShortcut(modifiers, key);
        }
    }
}
