using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Image[] reputationStars; // Array of 5 star Image components
    public Sprite starOnSprite; // Star filled/active sprite
    public Sprite starOffSprite; // Star empty/inactive sprite
    
    [Header("Day Display")]
    public TextMeshProUGUI dayText;
    
    [Header("Hand Display")]
    public Transform handContainer;
    public GameObject cardUIPrefab;
    private List<GameObject> cardUIObjects = new List<GameObject>();
    
    [Header("Fan Layout Settings")]
    public float fanSpread = 30f; // Total angle spread of the fan (degrees)
    public float fanRadius = 300f; // How far down the cards are positioned
    public float cardVerticalOffset = 50f; // How much cards lift up in the center
    public Vector3 fanCenterPosition = new Vector3(0, -200, 0); // Center position of the fan
    
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
    private Dictionary<GameObject, int> menuToSeatIndex = new Dictionary<GameObject, int>(); // Map menu to seat index
    
    private int selectedSeat = -1;
    private bool isExecutingOrder = false; // Flag to prevent interactions during execution
    private bool isTarikNafasMode = false; // Flag for Tarik Nafas card selection mode
    private int tarikNafasCardIndex = -1; // Index of the Tarik Nafas card being played
    
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
        // Update star visuals
        if (reputationStars != null && reputationStars.Length > 0)
        {
            int currentReputation = GameManager.Instance.reputation;
            
            for (int i = 0; i < reputationStars.Length; i++)
            {
                if (reputationStars[i] != null)
                {
                    // Show star ON if within current reputation, otherwise show star OFF
                    if (i < currentReputation)
                    {
                        reputationStars[i].sprite = starOnSprite;
                        reputationStars[i].color = Color.white; // Full opacity
                    }
                    else
                    {
                        reputationStars[i].sprite = starOffSprite;
                        reputationStars[i].color = new Color(1f, 1f, 1f, 0.5f); // Slightly transparent
                    }
                }
            }
            
            Debug.Log($"Updated reputation display: {currentReputation}/5 stars");
        }
        else
        {
            Debug.LogWarning("Reputation stars array is not set up!");
        }
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
        
        // === FAN LAYOUT CALCULATION ===
        int handSize = MasakuCardManager.Instance.GetHand().Count;
        
        // Calculate angle for this card
        float angleStep = handSize > 1 ? fanSpread / (handSize - 1) : 0;
        float cardAngle = (index * angleStep) - (fanSpread / 2f); // -15 to +15 for 5 cards with 30° spread
        
        // Calculate position on arc
        float angleRad = cardAngle * Mathf.Deg2Rad;
        float x = Mathf.Sin(angleRad) * fanRadius;
        float y = -Mathf.Cos(angleRad) * fanRadius;
        
        // Add vertical offset (cards in center are higher)
        float normalizedPosition = Mathf.Abs((index - (handSize - 1) / 2f) / (handSize / 2f)); // 0 at center, 1 at edges
        float verticalLift = cardVerticalOffset * (1f - normalizedPosition);
        y += verticalLift;
        
        // Apply position
        RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = fanCenterPosition + new Vector3(x, y, 0);
            rectTransform.localRotation = Quaternion.Euler(0, 0, -cardAngle); // Rotate card to follow fan
            
            // Set card to be in front based on index (right cards on top)
            rectTransform.SetAsLastSibling();
        }
        // === END FAN LAYOUT ===
        
        // Get Button component (Button has an Image component built-in)
        Button cardButton = cardUI.GetComponent<Button>();
        
        if (cardButton != null)
        {
            // Debug.Log($"Button found! Interactable: {cardButton.interactable}");
            
            // Curse cards should not be interactable
            if (card.isCurseCard)
            {
                cardButton.interactable = false;
                
                // Keep curse button color unchanged when disabled
                ColorBlock colors = cardButton.colors;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray but visible
                cardButton.colors = colors;
            }
            else
            {
                // Make sure button is interactable for non-curse cards
                cardButton.interactable = true;
            }
            
            // Get the Image component from the Button
            Image cardImage = cardButton.GetComponent<Image>();
            
            if (cardImage != null && card.cardImage != null)
            {
                // Set the card's designed image
                cardImage.sprite = card.cardImage;
                
                // Make sure raycast target is enabled
                cardImage.raycastTarget = true;
                // Debug.Log($"Set card image for: {card.cardName}, Raycast Target: {cardImage.raycastTarget}");
                
                // Visual feedback for Tarik Nafas mode
                if (isTarikNafasMode && index == tarikNafasCardIndex)
                {
                    // Highlight the Tarik Nafas card being played
                    cardUI.transform.localScale = Vector3.one * 1.3f;
                    cardImage.color = new Color(0.5f, 1f, 0.5f); // Greenish tint
                }
                // Visual feedback for curse cards (cannot be played)
                else if (card.isCurseCard)
                {
                    cardUI.transform.localScale = Vector3.one * 0.9f; // Slightly smaller
                    cardImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Grayed out but opaque
                }
                // Check if selected (for both boon and normal cards)
                else if (MasakuCardManager.Instance.IsCardSelected(index))
                {
                    if (card.isBoonCard)
                    {
                        // Selected boon card: even larger and brighter
                        cardUI.transform.localScale = Vector3.one * 1.3f;
                        cardImage.color = new Color(1f, 1f, 0.5f, 1f); // Bright yellow
                    }
                    else
                    {
                        // Selected normal card
                        cardUI.transform.localScale = Vector3.one * 1.2f; // Scale up by 20%
                        cardImage.color = Color.white; // Keep normal color
                    }
                }
                // Visual feedback for unselected boon cards (glowing effect)
                else if (card.isBoonCard)
                {
                    cardUI.transform.localScale = Vector3.one * 1.15f; // Slightly larger
                    cardImage.color = new Color(1f, 0.9f, 0.3f, 1f); // Golden/yellow glow
                }
                else
                {
                    cardUI.transform.localScale = Vector3.one; // Normal scale
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
        // Prevent interaction during execution
        if (isExecutingOrder)
        {
            Debug.Log("Cannot select cards during order execution!");
            return;
        }
        
        Debug.Log($"===== CARD CLICKED! Index: {handIndex} =====");
        
        List<ActionCard> hand = MasakuCardManager.Instance.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogError($"Invalid hand index: {handIndex}, hand count: {hand.Count}");
            return;
        }
        
        ActionCard card = hand[handIndex];
        Debug.Log($"Card clicked: {card.cardName}");
        
        // Prevent clicking curse cards
        if (card.isCurseCard)
        {
            Debug.Log("Cannot play or select curse cards!");
            return;
        }
        
        // Check if it's a special card (like Tarik Nafas) - NOT boon cards
        if (card.isSpecialCard && !card.isBoonCard)
        {
            // If already in Tarik Nafas mode, cancel it
            if (isTarikNafasMode && handIndex == tarikNafasCardIndex)
            {
                Debug.Log("Cancelling Tarik Nafas mode");
                isTarikNafasMode = false;
                tarikNafasCardIndex = -1;
                UpdateHandDisplay();
                return;
            }
            
            Debug.Log($"Tarik Nafas card clicked! Now select a card to discard.");
            
            // Enter Tarik Nafas mode - player must select a card to discard
            isTarikNafasMode = true;
            tarikNafasCardIndex = handIndex;
            
            // Visual feedback - scale up the Tarik Nafas card
            UpdateHandDisplay();
            return;
        }
        
        // If in Tarik Nafas mode, this card will be discarded
        if (isTarikNafasMode)
        {
            // Cannot discard curse cards
            if (card.isCurseCard)
            {
                Debug.Log("Cannot discard curse cards!");
                return;
            }
            
            // Cannot discard boon cards
            if (card.isBoonCard)
            {
                Debug.Log("Cannot discard boon cards!");
                return;
            }
            
            Debug.Log($"Discarding {card.cardName} and playing Tarik Nafas");
            
            // Play the Tarik Nafas card with the selected card to discard
            bool success = MasakuCardManager.Instance.PlayTarikNafas(tarikNafasCardIndex, handIndex);
            
            if (success)
            {
                // Exit Tarik Nafas mode
                isTarikNafasMode = false;
                tarikNafasCardIndex = -1;
                
                // Update hand display
                UpdateHandDisplay();
            }
            return;
        }
        
        // Toggle selection by index for normal cards AND boon cards
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
        
        // Only clear card selection if NOT currently executing an order
        // (prevents clearing selection mid-execution when new customer arrives)
        if (!isExecutingOrder)
        {
            MasakuCardManager.Instance.ClearSelection();
            selectedSeat = -1;
        }
        
        // Destroy all existing menus
        foreach (GameObject menu in activeMenus)
        {
            if (menu != null)
                Destroy(menu);
        }
        activeMenus.Clear();
        menuToSeatIndex.Clear(); // Clear seat mapping
        
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
                        menuToSeatIndex[menuInstance] = i; // Map this menu to its seat index
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
        
        // Restore selection scales after recreating menus
        UpdateMenuScales();
        
        // Update button states based on whether there are customers
        UpdateButtonStates();
        
        // Update hand display to reflect cleared selection
        UpdateHandDisplay();
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
        // Prevent interaction during execution
        if (isExecutingOrder)
        {
            Debug.Log("Cannot select seats during order execution!");
            return;
        }
        
        // Toggle selection: if clicking the same seat, deselect it
        if (selectedSeat == seatIndex)
        {
            selectedSeat = -1;
            Debug.Log($"Kursi {seatIndex} dibatalkan");
        }
        else
        {
            selectedSeat = seatIndex;
            Debug.Log($"Kursi {seatIndex} dipilih");
        }
        
        // Update menu scales based on selection
        UpdateMenuScales();
    }
    
    void UpdateMenuScales()
    {
        // Scale menus based on current selection
        foreach (GameObject menu in activeMenus)
        {
            if (menu != null && menuToSeatIndex.ContainsKey(menu))
            {
                RectTransform menuRect = menu.GetComponent<RectTransform>();
                if (menuRect != null)
                {
                    int menuSeat = menuToSeatIndex[menu];
                    
                    if (menuSeat == selectedSeat && selectedSeat >= 0)
                    {
                        // Scale up selected menu
                        menuRect.localScale = Vector3.one * 1.2f;
                    }
                    else
                    {
                        // Normal scale for others (including deselected)
                        menuRect.localScale = Vector3.one;
                    }
                }
            }
        }
    }
    
    void UpdateButtonStates()
    {
        // Check if there are any active customers
        bool hasCustomers = activeMenus.Count > 0;
        
        // Keep buttons disabled during execution, even if customers are present
        bool shouldEnable = hasCustomers && !isExecutingOrder;
        
        // Disable submit and end turn buttons if no customers or if executing
        if (submitOrderButton != null)
        {
            submitOrderButton.interactable = shouldEnable;
        }
        
        if (endTurnButton != null)
        {
            endTurnButton.interactable = shouldEnable;
        }
        
        Debug.Log($"Buttons enabled: {shouldEnable} (Active menus: {activeMenus.Count}, Executing: {isExecutingOrder})");
    }
    
    void SetUIInteractable(bool interactable)
    {
        // Don't enable buttons if there are no customers
        bool hasCustomers = activeMenus.Count > 0;
        
        // Enable/disable submit and end turn buttons
        if (submitOrderButton != null)
        {
            submitOrderButton.interactable = interactable && hasCustomers;
        }
        
        if (endTurnButton != null)
        {
            endTurnButton.interactable = interactable && hasCustomers;
        }
        
        // Enable/disable all card buttons WITHOUT changing color
        foreach (GameObject cardObj in cardUIObjects)
        {
            if (cardObj != null)
            {
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    cardButton.interactable = interactable;
                    
                    // Keep button colors unchanged when disabled
                    ColorBlock colors = cardButton.colors;
                    colors.disabledColor = Color.white; // Same as normal color
                    cardButton.colors = colors;
                }
            }
        }
        
        // Enable/disable all menu buttons WITHOUT changing color
        foreach (GameObject menu in activeMenus)
        {
            if (menu != null)
            {
                Button menuButton = menu.GetComponent<Button>();
                if (menuButton == null)
                {
                    menuButton = menu.GetComponentInChildren<Button>();
                }
                
                if (menuButton != null)
                {
                    menuButton.interactable = interactable;
                    
                    // Keep button colors unchanged when disabled
                    ColorBlock colors = menuButton.colors;
                    colors.disabledColor = Color.white; // Same as normal color
                    menuButton.colors = colors;
                }
            }
        }
        
        Debug.Log($"UI Interactable set to: {interactable}");
    }
    
    void OnSubmitOrderClicked()
    {
        // Check if cards are selected
        List<ActionCard> selectedCards = MasakuCardManager.Instance.GetSelectedCards();
        if (selectedCards.Count == 0)
        {
            Debug.LogWarning("Pilih kartu terlebih dahulu!");
            return;
        }
        
        // Check if only boon cards are selected (no customer needed)
        bool allBoonCards = selectedCards.All(c => c.isBoonCard);
        
        if (allBoonCards)
        {
            Debug.Log("Playing boon cards only - no customer selection needed");
            StartCoroutine(ExecuteBoonCards());
        }
        else
        {
            // Normal cards need customer selection
            if (selectedSeat < 0)
            {
                Debug.LogWarning("Pilih kursi customer terlebih dahulu!");
                return;
            }
            
            // Start executing selected cards
            StartCoroutine(ExecuteAndSubmitOrder());
        }
    }
    
    IEnumerator ExecuteBoonCards()
    {
        // Set flag to prevent interactions
        isExecutingOrder = true;
        
        // Disable all interactive buttons
        SetUIInteractable(false);
        
        // Get selected boon cards
        List<ActionCard> selectedCards = MasakuCardManager.Instance.GetSelectedCards();
        
        // Play each boon card
        foreach (ActionCard card in selectedCards)
        {
            if (card.isBoonCard)
            {
                MasakuCardManager.Instance.PlayCard(card);
                yield return new WaitForSeconds(0.3f); // Small delay between boons
            }
        }
        
        // Update hand display
        UpdateHandDisplay();
        
        // Re-enable interactions
        isExecutingOrder = false;
        SetUIInteractable(true);
    }
    
    IEnumerator ExecuteAndSubmitOrder()
    {
        // Set flag to prevent interactions
        isExecutingOrder = true;
        
        // Disable all interactive buttons
        SetUIInteractable(false);
        
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
        
        // Re-enable interactions
        isExecutingOrder = false;
        SetUIInteractable(true);
    }
    
    void OnEndTurnClicked()
    {
        GameManager.Instance.EndPlayerTurn();
        
        // Don't refresh here - hand is empty at this moment
        // UI will refresh when StartPlayerTurn() is called after patience phase
    }
}
