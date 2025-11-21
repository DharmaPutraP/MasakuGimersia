using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum CustomerType
{
    Knight,
    Elf,
    Barbarian,
    Wizard
}

[CreateAssetMenu(fileName = "New Customer", menuName = "Masaku/Customer")]
public class Customer : ScriptableObject
{
    public string customerName;
    public CustomerType customerType;
    public Sprite customerSprite; 
    public GameObject customerPrefab; 
    
    [Header("Patience Settings")]
    public int maxPatience;
    public int patienceDecayPerTurn;
    
    [Header("Order Requirements")]
    public List<CardType> requiredCards; 
    
    [Header("Rewards")]
    public int focusReward;
    
    [Header("Special Traits")]
    public bool isBarbarian;
    public bool isWizard; 
    
    [TextArea(3, 5)]
    public string orderDescription;
}
