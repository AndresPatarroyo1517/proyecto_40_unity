using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaActiva : MonoBehaviour
{
    [Header("Trap Objects")]
    public GameObject trapObject; 
    public GameObject triggerZone; 
    
    [Header("Activation Settings")]
    public float activationDelay = 0.5f;
    public float trapDuration = 3f;
    public float trapSpeed = 10f; 
    public bool resetAfterUse = true; 
    
    [Header("Movement Settings")]
    public TrapMovementType movementType = TrapMovementType.None;
    public Vector3 moveDirection = Vector3.forward;
    public float moveDistance = 5f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio Sources")]
    public AudioSource activationAudioSource; 
    public AudioSource trapAudioSource;
    
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isActivated = false;
    private bool isActive = false;
    private Coroutine trapCoroutine;
    
    void Start()
    {
        if (trapObject != null)
        {
            originalPosition = trapObject.transform.position;
            trapObject.SetActive(false);
            
            targetPosition = originalPosition + (moveDirection.normalized * moveDistance);
        }
        
        if (triggerZone != null)
        {
            Collider triggerCollider = triggerZone.GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
            
            TrapTrigger trigger = triggerZone.GetComponent<TrapTrigger>();
            if (trigger == null)
            {
                trigger = triggerZone.AddComponent<TrapTrigger>();
            }
            trigger.parentTrap = this;
        }
    }
    
    public void OnPlayerEnterTrigger()
    {
        if (!isActivated && !isActive)
        {
            trapCoroutine = StartCoroutine(ActivateTrapSequence());
        }
    }
    
    IEnumerator ActivateTrapSequence()
    {
        isActivated = true;
        
        yield return new WaitForSeconds(activationDelay);
        
        ActivateTrap();
        
        yield return new WaitForSeconds(trapDuration);
        
        DeactivateTrap();
        
        if (resetAfterUse)
        {
            yield return new WaitForSeconds(1f);
            ResetTrap();
        }
    }
    
    void ActivateTrap()
    {
        isActive = true;
        
        if (trapObject != null)
        {
            trapObject.SetActive(true);
            
            if (activationAudioSource != null)
            {
                activationAudioSource.Play();
            }
            
            switch (movementType)
            {
                case TrapMovementType.Linear:
                    StartCoroutine(MoveLinear());
                    break;
                case TrapMovementType.Fall:
                    StartCoroutine(MoveFall());
                    break;
                case TrapMovementType.Swing:
                    StartCoroutine(MoveSwing());
                    break;
            }
        }
    }
    
    void DeactivateTrap()
    {
        isActive = false;
        
        // Reproducir sonido de la trampa
        if (trapAudioSource != null)
        {
            trapAudioSource.Play();
        }
    }
    
    void ResetTrap()
    {
        isActivated = false;
        isActive = false;
        
        if (trapObject != null)
        {
            trapObject.SetActive(false);
            trapObject.transform.position = originalPosition;
        }
        
        Debug.Log("Trampa reseteada y lista para reactivarse");
    }
    
    IEnumerator MoveLinear()
    {
        float elapsed = 0f;
        float duration = moveDistance / trapSpeed;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveValue = movementCurve.Evaluate(t);
            
            trapObject.transform.position = Vector3.Lerp(originalPosition, targetPosition, curveValue);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        trapObject.transform.position = targetPosition;
    }
    
    IEnumerator MoveFall()
    {
        Rigidbody rb = trapObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = trapObject.AddComponent<Rigidbody>();
        }
        
        rb.isKinematic = false;
        rb.AddForce(Vector3.down * trapSpeed, ForceMode.Impulse);
        
        yield return null;
    }
    
    IEnumerator MoveSwing()
    {
        float elapsed = 0f;
        float duration = 2f; // Duración del balanceo
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float swing = Mathf.Sin(t * Mathf.PI * 4) * moveDistance; // 4 oscilaciones completas
            
            Vector3 swingPosition = originalPosition + (moveDirection.normalized * swing);
            trapObject.transform.position = swingPosition;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        trapObject.transform.position = originalPosition;
    }
    
    public void ForceActivate()
    {
        if (!isActivated && !isActive)
        {
            trapCoroutine = StartCoroutine(ActivateTrapSequence());
        }
    }
    
    public void ForceReset()
    {
        if (trapCoroutine != null)
        {
            StopCoroutine(trapCoroutine);
        }
        
        ResetTrap();
    }
    
    void OnDrawGizmosSelected()
    {
        if (trapObject != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(trapObject.transform.position, Vector3.one);
            
            if (movementType != TrapMovementType.None)
            {
                Gizmos.color = Color.yellow;
                Vector3 target = trapObject.transform.position + (moveDirection.normalized * moveDistance);
                Gizmos.DrawLine(trapObject.transform.position, target);
                Gizmos.DrawWireCube(target, Vector3.one * 0.5f);
            }
        }
        
        if (triggerZone != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(triggerZone.transform.position, triggerZone.transform.localScale);
        }
    }
}

[System.Serializable]
public enum TrapMovementType
{
    None,       
    Linear,     
    Fall,       
    Swing       
}

public class TrapTrigger : MonoBehaviour
{
    [System.NonSerialized]
    public TrampaActiva parentTrap;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && parentTrap != null)
        {
            parentTrap.OnPlayerEnterTrigger();
        }
    }
}