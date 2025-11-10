# Masaku Gimersia - Complete Game Workflow

## 🎮 Game Overview

A deck-building cooking game where you serve customers using action cards while managing Focus and Reputation over 7 days.

---

## 📊 Core Game Loop

```
Game Start
    ↓
Day Start (Day 1-7)
    ↓
Load Customer Deck for Day
    ↓
Fill 4 Customer Seats
    ↓
┌─────────────────────────┐
│   PLAYER TURN PHASE     │
├─────────────────────────┤
│ 1. Restore Focus (3+)   │
│ 2. Draw 5 Cards         │
│ 3. Clear Prep Station   │
└─────────────────────────┘
    ↓
┌──────────────────────────────────────────┐
│         PLAYER ACTIONS                    │
├──────────────────────────────────────────┤
│ ► Play Cards (spend Focus)               │
│   - Move to location                      │
│   - Add to Preparation Station            │
│                                           │
│ ► Submit Order to Customer                │
│   - Select seat (0-3)                     │
│   - Check combo matches order             │
│   - Give reward OR decrease patience      │
│                                           │
│ ► End Turn                                │
└──────────────────────────────────────────┘
    ↓
┌─────────────────────────┐
│   PATIENCE PHASE        │
├─────────────────────────┤
│ All customers lose      │
│ patience (decay rate)   │
│                         │
│ If patience = 0:        │
│   - Lose reputation     │
│   - Barbarian adds curse│
│   - Remove customer     │
└─────────────────────────┘
    ↓
Fill Empty Seats (if deck has more customers)
    ↓
Check Day Complete
    ↓
┌─────────────┬─────────────┐
│  More Days  │  Day 7 Done │
│  Reputation │  Reputation │
│     > 0     │     > 0     │
└─────────────┴─────────────┘
    ↓              ↓
Next Day        VICTORY
    ↓              ↓
Reputation      GAME OVER
   = 0
```

---

## 🎯 Detailed System Workflows

### 1️⃣ **Game Initialization** (`GameManager.Start()`)

```csharp
Start()
├─ StartDay(1)
│  ├─ LoadDayDeck(1)
│  │  ├─ Get DayConfiguration[0]
│  │  ├─ Queue all CustomerTypes for day
│  │  └─ Debug: X pelanggan loaded
│  │
│  ├─ FillEmptySeats()
│  │  └─ For each empty seat (0-3):
│  │     ├─ Dequeue customer from day deck
│  │     └─ SpawnCustomer(customer, seatIndex)
│  │        ├─ Instantiate customerPrefab at seat position
│  │        ├─ CustomerInstance.Initialize()
│  │        │  ├─ Set customerData
│  │        │  ├─ Set currentPatience = maxPatience
│  │        │  ├─ Update sprite
│  │        │  ├─ UpdatePatienceBar()
│  │        │  └─ UpdateOrderDisplay()
│  │        └─ Store in activeCustomers[seatIndex]
│  │
│  └─ StartPlayerTurn()
│     ├─ Reset Focus = 3 + bonusFocusNextTurn
│     ├─ Clear bonusFocusNextTurn
│     ├─ MasakuCardManager.DrawToHandSize()
│     │  ├─ Calculate cards needed (5 - hand.Count)
│     │  └─ DrawCards(count)
│     │     └─ For each card:
│     │        ├─ Check deck not empty (else reshuffle)
│     │        ├─ Take top card from deck
│     │        ├─ Add to hand
│     │        └─ Debug log
│     ├─ Clear preparationStation
│     └─ Debug: Turn started with X focus
```

---

### 2️⃣ **Playing a Card** (`MasakuCardManager.PlayCard()`)

```csharp
Player Clicks Card UI
    ↓
MasakuUI.OnCardClicked(handIndex)
    ↓
MasakuCardManager.PlayCardByIndex(handIndex)
    ↓
MasakuCardManager.PlayCard(card)
    ├─ Validate: Card in hand?
    │  └─ NO → Warning & return false
    │
    ├─ Check: Is Special Card? (e.g., Tarik Nafas)
    │  └─ YES:
    │     ├─ ExecuteSpecialCard(card)
    │     │  ├─ If discardCount > 0:
    │     │  │  └─ Discard 1 card from hand
    │     │  └─ If drawCount > 0:
    │     │     └─ DrawCards(drawCount)
    │     ├─ Remove from hand
    │     ├─ Add to discardPile
    │     └─ return true
    │
    ├─ Check: Enough Focus?
    │  └─ NO → Warning & return false
    │
    ├─ SpendFocus(card.focusCost)
    │
    ├─ Check: Has targetTag? AND playerMovement exists?
    │  └─ YES:
    │     ├─ Get location: KitchenLocationManager.GetLocationPosition(targetTag)
    │     └─ PlayerMovement.MoveToLocation(targetPos, card)
    │        ├─ Set targetPosition
    │        ├─ Set isMoving = true
    │        ├─ Animator: IsWalking = true
    │        └─ Debug: Moving to targetTag
    │
    ├─ GameManager.AddCardToPreparation(card.cardType)
    │  ├─ Add to preparationStation list
    │  └─ Debug: Card added, total: X
    │
    ├─ Remove card from hand
    ├─ Add card to discardPile
    ├─ Debug: Playing card
    └─ return true
```

