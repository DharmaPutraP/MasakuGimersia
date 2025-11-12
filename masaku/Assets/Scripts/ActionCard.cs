using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum CardType
{
    PotongSayuran,
    PotongDaging,
    PanaskanAir,
    PanaskanDaging,
    TarikNafas,
    BarbarianCurse,
    WizardBoonDraw,
    WizardBoonFocus
}

[CreateAssetMenu(fileName = "New Action Card", menuName = "Cards/Action Card")]
public class ActionCard : ScriptableObject
{
    public string cardName;
    public CardType cardType;
    public int focusCost;
    
    [Header("Movement Targets")]
    public string pickupTag; // Tag untuk mengambil bahan (optional)
    public string targetTag; // Tag lokasi tujuan utama
    
    public Sprite cardImage;
    [TextArea(3, 5)]
    public string description;
    
    // Untuk kartu khusus seperti "Tarik Nafas"
    public bool isSpecialCard;
    public int discardCount;
    public int drawCount;
    public int focusGrant; // Focus yang diberikan saat dimainkan (untuk Wizard Boon Focus)
    
    // For curse cards that cannot be played
    public bool isCurseCard;
    
    // For boon cards (one-time use, disappear after use or at end of day)
    public bool isBoonCard;
}
