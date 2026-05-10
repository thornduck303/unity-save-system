# Unity Save System
Slot-based save system for Unity with built-in JSON serialization and encryption.

This system allows you to easily save, load, copy, delete, and manage save slots using strongly typed data classes.

## How to use
### 1. Create a serializable data class

```csharp
using System;

[Serializable]
public class PlayerData
{
  public int coins;
  public int level;
  public float health;
}
```

### 2. Save data
```csharp
PlayerData data = new()
{
  coins = 150,
  level = 5,
  health = 72.5f
};

SaveSystem.SaveDataInSlot(data, 0);
```

### 3. Load data
```csharp
PlayerData data = SaveSystem.LoadDataFromSlot<PlayerData>(0);

if (data != null)
  Debug.Log($"Coins: {data.coins}");
```

## Basic operations
### Check if exists
```csharp
bool doesExists = SaveSystem.DoesSlotExist(0);
```

### Delete slot
```csharp
SaveSystem.DeleteSlot(0);
```
or
```csharp
SaveSystem.DeleteAllSlots();
```

### Copy slot to
```csharp
SaveSystem.CopySlot(0, 1);
```

### Retrive all active slots
```csharp
List<int> slots = SaveSystem.FindAllSavedSlots();
```

## Notes
Save files are stored inside ```Application.persistentDataPath```.

Each file follows this format:
```
save0.save
save1.save
save2.save
```

