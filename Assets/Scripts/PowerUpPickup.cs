using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    public PowerUpData powerUpData;
    
    void Start()
    {
        if (powerUpData == null)
        {
            Debug.LogError($"PowerUpData no asignado en {gameObject.name}");
        }
    }
}