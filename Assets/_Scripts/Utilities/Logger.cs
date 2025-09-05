using System.Diagnostics;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;

public static class Logger
{
    [Conditional("UNITY_EDITOR")]
    public static void Log(string message)
    {
        Debug.Log($"<color=#ffff00>{message}</color>");
    }

    public static void LogError(string message)
    {
        Debug.LogError($"<color=#ff0000>{message}</color>");
    }

    public static void UnityEditorLog(string message)
    {
        // Unity 에디터에서만 로그를 출력
#if UNITY_EDITOR
        Debug.Log($"<color=#00ff00>{message}</color>");
#endif
    }
}