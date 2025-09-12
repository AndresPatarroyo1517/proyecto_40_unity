using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType
{
    Speed,
    Invincibility,
    Jump,
    DoubleCoins,
    InstantWin
}

[CreateAssetMenu(fileName = "New PowerUp", menuName = "PowerUps/PowerUp")]
public class PowerUpData : ScriptableObject
{
    public PowerUpType type;
    public float duration;
    public float multiplier;
    public string displayName;
    public Color effectColor = Color.white;
}

[System.Serializable]
public class ActivePowerUp
{
    public PowerUpType type;
    public float remainingTime;
    public float multiplier;
    
    public ActivePowerUp(PowerUpType type, float duration, float multiplier)
    {
        this.type = type;
        this.remainingTime = duration;
        this.multiplier = multiplier;
    }
}