**Player Movement During Card Play:**

```csharp
PlayerMovement.Update()
    └─ If isMoving:
       └─ MoveTowardsTarget()
          ├─ Calculate distance to target
          ├─ If distance <= stoppingDistance:
          │  └─ ArrivedAtDestination()
          │     ├─ Stop moving
          │     ├─ Animator: IsWalking = false
          │     ├─ Debug: Arrived at targetTag
          │     └─ ExecuteActionAtLocation()
          │        ├─ Switch on cardType:
          │        │  ├─ PotongSayuran → PerformAction("Potong Sayuran", 2s)
          │        │  ├─ PotongDaging → PerformAction("Potong Daging", 2s)
          │        │  ├─ PanaskanAir → PerformAction("Panaskan Air", 3s)
          │        │  └─ PanaskanDaging → PerformAction("Panaskan Daging", 3s)
          │        └─ Trigger animation
          │
          ├─ Else: Move towards target
          │  ├─ Calculate direction
          │  ├─ Move position (speed * deltaTime)
          │  └─ Rotate to face direction
          └─ Return
```

---

### 3️⃣ **Submitting Order to Customer** (`GameManager.SubmitOrder()`)

```csharp
Player Clicks Seat Button
    ↓
MasakuUI.OnSeatSelected(seatIndex)
    ├─ Set selectedSeat = seatIndex
    └─ Debug: Seat selected
    ↓
Player Clicks "Submit Order" Button
    ↓
MasakuUI.OnSubmitOrderClicked()
    ├─ Validate: Seat selected?
    ├─ Validate: Preparation station not empty?
    └─ GameManager.SubmitOrder(selectedSeat)
       ├─ Validate: seatIndex valid (0-3)?
       ├─ Validate: Customer exists at seat?
       ├─ Validate: Preparation station not empty?
       │
       └─ CustomerInstance.TryServeOrder(preparationStation)
          ├─ IsOrderCorrect(comboCards)
          │  ├─ Check: Same number of cards?
          │  ├─ Check: All cards match requiredCards?
          │  │  └─ Use list comparison (order doesn't matter)
          │  └─ Return true/false
          │
          ├─ If correct:
          │  └─ OnOrderCorrect()
          │     ├─ Set isServed = true
          │     ├─ Debug: Customer satisfied
          │     ├─ GameManager.AddFocus(focusReward)
          │     ├─ Check: Is Wizard AND patience > 5?
          │     │  └─ YES: GameManager.GiveWizardBoon()
          │     │     ├─ Random(0,2)
          │     │     ├─ 0 → Add wizardBoonDraw to deck
          │     │     └─ 1 → Add wizardBoonFocus to deck
          │     ├─ GameManager.RemoveCustomer(seatIndex)
          │     │  └─ Set activeCustomers[seatIndex] = null
          │     └─ StartCoroutine(CustomerLeaveHappy())
          │        ├─ Wait 1 second
          │        └─ Destroy(gameObject)
          │
          └─ If incorrect:
             └─ OnOrderWrong()
                ├─ Debug: Wrong order
                ├─ currentPatience -= 1
                ├─ UpdatePatienceBar()
                └─ Check: patience <= 0?
                   └─ YES: OnCustomerAngry()
       ├─ Clear preparationStation
       └─ Debug result
```

---

### 4️⃣ **End Turn Phase** (`GameManager.EndPlayerTurn()`)

