using UnityEngine;
using System;

public class SoundEvents : MonoBehaviour
{
    #region [Singleton]
    public static SoundEvents Instance { get; private set; }

    private void SingleTon()
    {
        Instance = this;
    }

    private void EmptySingleton()
    {
        if (Instance != null)
            Instance = null;
    }
    #endregion

    private void Awake()
    {
        SingleTon();
    }

    private void OnDestroy()
    {
        EmptySingleton();
    }

    public event EventHandler OnPlayUIPopupFx;
    public void InvokeOnPlayUIPopupFx() => OnPlayUIPopupFx?.Invoke(this, EventArgs.Empty);


    public event EventHandler OnPlayButtonFx;
    public void InvokeOnPlayButtonFx() => OnPlayButtonFx?.Invoke(this, EventArgs.Empty);
}
