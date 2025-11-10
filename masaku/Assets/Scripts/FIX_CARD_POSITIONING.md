# Fix Card Positioning and Clickability

## Problem 1: Cards appearing in bottom right instead of bottom center

### Solution: Configure HandContainer

1. **Select HandContainer** in Hierarchy
2. In Inspector, set **RectTransform**:

   ```
   Anchors: Bottom-Center
   - Click the anchor preset (square icon in RectTransform)
   - Hold Shift + Alt and click bottom-center anchor

   Or manually set:
   - Anchor Min: X = 0.5, Y = 0
   - Anchor Max: X = 0.5, Y = 0
   - Pivot: X = 0.5, Y = 0

   Position:
   - Pos X: 0
   - Pos Y: 20 (or desired height from bottom)
   - Pos Z: 0

   Size:
   - Width: 800 (adjust based on how many cards you want to show)
   - Height: 220 (adjust to card height + padding)
   ```

3. **Add Horizontal Layout Group** to HandContainer:

   - Select HandContainer
   - Add Component → Layout → **Horizontal Layout Group**
   - Settings:
     ```
     ✓ Child Force Expand: Width = OFF, Height = OFF
     Child Alignment: Middle Center
     Spacing: 10 (gap between cards)
     Padding: Left/Right/Top/Bottom = 10
     ```

4. **Add Content Size Fitter** (optional, for auto-sizing):
   - Add Component → Layout → **Content Size Fitter**
   - Horizontal Fit: Preferred Size
   - Vertical Fit: Preferred Size

---

## Problem 2: Cards not clickable despite having Button component

### Solution A: Enable Interactable on Button

1. **Select CardUI** prefab in `Assets/Prefabs/`
2. In Inspector, find **Button** component
3. Make sure **Interactable** is ✓ **CHECKED**

### Solution B: Enable Raycast Target on Image

1. Select CardUI prefab
2. In Inspector, find **Image** component
3. Make sure **Raycast Target** is ✓ **CHECKED**
   - Your screenshot shows it's checked ✓, which is good!

### Solution C: Check Canvas Raycaster

1. Select **Canvas** in Hierarchy
2. Make sure it has **Graphic Raycaster** component
   - If missing: Add Component → Event → Graphic Raycaster

### Solution D: Check EventSystem

1. In Hierarchy, find **EventSystem** (should auto-create with Canvas)
2. If missing: Right-click Hierarchy → UI → Event System
3. Make sure it's enabled and active

### Solution E: Check for Blocking UI Elements

Cards might be under another UI element that's blocking clicks:

1. In Hierarchy, check UI order (top = drawn last = on top)
2. Make sure HandContainer is **below** (later in hierarchy) than other UI panels
3. Or increase Canvas sorting order

---

## Quick Test

After applying fixes:

1. Run the game
2. Cards should appear **centered at bottom**
3. Try clicking a card - should see yellow tint when selected
4. Console should show: `"Kartu dipilih: [CardName]"` or `"Kartu dibatalkan: [CardName]"`

---

## Recommended HandContainer Hierarchy

```
Canvas
├── [Other UI elements]
└── HandContainer (Horizontal Layout Group + Content Size Fitter)
    ├── CardUI (Clone) - appears at runtime
    ├── CardUI (Clone) - appears at runtime
    └── CardUI (Clone) - appears at runtime
```

**Anchors**: Bottom-Center
**Layout**: Horizontal with spacing
**Position**: Centered at bottom of screen
