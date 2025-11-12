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
    
    [Header("Card Selection")]
    private List<int> selectedIndices = new List<int>(); // Track by hand index instead of card reference
    
    [Header("Curse Mechanic")]
    private bool hasPermanentCurse = false; // Barbarian curse active for this day
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        // Initialize deck in Awake to ensure it's ready before other scripts use it
        InitializeDeck();
    }
    
    void Start()
    {
        // Deck already initialized in Awake
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
        SortHand(); // Sort hand after drawing
    }
    
    public void DrawCards(int count, bool allowExceedMax = false)
    {
        // If permanent curse is active but not in hand, add it first (only once)
        if (hasPermanentCurse && curseCard != null)
        {
            // Count how many curse cards are already in hand
            int curseCount = hand.Count(c => c == curseCard);
            
            if (curseCount == 0 && hand.Count < maxHandSize)
            {
                hand.Add(curseCard);
                Debug.Log("Curse card automatically added to hand (permanent effect)");
            }
            else if (curseCount > 1)
            {
                // Remove duplicates if somehow they exist
                Debug.LogWarning($"Found {curseCount} curse cards in hand! Removing duplicates...");
                while (hand.Count(c => c == curseCard) > 1)
                {
                    int duplicateIndex = hand.FindLastIndex(c => c == curseCard);
                    hand.RemoveAt(duplicateIndex);
                }
            }
        }
        
        for (int i = 0; i < count; i++)
        {
            // Only check max hand size if not allowed to exceed
            if (!allowExceedMax && hand.Count >= maxHandSize)
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
                
                // Don't draw curse card if permanent curse is active (it's already in hand)
                if (hasPermanentCurse && drawnCard == curseCard)
                {
                    Debug.Log("Skipping curse card draw (already permanent in hand)");
                    i--; // Don't count this as a drawn card
                    continue;
                }
                
                hand.Add(drawnCard);
                Debug.Log($"Tarik kartu: {drawnCard.cardName}");
            }
            else
            {
                Debug.Log("Deck dan discard pile kosong!");
                break;
            }
        }
        
        SortHand(); // Sort hand after drawing cards
    }
    
    void SortHand()
    {
        // Sort by CardType enum order
        hand.Sort((a, b) => a.cardType.CompareTo(b.cardType));
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
    
    // NEW: Select card for combo (doesn't execute immediately)
    public bool SelectCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning("Index kartu tidak valid!");
            return false;
        }
        
        ActionCard card = hand[handIndex];
        
        // Prevent selecting curse cards
        if (card.isCurseCard)
        {
            Debug.LogWarning("Cannot select curse cards!");
            return false;
        }
        
        // Check if already selected
        if (selectedIndices.Contains(handIndex))
        {
            Debug.LogWarning("Kartu sudah dipilih!");
            return false;
        }
        
        // Check if special card (like Tarik Nafas - these have their own flow)
        // BUT allow boon cards to be selected normally
        if (card.isSpecialCard && !card.isBoonCard)
        {
            Debug.LogWarning("Kartu spesial tidak bisa digunakan untuk combo!");
            return false;
        }
        
        // Boon cards can be selected (will be played via submit button)
        // They don't cost focus
        if (card.isBoonCard)
        {
            selectedIndices.Add(handIndex);
            Debug.Log($"Boon card selected: {card.cardName}");
            return true;
        }
        
        // Check focus
        int totalFocusCost = card.focusCost;
        foreach (int idx in selectedIndices)
        {
            totalFocusCost += hand[idx].focusCost;
        }
        
        if (totalFocusCost > GameManager.Instance.currentFocus)
        {
            Debug.LogWarning($"Fokus tidak cukup! Total butuh {totalFocusCost}, punya {GameManager.Instance.currentFocus}");
            return false;
        }
        
        // Add to selection
        selectedIndices.Add(handIndex);
        
        Debug.Log($"Kartu dipilih: {card.cardName} (Index: {handIndex}, Total: {selectedIndices.Count})");
        return true;
    }
    
    // NEW: Deselect card from combo
    public bool DeselectCard(int handIndex)
    {
        if (selectedIndices.Contains(handIndex))
        {
            selectedIndices.Remove(handIndex);
            Debug.Log($"Kartu dibatalkan: {hand[handIndex].cardName}");
            return true;
        }
        return false;
    }
    
    // NEW: Clear all selections
    public void ClearSelection()
    {
        selectedIndices.Clear();
        Debug.Log("Semua pilihan kartu dibatalkan");
    }
    
    // NEW: Get selected cards
    public List<ActionCard> GetSelectedCards()
    {
        List<ActionCard> selected = new List<ActionCard>();
        foreach (int idx in selectedIndices)
        {
            if (idx >= 0 && idx < hand.Count)
            {
                selected.Add(hand[idx]);
            }
        }
        return selected;
    }
    
    // NEW: Check if card is selected by index
    public bool IsCardSelected(int handIndex)
    {
        return selectedIndices.Contains(handIndex);
    }
    
    // NEW: Execute all selected cards at once
    public IEnumerator ExecuteSelectedCards()
    {
        if (selectedIndices.Count == 0)
        {
            Debug.LogWarning("Tidak ada kartu yang dipilih!");
            yield break;
        }
        
        Debug.Log($"=== Mulai eksekusi {selectedIndices.Count} kartu ===");
        
        // Get cards by indices and sort indices in descending order (to remove from hand correctly)
        List<ActionCard> cardsToExecute = new List<ActionCard>();
        List<int> sortedIndices = new List<int>(selectedIndices);
        sortedIndices.Sort();
        
        foreach (int idx in sortedIndices)
        {
            if (idx >= 0 && idx < hand.Count)
            {
                ActionCard card = hand[idx];
                
                // Skip curse cards (safety check)
                if (card.isCurseCard)
                {
                    Debug.LogWarning($"Skipping curse card at index {idx}");
                    continue;
                }
                
                cardsToExecute.Add(card);
            }
        }
        
        // Calculate and spend total focus
        int totalFocus = 0;
        foreach (ActionCard card in cardsToExecute)
        {
            totalFocus += card.focusCost;
        }
        
        if (!GameManager.Instance.SpendFocus(totalFocus))
        {
            Debug.LogWarning("Fokus tidak cukup!");
            yield break;
        }
        
        // Execute each card sequentially
        foreach (ActionCard card in cardsToExecute)
        {
            // Move player to location if tag exists
            if (!string.IsNullOrEmpty(card.targetTag) && playerMovement != null)
            {
                Vector3 targetPos = KitchenLocationManager.Instance.GetLocationPosition(card.targetTag);
                if (targetPos != Vector3.zero)
                {
                    playerMovement.MoveToLocation(targetPos, card);
                    
                    // Wait for movement to complete
                    while (playerMovement.IsMoving())
                    {
                        yield return null;
                    }
                }
            }
            
            // Add card to preparation station
            GameManager.Instance.AddCardToPreparation(card.cardType);
            
            Debug.Log($"Kartu dieksekusi: {card.cardName}");
        }
        
        // After all cards executed, move to serving counter
        if (playerMovement != null)
        {
            Vector3 servingPos = KitchenLocationManager.Instance.GetLocationPosition("ServingCounter");
            if (servingPos != Vector3.zero)
            {
                Debug.Log("→ Bergerak ke Serving Counter untuk menyajikan pesanan");
                
                // Create a dummy card for serving counter movement
                ActionCard servingCard = ScriptableObject.CreateInstance<ActionCard>();
                servingCard.targetTag = "ServingCounter";
                servingCard.pickupTag = "";
                
                playerMovement.MoveToLocation(servingPos, servingCard);
                
                // Wait for movement to serving counter to complete
                while (playerMovement.IsMoving())
                {
                    yield return null;
                }
                
                Debug.Log("✓ Sampai di Serving Counter!");
            }
            else
            {
                Debug.LogWarning("ServingCounter location not found!");
            }
        }
        
        // Move cards from hand to discard pile (remove in reverse order to preserve indices)
        // But protect curse cards from being removed
        sortedIndices.Sort((a, b) => b.CompareTo(a)); // Sort descending
        foreach (int idx in sortedIndices)
        {
            if (idx >= 0 && idx < hand.Count)
            {
                ActionCard card = hand[idx];
                
                // Don't remove curse cards from hand
                if (hasPermanentCurse && card == curseCard)
                {
                    Debug.Log("Protecting curse card from removal");
                    continue;
                }
                
                hand.RemoveAt(idx);
                discardPile.Add(card);
            }
        }
        
        Debug.Log("=== Semua kartu selesai dieksekusi ===");
        
        // Clear selection
        selectedIndices.Clear();
    }
    
    public bool PlayCard(ActionCard card)
    {
        if (!hand.Contains(card))
        {
            Debug.LogWarning("Kartu tidak ada di tangan!");
            return false;
        }
        
        // Cek apakah kartu spesial (Tarik Nafas, Wizard Boon)
        if (card.isSpecialCard)
        {
            ExecuteSpecialCard(card);
            hand.Remove(card);
            
            // Boon cards disappear after use (don't go to discard)
            if (card.isBoonCard)
            {
                Debug.Log($"{card.cardName} digunakan dan hilang (one-time use)");
                // Card is removed from game completely
            }
            else
            {
                discardPile.Add(card);
            }
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
        
        // Grant focus if card has it (Wizard Boon Focus)
        if (card.focusGrant > 0)
        {
            GameManager.Instance.AddFocus(card.focusGrant);
            Debug.Log($"+{card.focusGrant} Focus dari {card.cardName}!");
        }
        
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
        
        // Tarik kartu baru (Wizard Boon Draw or Tarik Nafas)
        if (card.drawCount > 0)
        {
            // Boon cards can exceed max hand size
            bool canExceedMax = card.isBoonCard;
            DrawCards(card.drawCount, canExceedMax);
            
            if (canExceedMax)
            {
                Debug.Log($"Wizard Boon: Drew {card.drawCount} cards (can exceed hand limit)");
            }
        }
    }
    
    // New method for player-controlled Tarik Nafas
    public bool PlayTarikNafas(int tarikNafasIndex, int discardIndex)
    {
        if (tarikNafasIndex < 0 || tarikNafasIndex >= hand.Count)
        {
            Debug.LogError("Invalid Tarik Nafas card index!");
            return false;
        }
        
        if (discardIndex < 0 || discardIndex >= hand.Count)
        {
            Debug.LogError("Invalid discard card index!");
            return false;
        }
        
        if (tarikNafasIndex == discardIndex)
        {
            Debug.LogWarning("Cannot discard the Tarik Nafas card itself! Choose another card.");
            return false;
        }
        
        ActionCard tarikNafasCard = hand[tarikNafasIndex];
        ActionCard discardCard = hand[discardIndex];
        
        if (!tarikNafasCard.isSpecialCard)
        {
            Debug.LogError("Selected card is not Tarik Nafas!");
            return false;
        }
        
        Debug.Log($"Playing Tarik Nafas: Discarding {discardCard.cardName}");
        
        // Remove the discard card from hand
        hand.Remove(discardCard);
        discardPile.Add(discardCard);
        
        // Remove Tarik Nafas card from hand
        hand.Remove(tarikNafasCard);
        discardPile.Add(tarikNafasCard);
        
        // Draw new card(s)
        if (tarikNafasCard.drawCount > 0)
        {
            DrawCards(tarikNafasCard.drawCount);
        }
        
        Debug.Log($"Tarik Nafas complete! Discarded {discardCard.cardName}, drew {tarikNafasCard.drawCount} new card(s)");
        return true;
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
        // Create a copy to iterate (avoid modification during iteration)
        List<ActionCard> cardsToDiscard = new List<ActionCard>(hand);
        
        foreach (ActionCard card in cardsToDiscard)
        {
            // Keep curse card in hand if permanent curse is active
            if (hasPermanentCurse && card == curseCard)
            {
                Debug.Log("Curse card stays in hand (permanent effect)");
                continue; // Skip discarding the curse
            }
            
            hand.Remove(card);
            discardPile.Add(card);
        }
        
        Debug.Log($"Kartu dibuang. Curse tetap di tangan: {hasPermanentCurse}");
    }
    
    public void AddCurseToDiscard()
    {
        if (curseCard != null)
        {
            // NEW: Add curse permanently to hand instead of discard pile
            hasPermanentCurse = true;
            
            // Add to hand immediately if there's space
            if (hand.Count < maxHandSize)
            {
                hand.Add(curseCard);
                Debug.Log("Kartu CURSE ditambahkan ke tangan Anda secara permanen untuk hari ini!");
            }
            else
            {
                Debug.Log("Tangan penuh! Curse akan ditambahkan saat ada ruang.");
            }
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
        RemoveCurse(); // Remove curse when day resets
        RemoveBoonCards(); // Remove all boon cards when day resets
    }
    
    public void RemoveCurse()
    {
        if (hasPermanentCurse && curseCard != null)
        {
            hasPermanentCurse = false;
            
            // Remove curse from hand
            if (hand.Contains(curseCard))
            {
                hand.Remove(curseCard);
                Debug.Log("Curse removed from hand (new day)");
            }
            
            // Remove curse from deck if somehow there
            while (deck.Contains(curseCard))
            {
                deck.Remove(curseCard);
            }
            
            // Remove curse from discard pile
            while (discardPile.Contains(curseCard))
            {
                discardPile.Remove(curseCard);
            }
            
            Debug.Log("Barbarian curse effect removed - new day!");
        }
    }
    
    public void RemoveBoonCards()
    {
        // Remove all boon cards from hand
        int removedFromHand = hand.RemoveAll(c => c.isBoonCard);
        
        // Remove all boon cards from deck
        int removedFromDeck = deck.RemoveAll(c => c.isBoonCard);
        
        // Remove all boon cards from discard pile
        int removedFromDiscard = discardPile.RemoveAll(c => c.isBoonCard);
        
        int totalRemoved = removedFromHand + removedFromDeck + removedFromDiscard;
        
        if (totalRemoved > 0)
        {
            Debug.Log($"Removed {totalRemoved} unused boon card(s) - new day!");
        }
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
