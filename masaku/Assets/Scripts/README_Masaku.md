# PROJECT MASAKU - Complete Game Design Implementation

## 🎮 Game Concept

**Slay The Spire + Overcooked/Dinner Dash**

Anda adalah pemilik kafe yang mengadakan "Fantasy Week" - event 7 hari melayani pelanggan berkostum (Knight, Elf, Barbarian, Wizard) dengan sistem kartu aksi.

## 🎯 Win/Lose Conditions

- **Win:** Bertahan 7 hari operasional dengan Reputasi > 0
- **Lose:** Reputasi mencapai 0
- **Starting Reputation:** 5 poin

## 📦 Files Created

### Core Systems

1. **Customer.cs** - ScriptableObject untuk data customer
2. **CustomerInstance.cs** - Instance customer di scene dengan patience system
3. **GameManager.cs** - Core game loop (day cycle, turn cycle, reputation)
4. **MasakuCardManager.cs** - Card deck management system
5. **ActionCard.cs** - ScriptableObject untuk kartu aksi (updated)
6. **MasakuUI.cs** - Complete UI system
7. **GameSetup.cs** - Helper untuk testing (updated)

### Legacy Files (Optional)

- Card.cs, CardManager.cs, PlayerMovement.cs, CardUI.cs - Dapat dihapus jika tidak digunakan

## 🎴 Card System

### Base Action Cards (2x each = 10 total)

- **Potong Sayuran** - Cost: 1 Fokus
- **Potong Daging** - Cost: 1 Fokus
- **Panaskan Air** - Cost: 1 Fokus
- **Panaskan Daging** - Cost: 1 Fokus
- **Tarik Nafas** - Cost: 0 (Discard 1, Draw 1)

### Special Cards

- **Barbarian Curse** - Added to deck when Barbarian leaves angry
- **Wizard Boon (Draw)** - Draw 2 cards instantly
- **Wizard Boon (Focus)** - +1 Focus next turn

## 👥 Customers

### Knight

- **Order:** 1x Potong Daging + 1x Panaskan Air
- **Patience:** 10 (Decay: 2/turn) - Tidak Sabaran
- **Reward:** +3 Fokus

### Elf

- **Order:** 2x Potong Sayuran + 1x Panaskan Air
- **Patience:** 20 (Decay: 1/turn) - Sabar
- **Reward:** +5 Fokus

### Barbarian

- **Order:** 1x Potong Daging + 1x Panaskan Daging
- **Patience:** 5 (Decay: 2/turn) - Mengancam
- **Reward:** +3 Fokus
- **Penalty:** -1 Reputation + Add 1 Curse to deck

### Wizard

- **Order:** 1x Potong Sayuran + 1x Potong Daging + 1x Panaskan Air
- **Patience:** 10 (Decay: 2/turn)
- **Reward:** +2 Fokus
- **Boon:** If served with >5 patience, gives random boon

## 🔄 Game Loop

### Macro Loop (Day Cycle)

1. **Start Day** - Load customer deck, fill 4 seats
2. **Play Turns** - Execute player turns repeatedly
3. **Fill Seats** - Replace served/angry customers
4. **End Day** - When deck empty + all seats empty
5. **Next Day** - Progress to next day or game over/victory

### Micro Loop (Turn Cycle)

1. **Start Turn**

   - Draw cards to hand (5 max)
   - Reset Focus to 3 (+ bonus from Wizard)
   - Clear Preparation Station

2. **Player Phase**

   - Play Action Cards (spend Focus)
   - Cards go to Preparation Station
   - Press "Submit Order" + Select Customer

3. **Submit Phase**

   - Check if combo matches order
   - **Success:** Customer leaves happy, gain Focus reward
   - **Fail:** Customer loses 1 patience, combo wasted

4. **End Turn**
   - Discard all cards in hand
   - Clear Preparation Station
   - All customers lose patience
   - Check for angry customers

## 📅 Day Configurations

```
Day 1: [Knight, Knight, Knight]
Day 2: [Knight, Elf, Knight]
Day 3: [Knight, Barbarian, Elf, Knight]
Day 4: [Knight, Wizard, Barbarian, Elf]
Day 5: [Knight, Barbarian, Elf, Knight, Barbarian, Wizard]
Day 6: [Knight, Elf, Wizard, Knight, Barbarian, Elf, Knight]
Day 7: [Barbarian, Wizard, Elf, Barbarian, Wizard, Elf, Barbarian]
```

## 🎨 Setup Instructions

### Step 1: Create Customer ScriptableObjects

Right-click in Project → **Create → Masaku → Customer**

Create 4 customers with these exact settings:

**Knight:**

```
Customer Type: Knight
Max Patience: 10
Patience Decay Per Turn: 2
Required Cards: [PotongDaging, PanaskanAir]
Focus Reward: 3
Is Barbarian: false
Is Wizard: false
```

**Elf:**

```
Customer Type: Elf
Max Patience: 20
Patience Decay Per Turn: 1
Required Cards: [PotongSayuran, PotongSayuran, PanaskanAir]
Focus Reward: 5
Is Barbarian: false
Is Wizard: false
```

**Barbarian:**

```
Customer Type: Barbarian
Max Patience: 5
Patience Decay Per Turn: 2
Required Cards: [PotongDaging, PanaskanDaging]
Focus Reward: 3
Is Barbarian: TRUE
Is Wizard: false
```

