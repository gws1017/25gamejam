using UnityEngine;

public interface IToggleUI 
{
    public void Show();
    public void Hide(bool resumeGame);
    public void SubscribeOnClickEvents();
    public void UnsubscribeOnClickEvents();
}
