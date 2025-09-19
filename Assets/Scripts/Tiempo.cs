using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tiempo : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText; 
    

    private bool startOnAwake = true;
    private bool resetOnSceneChange = true;
    private TimeFormat timeFormat = TimeFormat.MinutesSecondsMilliseconds; 
    
    [Header("Ajustes")]
    public string prefix = "Tiempo: "; 
    public string suffix = ""; 
    public Color normalColor = Color.white; 
    public Color warningColor = Color.yellow; 
    public float warningTime = 60f; 
    
    private bool showMilliseconds = true; 
    private float updateFrequency = 0.01f; 
    private bool showDebugMessages = false;
    
    private float elapsedTime = 0f;
    private bool isRunning = false;
    private Coroutine timerCoroutine;
    
    public float ElapsedTime => elapsedTime;
    public bool IsRunning => isRunning;
    
    void Start()
    {
        if (timerText == null)
        {
            timerText = GetComponent<TextMeshProUGUI>();
            if (timerText == null)
            {
                Debug.LogError("SceneTimer: No se encontró TextMeshProUGUI. Asigna uno en el inspector o agrega el componente.");
                enabled = false;
                return;
            }
        }
        
        // Resetear tiempo si está configurado
        if (resetOnSceneChange)
        {
            elapsedTime = 0f;
        }
        
        // Inicializar display
        UpdateTimerDisplay();
        
        // Comenzar automáticamente si está configurado
        if (startOnAwake)
        {
            StartTimer();
        }
        
        if (showDebugMessages)
        {
            Debug.Log("SceneTimer: Iniciado en la escena");
        }
    }
    
    public void StartTimer()
    {
        if (!isRunning)
        {
            isRunning = true;
            timerCoroutine = StartCoroutine(TimerCoroutine());
            
            if (showDebugMessages)
            {
                Debug.Log("SceneTimer: Timer iniciado");
            }
        }
    }
    
    public void PauseTimer()
    {
        if (isRunning)
        {
            isRunning = false;
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            
            if (showDebugMessages)
            {
                Debug.Log($"SceneTimer: Timer pausado en {elapsedTime:F2} segundos");
            }
        }
    }
    
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay();
        
        if (showDebugMessages)
        {
            Debug.Log("SceneTimer: Timer reseteado");
        }
    }
    
    public void RestartTimer()
    {
        PauseTimer();
        ResetTimer();
        StartTimer();
    }
    
    IEnumerator TimerCoroutine()
    {
        while (isRunning)
        {
            elapsedTime += updateFrequency;
            UpdateTimerDisplay();
            yield return new WaitForSeconds(updateFrequency);
        }
    }
    
    void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        string timeString = FormatTime(elapsedTime);
        timerText.text = prefix + timeString + suffix;
        
        // Cambiar color si es necesario
        if (warningTime > 0 && elapsedTime >= warningTime)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }
    
    string FormatTime(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600f);
        int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);
        
        switch (timeFormat)
        {
            case TimeFormat.SecondsOnly:
                if (showMilliseconds)
                    return $"{time:F2}s";
                else
                    return $"{Mathf.FloorToInt(time)}s";
                
            case TimeFormat.MinutesSeconds:
                return $"{minutes:D2}:{seconds:D2}";
                
            case TimeFormat.MinutesSecondsMilliseconds:
                if (showMilliseconds)
                    return $"{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
                else
                    return $"{minutes:D2}:{seconds:D2}";
                    
            case TimeFormat.HoursMinutesSeconds:
                if (hours > 0)
                    return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                else
                    return $"{minutes:D2}:{seconds:D2}";
                    
            case TimeFormat.Complete:
                if (showMilliseconds)
                {
                    if (hours > 0)
                        return $"{hours:D2}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
                    else
                        return $"{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
                }
                else
                {
                    if (hours > 0)
                        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                    else
                        return $"{minutes:D2}:{seconds:D2}";
                }
                
            default:
                return $"{minutes:D2}:{seconds:D2}";
        }
    }
    
    // Métodos públicos para control externo
    public void SetElapsedTime(float time)
    {
        elapsedTime = Mathf.Max(0f, time);
        UpdateTimerDisplay();
    }
    
    public void AddTime(float timeToAdd)
    {
        elapsedTime += timeToAdd;
        UpdateTimerDisplay();
    }
    
    public string GetFormattedTime()
    {
        return FormatTime(elapsedTime);
    }
    
    public void SetTimeFormat(TimeFormat newFormat)
    {
        timeFormat = newFormat;
        UpdateTimerDisplay();
    }
    
    public void SetPrefix(string newPrefix)
    {
        prefix = newPrefix;
        UpdateTimerDisplay();
    }
    
    public void SetSuffix(string newSuffix)
    {
        suffix = newSuffix;
        UpdateTimerDisplay();
    }
    
    void OnDisable()
    {
        if (isRunning && timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }
    
    void OnDestroy()
    {
        // Limpiar corrutina al destruir
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }
}

[System.Serializable]
public enum TimeFormat
{
    SecondsOnly,                    
    MinutesSeconds,                 
    MinutesSecondsMilliseconds,     
    HoursMinutesSeconds,            
    Complete                        
}