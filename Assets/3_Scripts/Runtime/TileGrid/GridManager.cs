using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GridManager : MonoBehaviour
{
    #region SETUP
    // Grid manager
    public static GridManager instance { get; private set; }

    // On awake
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        GameManager.onGameStateChange += OnGameStageChange;
    }

    // On destroy
    private void OnDestroy()
    {
        GameManager.onGameStateChange -= OnGameStageChange;
        if (instance == this)
        {
            instance = null;
        }
        Unload();
    }

    // Load stage
    private void OnGameStageChange(GameState state, bool immediately)
    {
        // Unload
        if (state == GameState.Title || state == GameState.PlayerSetup)
        {
            Unload();
        }
        // Load
        else if (state == GameState.GameLoad)
        {
            Invoke("Load", 0.1f);
        }
    }
    #endregion

    #region LAYOUT
    [Header("Layout")]
    // The tile size in meters/units
    public float tileSize = 1f;
    // The tile prefabs supported
    public GridTile[] tilePrefabs;
    // The prop prefabs supported
    public GridProp[] propPrefabs;
    // Win props
    public GameObject winPropEffect;
    // The character prefabs
    public GridCharacter[] characterPrefabs;

    // Size
    public float width { get; private set; }
    public float height { get; private set; }
    // All data
    public StageData data { get; private set; }
    // All tiles
    public Dictionary<int, GridTile> tiles { get; private set; }
    // All props
    public GridProp[] props { get; private set; }
    private GameObject[] winEffects;
    // All characters
    public GridCharacter[] characters { get; private set; }

    // Delegates
    public static event Action<GridManager> onGridLoaded;

    // Unload data
    public void Unload()
    {
        // Unload characters
        if (characters != null)
        {
            foreach (GridCharacter c in characters)
            {
                if(c != null)
                {
                    Pool.instance.Unload(c.gameObject);
                }
                
            }
            characters = null;
        }
        // Unload tiles
        if (tiles != null)
        {
            foreach (int tileIndex in tiles.Keys)
            {
                GridTile tile = tiles[tileIndex];
                if (tile != null)
                {
                    Pool.instance.Unload(tile.gameObject);
                }
            }
            tiles = null;
        }
        // Unload props
        if (props != null)
        {
            foreach (GridProp prop in props)
            {
                if (prop != null)
                {
                    Pool.instance.Unload(prop.gameObject);
                }
            }
            props = null;
        }
        // Unload effects
        if (winEffects != null)
        {
            foreach (GameObject winEffect in winEffects)
            {
                if (winEffect != null)
                {
                    Pool.instance.Unload(winEffect);
                }
            }
        }
    }

    // Load data
    public void Load()
    {
        GetStageData(GameManager.instance.currentStage, false, Load);
    }

    // Load stage
    private void Load(StageData stageData)
    {
        // Unload
        Unload();

        // Set instance
        instance = this;

        // Get stage data
        data = stageData;
        // Set size
        width = tileSize * data.columns;
        height = tileSize * data.rows;
        Debug.Log("GRID MANAGER - LOAD STAGE: " + data.stageID);

        // Generate tiles
        tiles = new Dictionary<int, GridTile>();
        foreach (TileData tileDatum in data.tiles)
        {
            // Ensure unique index
            if (tiles.ContainsKey(tileDatum.tileIndex))
            {
                Debug.LogError("GRID MANAGER - CANNOT HAVE MULTIPLE TILES FOR INDEX: " + tileDatum.tileIndex);
                continue;
            }
            // Get tile prefab
            GridTile prefab = tileDatum.tilePrefab >= 0 && tileDatum.tilePrefab < tilePrefabs.Length ? tilePrefabs[tileDatum.tilePrefab] : null;
            if (prefab == null)
            {
                Debug.LogError("GRID MANAGER - NO TILE FOR PREFAB INDEX: " + tileDatum.tilePrefab);
                continue;
            }

            // Load tile instance
            GridTile inst = Pool.instance.Load(prefab.gameObject).GetComponent<GridTile>();
            inst.movable = null;
            if (inst == null)
            {
                Debug.LogError("GRID MANAGER - CANNOT INSTANTIATE TILE PREFAB: " + prefab.gameObject.name);
                continue;
            }

            // Set position
            Transform tileTransform = inst.transform;
            tileTransform.SetParent(transform);
            tileTransform.localPosition = GetTilePosition(tileDatum.tileIndex);

            // Load data
            inst.LoadData(tileDatum);
            tiles[tileDatum.tileIndex] = inst;
        }

        // Generate props
        List<GameObject> winList = new List<GameObject>();
        props = new GridProp[data.props.Length];
        for (int p = 0; p < data.props.Length; p++)
        {
            // Prop datum
            PropData propDatum = data.props[p];

            // Ensure tile exists
            if (!tiles.ContainsKey(propDatum.tileIndex))
            {
                Debug.LogError("GRID MANAGER - NO TILE FOUND FOR INDEX: " + propDatum.tileIndex);
                continue;
            }
            // Get tile
            GridTile tile = tiles[propDatum.tileIndex];
            if (tile.movable != null)
            {
                Debug.LogError("GRID MANAGER - CANNOT HAVE MULTIPLE PROPS ON THE SAME TILE FOR INDEX: " + propDatum.tileIndex);
                continue;
            }
            // Get prop prefab
            GridProp prefab = propDatum.propPrefab >= 0 && propDatum.propPrefab < propPrefabs.Length ? propPrefabs[propDatum.propPrefab] : null;
            if (prefab == null)
            {
                Debug.LogWarning("GRID MANAGER - NO PROP FOR PREFAB INDEX: " + propDatum.propPrefab);
                prefab = propPrefabs[0];
            }

            // Load prop instance
            GridProp inst = Pool.instance.Load(prefab.gameObject).GetComponent<GridProp>();
            if (inst == null)
            {
                Debug.LogError("GRID MANAGER - CANNOT INSTANTIATE PROP PREFAB: " + prefab.gameObject.name);
                continue;
            }

            // Set position
            Transform trans = inst.transform;
            trans.SetParent(transform);
            trans.localPosition = GetTilePosition(propDatum.tileIndex);
            Direction direction = propDatum.direction;
            if (direction == Direction.None)
            {
                int random = UnityEngine.Random.Range(0, 5) + 1;
                direction = (Direction)random;
            }
            trans.localRotation = GetRotation(direction);

            // Load data
            inst.LoadData(propDatum);
            props[p] = inst;

            // Add win effect
            if (propDatum.isWinProp)
            {
                GameObject winEffect = Pool.instance.Load(winPropEffect);
                winEffect.transform.SetParent(inst.transform);
                winEffect.transform.localPosition = new Vector3(0f, 1f, 0f);
                winEffect.transform.localRotation = Quaternion.identity;
                winEffect.transform.localScale = Vector3.one;
                winList.Add(winEffect);
            }
        }
        winEffects = winList.ToArray();

        // Generate characters
        if (GameManager.instance.players != null && GameManager.instance.players.Count > 0)
        {
            if (GameManager.instance.players.Count > data.players.Length)
            {
                Debug.Log("STAGE: " + data.stageID + "\nPLAYERS: " + data.players.Length);
            }

            characters = new GridCharacter[GameManager.instance.players.Count];
            for (int p = 0; p < GameManager.instance.players.Count; p++)
            {
                // Player
                GamePlayer player = GameManager.instance.players[p];
                PlayerData playerData = data.players[p];

                // Get character data
                int index = player.characterIndex;
                if (index < 0 || index >= GameManager.instance.gameData.characters.Length)
                {
                    index = UnityEngine.Random.Range(0, GameManager.instance.gameData.characters.Length);
                }
                GameCharacter character = GameManager.instance.gameData.characters[index];

                // Get prefab
                GridCharacter prefab = GetCharacterPrefab(character.characterID);

                // Get instance
                GridCharacter mover = Pool.instance.Load(prefab.gameObject).GetComponent<GridCharacter>();
                mover.playerIndex = p;
                mover.Place(character.characterVariant, playerData.tileIndex, playerData.direction);

                // Set character
                characters[p] = mover;
            }
        }

        // Delegate
        if (onGridLoaded != null)
        {
            onGridLoaded(this);
        }

        // Set game intro
        if (GameManager.instance != null)
        {
            GameManager.instance.SetState(GameState.GameIntro);
        }
    }

    // Get character by id
    public GridCharacter GetCharacterPrefab(string characterID)
    {
        if (characterPrefabs != null && characterPrefabs.Length > 0)
        {
            foreach (GridCharacter character in characterPrefabs)
            {
                if (string.Equals(characterID, character.gameObject.name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return character;
                }
            }
            return characterPrefabs[0];
        }
        return null;
    }

    // Get tile from index
    public GridTile GetTile(int tileIndex)
    {
        if (tiles != null && tiles.ContainsKey(tileIndex))
        {
            return tiles[tileIndex];
        }
        return null;
    }

    // Get tile position
    public Vector3 GetTilePosition(int tileIndex)
    {
        // Get x & y
        int x; int y;
        GetPath(data, tileIndex, out x, out y);

        // Return position
        float halfSize = tileSize / 2f;
        return new Vector3(tileSize * x + halfSize - width / 2f, 0f, tileSize * -y - halfSize + height / 2f);
    }

    // Get tile index
    public static int GetTileIndex(StageData stageData, int column, int row)
    {
        return row * stageData.columns + column;
    }

    // Get x and y from index
    public static void GetPath(StageData stageData, int tileIndex, out int x, out int y)
    {
        // Get
        if (stageData != null)
        {
            y = Mathf.FloorToInt((float)tileIndex / (float)stageData.columns);
            x = tileIndex - y * stageData.columns;
        }
        // Failed
        else
        {
            x = -1;
            y = -1;
        }
    }

    // Whether tile exists
    public static int HasTile(StageData stageData, int tileIndex)
    {
        if (stageData != null)
        {
            for (int t = 0; t < stageData.tiles.Length; t++)
            {
                TileData tileDatum = stageData.tiles[t];
                if (tileDatum.tileIndex == tileIndex)
                {
                    return t;
                }
            }
        }
        return -1;
    }

    // Get rotation for direction
    public static Quaternion GetRotation(Direction direction)
    {
        float y = 0f;
        switch (direction)
        {
            case Direction.Up:
                y = 0f;
                break;
            case Direction.Down:
                y = 180f;
                break;
            case Direction.Left:
                y = -90f;
                break;
            case Direction.Right:
                y = 90f;
                break;
            default:
                break;
        }
        return Quaternion.Euler(0f, y, 0f);
    }
    #endregion

    #region STAGES
    // Get stage directory
    public static string GetStageDirectory()
    {
        return Application.streamingAssetsPath + "/Stages/";
    }
    // Get all stage paths
    public static string[] GetStagePaths()
    {
        return Directory.GetFiles(GetStageDirectory(), "*.json");
    }
    // Get specific stage path
    public static string GetStagePath(string stageID)
    {
        return GetStageDirectory() + stageID + ".json";
    }

    // Get stage data
    public static void GetStageData(string stageID, bool shouldAsync, Action<StageData> onStageDataLoad)
    {
        // Determine path
        string stagePath = GetStagePath(stageID);

#if !UNITY_WEBGL
        // Ensure existing
        if (!File.Exists(stagePath))
        {
            stagePath = GetStagePaths()[0];
        }
#endif
        //Debug.Log("Getting data from:" + stagePath);
        // Load json
        FileManager.LoadJson<StageData>(stagePath, onStageDataLoad);
    }
    #endregion

    #region CHARACTERS
    public void ResetRound()
    {
        for(int i = 0;i<characters.Length;i++)
        {
            Debug.Log("Have char " + characters[i].name);
            int t = data.players[i].tileIndex;
            int v = GameManager.instance.gameData.characters[i].characterVariant;
            characters[i].Place(v, t, Direction.Down);
        }
    }
    #endregion
}
