using UnityEngine;
using Mirror;
using System;

public class Health : NetworkBehaviour
{
    [SyncVar(hook="OnHealthChanged")] public float current;
    public float maxHealth;

    public override void OnStartAuthority()
    {
        if (CanvasManager.instance.healthBar != null)
        {
            CanvasManager.instance.healthBar.minValue = 0f;
            CanvasManager.instance.healthBar.maxValue = maxHealth;
            CanvasManager.instance.healthBar.value = current;
            CanvasManager.instance.healthBar.gameObject.SetActive(true);
        }
    }

    [Server]
    public void GiveHealth(float _healthToGive)
    {
        current = Mathf.Clamp(current + _healthToGive, 0, maxHealth);
    }

    [Server]
    public void TakeDamage(float _damageToTake)
    { 
        current = Mathf.Clamp(current - _damageToTake, 0, maxHealth);

        if (current <= 0f)
        {
            Debug.Log($"[Server]: Client: {netId} has died at {NetworkTime.time}.");
        }
    }

    public void OnHealthChanged(float _oldValue, float _newValue)
    {
        if (isOwned)
        {
            Debug.Log($"Your health changed from {_oldValue} to {_newValue}.");

            if (CanvasManager.instance.healthBar != null)
            {
                CanvasManager.instance.healthBar.value = _newValue;
            }
        }
    }
}