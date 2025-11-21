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
    
    [Header("Sound Effects")]
    public AudioSource audioSource; // AudioSource for UI sounds
    public AudioClip buttonClickSound; // Sound for Submit and End Turn buttons
    public AudioClip cardSelectSound; // Sound when selecting a card
    public AudioClip tarikNafasSound; // Sound when using Tarik Nafas cards
    
    [Header("Hint System")]
    public TextMeshProUGUI hintText; // Text component for displaying hints
    public CanvasGroup hintCanvasGroup; // For fade in/out animation
    public float hintFadeDuration = 0.3f; // Duration of fade animation
    public float hintDisplayDuration = 2f; // How long hint stays visible
    private Coroutine currentHintCoroutine;
    
    [Header("Pause Menu")]
    public GameObject pausePanel; // Pause menu panel
    public Button pauseContinueButton; // Continue button
    public Button pauseExitButton; // Exit to main menu button
    private bool isPaused = false;
    
    [Header("Shuffle Animation")]
    public float shuffleAnimationDuration = 1.0f; // Duration of shuffle animation
    private bool isShuffling = false;
    
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
        if (submitOrderButton != null)
            submitOrderButton.onClick.AddListener(OnSubmitOrderClicked);
        
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
        
        if (pauseContinueButton != null)
            pauseContinueButton.onClick.AddListener(OnPauseContinueClicked);
        
        if (pauseExitButton != null)
            pauseExitButton.onClick.AddListener(OnPauseExitClicked);
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
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
        
        if (hintCanvasGroup != null)
        {
            hintCanvasGroup.alpha = 0f;
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
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
        
    }
    
    void UpdateReputationDisplay()
    {
        if (reputationStars != null && reputationStars.Length > 0)
        {
            int currentReputation = GameManager.Instance.reputation;
            
            for (int i = 0; i < reputationStars.Length; i++)
            {
                if (reputationStars[i] != null)
                {
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
    
    public void PlayShuffleAnimation()
    {
        if (!isShuffling)
        {
            StartCoroutine(ShuffleCardsAnimation());
        }
    }
    
    IEnumerator ShuffleCardsAnimation()
    {
        isShuffling = true;
        
        List<CardAnimData> cardData = new List<CardAnimData>();
        
        foreach (GameObject cardUI in cardUIObjects)
        {
            if (cardUI != null)
            {
                RectTransform rect = cardUI.GetComponent<RectTransform>();
                cardData.Add(new CardAnimData
                {
                    rectTransform = rect,
                    originalPosition = rect.anchoredPosition,
                    originalRotation = rect.localRotation,
                    originalScale = rect.localScale
                });
            }
        }
        
        if (cardData.Count == 0)
        {
            isShuffling = false;
            yield break;
        }
        
        float elapsedTime = 0f;
        float shuffleDuration = shuffleAnimationDuration;
        
        while (elapsedTime < shuffleDuration * 0.3f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (shuffleDuration * 0.3f);
            float easeProgress = EaseInOut(progress);
            
            foreach (var data in cardData)
            {
                if (data.rectTransform != null)
                {
                    data.rectTransform.anchoredPosition = Vector3.Lerp(data.originalPosition, Vector3.zero, easeProgress);
                    float rotation = Mathf.Lerp(0, 720f, easeProgress);
                    data.rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);
                    data.rectTransform.localScale = Vector3.Lerp(data.originalScale, data.originalScale * 0.5f, easeProgress);
                }
            }
            
            yield return null;
        }
        
        float spinStart = elapsedTime;
        while (elapsedTime < shuffleDuration * 0.6f)
        {
            elapsedTime += Time.deltaTime;
            float spinTime = elapsedTime - spinStart;
            
            foreach (var data in cardData)
            {
                if (data.rectTransform != null)
                {
                    float rotation = (spinTime * 720f) % 360f;
                    float wobbleX = Mathf.Sin(spinTime * 20f) * 30f;
                    float wobbleY = Mathf.Cos(spinTime * 20f) * 30f;
                    data.rectTransform.anchoredPosition = new Vector3(wobbleX, wobbleY, 0);
                    data.rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);
                }
            }
            
            yield return null;
        }
        
        float expandStart = elapsedTime;
        while (elapsedTime < shuffleDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = (elapsedTime - expandStart) / (shuffleDuration * 0.4f);
            float easeProgress = EaseInOut(progress);
            
            foreach (var data in cardData)
            {
                if (data.rectTransform != null)
                {
                    data.rectTransform.anchoredPosition = Vector3.Lerp(data.rectTransform.anchoredPosition, data.originalPosition, easeProgress);
                    data.rectTransform.localRotation = Quaternion.Lerp(data.rectTransform.localRotation, data.originalRotation, easeProgress);
                    data.rectTransform.localScale = Vector3.Lerp(data.rectTransform.localScale, data.originalScale, easeProgress);
                }
            }
            
            yield return null;
        }
        
        foreach (var data in cardData)
        {
            if (data.rectTransform != null)
            {
                data.rectTransform.anchoredPosition = data.originalPosition;
                data.rectTransform.localRotation = data.originalRotation;
                data.rectTransform.localScale = data.originalScale;
            }
        }
        
        isShuffling = false;
    }
    
    private class CardAnimData
    {
        public RectTransform rectTransform;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public Vector3 originalScale;
    }
    
    private float EaseInOut(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
    
    public void UpdateHandDisplay()
    {
        if (isShuffling)
        {
            return;
        }
        
        if (MasakuCardManager.Instance != null && MasakuCardManager.Instance.ShouldPlayShuffleAnimation())
        {
            StartCoroutine(UpdateHandWithShuffleAnimation());
            return;
        }
        
        UpdateHandDisplayImmediate();
    }
    
    IEnumerator UpdateHandWithShuffleAnimation()
    {
        
        if (cardUIObjects.Count > 0 && !isShuffling)
        {
            yield return StartCoroutine(ShuffleCardsAnimation());
        }
        
        UpdateHandDisplayImmediate();
    }
    
    void UpdateHandDisplayImmediate()
    {
        
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
        
        foreach (GameObject cardUI in cardUIObjects)
        {
            Destroy(cardUI);
        }
        cardUIObjects.Clear();
        
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
        
        int handSize = MasakuCardManager.Instance.GetHand().Count;
        
        float angleStep = handSize > 1 ? fanSpread / (handSize - 1) : 0;
        float cardAngle = (index * angleStep) - (fanSpread / 2f); // -15 to +15 for 5 cards with 30° spread
        
        float angleRad = cardAngle * Mathf.Deg2Rad;
        float x = Mathf.Sin(angleRad) * fanRadius;
        float y = -Mathf.Cos(angleRad) * fanRadius;
        
        float normalizedPosition = Mathf.Abs((index - (handSize - 1) / 2f) / (handSize / 2f)); // 0 at center, 1 at edges
        float verticalLift = cardVerticalOffset * (1f - normalizedPosition);
        y += verticalLift;
        
        RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = fanCenterPosition + new Vector3(x, y, 0);
            rectTransform.localRotation = Quaternion.Euler(0, 0, -cardAngle); // Rotate card to follow fan
            
            rectTransform.SetAsLastSibling();
        }
        
        Button cardButton = cardUI.GetComponent<Button>();
        
        if (cardButton != null)
        {
            
            if (card.isCurseCard)
            {
                cardButton.interactable = false;
                
                ColorBlock colors = cardButton.colors;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray but visible
                cardButton.colors = colors;
            }
            else
            {
                cardButton.interactable = true;
            }
            
            Image cardImage = cardButton.GetComponent<Image>();
            
            if (cardImage != null && card.cardImage != null)
            {
                cardImage.sprite = card.cardImage;
                
                cardImage.raycastTarget = true;
                
                if (isTarikNafasMode && index == tarikNafasCardIndex)
                {
                    cardUI.transform.localScale = Vector3.one * 1.3f;
                    cardImage.color = new Color(0.5f, 1f, 0.5f); // Greenish tint
                }
                else if (card.isCurseCard)
                {
                    cardUI.transform.localScale = Vector3.one * 0.9f; // Slightly smaller
                    cardImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Grayed out but opaque
                }
                else if (MasakuCardManager.Instance.IsCardSelected(index))
                {
                    if (card.isBoonCard)
                    {
                        cardUI.transform.localScale = Vector3.one * 1.3f;
                        cardImage.color = new Color(1f, 1f, 0.5f, 1f); // Bright yellow
                    }
                    else
                    {
                        cardUI.transform.localScale = Vector3.one * 1.2f; // Scale up by 20%
                        cardImage.color = Color.white; // Keep normal color
                    }
                }
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
            
            int cardIndex = index;
            cardButton.onClick.AddListener(() => OnCardClicked(cardIndex));
        }
        else
        {
            Debug.LogError("CardUI prefab missing Button component!");
        }
    }
    
    void OnCardClicked(int handIndex)
    {
        if (isExecutingOrder)
        {
            return;
        }
        
        
        List<ActionCard> hand = MasakuCardManager.Instance.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogError($"Invalid hand index: {handIndex}, hand count: {hand.Count}");
            return;
        }
        
        ActionCard card = hand[handIndex];
        
        if (card.isCurseCard)
        {
            ShowHint("Curse cards cannot be selected!");
            return;
        }
        
        if (card.isSpecialCard && !card.isBoonCard)
        {
            if (isTarikNafasMode && handIndex == tarikNafasCardIndex)
            {
                isTarikNafasMode = false;
                tarikNafasCardIndex = -1;
                UpdateHandDisplay();
                return;
            }
            
            ShowHint("Select a card to discard");
            
            PlaySound(tarikNafasSound);
            
            isTarikNafasMode = true;
            tarikNafasCardIndex = handIndex;
            
            UpdateHandDisplay();
            return;
        }
        
        if (isTarikNafasMode)
        {
            if (card.isCurseCard)
            {
                ShowHint("Cannot discard curse cards!");
                return;
            }
            
            if (card.isBoonCard)
            {
                ShowHint("Cannot discard boon cards!");
                return;
            }
            
            PlaySound(cardSelectSound);
            
            bool success = MasakuCardManager.Instance.PlayTarikNafas(tarikNafasCardIndex, handIndex);
            
            if (success)
            {
                isTarikNafasMode = false;
                tarikNafasCardIndex = -1;
                
                UpdateHandDisplay();
            }
            return;
        }
        
        if (MasakuCardManager.Instance.IsCardSelected(handIndex))
        {
            MasakuCardManager.Instance.DeselectCard(handIndex);
            
            PlaySound(cardSelectSound);
        }
        else
        {
            MasakuCardManager.Instance.SelectCard(handIndex);
            
            int selectedCount = MasakuCardManager.Instance.GetSelectedCards().Count;
            int totalFocus = 0;
            foreach (ActionCard c in MasakuCardManager.Instance.GetSelectedCards())
            {
                totalFocus += c.focusCost;
            }
            ShowHint($"✓ {selectedCount} card(s) selected | Focus cost: {totalFocus}/{GameManager.Instance.currentFocus}");
            
            PlaySound(cardSelectSound);
        }
        
        UpdateHandDisplay();
    }
    
    void UpdatePreparationStationDisplay()
    {
        if (preparationText != null)
        {
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
        
        CustomerInstance[] activeCustomers = GameManager.Instance.GetActiveCustomers();
        
        if (!isExecutingOrder)
        {
            MasakuCardManager.Instance.ClearSelection();
            selectedSeat = -1;
        }
        
        foreach (GameObject menu in activeMenus)
        {
            if (menu != null)
                Destroy(menu);
        }
        activeMenus.Clear();
        menuToSeatIndex.Clear(); // Clear seat mapping
        
        int menuCount = 0;
        for (int i = 0; i < activeCustomers.Length; i++)
        {
            if (activeCustomers[i] != null)
            {
                Customer customer = activeCustomers[i].GetCustomer();
                if (customer != null)
                {
                    
                    GameObject menuPrefab = GetMenuPrefabForCustomerType(customer.customerType);
                    
                    if (menuPrefab != null)
                    {
                        GameObject menuInstance = Instantiate(menuPrefab, customerMenuContainer);
                        activeMenus.Add(menuInstance);
                        menuToSeatIndex[menuInstance] = i; // Map this menu to its seat index
                        menuCount++;
                        
                        Button menuButton = menuInstance.GetComponent<Button>();
                        if (menuButton == null)
                        {
                            menuButton = menuInstance.GetComponentInChildren<Button>();
                        }
                        
                        if (menuButton != null)
                        {
                            int seatIndex = i; // Capture seat index for this menu
                            menuButton.onClick.AddListener(() => OnSeatSelected(seatIndex));
                            
                            menuButton.interactable = true;
                            Image menuImage = menuButton.GetComponent<Image>();
                            if (menuImage != null)
                            {
                                menuImage.raycastTarget = true;
                            }
                            
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
        }
        
        UpdateMenuScales();
        
        UpdateButtonStates();
        
        if (GameManager.Instance.isPlayerTurn)
        {
            UpdateHandDisplay();
        }
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
        if (isExecutingOrder)
        {
            return;
        }
        
        if (selectedSeat == seatIndex)
        {
            selectedSeat = -1;
        }
        else
        {
            selectedSeat = seatIndex;
        }
        
        UpdateMenuScales();
    }
    
    void UpdateMenuScales()
    {
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
                        menuRect.localScale = Vector3.one * 1.2f;
                        
                        UnityEngine.UI.Outline outline = menu.GetComponent<UnityEngine.UI.Outline>();
                        if (outline == null)
                        {
                            outline = menu.AddComponent<UnityEngine.UI.Outline>();
                            outline.effectColor = Color.yellow; // Yellow outline for selection
                            outline.effectDistance = new Vector2(5, -5); // Offset for outline visibility
                        }
                        outline.enabled = true;
                    }
                    else
                    {
                        menuRect.localScale = Vector3.one;
                        
                        UnityEngine.UI.Outline outline = menu.GetComponent<UnityEngine.UI.Outline>();
                        if (outline != null)
                        {
                            outline.enabled = false;
                        }
                    }
                }
            }
        }
    }
    
    void UpdateButtonStates()
    {
        bool hasCustomers = activeMenus.Count > 0;
        
        bool shouldEnable = hasCustomers && !isExecutingOrder;
        
        if (submitOrderButton != null)
        {
            submitOrderButton.interactable = shouldEnable;
        }
        
        if (endTurnButton != null)
        {
            endTurnButton.interactable = shouldEnable;
        }
    }
    
    void SetUIInteractable(bool interactable)
    {
        bool hasCustomers = activeMenus.Count > 0;
        
        if (submitOrderButton != null)
        {
            submitOrderButton.interactable = interactable && hasCustomers;
        }
        
        if (endTurnButton != null)
        {
            endTurnButton.interactable = interactable && hasCustomers;
        }
        
        foreach (GameObject cardObj in cardUIObjects)
        {
            if (cardObj != null)
            {
                Button cardButton = cardObj.GetComponent<Button>();
                if (cardButton != null)
                {
                    cardButton.interactable = interactable;
                    
                    ColorBlock colors = cardButton.colors;
                    colors.disabledColor = Color.white; // Same as normal color
                    cardButton.colors = colors;
                }
            }
        }
        
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
                    
                    ColorBlock colors = menuButton.colors;
                    colors.disabledColor = Color.white; // Same as normal color
                    menuButton.colors = colors;
                }
            }
        }
        
    }
    
    void OnSubmitOrderClicked()
    {
        PlaySound(buttonClickSound);
        
        List<ActionCard> selectedCards = MasakuCardManager.Instance.GetSelectedCards();
        if (selectedCards.Count == 0)
        {
            Debug.LogWarning("Pilih kartu terlebih dahulu!");
            ShowHint("Select cards first!");
            return;
        }
        
        int totalFocusCost = 0;
        foreach (ActionCard card in selectedCards)
        {
            totalFocusCost += card.focusCost;
        }
        
        if (totalFocusCost > GameManager.Instance.currentFocus)
        {
            Debug.LogWarning($"Not enough focus! Need {totalFocusCost}, have {GameManager.Instance.currentFocus}");
            ShowHint($"❌ Not enough focus! Need {totalFocusCost}, have {GameManager.Instance.currentFocus}");
            return;
        }
        
        bool allBoonCards = selectedCards.All(c => c.isBoonCard);
        
        if (allBoonCards)
        {
            StartCoroutine(ExecuteBoonCards());
        }
        else
        {
            if (selectedSeat < 0)
            {
                Debug.LogWarning("Pilih kursi customer terlebih dahulu!");
                ShowHint("Select a customer seat first!");
                return;
            }
            
            StartCoroutine(ExecuteAndSubmitOrder());
        }
    }
    
    IEnumerator ExecuteBoonCards()
    {
        isExecutingOrder = true;
        
        SetUIInteractable(false);
        
        List<ActionCard> selectedCards = MasakuCardManager.Instance.GetSelectedCards();
        
        foreach (ActionCard card in selectedCards)
        {
            if (card.isBoonCard)
            {
                MasakuCardManager.Instance.PlayCard(card);
                yield return new WaitForSeconds(0.3f); // Small delay between boons
            }
        }
        
        UpdateHandDisplay();
        
        isExecutingOrder = false;
        SetUIInteractable(true);
    }
    
    IEnumerator ExecuteAndSubmitOrder()
    {
        isExecutingOrder = true;
        
        SetUIInteractable(false);
        
        yield return StartCoroutine(MasakuCardManager.Instance.ExecuteSelectedCards());
        
        UpdateHandDisplay();
        
        if (GameManager.Instance.preparationStation.Count > 0)
        {
            GameManager.Instance.SubmitOrder(selectedSeat);
            selectedSeat = -1;
            
            UpdateCustomerMenus();
        }
        
        isExecutingOrder = false;
        SetUIInteractable(true);
    }
    
    void OnEndTurnClicked()
    {
        PlaySound(buttonClickSound);
        
        if (endTurnButton != null)
            endTurnButton.interactable = false;
        
        GameManager.Instance.EndPlayerTurn();
        
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pause the game
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
    }
    
    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume the game
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
    
    void OnPauseContinueClicked()
    {
        PlaySound(buttonClickSound);
        
        ResumeGame();
    }
    
    void OnPauseExitClicked()
    {
        PlaySound(buttonClickSound);
        
        Time.timeScale = 1f;
        isPaused = false;
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    public void ShowHint(string message)
    {
        if (hintText == null || hintCanvasGroup == null) return;
        
        if (currentHintCoroutine != null)
        {
            StopCoroutine(currentHintCoroutine);
        }
        
        currentHintCoroutine = StartCoroutine(ShowHintCoroutine(message));
    }
    
    IEnumerator ShowHintCoroutine(string message)
    {
        hintText.text = message;
        
        float elapsedTime = 0f;
        while (elapsedTime < hintFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            hintCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / hintFadeDuration);
            yield return null;
        }
        hintCanvasGroup.alpha = 1f;
        
        yield return new WaitForSeconds(hintDisplayDuration);
        
        elapsedTime = 0f;
        while (elapsedTime < hintFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            hintCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / hintFadeDuration);
            yield return null;
        }
        hintCanvasGroup.alpha = 0f;
        
        currentHintCoroutine = null;
    }
    
    public void HideHint()
    {
        if (currentHintCoroutine != null)
        {
            StopCoroutine(currentHintCoroutine);
            currentHintCoroutine = null;
        }
        
        if (hintCanvasGroup != null)
        {
            hintCanvasGroup.alpha = 0f;
        }
    }
}
