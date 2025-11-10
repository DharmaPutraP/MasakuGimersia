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
    public Transform customerMenuContainer; // Container to hold dynamically created menus
    public GameObject knightMenuPrefab; // Prefab for Knight menu
    public GameObject elfMenuPrefab; // Prefab for Elf menu
    public GameObject wizardMenuPrefab; // Prefab for Wizard menu
    public GameObject barbarianMenuPrefab; // Prefab for Barbarian menu
    
    private List<GameObject> activeMenus = new List<GameObject>(); // Track created menus
    
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
        
        // Add null check for seatButtons array
        if (seatButtons != null && seatButtons.Length > 0)
        {
            for (int i = 0; i < seatButtons.Length; i++)
            {
                if (seatButtons[i] != null)
                {
                    int seatIndex = i;
                    seatButtons[i].onClick.AddListener(() => OnSeatSelected(seatIndex));
                }
            }
        }
        
        UpdateUI();
        UpdateCustomerMenus(); // Initialize customer menus visibility
    }
    
    void Update()
    {
        // Only update displays that change frequently
        // DON'T call UpdateHandDisplay() every frame - it destroys and recreates cards!
        // DON'T call UpdateCustomerMenus() every frame - it destroys and recreates menus!
        UpdateFocusDisplay();
        UpdateReputationDisplay();
        UpdateDayDisplay();
        UpdatePreparationStationDisplay();
    }
    
    public void UpdateUI()
    {
        UpdateFocusDisplay();
        UpdateReputationDisplay();
        UpdateDayDisplay();
        UpdateHandDisplay();
        UpdatePreparationStationDisplay();
        UpdateCustomerMenus(); // Only update menus when explicitly called
    }
    
    void UpdateFocusDisplay()
    {
        if (focusText != null)
        {
            focusText.text = $"{GameManager.Instance.currentFocus}";
        }
        
        // if (focusBar != null)
        // {
        //     float focusPercent = GameManager.Instance.currentFocus / 10f; // Max assumed 10
        //     focusBar.fillAmount = focusPercent;
        // }
    }
    
    void UpdateReputationDisplay()
    {
        if (reputationText != null)
        {
            reputationText.text = $"Reputasi: {GameManager.Instance.reputation}/5";
        }
        
        // Update hearts visual
        // for (int i = 0; i < reputationHearts.Length; i++)
        // {
        //     if (reputationHearts[i] != null)
        //     {
        //         reputationHearts[i].SetActive(i < GameManager.Instance.reputation);
        //     }
        // }
    }
    
    void UpdateDayDisplay()
    {
        if (dayText != null)
        {
            dayText.text = $"Hari {GameManager.Instance.currentDay}/7";
        }
    }
    
    public void UpdateHandDisplay()
    {
        Debug.Log("=== UpdateHandDisplay called ===");
        
        // Debug check
        if (handContainer == null)
        {
            Debug.LogError("HandContainer is NULL! Assign it in MasakuUI Inspector.");
            return;
        }
        
        if (cardUIPrefab == null)
        {
            Debug.LogError("CardUI Prefab is NULL! Assign it in MasakuUI Inspector.");
            return;
        }
        
        // Hapus UI kartu lama
        Debug.Log($"Destroying {cardUIObjects.Count} old card UI objects");
        foreach (GameObject cardUI in cardUIObjects)
        {
            Destroy(cardUI);
        }
        cardUIObjects.Clear();
        
        // Buat UI untuk kartu di tangan
        List<ActionCard> hand = MasakuCardManager.Instance.GetHand();
        Debug.Log($"UpdateHandDisplay: Creating UI for {hand.Count} cards in hand");
        
        for (int i = 0; i < hand.Count; i++)
        {
            CreateCardUI(hand[i], i);
        }
        
        Debug.Log($"Total card UI objects created: {cardUIObjects.Count}");
    }
    
    void CreateCardUI(ActionCard card, int index)
    {
        if (cardUIPrefab == null || handContainer == null) return;
        
        GameObject cardUI = Instantiate(cardUIPrefab, handContainer);
        cardUIObjects.Add(cardUI);
        
        Debug.Log($"Created CardUI for: {card.cardName}, GameObject: {cardUI.name}, Active: {cardUI.activeInHierarchy}");
        
        // Get Button component (Button has an Image component built-in)
        Button cardButton = cardUI.GetComponent<Button>();
        
        if (cardButton != null)
        {
            // Debug.Log($"Button found! Interactable: {cardButton.interactable}");
            
            // Make sure button is interactable
            cardButton.interactable = true;
            
            // Get the Image component from the Button
            Image cardImage = cardButton.GetComponent<Image>();
            
            if (cardImage != null && card.cardImage != null)
            {
                // Set the card's designed image
                cardImage.sprite = card.cardImage;
                
                // Make sure raycast target is enabled
                cardImage.raycastTarget = true;
                // Debug.Log($"Set card image for: {card.cardName}, Raycast Target: {cardImage.raycastTarget}");
                
                // Highlight if selected (add yellow tint) - now using index
                if (MasakuCardManager.Instance.IsCardSelected(index))
                {
                    cardImage.color = new Color(1f, 1f, 0.5f, 1f); // Yellow tint
                }
                else
                {
                    cardImage.color = Color.white; // Normal color
                }
            }
            else
            {
                if (cardImage == null) Debug.LogError($"Button missing Image component!");
                if (card.cardImage == null) Debug.LogError($"Card '{card.cardName}' missing cardImage sprite!");
            }
            
            // Add click listener
            int cardIndex = index;
            cardButton.onClick.AddListener(() => OnCardClicked(cardIndex));
            // Debug.Log($"Click listener added for card index: {cardIndex}");
        }
        else
        {
            Debug.LogError("CardUI prefab missing Button component!");
        }
    }
    
    void OnCardClicked(int handIndex)
    {
        Debug.Log($"===== CARD CLICKED! Index: {handIndex} =====");
        
        List<ActionCard> hand = MasakuCardManager.Instance.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogError($"Invalid hand index: {handIndex}, hand count: {hand.Count}");
            return;
        }
        
        ActionCard card = hand[handIndex];
        Debug.Log($"Card clicked: {card.cardName}");
        
        // Toggle selection by index
        if (MasakuCardManager.Instance.IsCardSelected(handIndex))
        {
            Debug.Log($"Deselecting card at index: {handIndex}");
            MasakuCardManager.Instance.DeselectCard(handIndex);
        }
        else
        {
            Debug.Log($"Selecting card at index: {handIndex}");
            MasakuCardManager.Instance.SelectCard(handIndex);
        }
        
        // Update UI to show selection
        UpdateHandDisplay();
    }
    
    void UpdatePreparationStationDisplay()
    {
        if (preparationText != null)
        {
            // Show selected cards (before execution)
            List<ActionCard> selectedCards = MasakuCardManager.Instance.GetSelectedCards();
            
            string prepText = "Kartu Dipilih: ";
            if (selectedCards.Count == 0)
            {
                prepText += "[Belum ada]";
            }
            else
            {
                foreach (ActionCard card in selectedCards)
                {
                    prepText += $"[{card.cardName}] ";
                }
            }
            
            // Show preparation station (after execution)
            if (GameManager.Instance.preparationStation.Count > 0)
            {
                prepText += "\nDi Stasiun: ";
                foreach (CardType card in GameManager.Instance.preparationStation)
                {
                    prepText += $"[{card}] ";
                }
            }
            
            preparationText.text = prepText;
        }
    }
    
    public void UpdateCustomerMenus()
    {
        if (customerMenuContainer == null)
        {
            Debug.LogError("CustomerMenuContainer is NULL! Assign it in MasakuUI Inspector.");
            return;
        }
        
        // Get active customers from GameManager
        CustomerInstance[] activeCustomers = GameManager.Instance.GetActiveCustomers();
        
        Debug.Log($"=== Updating Customer Menus: {activeCustomers.Length} seats ===");
        
        // Destroy all existing menus
        foreach (GameObject menu in activeMenus)
        {
            if (menu != null)
                Destroy(menu);
        }
        activeMenus.Clear();
        
        // Create menu for each active customer
        int menuCount = 0;
        for (int i = 0; i < activeCustomers.Length; i++)
        {
            if (activeCustomers[i] != null)
            {
                Customer customer = activeCustomers[i].GetCustomer();
                if (customer != null)
                {
                    Debug.Log($"Seat {i}: {customer.customerName} (Type: {customer.customerType})");
                    
                    // Get the appropriate menu prefab for this customer type
                    GameObject menuPrefab = GetMenuPrefabForCustomerType(customer.customerType);
                    
                    if (menuPrefab != null)
                    {
                        // Instantiate menu
                        GameObject menuInstance = Instantiate(menuPrefab, customerMenuContainer);
                        activeMenus.Add(menuInstance);
                        menuCount++;
                        
                        Debug.Log($"Created menu for {customer.customerName} at seat {i}");
                        
                        // Add click listener to the menu
                        Button menuButton = menuInstance.GetComponent<Button>();
                        if (menuButton == null)
                        {
                            // If menu doesn't have Button component on root, try to find it in children
                            menuButton = menuInstance.GetComponentInChildren<Button>();
                        }
                        
                        if (menuButton != null)
                        {
                            int seatIndex = i; // Capture seat index for this menu
                            menuButton.onClick.AddListener(() => OnSeatSelected(seatIndex));
                            
                            // Make sure button is interactable and image has raycast target
                            menuButton.interactable = true;
                            Image menuImage = menuButton.GetComponent<Image>();
                            if (menuImage != null)
                            {
                                menuImage.raycastTarget = true;
                            }
                            
                            Debug.Log($"Added click listener to menu for seat {seatIndex}");
                        }
                        else
                        {
                            Debug.LogWarning($"Menu for {customer.customerName} has no Button component! Add a Button to the prefab.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Menu prefab for {customer.customerType} is NULL!");
                    }
                }
                else
                {
                    Debug.LogWarning($"Seat {i}: Customer instance exists but customerData is null!");
                }
            }
            else
            {
                Debug.Log($"Seat {i}: Empty");
            }
        }
        
        Debug.Log($"Total menus created: {menuCount}");
    }
    
    GameObject GetMenuPrefabForCustomerType(CustomerType type)
    {
        GameObject prefab = null;
        
        switch (type)
        {
            case CustomerType.Knight:
                prefab = knightMenuPrefab;
                break;
            case CustomerType.Elf:
                prefab = elfMenuPrefab;
                break;
            case CustomerType.Wizard:
                prefab = wizardMenuPrefab;
                break;
            case CustomerType.Barbarian:
                prefab = barbarianMenuPrefab;
                break;
        }
        
        if (prefab == null)
        {
            Debug.LogError($"Menu prefab for {type} is NULL! Assign it in MasakuUI Inspector.");
        }
        
        return prefab;
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
        
        // Check if cards are selected
        List<ActionCard> selectedCards = MasakuCardManager.Instance.GetSelectedCards();
        if (selectedCards.Count == 0)
        {
            Debug.LogWarning("Pilih kartu terlebih dahulu!");
            return;
        }
        
        // Start executing selected cards
        StartCoroutine(ExecuteAndSubmitOrder());
    }
    
    IEnumerator ExecuteAndSubmitOrder()
    {
        // Execute all selected cards
        yield return StartCoroutine(MasakuCardManager.Instance.ExecuteSelectedCards());
        
        // Update UI to remove executed cards from hand display
        UpdateHandDisplay();
        
        // After all movements complete, submit order
        if (GameManager.Instance.preparationStation.Count > 0)
        {
            GameManager.Instance.SubmitOrder(selectedSeat);
            selectedSeat = -1;
            
            // Update customer menus after serving (customer may have left)
            UpdateCustomerMenus();
        }
    }
    
    void OnEndTurnClicked()
    {
        GameManager.Instance.EndPlayerTurn();
        
        // Don't refresh here - hand is empty at this moment
        // UI will refresh when StartPlayerTurn() is called after patience phase
    }
}
