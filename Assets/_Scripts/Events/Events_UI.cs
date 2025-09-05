using System;
using UnityEngine;

public class Events_UI 
{
    public event EventHandler OnOptionsButtonClicked;
    public void InvokeOnOptionsButtonsClicked() => OnOptionsButtonClicked?.Invoke(this, EventArgs.Empty);
}
