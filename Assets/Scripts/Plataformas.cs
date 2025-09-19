using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plataformas : MonoBehaviour
{
[Header("Ajustes")]
    public float deactivationTime = 10f; 
    public bool startOnAwake = true; 
    public bool destroyInsteadOfDeactivate = false; 
    
    [Header("Opciones")]
    public GameObject[] additionalObjectsToDeactivate; 
    public bool deactivateChildren = true; 
    
    [Header("Debug")]
    public bool showDebugMessages = false;
    
    private Coroutine deactivationCoroutine;
    private bool isActive = true;
    
    void Start()
    {
        if (startOnAwake)
        {
            StartDeactivationTimer();
        }
    }
    
    public void StartDeactivationTimer()
    {
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
        }
        
        deactivationCoroutine = StartCoroutine(DeactivationCountdown());
        
        if (showDebugMessages)
        {
            Debug.Log($"{gameObject.name}: Timer iniciado - se desactivará en {deactivationTime} segundos");
        }
    }
    
    public void StartDeactivationTimer(float customTime)
    {
        deactivationTime = customTime;
        StartDeactivationTimer();
    }
    
    public void CancelDeactivationTimer()
    {
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
            deactivationCoroutine = null;
            
            if (showDebugMessages)
            {
                Debug.Log($"{gameObject.name}: Timer cancelado");
            }
        }
    }
    
    public void RestartTimer()
    {
        CancelDeactivationTimer();
        StartDeactivationTimer();
    }
    
    public void RestartTimer(float newTime)
    {
        CancelDeactivationTimer();
        StartDeactivationTimer(newTime);
    }
    
    IEnumerator DeactivationCountdown()
    {
        float remainingTime = deactivationTime;
        
        while (remainingTime > 0f && isActive)
        {
            if (showDebugMessages && remainingTime <= 3f)
            {
                Debug.Log($"{gameObject.name}: Se desactivará en {remainingTime:F1} segundos");
            }
            
            yield return new WaitForSeconds(0.1f);
            remainingTime -= 0.1f;
        }
        
        if (isActive)
        {
            PerformDeactivation();
        }
    }
    
    void PerformDeactivation()
    {
        isActive = false;
        
        if (showDebugMessages)
        {
            Debug.Log($"{gameObject.name}: ¡Tiempo agotado! Desactivando...");
        }
        
        if (additionalObjectsToDeactivate != null)
        {
            foreach (GameObject obj in additionalObjectsToDeactivate)
            {
                if (obj != null)
                {
                    if (destroyInsteadOfDeactivate)
                    {
                        Destroy(obj);
                    }
                    else
                    {
                        obj.SetActive(false);
                    }
                }
            }
        }
        
        if (deactivateChildren)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                {
                    if (destroyInsteadOfDeactivate)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }
        
        if (destroyInsteadOfDeactivate)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public void SetDeactivationTime(float newTime)
    {
        deactivationTime = newTime;
    }
    
    public float GetRemainingTime()
    {
        if (deactivationCoroutine != null)
        {
            return deactivationTime;
        }
        return 0f;
    }
    
    public bool IsTimerActive()
    {
        return deactivationCoroutine != null && isActive;
    }
    
    public void ReactivateAndRestart()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        
        isActive = true;
        StartDeactivationTimer();
    }
    
    void OnDisable()
    {
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
            deactivationCoroutine = null;
        }
    }
}
