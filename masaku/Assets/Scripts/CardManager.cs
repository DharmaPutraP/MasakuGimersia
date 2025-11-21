using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    
    [Header("Card Setup")]
    public List<ActionCard> allActionCards;
    
    [Header("Player Reference")]
    public PlayerMovement playerMovement;
    
    [Header("Game State")]
    public int currentFocus = 3; 
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
    
    void InitializeDeck()
    {
        deck.Clear();
        
        foreach (ActionCard card in allActionCards)
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
    
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (hand.Count >= maxHandSize)
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
                hand.Add(drawnCard);
            }
        }
    }
    
    void ReshuffleDiscardPile()
    {
        if (discardPile.Count > 0)
        {
            deck.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }
    }
    
    public void PlayCard(ActionCard card)
    {
        if (!hand.Contains(card))
        {
            return;
        }
        
        if (card.isSpecialCard)
        {
            ExecuteSpecialCard(card);
        }
        else
        {
            if (currentFocus < card.focusCost)
            {
                return;
            }
            
            currentFocus -= card.focusCost;
            
            ExecuteCardAction(card);
        }
        
        hand.Remove(card);
        discardPile.Add(card);
    }
    
    void ExecuteCardAction(ActionCard card)
    {
        GameObject targetLocation = GameObject.FindGameObjectWithTag(card.targetTag);
        
        if (targetLocation != null)
        {
            if (playerMovement != null)
            {
                playerMovement.MoveToLocation(targetLocation.transform.position, card);
            }
            else
            {
            }
        }
        else
        {
        }
    }
    
    void ExecuteSpecialCard(ActionCard card)
    {
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
