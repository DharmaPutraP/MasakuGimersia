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
    
    [Header("Patience Settings")]
    public int maxPatience;
    public int patienceDecayPerTurn;
    
    [Header("Order Requirements")]
    public List<CardType> requiredCards; // Kombo kartu yang dibutuhkan
    
    [Header("Rewards")]
    public int focusReward; // Fokus yang didapat saat pesanan selesai
    
    [Header("Special Traits")]
    public bool isBarbarian; // Memberikan curse jika marah
    public bool isWizard; // Memberikan boon jika dilayani cepat
    
    [TextArea(3, 5)]
    public string orderDescription;
}
