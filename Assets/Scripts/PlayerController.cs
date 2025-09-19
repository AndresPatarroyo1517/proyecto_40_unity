using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float baseSpeed = 5f;
    public float jumpForce = 3f;
    public float maxVelocity = 10f;
    
    [Header("Piso")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    [Header("UI")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI powerUpText;
    
    [Header("Efectos")]
    public Transform particleTransform;
    public Material invincibilityMaterial;
    
    [Header("Ajustes")]
    public int coinsToWin = 50;
    public int nextSceneIndex = 1;
    
    private Rigidbody rb;
    private AudioSource audioSource;
    private ParticleSystem particleSystem;
    private Renderer playerRenderer;
    private Material originalMaterial;
    
    private DeathManager deathManager;
    
    private int coinCount = 0;
    private bool isGrounded;
    private float currentSpeed;
    private int coinMultiplier = 1;
    
    private List<ActivePowerUp> activePowerUps = new List<ActivePowerUp>();
    private Dictionary<PowerUpType, bool> powerUpStates = new Dictionary<PowerUpType, bool>();
    
    private bool uiNeedsUpdate = true;
    private bool powerUpUINeedsUpdate = false;

    public int CoinCount => coinCount;
    public int CoinsToWin => coinsToWin;
    
    private GameManager gameManager;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        playerRenderer = GetComponent<Renderer>();
        gameManager = FindObjectOfType<GameManager>();
	deathManager = FindObjectOfType<DeathManager>();
        
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
        CheckGrounded();
        HandleJumpInput();
        UpdatePowerUps();
        
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
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
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
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        if (horizontalVelocity.magnitude < maxVelocity)
        {
            rb.AddForce(movement);
        }
    }
    
    void Jump()
    {
        Vector3 velocity = rb.velocity;
        velocity.y = 0;
        rb.velocity = velocity;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    void UpdatePowerUps()
    {
        bool powerUpChanged = false;
        
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            activePowerUps[i].remainingTime -= Time.deltaTime;
            
            if (activePowerUps[i].remainingTime <= 0)
            {
                RemovePowerUp(activePowerUps[i].type);
                activePowerUps.RemoveAt(i);
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
       
        RemovePowerUpOfType(powerUpData.type);
        
        ActivePowerUp newPowerUp = new ActivePowerUp(
            powerUpData.type, 
            powerUpData.duration, 
            powerUpData.multiplier
        );
        
        activePowerUps.Add(newPowerUp);
        powerUpStates[powerUpData.type] = true;
        
        ApplyPowerUpEffects(powerUpData);
        powerUpUINeedsUpdate = true;
       
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
                break;
                
            case PowerUpType.DoubleCoins:
                coinMultiplier = Mathf.RoundToInt(powerUp.multiplier);
                Debug.Log($"Coin multiplier activado: x{coinMultiplier} por {powerUp.duration}s");
                break;
                
            case PowerUpType.InstantWin:
                InstantWin();
                break;
        }
    }
    
    void RemovePowerUpOfType(PowerUpType type)
    {
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            if (activePowerUps[i].type == type)
            {
                activePowerUps.RemoveAt(i);
            }
        }
        
        powerUpStates[type] = false;
        RemovePowerUpEffect(type);
    }
    
    void RemovePowerUp(PowerUpType type)
    {
        powerUpStates[type] = false;
        RemovePowerUpEffect(type);
    }
    
    void RemovePowerUpEffect(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Speed:
                currentSpeed = baseSpeed;
                Debug.Log("Speed power-up terminado");
                break;
                
            case PowerUpType.Invincibility:
                if (playerRenderer != null && originalMaterial != null)
                    playerRenderer.material = originalMaterial;
                Debug.Log("Invincibility power-up terminado");
                break;
                
            case PowerUpType.DoubleCoins:
                coinMultiplier = 1;
                Debug.Log("Double coins power-up terminado - multiplicador reset a x1");
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
	else if (other.CompareTag("Trap")) 
    {
        if (deathManager != null && !HasPowerUp(PowerUpType.Invincibility))
        {
            deathManager.TriggerDeath("¡Tocaste una trampa!");
        }
        else if (HasPowerUp(PowerUpType.Invincibility))
        {
            Debug.Log("¡Invencibilidad te protegió de la trampa!");
        }
    }
    }
    
    void CollectCoin(GameObject coin)
    {
        if (audioSource != null)
            audioSource.Play();
        
        int coinsToAdd = coinMultiplier;
        coinCount += coinsToAdd;
        
        ShowParticleEffect(coin.transform.position);
        coin.SetActive(false);
        uiNeedsUpdate = true;
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
        if (gameManager != null)
        {
            gameManager.UpdateCoinUI(coinCount, coinsToWin);
        }
        else if (coinText != null)
        {
            coinText.text = $"Monedas: {coinCount}/{coinsToWin}";
        }
    }
    
    void UpdatePowerUpUI()
    {
        if (gameManager != null)
        {
            gameManager.UpdatePowerUpUI(activePowerUps);
        }
        else if (powerUpText != null)
        {
            if (activePowerUps.Count == 0)
            {
                powerUpText.text = "";
                return;
            }
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var powerUp in activePowerUps)
            {
                string powerUpName = GetPowerUpDisplayName(powerUp.type);
                sb.AppendLine($"{powerUpName}: {powerUp.remainingTime:F1}s");
            }
            powerUpText.text = sb.ToString();
        }
    }
    
    string GetPowerUpDisplayName(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Speed: return "Velocidad";
            case PowerUpType.Invincibility: return "Invencibilidad";
            case PowerUpType.Jump: return "Super Salto";
            case PowerUpType.DoubleCoins: return $"Monedas x{coinMultiplier}";
            case PowerUpType.InstantWin: return "Victoria Instant";
            default: return type.ToString();
        }
    }
    
    public void LoadNextLevel()
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
        StartCoroutine(WinSequence("¡Power-up de Victoria Secreta!"));
    }
    
    void NormalWin()
    {
        Debug.Log("¡Todas las monedas recolectadas!");
        StartCoroutine(WinSequence("¡Nivel Completado!"));
    }
    
    private IEnumerator WinSequence(string winMessage)
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