**Wizard:**

```
Customer Type: Wizard
Max Patience: 10
Patience Decay Per Turn: 2
Required Cards: [PotongSayuran, PotongDaging, PanaskanAir]
Focus Reward: 2
Is Barbarian: false
Is Wizard: TRUE
```

### Step 2: Create Action Card ScriptableObjects

Right-click → **Create → Cards → Action Card**

Create 6 cards:

1. Potong Sayuran (Focus Cost: 1, Card Type: PotongSayuran)
2. Potong Daging (Focus Cost: 1, Card Type: PotongDaging)
3. Panaskan Air (Focus Cost: 1, Card Type: PanaskanAir)
4. Panaskan Daging (Focus Cost: 1, Card Type: PanaskanDaging)
5. Tarik Nafas (Is Special Card: true, Discard: 1, Draw: 1)
6. Barbarian Curse (For curse mechanic)

### Step 3: Setup Scene Hierarchy

```
Scene
├── GameManager (GameManager script)
│   ├── CustomerSeats (4 empty GameObjects as seats)
│   │   ├── Seat_0
│   │   ├── Seat_1
│   │   ├── Seat_2
│   │   └── Seat_3
│   └── MasakuCardManager (MasakuCardManager script)
├── Canvas (MasakuUI script)
│   ├── FocusDisplay
│   ├── ReputationDisplay
│   ├── DayDisplay
│   ├── HandContainer
│   ├── PreparationStation
│   ├── SubmitButton
│   ├── EndTurnButton
│   └── SeatButtons (4 buttons)
└── CustomerPrefab (template)
```

### Step 4: Configure GameManager

1. **All Customers:** Drag 4 Customer ScriptableObjects
2. **Customer Seats:** Assign 4 seat Transforms
3. **Customer Prefab:** Assign prefab with CustomerInstance script
4. **Card Manager:** Assign MasakuCardManager GameObject
5. **Day Configurations:** Setup 7 day configs (see above)

### Step 5: Configure MasakuCardManager

1. **Base Action Cards:** Drag 5 base action cards
2. **Curse Card:** Assign Barbarian Curse card

### Step 6: Configure MasakuUI

Assign all UI references in Inspector

## 🎮 Testing Controls

### Keyboard Shortcuts (via GameSetup.cs)

- **1-5:** Play cards from hand
- **Q:** Submit order to Seat 1
- **W:** Submit order to Seat 2
- **E:** Submit order to Seat 3
- **R:** Submit order to Seat 4
- **Space:** End turn

## 📊 Game Flow Example

```
Day 1 Start
└─ Knight spawns at Seat 0, 1, 2
└─ Player Turn 1
   ├─ Draw 5 cards, Get 3 Focus
   ├─ Play "Potong Daging" (1 Focus) → Prep Station
   ├─ Play "Panaskan Air" (1 Focus) → Prep Station
   ├─ Submit Order to Seat 0 (Knight)
   │  └─ Success! +3 Focus (total: 4)
   ├─ Play "Potong Daging" (1 Focus) → Prep Station
   ├─ Play "Panaskan Air" (1 Focus) → Prep Station
   ├─ Submit Order to Seat 1 (Knight)
   │  └─ Success! +3 Focus (total: 5)
   └─ End Turn
      └─ Remaining Knight loses 2 patience (8/10)
└─ Player Turn 2
   └─ ...
```

## 🔍 Core Mechanics

### Focus Economy

- Start each turn with only 3 Focus
- Must serve customers to gain more Focus
- Creates sequencing puzzle: "Which customer to serve first?"

### Patience System

- Each customer has patience bar
- Decreases every turn (based on customer type)
- Reaches 0 = Customer leaves angry = Lose reputation

### Combo System

- Play cards to Preparation Station
- Submit order to specific customer
- Cards must match customer's order exactly
- Wrong order = -1 patience + waste combo

### Reputation System

- Start with 5 reputation
- Lose 1 per angry customer
- Barbarian = -1 rep + 1 curse
- Reach 0 = Game Over

## 🎨 Assets Needed

### Visual Assets

- Customer sprites (Knight, Elf, Barbarian, Wizard)
- Card backgrounds
- Food icons (Sayuran, Daging, Air, Api)
- Dish sprites (4 final dishes)
- Focus bar
- Reputation hearts
- Background images

### Audio Assets

- Card play sound
- Order complete sound
- Customer angry sound
- Customer happy sound
- Day complete jingle
- Background music

## 🐛 Debug Info

The system uses Debug.Log extensively:

- Turn start/end
- Card plays
- Order submissions
- Customer patience changes
- Day transitions

Check Console for game flow information.

## 🚀 Next Steps

1. Create all ScriptableObjects
2. Build scene hierarchy
3. Assign all references in Inspector
4. Create UI prefabs
5. Test with keyboard shortcuts
6. Add visual feedback
7. Create customer/card art
8. Add animations
9. Add sound effects
10. Polish and balance

## 📝 Notes

- System designed for exactly 10 cards (2x each of 5 types)
- Deck auto-reshuffles from discard when empty
- Curse cards dilute deck efficiency
- Wizard boons provide strategic advantage
- Day 7 is final boss (3 Barbarians!)

Good luck with your Golden Bean Award! ☕🏆