```csharp
Player Clicks "End Turn" Button
    ↓
MasakuUI.OnEndTurnClicked()
    ↓
GameManager.EndPlayerTurn()
    ├─ Check: isPlayerTurn? (prevent double-click)
    ├─ Set isPlayerTurn = false
    ├─ Debug: Turn ended
    │
    ├─ MasakuCardManager.DiscardHand()
    │  └─ Move all cards from hand to discardPile
    │
    ├─ Clear preparationStation
    │
    └─ StartCoroutine(PatiencePhase())
       ├─ Debug: Patience Phase
       │
       ├─ For each activeCustomer:
       │  └─ CustomerInstance.DecreasePatience()
       │     ├─ Check: already served? → Skip
       │     ├─ currentPatience -= patienceDecayPerTurn
       │     ├─ Clamp to 0
       │     ├─ UpdatePatienceBar()
       │     │  └─ Scale bar: (current/max)
       │     │
       │     └─ Check: patience <= 0?
       │        └─ YES: OnCustomerAngry()
       │           ├─ Debug: Customer left angry
       │           ├─ GameManager.LoseReputation(1)
       │           │  ├─ reputation -= 1
       │           │  ├─ Clamp to 0
       │           │  ├─ Debug: Reputation lost
       │           │  └─ If reputation = 0: GameOver()
       │           ├─ Check: isBarbarian?
       │           │  └─ YES: GameManager.AddCurseCard()
       │           │     └─ MasakuCardManager.AddCurseToDiscard()
       │           ├─ GameManager.RemoveCustomer(seatIndex)
       │           └─ Destroy(gameObject)
       │
       ├─ Check: IsDayComplete()?
       │  ├─ currentDayDeck.Count = 0?
       │  ├─ All activeCustomers = null?
       │  └─ Return true if both
       │
       ├─ If Day Complete:
       │  └─ EndDay()
       │     ├─ Debug: Day X complete
       │     ├─ Debug: Reputation remaining
       │     │
       │     ├─ Check: reputation <= 0?
       │     │  └─ YES: GameOver()
       │     │
       │     ├─ Check: currentDay >= 7?
       │     │  └─ YES: Victory()
       │     │
       │     └─ Else: PrepareNextDay()
       │        ├─ Wait 2 seconds
       │        ├─ MasakuCardManager.ResetDeck()
       │        │  └─ InitializeDeck()
       │        │     ├─ Clear deck/hand/discard
       │        │     ├─ Add 2x each baseActionCard
       │        │     └─ ShuffleDeck()
       │        └─ StartDay(currentDay + 1)
       │
       └─ Else (Day Not Complete):
          ├─ FillEmptySeats()
          │  └─ Spawn new customers from day deck
          ├─ Wait 1 second
          └─ StartPlayerTurn()
```

---

## 🧩 Key Systems Integration

### **Card System Flow**

```
ActionCard ScriptableObject
    ├─ cardName: "Potong Daging"
    ├─ cardType: PotongDaging
    ├─ focusCost: 1
    ├─ targetTag: "CuttingBoard"
    ├─ cardImage: Sprite
    ├─ description: "..."
    └─ isSpecialCard: false

Used by:
├─ MasakuCardManager (deck/hand/discard management)
├─ PlayerMovement (move to targetTag location)
├─ GameManager (add to preparation station)
└─ MasakuUI (display card in hand)
```

### **Customer System Flow**

```
Customer ScriptableObject
    ├─ customerName: "Knight"
    ├─ customerType: Knight
    ├─ maxPatience: 10
    ├─ patienceDecayPerTurn: 2
    ├─ requiredCards: [PotongDaging, PanaskanDaging]
    ├─ focusReward: 2
    ├─ isBarbarian: false
    └─ isWizard: false

Instantiated as:
├─ CustomerInstance (MonoBehaviour on GameObject)
    ├─ customerData: Customer
    ├─ currentPatience: int (dynamic)
    ├─ seatIndex: 0-3
    └─ UI References:
       ├─ customerSprite: SpriteRenderer
       ├─ patienceBarTransform: Transform
       └─ orderDisplayText: TextMeshProUGUI

Managed by:
└─ GameManager
   ├─ customerSeats[4]: Transform array
   └─ activeCustomers[4]: CustomerInstance array
```

### **Location System Flow**

```
KitchenLocationManager
    └─ locations: List<KitchenLocation>
       ├─ locationTag: "CuttingBoard"
       ├─ locationTransform: Transform
       └─ locationName: "Main Cutting Board"

Used by:
└─ MasakuCardManager.PlayCard()
   └─ Get location → Move player → Execute action
```

### **UI System Flow**

```
MasakuUI (Real-time Updates)
    ├─ Update() called every frame
    │  └─ UpdateUI()
    │     ├─ UpdateFocusDisplay()
    │     │  └─ focusText.text = "Fokus: X"
    │     ├─ UpdateReputationDisplay()
    │     │  ├─ reputationText.text = "Reputasi: X/5"
    │     │  └─ Show/hide heart icons
    │     ├─ UpdateDayDisplay()
    │     │  └─ dayText.text = "Hari X/7"
    │     ├─ UpdateHandDisplay()
    │     │  ├─ Destroy old card UI objects
    │     │  ├─ Get hand from MasakuCardManager
    │     │  └─ CreateCardUI() for each card
    │     └─ UpdatePreparationStationDisplay()
    │        └─ Show cards in prep station
    │
    └─ Button Listeners:
       ├─ Card clicked → PlayCardByIndex()
       ├─ Seat clicked → Select seat
       ├─ Submit Order → SubmitOrder(selectedSeat)
       └─ End Turn → EndPlayerTurn()
```

---

## 📦 Data Objects (ScriptableObjects)

