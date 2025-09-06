using System;
using UnityEngine;

public class ItemEvents : MonoBehaviour
{
    #region [Singleton]
    public static ItemEvents Instance { get; private set; }

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

    public event EventHandler OnAddHeartItemBought;
    public void InvokeOnAddHeartItemBought() => OnAddHeartItemBought?.Invoke(this, EventArgs.Empty);


    public event EventHandler OnInvincibilityBought;
    public void InvokeOnInvincibilityBought() => OnInvincibilityBought?.Invoke(this, EventArgs.Empty);
    public event EventHandler OnInvincibilityExpired;
    public void InvokeOnInvincibilityExpired() => OnInvincibilityExpired?.Invoke(this, EventArgs.Empty);


    public event EventHandler OnEnemySpeedDownBought;
    public void InvokeOnEnemySpeedDownBought() => OnEnemySpeedDownBought?.Invoke(this, EventArgs.Empty);
    public event EventHandler OnEnemySpeedDownExpired;
    public void InvokeOnEnemySpeedDownExpired() => OnEnemySpeedDownExpired?.Invoke(this, EventArgs.Empty);


    public event EventHandler OnEnemyDefenseDownBought;
    public void InvokeOnEnemyDefenseDownBought() => OnEnemyDefenseDownBought?.Invoke(this, EventArgs.Empty);
    public event EventHandler OnEnemyDefenseDownExpired;
    public void InvokeOnEnemyDefenseDownExpired() => OnEnemyDefenseDownExpired?.Invoke(this, EventArgs.Empty);


    public event EventHandler OnAttackBoostBought;
    public void InvokeOnAttackBoostBought() => OnAttackBoostBought?.Invoke(this, EventArgs.Empty);
    public event EventHandler OnAttackBoostExpired;
    public void InvokeOnAttackBoostExpired() => OnAttackBoostExpired?.Invoke(this, EventArgs.Empty);


    public event EventHandler OnDefenseBoostBought;
    public void InvokeOnDefenseBoostBought() => OnDefenseBoostBought?.Invoke(this, EventArgs.Empty);
    public event EventHandler OnDefenseBoostExpired;
    public void InvokeOnDefenseBoostExpired() => OnDefenseBoostExpired?.Invoke(this, EventArgs.Empty);
}
