using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MasakuUI : MonoBehaviour
{
    public static MasakuUI Instance;
    
    [Header("Focus Display")]
    public TextMeshProUGUI focusText;
    public Image focusBar;
    
    [Header("Reputation Display")]
    public TextMeshProUGUI reputationText;
    public GameObject[] reputationHearts;
    
    [Header("Day Display")]
    public TextMeshProUGUI dayText;
    
    [Header("Hand Display")]
    public Transform handContainer;
    public GameObject cardUIPrefab;
    private List<GameObject> cardUIObjects = new List<GameObject>();
    
    [Header("Preparation Station Display")]
    public Transform preparationContainer;
    public TextMeshProUGUI preparationText;
    
    [Header("Buttons")]
    public Button submitOrderButton;
    public Button endTurnButton;
    public Button[] seatButtons; // 4 tombol untuk 4 kursi
    
    [Header("Customer Display")]
    public Transform[] customerUIPositions; // UI untuk menampilkan customer
    
    private int selectedSeat = -1;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // Setup button listeners
        if (submitOrderButton != null)
            submitOrderButton.onClick.AddListener(OnSubmitOrderClicked);
        
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
        
        for (int i = 0; i < seatButtons.Length; i++)
        {
            int seatIndex = i;
            seatButtons[i].onClick.AddListener(() => OnSeatSelected(seatIndex));
        }
        
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        UpdateFocusDisplay();
        UpdateReputationDisplay();
        UpdateDayDisplay();
        UpdateHandDisplay();
        UpdatePreparationStationDisplay();
    }
    
    void UpdateFocusDisplay()
    {
        if (focusText != null)
        {
            focusText.text = $"Fokus: {GameManager.Instance.currentFocus}";
        }
        
        if (focusBar != null)
        {
            float focusPercent = GameManager.Instance.currentFocus / 10f; // Max assumed 10
            focusBar.fillAmount = focusPercent;
        }
    }
    
    void UpdateReputationDisplay()
    {
        if (reputationText != null)
        {
            reputationText.text = $"Reputasi: {GameManager.Instance.reputation}/5";
        }
        
        // Update hearts visual
        for (int i = 0; i < reputationHearts.Length; i++)
        {
            if (reputationHearts[i] != null)
            {
                reputationHearts[i].SetActive(i < GameManager.Instance.reputation);
            }
        }
    }
    
    void UpdateDayDisplay()
    {
        if (dayText != null)
        {
            dayText.text = $"Hari {GameManager.Instance.currentDay}/7";
        }
    }
    
    void UpdateHandDisplay()
    {
        // Hapus UI kartu lama
        foreach (GameObject cardUI in cardUIObjects)
        {
            Destroy(cardUI);
        }
        cardUIObjects.Clear();
        
        // Buat UI untuk kartu di tangan
        List<ActionCard> hand = MasakuCardManager.Instance.GetHand();
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
        
        // Setup card UI components
        TextMeshProUGUI nameText = cardUI.transform.Find("CardName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI costText = cardUI.transform.Find("FocusCost")?.GetComponent<TextMeshProUGUI>();
        Image cardImage = cardUI.transform.Find("CardImage")?.GetComponent<Image>();
        Button cardButton = cardUI.GetComponent<Button>();
        
        if (nameText != null) nameText.text = card.cardName;
        if (costText != null) costText.text = card.isSpecialCard ? "Spesial" : $"Fokus: {card.focusCost}";
        if (cardImage != null && card.cardImage != null) cardImage.sprite = card.cardImage;
        
        // Tambahkan listener untuk click
        if (cardButton != null)
        {
            int cardIndex = index;
            cardButton.onClick.AddListener(() => OnCardClicked(cardIndex));
        }
    }
    
    void OnCardClicked(int handIndex)
    {
        MasakuCardManager.Instance.PlayCardByIndex(handIndex);
    }
    
    void UpdatePreparationStationDisplay()
    {
        if (preparationText != null)
        {
            string prepText = "Stasiun Persiapan: ";
            if (GameManager.Instance.preparationStation.Count == 0)
            {
                prepText += "[Kosong]";
            }
            else
            {
                foreach (CardType card in GameManager.Instance.preparationStation)
                {
                    prepText += $"[{card}] ";
                }
            }
            preparationText.text = prepText;
        }
    }
    
    void OnSeatSelected(int seatIndex)
    {
        selectedSeat = seatIndex;
        Debug.Log($"Kursi {seatIndex} dipilih");
        
        // Highlight selected seat (optional visual feedback)
        for (int i = 0; i < seatButtons.Length; i++)
        {
            // Add visual feedback here
        }
    }
    
    void OnSubmitOrderClicked()
    {
        if (selectedSeat < 0)
        {
            Debug.LogWarning("Pilih kursi customer terlebih dahulu!");
            return;
        }
        
        if (GameManager.Instance.preparationStation.Count == 0)
        {
            Debug.LogWarning("Stasiun Persiapan kosong!");
            return;
        }
        
        GameManager.Instance.SubmitOrder(selectedSeat);
        selectedSeat = -1;
    }
    
    void OnEndTurnClicked()
    {
        GameManager.Instance.EndPlayerTurn();
    }
}
