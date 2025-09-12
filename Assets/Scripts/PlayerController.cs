using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float baseSpeed = 5f;
    public float jumpForce = 10f;
    public float maxVelocity = 10f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI powerUpText;
    
    [Header("Effects")]
    public Transform particleTransform;
    public Material invincibilityMaterial;
    
    [Header("Game Settings")]
    public int coinsToWin = 12;
    public int nextSceneIndex = 1;
    
    private Rigidbody rb;
    private AudioSource audioSource;
    private ParticleSystem particleSystem;
    private Renderer playerRenderer;
    private Material originalMaterial;
    
    private int coinCount = 0;
    private bool isGrounded;
    private float currentSpeed;
    private int coinMultiplier = 1;
    
    // Optimización: Cache para reducir llamadas a Update
    private float groundCheckTimer = 0f;
    private const float GROUND_CHECK_INTERVAL = 0.1f;
    
    private List<ActivePowerUp> activePowerUps = new List<ActivePowerUp>();
    private Dictionary<PowerUpType, bool> powerUpStates = new Dictionary<PowerUpType, bool>();
    
    // Flags para actualización de UI solo cuando sea necesario
    private bool uiNeedsUpdate = true;
    private bool powerUpUINeedsUpdate = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        playerRenderer = GetComponent<Renderer>();
        
        if (particleTransform != null)
            particleSystem = particleTransform.GetComponent<ParticleSystem>();
        
        if (rb == null)
        {
            Debug.LogError("Rigidbody no encontrado en PlayerController");
            enabled = false;
            return;
        }
        
        currentSpeed = baseSpeed;
        originalMaterial = playerRenderer?.material;
        
        if (particleSystem != null)
            particleSystem.Stop();
        
        InitializePowerUpStates();
        UpdateUI();
    }
    
    void InitializePowerUpStates()
    {
        foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType)))
        {
            powerUpStates[type] = false;
        }
    }
    
    void Update()
    {
        // Optimización: Ground check solo cada GROUND_CHECK_INTERVAL segundos
        groundCheckTimer += Time.deltaTime;
        if (groundCheckTimer >= GROUND_CHECK_INTERVAL)
        {
            CheckGrounded();
            groundCheckTimer = 0f;
        }
        
        HandleJumpInput();
        UpdatePowerUps();
        
        // Solo actualizar UI cuando sea necesario
        if (uiNeedsUpdate)
        {
            UpdateUI();
            uiNeedsUpdate = false;
        }
        
        if (powerUpUINeedsUpdate)
        {
            UpdatePowerUpUI();
            powerUpUINeedsUpdate = false;
        }
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            // Optimización: Solo raycast si no hay groundCheck
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundMask);
        }
    }
    
    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || powerUpStates[PowerUpType.Jump]))
        {
            Jump();
        }
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal * currentSpeed, 0f, vertical * currentSpeed);
        
        // Optimización: Limitar velocidad máxima para evitar aceleración infinita
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVelocity.magnitude < maxVelocity)
        {
            rb.AddForce(movement);
        }
    }
    
    void Jump()
    {
        // Resetear solo velocidad Y para salto más consistente
        Vector3 velocity = rb.velocity;
        velocity.y = 0;
        rb.velocity = velocity;
        
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    void UpdatePowerUps()
    {
        bool powerUpChanged = false;
        
        // Optimización: Usar for normal y break cuando sea necesario
        for (int i = 0; i < activePowerUps.Count; i++)
        {
            activePowerUps[i].remainingTime -= Time.deltaTime;
            
            if (activePowerUps[i].remainingTime <= 0)
            {
                RemovePowerUp(activePowerUps[i].type);
                activePowerUps.RemoveAt(i);
                i--; // Ajustar índice después de RemoveAt
                powerUpChanged = true;
            }
        }
        
        if (powerUpChanged)
        {
            powerUpUINeedsUpdate = true;
        }
    }
    
    public void ApplyPowerUp(PowerUpData powerUpData)
    {
        if (powerUpData == null)
        {
            Debug.LogWarning("PowerUpData es null");
            return;
        }
        
        // Remover powerup existente del mismo tipo
        RemovePowerUp(powerUpData.type);
        
        ActivePowerUp newPowerUp = new ActivePowerUp(
            powerUpData.type, 
            powerUpData.duration, 
            powerUpData.multiplier
        );
        
        activePowerUps.Add(newPowerUp);
        powerUpStates[powerUpData.type] = true;
        
        ApplyPowerUpEffects(powerUpData);
        powerUpUINeedsUpdate = true;
        
        Debug.Log($"Power-up {powerUpData.displayName} aplicado por {powerUpData.duration}s");
    }
    
    void ApplyPowerUpEffects(PowerUpData powerUp)
    {
        switch (powerUp.type)
        {
            case PowerUpType.Speed:
                currentSpeed = baseSpeed * powerUp.multiplier;
                break;
                
            case PowerUpType.Invincibility:
                if (playerRenderer != null && invincibilityMaterial != null)
                    playerRenderer.material = invincibilityMaterial;
                break;
                
            case PowerUpType.Jump:
                // Power-up de salto múltiple - sin efectos inmediatos
                break;
                
            case PowerUpType.DoubleCoins:
                coinMultiplier = Mathf.RoundToInt(powerUp.multiplier);
                break;
        }
    }
    
    void RemovePowerUp(PowerUpType type)
    {
        // Remover de la lista activa
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            if (activePowerUps[i].type == type)
            {
                activePowerUps.RemoveAt(i);
                break;
            }
        }
        
        powerUpStates[type] = false;
        
        switch (type)
        {
            case PowerUpType.Speed:
                currentSpeed = baseSpeed;
                break;
                
            case PowerUpType.Invincibility:
                if (playerRenderer != null && originalMaterial != null)
                    playerRenderer.material = originalMaterial;
                break;
                
            case PowerUpType.DoubleCoins:
                coinMultiplier = 1;
                break;
        }
        
        powerUpUINeedsUpdate = true;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Recolectable"))
        {
            CollectCoin(other.gameObject);
        }
        else if (other.CompareTag("PowerUp"))
        {
            CollectPowerUp(other.gameObject);
        }
    }
    
    void CollectCoin(GameObject coin)
    {
        if (audioSource != null)
            audioSource.Play();
        
        coinCount += coinMultiplier;
        
        ShowParticleEffect(coin.transform.position);
        
        coin.SetActive(false);
        
        uiNeedsUpdate = true;
        
        if (coinCount >= coinsToWin)
        {
            LoadNextLevel();
        }
    }
    
    void CollectPowerUp(GameObject powerUpObj)
    {
        PowerUpPickup pickup = powerUpObj.GetComponent<PowerUpPickup>();
        if (pickup != null && pickup.powerUpData != null)
        {
            ApplyPowerUp(pickup.powerUpData);
            ShowParticleEffect(powerUpObj.transform.position);
            powerUpObj.SetActive(false);
        }
    }
    
    void ShowParticleEffect(Vector3 position)
    {
        if (particleSystem != null && particleTransform != null)
        {
            particleTransform.position = position;
            particleSystem.Play();
        }
    }
    
    void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = $"Monedas: {coinCount}/{coinsToWin}";
        }
    }
    
    void UpdatePowerUpUI()
    {
        if (powerUpText != null)
        {
            if (activePowerUps.Count == 0)
            {
                powerUpText.text = "";
                return;
            }
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var powerUp in activePowerUps)
            {
                sb.AppendLine($"{powerUp.type}: {powerUp.remainingTime:F1}s");
            }
            powerUpText.text = sb.ToString();
        }
    }
    
    void LoadNextLevel()
    {
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning($"Escena {nextSceneIndex} no existe en Build Settings");
        }
    }
    
    void InstantWin()
    {
        Debug.Log("¡Power-up de victoria instantánea activado!");
        
        // Mostrar mensaje en UI
        if (coinText != null)
        {
            coinText.text = "¡VICTORIA INSTANTÁNEA!";
        }
        
        if (powerUpText != null)
        {
            powerUpText.text = "¡Nivel Completado!";
        }
        
        // Iniciar corrutina para esperar 10 segundos
        StartCoroutine(WinSequence("¡Power-up de Victoria Secreta!"));
    }
    
    void NormalWin()
    {
        Debug.Log("¡Todas las monedas recolectadas!");
        
        // Mostrar mensaje en UI
        if (coinText != null)
        {
            coinText.text = $"¡COMPLETADO! {coinCount}/{coinsToWin}";
        }
        
        if (powerUpText != null)
        {
            powerUpText.text = "¡Todas las monedas recolectadas!";
        }
        
        // Iniciar corrutina para esperar 10 segundos
        StartCoroutine(WinSequence("¡Nivel Completado!"));
    }
    
    private System.Collections.IEnumerator WinSequence(string winMessage)
    {
        Debug.Log(winMessage);
        
        // Esperar 10 segundos
        float countdown = 10f;
        while (countdown > 0)
        {
            // Opcional: Mostrar countdown en UI
            if (powerUpText != null)
            {
                powerUpText.text = $"{winMessage}\nCambiando en: {countdown:F1}s";
            }
            
            countdown -= Time.deltaTime;
            yield return null;
        }
        
        // Cambiar de escena
        Debug.Log("Cambiando a la siguiente escena...");
        LoadNextLevel();
    }
    
    public bool HasPowerUp(PowerUpType type)
    {
        return powerUpStates.ContainsKey(type) && powerUpStates[type];
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}