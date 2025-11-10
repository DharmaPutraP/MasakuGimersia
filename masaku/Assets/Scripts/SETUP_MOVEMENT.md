# Player Movement Setup Guide

## Overview

When a card is played, the player character will automatically move to the corresponding kitchen location based on the card's target tag.

## Step 1: Tag Your Kitchen Objects

Create these tags in Unity (Edit → Project Settings → Tags and Layers):

1. **CuttingBoard** - For cutting actions (Potong Sayuran, Potong Daging)
2. **Stove** - For cooking actions (Panaskan Air, Panaskan Daging)
3. **ServingCounter** - For serving customers
4. **StorageMeat** - For Meat Storage
5. **StorageVegetable** - For Vegetable Storage

### Example Scene Setup:

```
Kitchen
├── CuttingBoard_01 (Tag: "CuttingBoard")
├── CuttingBoard_02 (Tag: "CuttingBoard")
├── Stove_01 (Tag: "Stove")
├── Stove_02 (Tag: "Stove")
├── Freezer (Tag: "StorageMeat")
├── VegetableContainer (Tag: "StorageVegetable")
└── ServingCounter (Tag: "ServingCounter")
```

## Step 2: Setup KitchenLocationManager

1. Create empty GameObject in scene: `KitchenLocationManager`
2. Add component: `KitchenLocationManager` script
3. Click the context menu (⋮) on the component
4. Select **"Auto-Find Locations"** to automatically find all tagged objects

**Or manually assign:**

- Add entries to the Locations list
- Set Location Tag (e.g., "CuttingBoard")
- Drag the GameObject to Location Transform
- Name it (e.g., "Main Cutting Board")

## Step 3: Configure Action Cards

For each ActionCard ScriptableObject, set the **Target Tag**:

| Card Name       | Target Tag                  |
| --------------- | --------------------------- |
| Potong Sayuran  | CuttingBoard                |
| Potong Daging   | CuttingBoard                |
| Panaskan Air    | Stove                       |
| Panaskan Daging | Stove                       |
| Tarik Nafas     | (leave empty - no movement) |

## Step 4: Setup MasakuCardManager

1. Select **MasakuCardManager** in Hierarchy
2. In Inspector, find **Player Reference** section
3. Drag your **Player** GameObject to the **Player Movement** field

## Step 5: Setup Player Character

Your player character should have:

- `PlayerMovement` component attached
- Optional: `Animator` component for animations

### PlayerMovement Settings:

- **Move Speed**: 5 (adjust for your game)
- **Rotation Speed**: 10
- **Stopping Distance**: 0.5
- **Animator**: Drag your Animator component (optional)

## How It Works

### Flow:

1. Player clicks/plays a card
2. `MasakuCardManager.PlayCard()` is called
3. System checks card's `targetTag`
4. `KitchenLocationManager` finds the location by tag
5. `PlayerMovement.MoveToLocation()` is called
6. Player moves to target position with animation
7. When arrived, `ExecuteActionAtLocation()` performs the action

### Example:

```csharp
// Card: "Potong Daging"
// Target Tag: "CuttingBoard"
//
// 1. Card played → Check tag "CuttingBoard"
// 2. Find location with tag "CuttingBoard"
// 3. Move player to that position
// 4. Execute cutting action with animation
```

## Animation Setup (Optional)

If you have an Animator, set up these parameters:

### Bool Parameters:

- **IsWalking** - True when moving, False when idle

### Trigger Parameters:

- **PotongSayuran**
- **PotongDaging**
- **PanaskanAir**
- **PanaskanDaging**

Example Animation Controller:

```
Idle → (IsWalking = true) → Walking
Walking → (IsWalking = false) → Idle
Idle → (Trigger: PotongDaging) → Cutting Animation → Idle
```

## Testing

1. Start the game
2. Play a card (e.g., "Potong Daging")
3. Watch player move to CuttingBoard
4. Check Console for debug messages:
   - "Bergerak ke lokasi: CuttingBoard"
   - "Sampai di lokasi: CuttingBoard"
   - "Memotong daging..."

## Troubleshooting

**Player doesn't move:**

- Check if card has targetTag set
- Verify location exists with that tag
- Ensure PlayerMovement is assigned in MasakuCardManager

**Player moves to wrong location:**

- Check targetTag spelling matches exactly
- Verify GameObject tags are set correctly

**Animation doesn't play:**

- Check if Animator is assigned
- Verify animation parameters exist
- Check animation transitions

## Advanced: Multiple Locations

If you have multiple CuttingBoards, the player will move to the first one found. To choose closest:

```csharp
// In KitchenLocationManager.cs
public Transform GetClosestLocationByTag(string tag, Vector3 fromPosition)
{
    Transform closest = null;
    float closestDistance = Mathf.Infinity;

    foreach (KitchenLocation location in locations)
    {
        if (location.locationTag == tag)
        {
            float distance = Vector3.Distance(fromPosition, location.locationTransform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = location.locationTransform;
            }
        }
    }

    return closest;
}
```
