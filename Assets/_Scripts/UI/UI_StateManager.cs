using System.Collections.Generic;
using UnityEngine;

public class UI_StateManager : MonoBehaviour
{
    #region [Singleton]
    public static UI_StateManager Instance { get; private set; }

    private void SingleTon()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);

        Instance = this;
    }
    #endregion

    public enum UIState
    {
        UI_Paused,
        UI_Shop,
        UI_Skill,
        UI_GameOver,
        UI_SoundSettings,
    }

    private UIState currentState;

    // Must implement IGamePauseUI interface
    [SerializeField] private MonoBehaviour ui_Paused;
    [SerializeField] private MonoBehaviour ui_Shop;
    [SerializeField] private MonoBehaviour ui_Skill;
    [SerializeField] private MonoBehaviour ui_GameOver;
    [SerializeField] private MonoBehaviour ui_SoundSettings;

    private readonly Dictionary<UIState, IToggleUI> UIStateDictionary = new();

    private void Awake()
    {
        SingleTon();
    }

    private void Start()
    {
        Build_UIStateDictionary();
        HideAll();

        // Default state
        SetState(UIState.UI_Paused);
    }

    #region Internal Logic
    private void Build_UIStateDictionary()
    {
        // Validate & Cast to interface; fail fast if miswired
        TryAdd(UIState.UI_Paused, ui_Paused);
        TryAdd(UIState.UI_Shop, ui_Shop);
        TryAdd(UIState.UI_Skill, ui_Skill);
        TryAdd(UIState.UI_GameOver, ui_GameOver);
        TryAdd(UIState.UI_SoundSettings, ui_GameOver);
    }

    private void TryAdd(UIState state, MonoBehaviour UIComponent)
    {
        if (UIComponent == null) return;

        // Read it as:
        // If UIComponent is NOT an IGamePauseUI -> bail out
        // If it is an IGamePauseUI -> create a new local variable named *** pausedUI *** (typed as IGamePauseUI) that refers to UIComponent cast to that interface.
        if (UIComponent is not IToggleUI pausedUI) return;

        UIStateDictionary[state] = pausedUI;
    }

    private void HideAll()
    {
        // Hide all UI components in the dictionary
        foreach (var pausedUI in UIStateDictionary.Values)
        {
            pausedUI.Hide();
        }
    }

    public void SetState(UIState newState)
    {
        // If the newState is not in the dictionary, return early
        if (!UIStateDictionary.ContainsKey(newState)) return;

        // First, hide all UI components
        HideAll();

        // Then, set the current state
        currentState = newState;

        // Finally, show only the currentState
        UIStateDictionary[currentState].Show();
    }
    #endregion
}
