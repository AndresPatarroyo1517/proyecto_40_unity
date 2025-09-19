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
    [Header("Power-Up Configuration")]
    public PowerUpType type;
    
    [Header("Duration & Effect")]
    public float duration = 5f;
    public float multiplier = 2f;
    
    [Header("Display")]
    public string displayName = "Power-Up";
    [TextArea(2, 4)]
    public string description = "Descripción del power-up";
    
    [Header("Visual")]
    public Color effectColor = Color.white;
    public Sprite icon;
    
    void OnValidate()
    {
        if (string.IsNullOrEmpty(displayName) || displayName == "Power-Up")
        {
            switch (type)
            {
                case PowerUpType.Speed:
                    displayName = "Velocidad";
                    if (string.IsNullOrEmpty(description))
                        description = $"Aumenta la velocidad de movimiento x{multiplier} por {duration} segundos";
                    break;
                case PowerUpType.Invincibility:
                    displayName = "Invencibilidad";
                    if (string.IsNullOrEmpty(description))
                        description = $"Te hace invencible por {duration} segundos";
                    break;
                case PowerUpType.Jump:
                    displayName = "Super Salto";
                    if (string.IsNullOrEmpty(description))
                        description = $"Permite saltar en el aire por {duration} segundos";
                    break;
                case PowerUpType.DoubleCoins:
                    displayName = $"Monedas x{(int)multiplier}";
                    if (string.IsNullOrEmpty(description))
                        description = $"Las monedas valen x{(int)multiplier} por {duration} segundos";
                    break;
                case PowerUpType.InstantWin:
                    displayName = "Victoria Instantánea";
                    if (string.IsNullOrEmpty(description))
                        description = "Completa el nivel instantáneamente";
                    break;
            }
        }
        
        switch (type)
        {
            case PowerUpType.DoubleCoins:
                if (multiplier < 1f)
                    multiplier = 2f;
                break;
            case PowerUpType.Speed:
                if (multiplier <= 0f)
                    multiplier = 1.5f;
                break;
            case PowerUpType.InstantWin:
                duration = 0f; 
                break;
        }
        
        if (duration < 0f)
            duration = 0f;
    }
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
    
    public float GetProgress()
    {
        return remainingTime / 10f; 
    }
}