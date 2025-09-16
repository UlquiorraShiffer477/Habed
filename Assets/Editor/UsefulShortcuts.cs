using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
 
static class UsefulShortcuts
{
    // Alt + C
    [Shortcut("Clear Console", KeyCode.C, ShortcutModifiers.Alt)]
    public static void ClearConsole()
    {
        // This simply does "LogEntries.Clear()" the long way:
		var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
		var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
		clearMethod.Invoke(null,null);

        
    }
}
