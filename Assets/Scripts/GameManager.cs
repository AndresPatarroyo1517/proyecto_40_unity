using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI powerUpText;
    public TextMeshProUGUI goalText;
    
    [Header("Scene Management")]
    public int nextSceneIndex = 1;
    
    [Header("Goal Settings")]
    public int requiredCoins = 50;
    
    private PlayerController playerController;
    
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        
        if (goalText != null)
        {
            goalText.text = $"Objetivo: Recolecta {requiredCoins} monedas y ve al portal";
        }
    }
    
    public void UpdateCoinUI(int currentCoins, int targetCoins)
    {
        if (coinText != null)
        {
            coinText.text = $"Monedas: {currentCoins}/{targetCoins}";
            
            if (currentCoins >= targetCoins)
            {
                coinText.color = Color.green;
            }
            else
            {
                coinText.color = Color.white;
            }
        }
    }
    
    public void UpdatePowerUpUI(List<ActivePowerUp> activePowerUps)
    {
        if (powerUpText != null)
        {
            if (activePowerUps.Count == 0)
            {
                powerUpText.text = "";
                return;
            }
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Power-ups Activos:");
            
            foreach (var powerUp in activePowerUps)
            {
                string powerUpName = GetPowerUpDisplayName(powerUp.type, powerUp.multiplier);
                string timeText = powerUp.remainingTime > 0 ? $"{powerUp.remainingTime:F1}s" : "Permanente";
                sb.AppendLine($"• {powerUpName}: {timeText}");
            }
            
            powerUpText.text = sb.ToString();
        }
    }
    
    string GetPowerUpDisplayName(PowerUpType type, float multiplier)
    {
        switch (type)
        {
            case PowerUpType.Speed: 
                return $"Velocidad x{multiplier:F1}";
            case PowerUpType.Invincibility: 
                return "Invencibilidad";
            case PowerUpType.Jump: 
                return "Super Salto";
            case PowerUpType.DoubleCoins: 
                return $"Monedas x{(int)multiplier}";
            case PowerUpType.InstantWin: 
                return "Victoria Instantánea";
            default: 
                return type.ToString();
        }
    }
    
    public void ShowMessage(string message, float duration = 3f)
    {
        StartCoroutine(ShowTemporaryMessage(message, duration));
    }
    
    IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        if (goalText != null)
        {
            string originalText = goalText.text;
            goalText.text = message;
            goalText.color = Color.yellow;
            
            yield return new WaitForSeconds(duration);
            
            goalText.text = originalText;
            goalText.color = Color.white;
        }
    }
    
    public void TriggerInstantWin()
    {
        StartCoroutine(WinSequence("¡Power-up de Victoria Secreta!"));
    }
    
    public void TriggerNormalWin()
    {
        StartCoroutine(WinSequence("¡Nivel Completado!"));
    }
    
    IEnumerator WinSequence(string winMessage)
    {
        if (coinText != null)
        {
            coinText.text = winMessage;
        }
        
        float countdown = 3f;
        while (countdown > 0)
        {
            if (powerUpText != null)
            {
                powerUpText.text = $"Cambiando en: {countdown:F1}s";
            }
            
            countdown -= Time.deltaTime;
            yield return null;
        }
        
        LoadNextLevel();
    }
    
    void LoadNextLevel()
    {
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning($"Scene {nextSceneIndex} not found in Build Settings");
        }
    }
}