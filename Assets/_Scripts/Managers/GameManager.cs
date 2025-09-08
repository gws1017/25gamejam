using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region [Singleton]
    public static GameManager Instance { get; private set; }

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

    [SerializeField] private Image clickBlocker;

    private float countdownToStartTimer = 3f;
    private bool isGamePaused = false;

    public PoolManager pool;
    public GameState currentGameState;
    public enum GameState
    {
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs
    {
        GameState currentGameState;

        public OnStateChangedEventArgs(GameState newState)
        {
            this.currentGameState = newState;
        }

        public GameState GetCurrentGameState() => currentGameState;
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameResumed;

    private Coroutine coroutine_PauseGame;

    private void Awake()
    {
        SingleTon();
    }

    private void Start()
    {
        SetState(GameState.CountdownToStart);
    }

    private void Update()
    {
        ManageGameState();
    }

    private void OnDestroy()
    {
        EmptySingleton();
    }

    #region Public API
    
    [ContextMenu("GameOverState")]
    public void GameOverState()
    {
        SetState(GameState.GameOver);
    }

    public void PauseGame()
    {
        if (isGamePaused) return;

        isGamePaused = true;
        Time.timeScale = 0f;

        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void PauseGameWithDelay()
    {
        this.StartCoroutineHelper(ref coroutine_PauseGame, PauseGameCoroutine());
        Debug.Log("PauseGameWithDelay called.");
    }

    public void PauseGameWithDelay_ForDyingAnim()
    {
        this.StartCoroutineHelper(ref coroutine_PauseGame, PauseGameCoroutine_ForDyingAnim());
        Debug.Log("PauseGameWithDelay called.");
    }

    public void ResumeGame()
    {
        if (!isGamePaused) return;

        isGamePaused = false;
        Time.timeScale = 1f;

        OnGameResumed?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Internal Logic
    private void ManageGameState()
    {
        switch (currentGameState)
        {
            case GameState.CountdownToStart:
                StartCountDownTimer();
                // 플레이어 슈팅 불가능
                // Debug.Log("Countdown To Start");
                break;
            case GameState.GamePlaying:
                // 플레이어 슈팅 가능
                // 적 스폰 시작
                // Debug.Log("Game Playing");
                break;
            case GameState.GameOver:
                break;
        }
    }

    private void StartCountDownTimer()
    {
        // Start Countdown Timer Logic
        countdownToStartTimer -= Time.deltaTime;

        // Timer hits zero, we start the game
        if (countdownToStartTimer < 0f)
            SetState(GameState.GamePlaying);
    }

    public void SetState(GameState newState)
    {
        currentGameState = newState;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs(newState));
        ManagerActionsPerState(newState);
    }

    private void ManagerActionsPerState(GameState state)
    {
        switch (state)
        {
            case GameState.CountdownToStart:
                // TurnOnClickBlocker();
                break;
            case GameState.GamePlaying:
                //TurnOffClickBlocker();
                break;
            case GameState.GameOver:
                //TurnOffClickBlocker();
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs(state));
                PauseGameWithDelay_ForDyingAnim();
                break;
        }
    }

    private IEnumerator PauseGameCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        PauseGame();
    }

    private IEnumerator PauseGameCoroutine_ForDyingAnim()
    {
        yield return new WaitForSeconds(0.8f);
        PauseGame();
    }

    private void TurnOnClickBlocker()
    {
        clickBlocker.gameObject.SetActive(true);
    }

    private void TurnOffClickBlocker()
    {
        clickBlocker.gameObject.SetActive(false);
    }
    #endregion
}
