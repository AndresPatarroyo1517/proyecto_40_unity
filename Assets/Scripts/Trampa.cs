using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampa : MonoBehaviour
{
 [Header("Ajustes de trampas")]
    public string trapName = "Trampa";
    public bool destroyOnTouch = false;
    public float damageDelay = 0f;
    
    [Header("Audio")]
    public AudioClip trapSound;
    
    [Header("Efectos")]
    public GameObject deathEffect;
    
    private DeathManager deathManager;
    private AudioSource audioSource;
    private bool hasTriggered = false;
    
    void Start()
    {
        deathManager = FindObjectOfType<DeathManager>();
        audioSource = GetComponent<AudioSource>();
        
        if (deathManager == null)
        {
            Debug.LogError($"DeathManager not found! Trap '{trapName}' needs a DeathManager in the scene.");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            TriggerTrap(other);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            TriggerTrap(collision.collider);
        }
    }
    
    void TriggerTrap(Collider player)
    {
        Debug.Log($"Player hit trap: {trapName}");
        
        if (audioSource != null && trapSound != null)
        {
            audioSource.PlayOneShot(trapSound);
        }
        
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, player.transform.position, Quaternion.identity);
            Destroy(effect, 3f); // Clean up effect after 3 seconds
        }
        
        if (deathManager != null)
        {
            if (damageDelay > 0)
            {
                Invoke(nameof(KillPlayer), damageDelay);
            }
            else
            {
                KillPlayer();
            }
        }
        
        if (destroyOnTouch)
        {
            gameObject.SetActive(false);
        }
    }
    
    void KillPlayer()
    {
        if (deathManager != null)
        {
            deathManager.KillPlayer($"¡Tocaste {trapName}!");
        }
    }
}