### **ActionCard**

- Location: `Assets/Card/`
- Examples:
  - Potong Daging.asset
  - Panaskan Air.asset
  - Tarik Nafas.asset
  - Barbarian Curse.asset
  - Wizard Boon Draw.asset
  - Wizard Boon Focus.asset

### **Customer**

- Location: `Assets/Customer/`
- Examples:
  - Knight.asset
  - Elf.asset
  - Barbarian.asset
  - Wizard.asset

### **DayConfiguration**

- Stored in: GameManager.dayConfigurations
- Structure:
  ```csharp
  DayConfiguration
  ├─ day: 1
  ├─ dayName: "Hari Pertama"
  └─ customerQueue: [Knight, Elf, Knight, Elf, Barbarian]
  ```

---

## 🎲 Special Mechanics

### **Wizard Boon System**

```
Wizard served with patience > 5
    ↓
GameManager.GiveWizardBoon()
    ├─ Random: 0 or 1
    ├─ 0 → Add "Wizard Boon Draw" to deck
    │      (Draw 2 cards when played)
    └─ 1 → Add "Wizard Boon Focus" to deck
           (+1 Focus next turn)
```

### **Barbarian Curse System**

```
Barbarian leaves angry (patience = 0)
    ↓
GameManager.AddCurseCard()
    ↓
MasakuCardManager.AddCurseToDiscard()
    └─ Add "Barbarian Curse" card to discard pile
       (Negative effect card in future draws)
```

### **Deck Cycling**

```
Draw card when deck empty
    ↓
ReshuffleDiscardPile()
    ├─ Move all cards from discardPile to deck
    ├─ Clear discardPile
    └─ ShuffleDeck()
       └─ Fisher-Yates shuffle algorithm
```

---

## 🏆 Win/Lose Conditions

### **Victory**

```
Complete Day 7 AND reputation > 0
    ↓
GameManager.Victory()
    └─ Debug: "Golden Bean Award!"
    └─ TODO: Load Victory scene
```

### **Game Over**

```
Reputation reaches 0 (any time)
    ↓
GameManager.GameOver()
    └─ Debug: "Reputasi habis!"
    └─ TODO: Load Game Over scene
```

---

## 🔧 Manager Hierarchy

```
GameManager (Singleton)
    ├─ Manages: Game state, day, reputation, focus
    ├─ References: MasakuCardManager, KitchenLocationManager
    └─ Controls: Customer spawning, turn flow, win/lose

MasakuCardManager (Singleton)
    ├─ Manages: Deck, hand, discard pile
    ├─ References: PlayerMovement
    └─ Controls: Card drawing, playing, shuffling

KitchenLocationManager (Singleton)
    ├─ Manages: Location tags and transforms
    └─ Provides: Location lookup by tag

MasakuUI (Singleton)
    ├─ Manages: All UI elements
    ├─ References: GameManager, MasakuCardManager
    └─ Controls: Display updates, button listeners

PlayerMovement
    ├─ Manages: Character movement and animation
    ├─ References: Animator
    └─ Controls: Movement to locations, action execution
```

---

## 🎬 Typical Play Session

```
1. Game Starts → Day 1
   - 4 customers sit down (from day deck)
   - Player draws 5 cards
   - Has 3 focus

2. Player Turn:
   - Plays "Potong Daging" (cost: 1 focus)
     → Character moves to CuttingBoard
     → Card added to Preparation Station
     → Now has 2 focus

   - Plays "Panaskan Daging" (cost: 1 focus)
     → Character moves to Stove
     → Card added to Preparation Station
     → Now has 1 focus

   - Clicks Seat 0 (Knight)
   - Clicks "Submit Order"
     → Checks: [PotongDaging, PanaskanDaging]
     → Matches Knight's order!
     → Knight gives +2 Focus reward
     → Knight leaves happy

3. Player ends turn

4. Patience Phase:
   - All remaining customers lose 2 patience
   - Customer 1 reaches 0 patience → Leaves angry
   - Lose 1 reputation (now 4/5)

5. New customers fill empty seats

6. Next turn starts with 3 focus again

7. Repeat until day complete

8. Next day starts...

9. Continue until Day 7 complete or reputation = 0
```

---

## 📝 Notes for Development

### **Current State:**

✅ Core game loop implemented  
✅ Card system with movement  
✅ Customer system with orders  
✅ Turn-based flow  
✅ Win/lose conditions  
✅ Special cards (Wizard Boon, Barbarian Curse)

### **TODO:**

- [ ] Victory/Game Over scene loading
- [ ] Sound effects and music
- [ ] Enhanced animations
- [ ] Tutorial system
- [ ] Save/Load system
- [ ] More card types
- [ ] More customer types
- [ ] Daily challenges/modifiers

---

_This workflow represents the complete game logic as implemented in the current codebase._
