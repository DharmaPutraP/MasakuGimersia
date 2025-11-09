using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MasakuCardManager : MonoBehaviour
{
    public static MasakuCardManager Instance;
    
    [Header("Card Setup")]
    public List<ActionCard> baseActionCards; // 5 kartu dasar
    public ActionCard curseCard; // Kartu kutukan Barbarian
    
    private List<ActionCard> deck = new List<ActionCard>();
    private List<ActionCard> hand = new List<ActionCard>();
    private List<ActionCard> discardPile = new List<ActionCard>();
    
    [Header("Hand Settings")]
    public int maxHandSize = 5;
    
    [Header("Player Reference")]
    public PlayerMovement playerMovement;
    
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
    }
    
    // Inisialisasi deck dengan 10 kartu tetap (2x masing-masing dari 5 kartu)
    public void InitializeDeck()
    {
        deck.Clear();
        discardPile.Clear();
        hand.Clear();
        
        // Tambahkan 2x setiap kartu dasar
        foreach (ActionCard card in baseActionCards)
        {
            deck.Add(card);
            deck.Add(card);
        }
        
        ShuffleDeck();
        Debug.Log($"Deck diinisialisasi: {deck.Count} kartu");
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
    
    public void DrawToHandSize()
    {
        int cardsToDraw = maxHandSize - hand.Count;
        DrawCards(cardsToDraw);
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
            else
            {
                Debug.Log("Deck dan discard pile kosong!");
                break;
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
    
    public bool PlayCard(ActionCard card)
    {
        if (!hand.Contains(card))
        {
            Debug.LogWarning("Kartu tidak ada di tangan!");
            return false;
        }
        
        // Cek apakah kartu spesial (Tarik Nafas)
        if (card.isSpecialCard)
        {
            ExecuteSpecialCard(card);
            hand.Remove(card);
            discardPile.Add(card);
            return true;
        }
        
        // Cek fokus untuk kartu biasa
        if (!GameManager.Instance.SpendFocus(card.focusCost))
        {
            Debug.LogWarning($"Fokus tidak cukup! Butuh {card.focusCost}, punya {GameManager.Instance.currentFocus}");
            return false;
        }
        
        // NEW: Move player to target location if tag exists
        if (!string.IsNullOrEmpty(card.targetTag) && playerMovement != null)
        {
            Vector3 targetPos = KitchenLocationManager.Instance.GetLocationPosition(card.targetTag);
            if (targetPos != Vector3.zero)
            {
                playerMovement.MoveToLocation(targetPos, card);
            }
        }
        
        // Tambahkan kartu ke preparation station
        GameManager.Instance.AddCardToPreparation(card.cardType);
        
        // Pindahkan kartu ke discard pile
        hand.Remove(card);
        discardPile.Add(card);
        
        Debug.Log($"Memainkan kartu: {card.cardName}");
        return true;
    }
    
    void ExecuteSpecialCard(ActionCard card)
    {
        Debug.Log($"Memainkan kartu spesial: {card.cardName}");
        
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
    
    public void PlayCardByIndex(int handIndex)
    {
        if (handIndex >= 0 && handIndex < hand.Count)
        {
            PlayCard(hand[handIndex]);
        }
    }
    
    public void DiscardHand()
    {
        while (hand.Count > 0)
        {
            ActionCard card = hand[0];
            hand.RemoveAt(0);
            discardPile.Add(card);
        }
        Debug.Log("Semua kartu di tangan dibuang");
    }
    
    public void AddCurseToDiscard()
    {
        if (curseCard != null)
        {
            discardPile.Add(curseCard);
            Debug.Log("Kartu CURSE ditambahkan ke discard pile!");
        }
    }
    
    public void AddCardToDeck(ActionCard card)
    {
        if (card != null)
        {
            deck.Add(card);
            Debug.Log($"{card.cardName} ditambahkan ke deck!");
        }
    }
    
    public void ResetDeck()
    {
        InitializeDeck();
    }
    
    public List<ActionCard> GetHand()
    {
        return new List<ActionCard>(hand);
    }
    
    public int GetDeckCount()
    {
        return deck.Count;
    }
    
    public int GetDiscardCount()
    {
        return discardPile.Count;
    }
}
