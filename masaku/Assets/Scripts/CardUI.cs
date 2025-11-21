using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform handContainer;
    public GameObject cardUIPrefab;
    
    [Header("Focus Display")]
    public TextMeshProUGUI focusText;
    
    private List<GameObject> cardUIObjects = new List<GameObject>();
    
    void Start()
    {
        UpdateHandUI();
        UpdateFocusUI();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdateHandUI();
            UpdateFocusUI();
        }
    }
    
    public void UpdateHandUI()
    {
        foreach (GameObject cardUI in cardUIObjects)
        {
            Destroy(cardUI);
        }
        cardUIObjects.Clear();
        
        List<ActionCard> hand = CardManager.Instance.GetHand();
        for (int i = 0; i < hand.Count; i++)
        {
            CreateCardUI(hand[i], i);
        }
    }
    
    void CreateCardUI(ActionCard card, int index)
    {
        if (cardUIPrefab == null || handContainer == null) return;
        
        GameObject cardUI = Instantiate(cardUIPrefab, handContainer);
        cardUIObjects.Add(cardUI);
        
        TextMeshProUGUI nameText = cardUI.transform.Find("CardName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI costText = cardUI.transform.Find("FocusCost")?.GetComponent<TextMeshProUGUI>();
        Image cardImage = cardUI.transform.Find("CardImage")?.GetComponent<Image>();
        Button cardButton = cardUI.GetComponent<Button>();
        
        if (nameText != null) nameText.text = card.cardName;
        if (costText != null) costText.text = card.isSpecialCard ? "Special" : $"Fokus: {card.focusCost}";
        if (cardImage != null && card.cardImage != null) cardImage.sprite = card.cardImage;
        
        if (cardButton != null)
        {
            int cardIndex = index;
            cardButton.onClick.AddListener(() => OnCardClicked(cardIndex));
        }
    }
    
    void OnCardClicked(int handIndex)
    {
        CardManager.Instance.PlayCardByIndex(handIndex);
        UpdateHandUI();
        UpdateFocusUI();
    }
    
    public void UpdateFocusUI()
    {
        if (focusText != null)
        {
            focusText.text = $"{CardManager.Instance.currentFocus}/{CardManager.Instance.maxFocus}";
        }
    }
}
