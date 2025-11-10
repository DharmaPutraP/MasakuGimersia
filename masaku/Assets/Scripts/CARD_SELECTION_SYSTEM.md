# Card Selection & Batch Execution System

## Overview

Players now select multiple cards to form a combo, then execute all movements at once when serving a customer.

---

## 🎮 New Gameplay Flow

### Old System (One-by-One):

```
Click Card → Character moves → Action done → Repeat
```

### New System (Batch Selection):

```
1. Select cards (click to add to combo)
2. Select customer seat
3. Click "Submit Order" button
4. Character executes ALL movements sequentially
5. Order submitted to customer
```

---

## 📋 Detailed Workflow

### Phase 1: Card Selection

```
Turn Start: [Hand: 5 cards] [Focus: 3] [Selected: None]
    ↓
Player clicks "Potong Daging" card
    ├─ Check: Card in hand? ✓
    ├─ Check: Already selected? ✗
    ├─ Check: Is special card? ✗
    ├─ Check: Enough focus? (1/3) ✓
    ├─ Add to selectedCards list
    └─ Highlight card (yellow tint)

[Selected: Potong Daging (1 focus)]
    ↓
Player clicks "Panaskan Air" card
    ├─ Check: Total focus needed? (1+1=2/3) ✓
    ├─ Add to selectedCards list
    └─ Highlight card

[Selected: Potong Daging, Panaskan Air (2 focus)]
    ↓
Player can click again to deselect
or continue selecting more cards
```

### Phase 2: Customer Selection

```
Player clicks Seat Button (e.g., Seat 0 = Knight)
    ├─ selectedSeat = 0
    └─ Visual feedback on button
```

### Phase 3: Submit & Execute

```
Player clicks "Submit Order" button
    ↓
OnSubmitOrderClicked()
    ├─ Validate: Seat selected? ✓
    ├─ Validate: Cards selected? ✓
    └─ Start: ExecuteAndSubmitOrder()

ExecuteAndSubmitOrder()
    ↓
    ├─ Calculate total focus: 2
    ├─ GameManager.SpendFocus(2)
    ├─ Focus: 3 → 1
    │
    ├─ Execute Card 1: "Potong Daging"
    │  ├─ Move to: StorageMeat (pickup)
    │  │  └─ Wait until arrived
    │  ├─ Move to: CuttingBoard (target)
    │  │  └─ Wait until arrived
    │  ├─ Perform cutting action
    │  └─ Add to preparationStation
    │
    ├─ Execute Card 2: "Panaskan Air"
    │  ├─ Move to: Stove (target - no pickup needed)
    │  │  └─ Wait until arrived
    │  ├─ Perform heating action
    │  └─ Add to preparationStation
    │
    ├─ All movements complete
    ├─ Move cards to discard pile
    ├─ Clear selection
    │
    └─ Submit Order:
       ├─ preparationStation: [PotongDaging, PanaskanAir]
       ├─ CustomerInstance.TryServeOrder()
       ├─ Check if matches Knight's order
       └─ Result: Success/Fail
```

---

## 🔧 Key Methods

### MasakuCardManager.cs

#### `SelectCard(ActionCard card)`

```csharp
// Add card to selection (doesn't execute)
// Returns: true if successfully selected
// Checks:
// - Card in hand?
// - Already selected?
// - Not special card?
// - Enough focus for total combo?
```

#### `DeselectCard(ActionCard card)`

```csharp
// Remove card from selection
// Returns: true if successfully deselected
```

#### `ClearSelection()`

```csharp
// Remove all selected cards
// Called at end of turn
```

#### `ExecuteSelectedCards()`

```csharp
// Coroutine that:
// 1. Spends total focus
// 2. Executes each card sequentially
// 3. Waits for movement to complete
// 4. Moves cards to discard
// 5. Clears selection
```

#### `IsCardSelected(ActionCard card)`

```csharp
// Check if card is currently selected
// Used for UI highlighting
```

---

## 🎨 UI Updates

### Card Display

```
Normal Card:
├─ Background: Default color
└─ Click: Add to selection

Selected Card:
├─ Background: Yellow tint (1f, 1f, 0.5f)
└─ Click: Remove from selection
```

### Preparation Display

```
Before Execution:
"Kartu Dipilih: [Potong Daging] [Panaskan Air]"

After Execution:
"Di Stasiun: [PotongDaging] [PanaskanAir]"
```

