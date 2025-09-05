using UnityEngine;
using System;
using System.Collections;

public static class ExtensionMethods
{
    public static void StartCoroutineHelper(this MonoBehaviour extension, ref Coroutine coroutineVariable, IEnumerator Coroutine)
    {
        if (coroutineVariable != null)
            extension.StopCoroutine(coroutineVariable);

        coroutineVariable = extension.StartCoroutine(Coroutine);
    }
}
