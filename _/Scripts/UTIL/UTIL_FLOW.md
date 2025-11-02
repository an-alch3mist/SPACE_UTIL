
# LOG
## file structure
```
Root/LOG
  ├── LOG.md
  └── /GameData
      ├── *.json
```
* for now Root directory shall be Application.dataPath

## Approach:

- access from enum values to avoid file path mismatch
- example: 
```cs
// unique for each game
public enum GameDataType
{
    inputKeyBindings, // saved or loaded as .LOG/GameData/inputKeyBindings.json
    playerStats, // saved or loaded as .LOG/GameData/playerStats.json
}
```

### Load
    scenario 0: when file exist
        try to parse the json file to class
            if not possible(as far as i know Unity3D JsonUtility shall return `null` if parsing file is corrupted)
                return default class variable values as new T()
                ```cs
                public class Data_0
                {
                    public string name = "somthng"; // default name val
                    public int i = 10;              // default i val
                    public class Data_0A data_0A = new Data_0A(); // init to default, probably new Data_0A has default non null values initializer too.

                    public Data_0()
                    {
                        this.name = "somthng";
                        this.i = 10;
                        this.data_0A = new Data_0A();
                    }
                }
                ```
            if succeded in parsing:
                return the parsed class with its values

    scenario 1: when file doesnt exist
        return the default class as new T(); similar to when parsing JSON is corrupted

### Save
    scenario:
        overwrite(since System.IO.File.WriteAllText behaviour shall create a file if it doesnt exist by default) existing file at `"Root/LOG/GameData/{GameDataType.inputKeyBindings.ToString()}.json"` to the string/class provided



## preference to call externally
+ T LOG.LoadGameData<T>(GameDataType.playerStats)
+ string LOG.LoadGameData(GameDataType.playerStats) // shall just return string of loaded content(return nothing if no file found)
+ void LOG.SaveGameData(GameDataType.playerStats, ClassName.ToJSON(prettify: true))

## response:
```cs
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Game data types - add new entries here for each saveable data type
/// Each enum value maps to a JSON file: LOG/GameData/{enumName}.json
/// </summary>
public enum GameDataType
{
    inputKeyBindings,
    playerStats,
}

/// <summary>
/// LOG - Local Game Data Management System
/// Handles loading and saving game data to JSON files
/// </summary>
public static class LOG
{
    private static string RootPath => Application.dataPath;
    private static string LogDirectory => Path.Combine(RootPath, "LOG");
    private static string GameDataDirectory => Path.Combine(LogDirectory, "GameData");

    /// <summary>
    /// Ensures the LOG directory structure exists
    /// </summary>
    private static void EnsureDirectoryExists()
    {
        if (!Directory.Exists(GameDataDirectory))
        {
            Directory.CreateDirectory(GameDataDirectory);
        }
    }

    /// <summary>
    /// Gets the full file path for a given GameDataType
    /// </summary>
    private static string GetFilePath(GameDataType dataType)
    {
        return Path.Combine(GameDataDirectory, $"{dataType}.json");
    }

    /// <summary>
    /// Load game data and deserialize to type T
    /// Returns default T instance if file doesn't exist or parsing fails
    /// </summary>
    public static T LoadGameData<T>(GameDataType dataType) where T : new()
    {
        string filePath = GetFilePath(dataType);

        // Scenario 1: File doesn't exist
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[LOG] File not found: {filePath}. Returning default instance.");
            return new T();
        }

        // Scenario 0: File exists
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            
            // Try to parse JSON
            T data = JsonUtility.FromJson<T>(jsonContent);

            // If parsing failed (returns null or default)
            if (data == null || EqualityComparer<T>.Default.Equals(data, default(T)))
            {
                Debug.LogWarning($"[LOG] Failed to parse JSON from: {filePath}. Returning default instance.");
                return new T();
            }

            Debug.Log($"[LOG] Successfully loaded: {filePath}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LOG] Error loading {filePath}: {e.Message}. Returning default instance.");
            return new T();
        }
    }

    /// <summary>
    /// Load game data as raw JSON string
    /// Returns empty string if file doesn't exist
    /// </summary>
    public static string LoadGameData(GameDataType dataType)
    {
        string filePath = GetFilePath(dataType);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[LOG] File not found: {filePath}. Returning empty string.");
            return string.Empty;
        }

        try
        {
            string content = File.ReadAllText(filePath);
            Debug.Log($"[LOG] Successfully loaded raw content from: {filePath}");
            return content;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LOG] Error reading {filePath}: {e.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Save game data from JSON string
    /// Creates file if it doesn't exist, overwrites if it does
    /// </summary>
    public static void SaveGameData(GameDataType dataType, string jsonContent)
    {
        EnsureDirectoryExists();
        string filePath = GetFilePath(dataType);

        try
        {
            File.WriteAllText(filePath, jsonContent);
            Debug.Log($"[LOG] Successfully saved: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LOG] Error saving {filePath}: {e.Message}");
        }
    }

    /// <summary>
    /// Check if a game data file exists (internal use)
    /// </summary>
    private static bool FileExists(GameDataType dataType)
    {
        return File.Exists(GetFilePath(dataType));
    }
}

// ============= USAGE EXAMPLES =============

/// <summary>
/// Example data class for player stats
/// </summary>
[Serializable]
public class PlayerStats
{
    public string playerName = "Player";
    public int level = 1;
    public float health = 100f;
    public int experience = 0;

    public PlayerStats()
    {
        playerName = "Player";
        level = 1;
        health = 100f;
        experience = 0;
    }
}

/// <summary>
/// Example data class for input key bindings
/// </summary>
[Serializable]
public class InputKeyBindings
{
    public string moveForward = "W";
    public string moveBackward = "S";
    public string moveLeft = "A";
    public string moveRight = "D";
    public string jump = "Space";

    public InputKeyBindings()
    {
        moveForward = "W";
        moveBackward = "S";
        moveLeft = "A";
        moveRight = "D";
        jump = "Space";
    }
}

/// <summary>
/// Example usage class
/// </summary>
public class GameDataManager : MonoBehaviour
{
    private void Start()
    {
        // Example 1: Load player stats (typed)
        PlayerStats stats = LOG.LoadGameData<PlayerStats>(GameDataType.playerStats);
        Debug.Log($"Loaded player: {stats.playerName}, Level: {stats.level}");

        // Example 2: Load as raw string
        string rawJson = LOG.LoadGameData(GameDataType.inputKeyBindings);
        if (!string.IsNullOrEmpty(rawJson))
        {
            Debug.Log($"Raw JSON: {rawJson}");
        }

        // Example 3: Save player stats
        PlayerStats newStats = new PlayerStats
        {
            playerName = "Hero",
            level = 5,
            health = 87.5f,
            experience = 1250
        };
        LOG.SaveGameData(GameDataType.playerStats, newStats.ToJson(prettify: true));

        // Example 4: Save from JSON string
        string customJson = JsonUtility.ToJson(new InputKeyBindings(), true);
        LOG.SaveGameData(GameDataType.inputKeyBindings, customJson);

        // Example 5: Check if file exists
        if (LOG.FileExists(GameDataType.playerStats))
        {
            Debug.Log("Player stats file exists!");
        }

        // Example 6: Delete a file
        // LOG.DeleteGameData(GameDataType.playerStats);

        // Example 5: Check if file exists
        if (LOG.FileExists(GameDataType.playerStats))
        {
            Debug.Log("Player stats file exists!");
        }
    }
}
```


