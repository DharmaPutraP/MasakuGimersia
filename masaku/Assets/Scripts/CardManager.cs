using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    
    [Header("Card Setup")]
    public List<ActionCard> allActionCards; // Assign di Inspector
    
    [Header("Player Reference")]
    public PlayerMovement playerMovement;
    
    [Header("Game State")]
    public int currentFocus = 3; // Fokus awal player
    public int maxFocus = 5;
    
    private List<ActionCard> deck = new List<ActionCard>();
    private List<ActionCard> hand = new List<ActionCard>();
    private List<ActionCard> discardPile = new List<ActionCard>();
    
    [Header("Hand Settings")]
    public int maxHandSize = 5;
    public int startingHandSize = 3;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        InitializeDeck();
        DrawCards(startingHandSize);
    }
    
    // Inisialisasi deck dengan 10 kartu tetap
    void InitializeDeck()
    {
        deck.Clear();
        
        // Tambahkan kartu sesuai spesifikasi (2x masing-masing)
        foreach (ActionCard card in allActionCards)
        {
            deck.Add(card);
            deck.Add(card); // Tambah duplikat untuk 2x
        }
        
        ShuffleDeck();
    }
    
    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            ActionCard temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
    
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (hand.Count >= maxHandSize)
            {
                Debug.Log("Tangan sudah penuh!");
                break;
            }
            
            if (deck.Count == 0)
            {
                ReshuffleDiscardPile();
            }
            
            if (deck.Count > 0)
            {
                ActionCard drawnCard = deck[0];
                deck.RemoveAt(0);
                hand.Add(drawnCard);
                Debug.Log($"Tarik kartu: {drawnCard.cardName}");
            }
        }
    }
    
    void ReshuffleDiscardPile()
    {
        if (discardPile.Count > 0)
        {
            Debug.Log("Mengocok ulang discard pile ke deck");
            deck.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }
    }
    
    // Method utama untuk memainkan kartu
    public void PlayCard(ActionCard card)
    {
        if (!hand.Contains(card))
        {
            Debug.LogWarning("Kartu tidak ada di tangan!");
            return;
        }
        
        // Cek apakah kartu spesial (Tarik Nafas)
        if (card.isSpecialCard)
        {
            ExecuteSpecialCard(card);
        }
        else
        {
            // Cek fokus untuk kartu biasa
            if (currentFocus < card.focusCost)
            {
                Debug.LogWarning("Fokus tidak cukup!");
                return;
            }
            
            // Kurangi fokus
            currentFocus -= card.focusCost;
            
            // Jalankan aksi kartu
            ExecuteCardAction(card);
        }
        
        // Pindahkan kartu ke discard pile
        hand.Remove(card);
        discardPile.Add(card);
        
        Debug.Log($"Memainkan kartu: {card.cardName}");
    }
    
    void ExecuteCardAction(ActionCard card)
    {
        // Cari lokasi berdasarkan tag
        GameObject targetLocation = GameObject.FindGameObjectWithTag(card.targetTag);
        
        if (targetLocation != null)
        {
            // Gerakkan player ke lokasi
            if (playerMovement != null)
            {
                playerMovement.MoveToLocation(targetLocation.transform.position, card);
            }
            else
            {
                Debug.LogError("PlayerMovement tidak ditemukan!");
            }
        }
        else
        {
            Debug.LogError($"Lokasi dengan tag '{card.targetTag}' tidak ditemukan!");
        }
    }
    
    void ExecuteSpecialCard(ActionCard card)
    {
        // Tarik Nafas: Buang 1 kartu, tarik 1 kartu
        if (card.discardCount > 0 && hand.Count > 1)
        {
            // Buang kartu pertama dari tangan (selain kartu Tarik Nafas yang dimainkan)
            ActionCard cardToDiscard = hand.FirstOrDefault(c => c != card);
            if (cardToDiscard != null)
            {
                hand.Remove(cardToDiscard);
                discardPile.Add(cardToDiscard);
                Debug.Log($"Membuang kartu: {cardToDiscard.cardName}");
            }
        }
        
        // Tarik kartu baru
        if (card.drawCount > 0)
        {
            DrawCards(card.drawCount);
        }
    }
    
    // Method untuk UI atau input player
    public void PlayCardByIndex(int handIndex)
    {
        if (handIndex >= 0 && handIndex < hand.Count)
        {
            PlayCard(hand[handIndex]);
        }
    }
    
    public List<ActionCard> GetHand()
    {
        return new List<ActionCard>(hand);
    }
    
    public void AddFocus(int amount)
    {
        currentFocus = Mathf.Min(currentFocus + amount, maxFocus);
    }
    
    public void ResetFocus()
    {
        currentFocus = maxFocus;
    }
}
