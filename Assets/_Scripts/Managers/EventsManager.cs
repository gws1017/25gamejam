using UnityEngine;

public class EventsManager : MonoSingleton<EventsManager>
{
    [Header("Events Manager Config")]
    public Events_UI Events_UI { get; private set; }

    protected override void OnEnable()
    {
        Events_UI = new Events_UI();
    }
}
