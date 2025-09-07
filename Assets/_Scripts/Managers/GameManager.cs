using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region [Singleton]
    public static GameManager Instance { get; private set; }

    private void SingleTon()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);

        Instance = this;
    }

    private void EmptySingleton()
    {
        if (Instance != null)
            Instance = null;
    }
    #endregion

    private float countdownToStartTimer = 3f;
    private bool isGamePaused = false;

    private GameState currentGameState;
    private enum GameState
    {
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameResumed;

    private Coroutine coroutine_PauseGame;

    private void Awake()
    {
        SingleTon();
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
                // UI 클릭 불가능
                Debug.Log("Countdown To Start");
                break;
            case GameState.GamePlaying:
                // 플레이어 슈팅 가능
                // UI 클릭 가능
                // 적 스폰 시작
                Debug.Log("Game Playing");
                break;
            case GameState.GameOver:
                // Handle Game Over Logic
                break;
        }
    }

    private void StartCountDownTimer()
    {
        // Start Countdown Timer Logic
        countdownToStartTimer -= Time.deltaTime;

        // Timer hits zero, we start the game
        if (countdownToStartTimer < 0f)
            ChangeState(GameState.GamePlaying);
    }

    private void ChangeState(GameState newState)
    {
        currentGameState = newState;
    }

    private IEnumerator PauseGameCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        PauseGame();
    }
    #endregion
}
