using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePause;
    public event EventHandler OnGameUnPause;
    private enum State
    {
        WaitingToStart,
        CutDownToStart,
        GamePlaying,
        GameOver
    }
    [SerializeField]private Player player; // fallback for single-player

    private State state;

    private float waitingToStartTimer = 1f;
    private float countDownToStartTimer = 3f;
    private float gamePlayingTimer = 60f; //游戏时间总长
    private float gamePlayingTimeTotal; 
    private bool isGamePause = false; 

    // Only start the countdown after a mode is selected
    private bool isModeSelected = false;

    private void Awake()
    {
        Instance = this;
        gamePlayingTimeTotal = gamePlayingTimer;
    }
    private void Start()
    {
        TurnToWaitingToStart();
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        OrderManager.Instance.OnRecipeSucceed += OrderManager_OnRecipeSucceed;
        OrderManager.Instance.OnRecipeFailed += OrderManager_OnRecipeFailed;
    }

    private void OrderManager_OnRecipeFailed(object sender, EventArgs e)
    {
        DecreaseGameTime(10); //失败减时
    }

    private void OrderManager_OnRecipeSucceed(object sender, EventArgs e)
    {
        AddGameTime(15);
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        ToggleGame();
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                // Do not auto-start the countdown. Wait for player to choose a mode.
                if (isModeSelected)
                {
                    waitingToStartTimer -= Time.deltaTime;
                    if (waitingToStartTimer <= 0)
                    {
                        TurnToCountDownToStart();
                    }
                }
                break;
            case State.CutDownToStart:
                countDownToStartTimer -= Time.deltaTime;
                if(countDownToStartTimer <= 0)
                {
                    TurnToGamePlaying(); //游戏开始
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if(gamePlayingTimer <= 0)
                {
                    gamePlayingTimer = 0;
                    TurnToGameOver(); //游戏结束
                }
                break;
            case State.GameOver:
                break;
        }
    }
    public void AddGameTime(float seconds)
    {
        gamePlayingTimer += seconds;
    }
    public void DecreaseGameTime(float seconds)
    {
        gamePlayingTimer -= seconds;
    }

    // Public entry for UI to start single player mode
    public void StartSinglePlayerMode()
    {
        isModeSelected = true;
        waitingToStartTimer = 0f;
        countDownToStartTimer = 3f;
        TurnToCountDownToStart();
    }


    public void StartMultiplayerMode()
    {
        isModeSelected = true;
        waitingToStartTimer = 0f;
        countDownToStartTimer = 3f; 
        TurnToCountDownToStart();
    }

    private void TurnToWaitingToStart()
    { 
        state = State.WaitingToStart;
        DisablePlayer();
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    private void TurnToCountDownToStart()
    {
        state = State.CutDownToStart;
        DisablePlayer();
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    private void TurnToGamePlaying()
    { 
        state = State.GamePlaying;
        EnablePlayer();
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    private void TurnToGameOver()
    {
        state = State.GameOver;
        DisablePlayer();
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    private void DisablePlayer()
    {
        if (Player.Instance != null)
        {
            Player.Instance.enabled = false;
            return;
        }
        if (player != null)
        {
            player.enabled = false;
        }
    }
    private void EnablePlayer()
    {
        if (Player.Instance != null)
        {
            Player.Instance.enabled = true;
            return;
        }
        if (player != null)
        {
            player.enabled = true;
        }
    }
    public bool IsWaitingToStartState()
    {
        return state == State.WaitingToStart;  //判断是否处于等待开始状态
    }
    public bool IsCountDownState()
    {
        return state == State.CutDownToStart;
    }
    public bool IsGamePlayingState()
    {
        return state == State.GamePlaying;
    }
    public bool IsGameOverState()
    {
        return state == State.GameOver;
    }
    public float GetCountDownTime()
    {
        return countDownToStartTimer; //返回倒计时时间
    }
    /// <summary>
    /// 控制开关游戏
    /// </summary>
    public void ToggleGame()
    {
        isGamePause = !isGamePause;
        if (isGamePause)
        {
            Time.timeScale = 0;
            OnGamePause?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1;
            OnGameUnPause?.Invoke(this, EventArgs.Empty);
        }
    }
    public float GetGamePlayingTime()
    {
        return gamePlayingTimer;
    }
    public float GetGamePlayingTimeNormalized()
    {
        return gamePlayingTimer / gamePlayingTimeTotal; //返回游戏时间归一化
    }
}