---

## 🎯 Example Gameplay

### Scenario: Serving Knight

```
Knight's Order: [PotongDaging, PanaskanAir]

Turn Start:
├─ Hand: [Potong Daging, Potong Sayuran, Panaskan Air,
│         Panaskan Daging, Tarik Nafas]
├─ Focus: 3
└─ Selected: []

Action 1: Click "Potong Daging" card
└─ Selected: [Potong Daging]
   Focus Preview: 1/3 will be spent

Action 2: Click "Panaskan Air" card
└─ Selected: [Potong Daging, Panaskan Air]
   Focus Preview: 2/3 will be spent

Action 3: Click Seat 0 (Knight)
└─ Seat 0 highlighted

Action 4: Click "Submit Order"
└─ Execution starts:

   [Character Movement Sequence]
   1. Walk to StorageMeat → Pick up meat
   2. Walk to CuttingBoard → Cut meat (2s animation)
   3. Walk to Stove → Heat water (3s animation)

   [Order Submission]
   4. Check: [PotongDaging, PanaskanAir] = Knight's order?
   5. ✓ Match! Knight satisfied
   6. Reward: +2 Focus
   7. Knight leaves happy

Result:
├─ Focus: 1 + 2 = 3
├─ Hand: [Potong Sayuran, Panaskan Daging, Tarik Nafas]
└─ Selected: []
```

---

## ⚠️ Important Rules

### Card Selection Rules:

1. ✅ Can select multiple cards
2. ✅ Can deselect by clicking again
3. ❌ Cannot select special cards (Tarik Nafas)
4. ❌ Cannot exceed available focus
5. ❌ Cannot select cards not in hand

### Execution Rules:

1. ✅ All movements happen sequentially
2. ✅ Character waits at each location
3. ✅ Cards move to discard after execution
4. ✅ Focus is spent at start of execution
5. ❌ Cannot cancel once execution starts

### Serving Rules:

1. ✅ Must select seat before submitting
2. ✅ Must select cards before submitting
3. ✅ Order submitted after all movements
4. ❌ Cannot serve without selected cards

---

## 🔄 Comparison: Old vs New

| Feature   | Old System          | New System                  |
| --------- | ------------------- | --------------------------- |
| Card Play | Immediate execution | Select first, execute later |
| Movement  | One card at a time  | All cards sequentially      |
| Planning  | Limited             | Full combo planning         |
| Focus     | Spent per card      | Spent for whole combo       |
| Undo      | Cannot undo         | Can deselect before submit  |
| Visual    | No preview          | See selected combo          |
| Serving   | Manual submit       | Auto-submit after execution |

---

## 🎬 Animation Timing

```
Card Selection Phase:
├─ Instant feedback (no animation)
└─ UI highlight only

Execution Phase:
├─ Movement 1: Pickup (walking animation)
├─ Action 1: Cutting/Heating (2-3s)
├─ Movement 2: Pickup (walking animation)
├─ Action 2: Cutting/Heating (2-3s)
└─ Total: ~10-15 seconds for 2 cards

Submission Phase:
├─ Order check (instant)
└─ Customer reaction (1s)
```

---

## 🐛 Troubleshooting

### Cards not highlighting when clicked:

- Check if `IsCardSelected()` is called in `CreateCardUI()`
- Verify card has Image component for color change

### Character doesn't move:

- Check if `ExecuteSelectedCards()` is started as coroutine
- Verify `playerMovement.IsMoving()` returns correct state

### Focus calculation wrong:

- Check if all selected cards' focusCost are summed
- Verify focus is spent at start of execution

### Can't deselect cards:

- Ensure `OnCardClicked()` checks `IsCardSelected()`
- Verify `DeselectCard()` is called properly

### Order not submitted:

- Check if `ExecuteAndSubmitOrder()` waits for execution
- Verify `preparationStation` has cards before submit

---

## ✅ Testing Checklist

- [ ] Click card → card highlights (yellow)
- [ ] Click again → card unhighlights
- [ ] Select 2+ cards → all highlight
- [ ] Check focus preview shows correct total
- [ ] Click submit → all movements happen in sequence
- [ ] Character waits at each location
- [ ] Order submitted after all movements
- [ ] Cards removed from hand
- [ ] Selection cleared after execution
- [ ] Focus spent correctly

---

_This system creates a more strategic, planning-focused gameplay where players think ahead about their full combo before committing!_
