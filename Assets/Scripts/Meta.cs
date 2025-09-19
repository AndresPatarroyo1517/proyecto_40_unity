using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Meta : MonoBehaviour
{
    [Header("Goal Settings")]
    public int requiredCoins = 50;
    
    [Header("Trigger Settings")]
    public GameObject triggerObject;
    
    [Header("Visual Feedback")]
    public GameObject activatedEffect;
    
    private PlayerController playerController;
    private GameManager gameManager;
    private bool isActivated = false;
    
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
        
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found in scene");
            enabled = false;
            return;
        }
        
        if (activatedEffect != null)
        {
            activatedEffect.SetActive(false);
        }
        
        if (triggerObject != null)
        {
            MetaTrigger trigger = triggerObject.GetComponent<MetaTrigger>();
            if (trigger == null)
            {
                trigger = triggerObject.AddComponent<MetaTrigger>();
            }
            trigger.meta = this;
        }
    }
    
    void Update()
    {
        CheckPortalActivation();
    }
    
    void CheckPortalActivation()
    {
        if (playerController == null) return;
        
        bool shouldBeActivated = playerController.CoinCount >= requiredCoins;
        
        if (shouldBeActivated && !isActivated)
        {
            ActivatePortal();
        }
        else if (!shouldBeActivated && isActivated)
        {
            DeactivatePortal();
        }
    }
    
    void ActivatePortal()
    {
        isActivated = true;
        
        if (activatedEffect != null)
        {
            activatedEffect.SetActive(true);
        }
        
        Debug.Log($"¡Portal activado! El jugador tiene {playerController.CoinCount}/{requiredCoins} monedas");
    }
    
    void DeactivatePortal()
    {
        isActivated = false;
        
        if (activatedEffect != null)
        {
            activatedEffect.SetActive(false);
        }
        
        Debug.Log($"Portal desactivado. El jugador tiene {playerController.CoinCount}/{requiredCoins} monedas");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (triggerObject == null)
        {
            HandleTrigger(other);
        }
    }
    
    public void OnTriggerObjectEnter(Collider other)
    {
        HandleTrigger(other);
    }
    
    void HandleTrigger(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isActivated)
            {
                TriggerGoal();
            }
            else
            {
                int missingCoins = requiredCoins - playerController.CoinCount;
                Debug.Log($"Portal inactivo. Necesitas {missingCoins} monedas más para activar el portal ({playerController.CoinCount}/{requiredCoins})");
            }
        }
    }
    
    void TriggerGoal()
    {
        Debug.Log("¡Portal activado! Pasando al siguiente nivel...");
        StartCoroutine(CompleteLevel());
    }
    
    IEnumerator CompleteLevel()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        yield return new WaitForSeconds(1f);
        
        if (gameManager != null)
        {
            gameManager.TriggerNormalWin();
        }
        else
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
}

public class MetaTrigger : MonoBehaviour
{
    [System.NonSerialized]
    public Meta meta;
    
    void OnTriggerEnter(Collider other)
    {
        if (meta != null)
        {
            meta.OnTriggerObjectEnter(other);
        }
    }
}