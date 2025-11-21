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
    
    public bool isSpecialCard;
    public int discardCount;
    public int drawCount;
    public int focusGrant; // Focus yang diberikan saat dimainkan (untuk Wizard Boon Focus)
    
    public bool isCurseCard;
    
    public bool isBoonCard;
}
