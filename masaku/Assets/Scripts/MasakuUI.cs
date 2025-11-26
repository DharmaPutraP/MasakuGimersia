using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MasakuUI : MonoBehaviour
{
    public static MasakuUI Instance;
    
    [Header("Focus Display")]
    public TextMeshProUGUI focusText;
    public Image focusBar;
    
    [Header("Reputation Display")]
    public Image[] reputationStars; 
    public Sprite starOnSprite; 
    public Sprite starOffSprite; 
    
    [Header("Day Display")]
    public TextMeshProUGUI dayText;
    
    [Header("Timer Display")]
    public TextMeshProUGUI timerText;
    
    [Header("Hand Display")]
    public Transform handContainer;
    public GameObject cardUIPrefab;
    private List<GameObject> cardUIObjects = new List<GameObject>();
    
    [Header("Fan Layout Settings")]
    public float fanSpread = 30f; 
    public float fanRadius = 300f; 
    public float cardVerticalOffset = 50f; 
    public Vector3 fanCenterPosition = new Vector3(0, -200, 0); 
    
    [Header("Preparation Station Display")]
    public Transform preparationContainer;
    public TextMeshProUGUI preparationText;
    
    [Header("Buttons")]
    public Button submitOrderButton;
    public Button endTurnButton;
    public Button[] seatButtons; 
    
    [Header("Customer Display")]
    public Transform[] customerUIPositions; 
    public Transform customerMenuContainer; 
    public GameObject knightMenuPrefab; 
    public GameObject elfMenuPrefab; 
    public GameObject wizardMenuPrefab; 
    public GameObject barbarianMenuPrefab; 
    
    [Header("Sound Effects")]
    public AudioSource audioSource; 
    public AudioClip buttonClickSound; 
    public AudioClip cardSelectSound; 
    public AudioClip tarikNafasSound; 
    
    [Header("Hint System")]
    public TextMeshProUGUI hintText; 
    public CanvasGroup hintCanvasGroup; 
    public float hintFadeDuration = 0.3f; 
    public float hintDisplayDuration = 2f; 
    private Coroutine currentHintCoroutine;
    
    [Header("Pause Menu")]
    public GameObject pausePanel; 
    public Button pauseContinueButton; 
    public Button pauseExitButton; 
    private bool isPaused = false;
    
    [Header("Shuffle Animation")]
    public float shuffleAnimationDuration = 1.0f; 
    private bool isShuffling = false;
    
    private List<GameObject> activeMenus = new List<GameObject>(); 
    private Dictionary<GameObject, int> menuToSeatIndex = new Dictionary<GameObject, int>(); 
    
    private int selectedSeat = -1;
    private int highlightedSeat = -1;
    private bool isExecutingOrder = false; 
    private bool isTarikNafasMode = false; 
    private int tarikNafasCardIndex = -1; 
    
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
        UpdateCustomerMenus(); 
        
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
        UpdateTimerDisplay();
        UpdatePreparationStationDisplay();
    }
    
    public void UpdateUI()
    {
        UpdateFocusDisplay();
        UpdateReputationDisplay();
        UpdateDayDisplay();
        UpdateTimerDisplay();
        UpdateHandDisplay();
        UpdatePreparationStationDisplay();
        UpdateCustomerMenus(); 
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
                        reputationStars[i].color = Color.white; 
                    }
                    else
                    {
                        reputationStars[i].sprite = starOffSprite;
                        reputationStars[i].color = new Color(1f, 1f, 1f, 0.5f); 
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
    
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            float time = GameManager.Instance.GetGameTime();
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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
        
        float phaseDuration = shuffleAnimationDuration / 3f;
        
        Sequence shuffleSequence = DOTween.Sequence();
        
        foreach (var data in cardData)
        {
            if (data.rectTransform != null)
            {
                shuffleSequence.Join(data.rectTransform.DOAnchorPos(new Vector2(500f,-50f), phaseDuration).SetEase(Ease.InOutQuad));
                shuffleSequence.Join(data.rectTransform.DORotate(new Vector3(0, 0, 720f), phaseDuration, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));
                shuffleSequence.Join(data.rectTransform.DOScale(data.originalScale * 0.5f, phaseDuration).SetEase(Ease.InOutQuad));
            }
        }
        
        shuffleSequence.AppendInterval(0.1f);
        
        foreach (var data in cardData)
        {
            if (data.rectTransform != null)
            {
                shuffleSequence.Join(data.rectTransform.DORotate(new Vector3(0, 0, 1440f), phaseDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            }
        }
        
        foreach (var data in cardData)
        {
            if (data.rectTransform != null)
            {
                shuffleSequence.Join(data.rectTransform.DOAnchorPos(data.originalPosition, phaseDuration).SetEase(Ease.InOutQuad));
                shuffleSequence.Join(data.rectTransform.DORotate(data.originalRotation.eulerAngles, phaseDuration, RotateMode.Fast).SetEase(Ease.InOutQuad));
                shuffleSequence.Join(data.rectTransform.DOScale(data.originalScale, phaseDuration).SetEase(Ease.InOutQuad));
            }
        }
        
        shuffleSequence.OnComplete(() => {
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
        });
        
        yield return shuffleSequence.WaitForCompletion();
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
        float cardAngle = (index * angleStep) - (fanSpread / 2f); 
        
        float angleRad = cardAngle * Mathf.Deg2Rad;
        float x = Mathf.Sin(angleRad) * fanRadius;
        float y = -Mathf.Cos(angleRad) * fanRadius;
        
        float normalizedPosition = Mathf.Abs((index - (handSize - 1) / 2f) / (handSize / 2f)); 
        float verticalLift = cardVerticalOffset * (1f - normalizedPosition);
        y += verticalLift;
        
        RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = fanCenterPosition + new Vector3(x, y, 0);
            rectTransform.localRotation = Quaternion.Euler(0, 0, -cardAngle); 
            
            rectTransform.SetAsLastSibling();
        }
        
        Button cardButton = cardUI.GetComponent<Button>();
        
        if (cardButton != null)
        {
            
            if (card.isCurseCard)
            {
                cardButton.interactable = false;
                
                ColorBlock colors = cardButton.colors;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); 
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
                    cardUI.transform.localScale = Vector3.one * 1.15f;
                    cardImage.color = new Color(0.5f, 1f, 0.5f); 
                }
                else if (card.isCurseCard)
                {
                    cardUI.transform.localScale = Vector3.one * 0.9f; 
                    cardImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); 
                }
                else if (MasakuCardManager.Instance.IsCardSelected(index))
                {
                    if (card.isBoonCard)
                    {
                        cardUI.transform.localScale = Vector3.one * 1.15f;
                        cardImage.color = new Color(1f, 1f, 0.5f, 1f); 
                    }
                    else
                    {
                        cardUI.transform.localScale = Vector3.one * 1.1f; 
                        cardImage.color = Color.white; 
                    }
                }
                else if (card.isBoonCard)
                {
                    cardUI.transform.localScale = Vector3.one * 1.1f; 
                    cardImage.color = new Color(1f, 0.9f, 0.3f, 1f); 
                }
                else
                {
                    cardUI.transform.localScale = Vector3.one; 
                    cardImage.color = Color.white; 
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
        
        int previousMenuCount = activeMenus.Count;
        
        foreach (GameObject menu in activeMenus)
        {
            if (menu != null)
                Destroy(menu);
        }
        activeMenus.Clear();
        menuToSeatIndex.Clear(); 
        
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
                        menuToSeatIndex[menuInstance] = i; 
                        menuCount++;
                        
                        Button menuButton = menuInstance.GetComponent<Button>();
                        if (menuButton == null)
                        {
                            menuButton = menuInstance.GetComponentInChildren<Button>();
                        }
                        
                        if (menuButton != null)
                        {
                            int seatIndex = i; 
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
        
        if (!isExecutingOrder && previousMenuCount > menuCount)
        {
            MasakuCardManager.Instance.ClearSelection();
            selectedSeat = -1;
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
        
        if (highlightedSeat == seatIndex)
        {
            highlightedSeat = -1;
        }
        else
        {
            highlightedSeat = seatIndex;
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
        UpdateCustomerHighlight();
    }
    
    public void OnCustomerClicked(int seatIndex)
    {
        if (isExecutingOrder)
        {
            return;
        }
        
        if (highlightedSeat == seatIndex)
        {
            highlightedSeat = -1;
        }
        else
        {
            highlightedSeat = seatIndex;
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
        UpdateCustomerHighlight();
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
                        menuRect.localScale = Vector3.one * 1.1f;
                        
                        UnityEngine.UI.Outline outline = menu.GetComponent<UnityEngine.UI.Outline>();
                        if (outline == null)
                        {
                            outline = menu.AddComponent<UnityEngine.UI.Outline>();
                            outline.effectColor = Color.yellow; 
                            outline.effectDistance = new Vector2(5, -5); 
                        }
                        outline.enabled = true;
                    }
                    else if (menuSeat == highlightedSeat && highlightedSeat >= 0)
                    {
                        menuRect.localScale = Vector3.one * 1.08f;
                        
                        UnityEngine.UI.Outline outline = menu.GetComponent<UnityEngine.UI.Outline>();
                        if (outline == null)
                        {
                            outline = menu.AddComponent<UnityEngine.UI.Outline>();
                        }
                        outline.effectColor = Color.yellow;
                        outline.effectDistance = new Vector2(5, -5);
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
    
    void UpdateCustomerHighlight()
    {
        CustomerInstance[] customers = GameManager.Instance.GetActiveCustomers();
        
        for (int i = 0; i < customers.Length; i++)
        {
            if (customers[i] != null)
            {
                if (i == highlightedSeat && highlightedSeat >= 0)
                {
                    customers[i].SetHighlight(true);
                }
                else
                {
                    customers[i].SetHighlight(false);
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
                    colors.disabledColor = Color.white; 
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
                    colors.disabledColor = Color.white; 
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
                yield return new WaitForSeconds(0.3f); 
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
            highlightedSeat = -1;
            
            UpdateCustomerMenus();
            UpdateCustomerHighlight();
        }
        
        isExecutingOrder = false;
        SetUIInteractable(true);
    }
    
    void OnEndTurnClicked()
    {
        PlaySound(buttonClickSound);
        
        if (isTarikNafasMode)
        {
            isTarikNafasMode = false;
            tarikNafasCardIndex = -1;
            UpdateHandDisplay();
        }
        
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
        Time.timeScale = 0f; 
        
        AudioListener.pause = true;
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
    }
    
    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        
        AudioListener.pause = false;
        
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
        AudioListener.pause = false;
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
