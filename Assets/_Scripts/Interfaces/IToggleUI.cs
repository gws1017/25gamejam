using UnityEngine;

public interface IToggleUI 
{
    public void Show();
    public void Hide();
    public void SubscribeOnClickEvents();
    public void UnsubscribeOnClickEvents();
}
