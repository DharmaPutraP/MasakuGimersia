using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MasakuCardManager : MonoBehaviour
{
    public static MasakuCardManager Instance;
    
    [Header("Card Setup")]
    public List<ActionCard> baseActionCards; 
    public ActionCard curseCard; 
    
    private List<ActionCard> deck = new List<ActionCard>();
    private List<ActionCard> hand = new List<ActionCard>();
    private List<ActionCard> discardPile = new List<ActionCard>();
    
    [Header("Hand Settings")]
    public int maxHandSize = 5;
    
    [Header("Player Reference")]
    public PlayerMovement playerMovement;
    
    [Header("Card Selection")]
    private List<int> selectedIndices = new List<int>(); 
    
    [Header("Curse Mechanic")]
    private bool hasPermanentCurse = false; 
    
    private bool shouldPlayShuffleAnimation = false;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        InitializeDeck();
    }
    
    void Start()
    {
    }
    
    public void InitializeDeck()
    {
        deck.Clear();
        discardPile.Clear();
        hand.Clear();
        
        foreach (ActionCard card in baseActionCards)
        {
            deck.Add(card);
            deck.Add(card);
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
    
    public void DrawToHandSize()
    {
        int cardsToDraw = maxHandSize - hand.Count;
        DrawCards(cardsToDraw);
        SortHand(); 
    }
    
    public void DrawCards(int count, bool allowExceedMax = false)
    {
        if (hasPermanentCurse && curseCard != null)
        {
            int curseCount = hand.Count(c => c == curseCard);
            
            if (curseCount == 0 && hand.Count < maxHandSize)
            {
                hand.Add(curseCard);
            }
            else if (curseCount > 1)
            {
                while (hand.Count(c => c == curseCard) > 1)
                {
                    int duplicateIndex = hand.FindLastIndex(c => c == curseCard);
                    hand.RemoveAt(duplicateIndex);
                }
            }
        }
        
        for (int i = 0; i < count; i++)
        {
            if (!allowExceedMax && hand.Count >= maxHandSize)
            {
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
                
                if (hasPermanentCurse && drawnCard == curseCard)
                {
                    i--; 
                    continue;
                }
                
                hand.Add(drawnCard);
            }
            else
            {
                break;
            }
        }
        
        SortHand(); 
    }
    
    void SortHand()
    {
        hand.Sort((a, b) => a.cardType.CompareTo(b.cardType));
    }
    
    void ReshuffleDiscardPile()
    {
        if (discardPile.Count > 0)
        {
            shouldPlayShuffleAnimation = true;
            
            deck.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }
    }
    
    public bool ShouldPlayShuffleAnimation()
    {
        bool result = shouldPlayShuffleAnimation;
        shouldPlayShuffleAnimation = false; 
        return result;
    }
    
    public bool SelectCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            return false;
        }
        
        ActionCard card = hand[handIndex];
        
        if (card.isCurseCard)
        {
            return false;
        }
        
        if (selectedIndices.Contains(handIndex))
        {
            return false;
        }
        
        if (card.isSpecialCard && !card.isBoonCard)
        {
            return false;
        }
        
        if (card.isBoonCard)
        {
            selectedIndices.Add(handIndex);
            return true;
        }
        
        int totalFocusCost = card.focusCost;
        foreach (int idx in selectedIndices)
        {
            totalFocusCost += hand[idx].focusCost;
        }
        
        if (totalFocusCost > GameManager.Instance.currentFocus)
        {
            return false;
        }
        
        selectedIndices.Add(handIndex);
        return true;
    }
    
    public bool DeselectCard(int handIndex)
    {
        if (selectedIndices.Contains(handIndex))
        {
            selectedIndices.Remove(handIndex);
            return true;
        }
        return false;
    }
    
    public void ClearSelection()
    {
        selectedIndices.Clear();
    }
    
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
    
    public bool IsCardSelected(int handIndex)
    {
        return selectedIndices.Contains(handIndex);
    }
    
    public IEnumerator ExecuteSelectedCards()
    {
        if (selectedIndices.Count == 0)
        {
            yield break;
        }
        List<ActionCard> cardsToExecute = new List<ActionCard>();
        List<int> sortedIndices = new List<int>(selectedIndices);
        sortedIndices.Sort();
        
        foreach (int idx in sortedIndices)
        {
            if (idx >= 0 && idx < hand.Count)
            {
                ActionCard card = hand[idx];
                
                if (card.isCurseCard)
                {
                    continue;
                }
                
                cardsToExecute.Add(card);
            }
        }
        
        int totalFocus = 0;
        foreach (ActionCard card in cardsToExecute)
        {
            totalFocus += card.focusCost;
        }
        
        if (!GameManager.Instance.SpendFocus(totalFocus))
        {
            yield break;
        }
        
        foreach (ActionCard card in cardsToExecute)
        {
            if (!string.IsNullOrEmpty(card.targetTag) && playerMovement != null)
            {
                Vector3 targetPos = KitchenLocationManager.Instance.GetLocationPosition(card.targetTag);
                if (targetPos != Vector3.zero)
                {
                    playerMovement.MoveToLocation(targetPos, card);
                    
                    while (playerMovement.IsMoving())
                    {
                        yield return null;
                    }
                }
            }
            
            GameManager.Instance.AddCardToPreparation(card.cardType);
        }
        
        if (playerMovement != null)
        {
            Vector3 servingPos = KitchenLocationManager.Instance.GetLocationPosition("ServingCounter");
            if (servingPos != Vector3.zero)
            {
                ActionCard servingCard = ScriptableObject.CreateInstance<ActionCard>();
                servingCard.targetTag = "ServingCounter";
                servingCard.pickupTag = "";
                
                playerMovement.MoveToLocation(servingPos, servingCard);
                
                while (playerMovement.IsMoving())
                {
                    yield return null;
                }
            }
            else
            {
            }
        }
        
        sortedIndices.Sort((a, b) => b.CompareTo(a)); 
        foreach (int idx in sortedIndices)
        {
            if (idx >= 0 && idx < hand.Count)
            {
                ActionCard card = hand[idx];
                
                if (hasPermanentCurse && card == curseCard)
                {
                    continue;
                }
                
                hand.RemoveAt(idx);
                discardPile.Add(card);
            }
        }
        selectedIndices.Clear();
    }
    
    public bool PlayCard(ActionCard card)
    {
        if (!hand.Contains(card))
        {
            return false;
        }
        
        if (card.isSpecialCard)
        {
            ExecuteSpecialCard(card);
            hand.Remove(card);
            
            if (card.isBoonCard)
            {
            }
            else
            {
                discardPile.Add(card);
            }
            return true;
        }
        
        if (!GameManager.Instance.SpendFocus(card.focusCost))
        {
            return false;
        }
        
        if (!string.IsNullOrEmpty(card.targetTag) && playerMovement != null)
        {
            Vector3 targetPos = KitchenLocationManager.Instance.GetLocationPosition(card.targetTag);
            if (targetPos != Vector3.zero)
            {
                playerMovement.MoveToLocation(targetPos, card);
            }
        }
        
        GameManager.Instance.AddCardToPreparation(card.cardType);
        
        hand.Remove(card);
        discardPile.Add(card);
        return true;
    }
    
    void ExecuteSpecialCard(ActionCard card)
    {
        if (card.focusGrant > 0)
        {
            GameManager.Instance.AddFocus(card.focusGrant);
        }
        
        if (card.discardCount > 0 && hand.Count > 1)
        {
            ActionCard cardToDiscard = hand.FirstOrDefault(c => c != card);
            if (cardToDiscard != null)
            {
                hand.Remove(cardToDiscard);
                discardPile.Add(cardToDiscard);
            }
        }
        
        if (card.drawCount > 0)
        {
            bool canExceedMax = card.isBoonCard;
            DrawCards(card.drawCount, canExceedMax);
            
            if (canExceedMax)
            {
            }
        }
    }
    
    public bool PlayTarikNafas(int tarikNafasIndex, int discardIndex)
    {
        if (tarikNafasIndex < 0 || tarikNafasIndex >= hand.Count)
        {
            return false;
        }
        
        if (discardIndex < 0 || discardIndex >= hand.Count)
        {
            return false;
        }
        
        if (tarikNafasIndex == discardIndex)
        {
            return false;
        }
        
        ActionCard tarikNafasCard = hand[tarikNafasIndex];
        ActionCard discardCard = hand[discardIndex];
        
        if (!tarikNafasCard.isSpecialCard)
        {
            return false;
        }
        hand.Remove(discardCard);
        discardPile.Add(discardCard);
        
        hand.Remove(tarikNafasCard);
        discardPile.Add(tarikNafasCard);
        
        if (tarikNafasCard.drawCount > 0)
        {
            DrawCards(tarikNafasCard.drawCount);
        }
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
        List<ActionCard> cardsToDiscard = new List<ActionCard>(hand);
        
        foreach (ActionCard card in cardsToDiscard)
        {
            if (hasPermanentCurse && card == curseCard)
            {
                continue; 
            }
            
            hand.Remove(card);
            discardPile.Add(card);
        }
        
        shouldPlayShuffleAnimation = true;
    }
    
    public void AddCurseToDiscard()
    {
        if (curseCard != null)
        {
            hasPermanentCurse = true;
            
            if (hand.Count < maxHandSize)
            {
                hand.Add(curseCard);
            }
            else
            {
            }
        }
    }
    
    public void AddCardToDeck(ActionCard card)
    {
        if (card != null)
        {
            deck.Add(card);
        }
    }
    
    public void ResetDeck()
    {
        deck.Clear();
        hand.Clear();
        discardPile.Clear();
        selectedIndices.Clear();
        
        foreach (ActionCard card in baseActionCards)
        {
            deck.Add(card);
            deck.Add(card);
        }
        
        ShuffleDeck();
        RemoveCurse(); 
        RemoveBoonCards(); 
    }
    
    public void RemoveCurse()
    {
        if (hasPermanentCurse && curseCard != null)
        {
            hasPermanentCurse = false;
            
            if (hand.Contains(curseCard))
            {
                hand.Remove(curseCard);
            }
            
            while (deck.Contains(curseCard))
            {
                deck.Remove(curseCard);
            }
            
            while (discardPile.Contains(curseCard))
            {
                discardPile.Remove(curseCard);
            }
        }
    }
    
    public void RemoveBoonCards()
    {
        int removedFromHand = hand.RemoveAll(c => c.isBoonCard);
        
        int removedFromDeck = deck.RemoveAll(c => c.isBoonCard);
        
        int removedFromDiscard = discardPile.RemoveAll(c => c.isBoonCard);
        
        int totalRemoved = removedFromHand + removedFromDeck + removedFromDiscard;
        
        if (totalRemoved > 0)
        {
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