# Board
Ah, you need a **deep clone**! For reference types (classes), copying the array elements just copies references, not the actual objects. Here's the solution:## How It Works 🔍

### **1. Value Types** (int, char, float, structs)
```csharp
Board<int> board = new Board<int>((10, 10), 0);
board[5, 5] = 100;

Board<int> copy = board.clone;
copy[5, 5] = 999;

// ✅ board[5, 5] is still 100 (independent)
```
### **2. Reference Types Cloning with ICloneable**
```csharp
[System.Serializable]
public class Tile : System.ICloneable
{
    public int health;
    public string type;
    
    public object Clone()
    {
        return new Tile { health = this.health, type = this.type };
    }
}

Board<Tile> board = new Board<Tile>((10, 10), null);
board[5, 5] = new Tile { health = 100, type = "grass" };

Board<Tile> copy = board.clone;
copy[5, 5].health = 0;  // Modify copy

// ✅ board[5, 5].health is still 100 (deep copy worked!)
```
### **3. Serializable Classes (JSON fallback)**
```csharp
[System.Serializable]
public class Enemy
{
    public int health;
    public List<string> inventory;
}

Board<Enemy> board = new Board<Enemy>((10, 10), null);
board[5, 5] = new Enemy 
{ 
    health = 50, 
    inventory = new List<string> { "sword", "shield" }
};

Board<Enemy> copy = board.clone;
copy[5, 5].health = 0;
copy[5, 5].inventory.Add("potion");

// ✅ Original is unchanged (JSON serialization deep copied everything)
```
## Performance Considerations

| Type | Method | Speed |
|------|--------|-------|
| `int`, `char`, `float` | Direct copy | ⚡ Fastest |
| `string` | Direct copy (immutable) | ⚡ Fastest |
| Class with `ICloneable` | Custom Clone() | ⚡ Fast |
| `[Serializable]` class | JSON round-trip | 🐌 Slower (but safe) |
## Recommended Pattern

For best performance, implement `ICloneable` on your game classes:
```csharp
[System.Serializable]
public class Cell : System.ICloneable
{
    public int value;
    public List<int> data;
    
    public object Clone()
    {
        return new Cell 
        { 
            value = this.value,
            data = new List<int>(this.data)  // Deep copy List
        };
    }
}

// Now clone is fast AND deep
Board<Cell> board = new Board<Cell>((100, 100), new Cell());
Board<Cell> copy = board.clone;  // ✅ Deep copy with ICloneable
```

This approach gives you:
- ✅ **Deep cloning** for reference types
- ✅ **Fast cloning** for value types  
- ✅ **Automatic fallback** to JSON if no ICloneable
- ✅ **Universal** - works with any type

Perfect for game dev where you might clone boards for undo/redo, AI simulation, or save states! 🎮