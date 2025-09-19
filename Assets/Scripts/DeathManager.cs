using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathManager : MonoBehaviour
{
    [Header("Death Settings")]
    public float fallThreshold = -10f; // Y position below which player dies
    public float deathDelay = 2f; // Time before respawn
    public bool showDeathMessage = true;
    
    [Header("UI References")]
    public TextMeshProUGUI deathMessageText;
    
    [Header("Audio")]
    public AudioClip deathSound;
    
    private PlayerController playerController;
    private AudioSource audioSource;
    private bool isDead = false;
    
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found! DeathManager needs a PlayerController in the scene.");
        }
        
        if (deathMessageText != null)
        {
            deathMessageText.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        CheckFallDeath();
    }
    
    void CheckFallDeath()
    {
        if (playerController != null && !isDead)
        {
            if (playerController.transform.position.y < fallThreshold)
            {
                TriggerDeath("¡Te has caído!");
            }
        }
    }
    
    public void TriggerDeath(string deathMessage = "¡Has muerto!")
    {
        if (isDead) return; 
        
        isDead = true;
        
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        if (showDeathMessage)
        {
            StartCoroutine(ShowDeathSequence(deathMessage));
        }
        else
        {
            StartCoroutine(RestartAfterDelay());
        }
    }
    
    IEnumerator ShowDeathSequence(string message)
    {
        if (deathMessageText != null)
        {
            deathMessageText.gameObject.SetActive(true);
            deathMessageText.text = message;
        }
        
        float countdown = deathDelay;
        while (countdown > 0)
        {
            if (deathMessageText != null)
            {
                deathMessageText.text = $"{message}\nReiniciando en: {countdown:F1}s";
            }
            countdown -= Time.deltaTime;
            yield return null;
        }
        
        RestartScene();
    }
    
    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        RestartScene();
    }
    
    void RestartScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        SceneManager.LoadScene(currentSceneIndex);
    }
    
    public void KillPlayer()
    {
        TriggerDeath("¡Tocaste una trampa!");
    }
    
    public void KillPlayer(string customMessage)
    {
        TriggerDeath(customMessage);
    }
}