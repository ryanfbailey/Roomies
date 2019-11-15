#define DRAW_ENABLED

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StageEditor : EditorWindow
{
    #region SETUP
    [MenuItem("Tools/Stage Editor")]
    public static void OpenEditor()
    {
        StageEditor window = GetWindow<StageEditor>();
        window.Show();
    }

    // Layout items
    private const float MARGIN = 10f;
    private const float BTN_WIDTH = 100f;
    private const float BTN_HEIGHT = 40f;
    private const float MINI_SIZE = 30f;
    private const float TILE_WIDTH = 100f;
    private const float TILE_HEIGHT = 50f;
    private const string NONE_ID = "NONE";

    // Colors
    private static Color emptyTileColor = Color.white;
    private static Color noTileColor = Color.black;
    private static Color wallColor = Color.grey;
    private static Color winColor = Color.magenta;
    private static Color propColor = new Color(1f, 0.5f, 0f);

    // Manager
    private GridManager _manager;
    private string[] _tiles;
    private string[] _players;
    private string[] _props;
    private string[] _directions;
    private Color[] _colors;
    private bool _isDrawing = false;

    // On gui
    private void OnGUI()
    {
        // Title
        GUILayout.Space(MARGIN);
        EditorGUILayout.LabelField("STAGE EDITOR", EditorStyles.boldLabel);

        // Ignore while playing
        if (Application.isPlaying)
        {
            GUI.color = Color.red;
            EditorGUILayout.LabelField("Cannot edit grid while playing!");
            GUI.color = Color.white;
            return;
        }
        // Check for press
        Event e = Event.current;
        if (e != null && e.isMouse)
        {
            bool shouldDown = e.type == EventType.MouseDown || e.type == EventType.MouseDrag;
#if !DRAW_ENABLED
            shouldDown = false;
#endif
            if (_isDrawing != shouldDown)
            {
                _isDrawing = shouldDown;
            }
        }

        // Find manager
        if (_manager == null || _stagePaths == null)
        {
            _manager = GameObject.FindObjectOfType<GridManager>();
            SetStage(-1);
            if (_manager == null)
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField("No Grid Manager found!  Please load the main scene!");
                GUI.color = Color.white;
                return;
            }
            else if (_stagePaths == null)
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField("No Grid Manager found!  Please load the main scene!");
                GUI.color = Color.white;
                return;
            }
            else
            {
                // Get tiles list
                List<string> tiles = new List<string>();
                tiles.Add(NONE_ID);
                foreach (GridTile prefab in _manager.tilePrefabs)
                {
                    tiles.Add(prefab.gameObject.name.ToUpper());
                }
                _tiles = tiles.ToArray();
                // Get prop list
                List<string> props = new List<string>();
                props.Add(NONE_ID);
                foreach (GridProp prefab in _manager.propPrefabs)
                {
                    props.Add(prefab.gameObject.name.ToUpper());
                }
                _props = props.ToArray();
                // Get directions
                List<string> dirs = new List<string>();
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    dirs.Add(dir == Direction.None ? "RANDOM" : dir.ToString().ToUpper());
                }
                _directions = dirs.ToArray();

                // Get data
                int maxPlayers = 4;
                _colors = null;
                GameManager gm = GameObject.FindObjectOfType<GameManager>();
                if (gm != null)
                {
                    string dataPath = Application.streamingAssetsPath + "/" + gm.gameDataPath + ".json";
                    string dataJson = File.ReadAllText(dataPath);
                    GameData data = JsonUtility.FromJson<GameData>(dataJson);
                    data.Setup();
                    maxPlayers = data.maxPlayers;
                    if (data.colors != null && data.colors.Length > 0)
                    {
                        _colors = new Color[data.colors.Length];
                        for (int c = 0; c < data.colors.Length; c++)
                        {
                            _colors[c] = data.colors[c].color;
                        }
                    }
                }

                // Set player list
                List<string> players = new List<string>();
                players.Add(NONE_ID);
                for (int p = 0; p < maxPlayers; p++)
                {
                    players.Add("PLAYER " + (p + 1));
                }
                _players = players.ToArray();

                // Set colors
                if (_colors == null)
                {
                    _colors = new Color[maxPlayers];
                    for (int i = 0; i < maxPlayers; i++)
                    {
                        _colors[i] = Color.black;
                    }
                }
            }
        }

        // Stage select
        StageSelectUI();

        // New stage
        if (_stageIndex >= _stagePaths.Length)
        {
            NewStageUI();
        }
        // Existing stage
        else if (_stageIndex >= 0)
        {
            StageUI();
        }
    }

    // Select stage
    private void SetStage(int index)
    {
        // Set index
        _stageIndex = index;
        // Get all paths
        _stagePaths = GridManager.GetStagePaths();

        // New stage
        if (_stageIndex >= _stagePaths.Length)
        {
            _newStageID = "";
            _newColumns = 17;
            _newRows = 13;
        }
        // Existing stage
        else if (_stageIndex >= 0)
        {
            string stageID = Path.GetFileNameWithoutExtension(_stagePaths[_stageIndex]);
            GridManager.GetStageData(stageID, false, delegate (StageData data)
            {
                _stage = data;
                _stageChanged = false;
                _stageEdit = EditType.PLAYER;
                _type = 0;
                _stageScroll = Vector2.zero;
            });
        }
    }
#endregion

#region STAGE SELECT
    // Stage select
    private string[] _stagePaths;
    private int _stageIndex;

    // Stage select
    private void StageSelectUI()
    {
        // Stage select
        GUILayout.Space(MARGIN);
        EditorGUILayout.LabelField("Stage Select");

        // Begin stage list
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(MARGIN);

        // Iterate stage paths
        for (int s = 0; s < _stagePaths.Length; s++)
        {
            // Enabled
            GUI.enabled = _stageIndex != s;

            // Button per id
            string stageID = Path.GetFileNameWithoutExtension(_stagePaths[s]);
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(stageID));
            if (GUILayout.Button(stageID, GUILayout.Width(size.x + MARGIN * 2f), GUILayout.Height(BTN_HEIGHT)))
            {
                SetStage(s);
                break;
            }
        }
        // New stage
        GUI.enabled = _stageIndex != _stagePaths.Length;
        if (GUILayout.Button("New Stage", GUILayout.Width(BTN_WIDTH), GUILayout.Height(BTN_HEIGHT)))
        {
            SetStage(_stagePaths.Length);
        }

        // End stage list
        GUI.enabled = true;
        GUILayout.Space(MARGIN);
        EditorGUILayout.EndHorizontal();
    }
#endregion

#region NEW STAGE
    // The new data
    private string _newStageID;
    private int _newColumns;
    private int _newRows;

    // New stage ui
    private void NewStageUI()
    {
        // Title
        GUILayout.Space(MARGIN);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("New Stage");
        EditorGUI.indentLevel++;

        // New data
        _newStageID = EditorGUILayout.TextField("Stage ID:", _newStageID);
        _newColumns = EditorGUILayout.IntSlider("Columns:", _newColumns, 10, 100);
        _newRows = EditorGUILayout.IntSlider("Rows:", _newRows, 10, 100);

        // Button Start
        GUILayout.Space(MARGIN);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(MARGIN);
        // Create stage
        GUI.color = Color.green;
        GUI.enabled = IsStageIdValid(_newStageID);
        if (GUILayout.Button("CREATE STAGE", GUILayout.Width(BTN_WIDTH), GUILayout.Height(BTN_HEIGHT)))
        {
            if (CreateNewStage(_newStageID, _newColumns, _newRows))
            {
                SetStage(-1);
            }
        }
        GUI.enabled = true;
        // Button End
        GUILayout.Space(MARGIN);
        EditorGUILayout.EndHorizontal();

        // End
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
    }

    // Is valid?
    private bool IsStageIdValid(string newStageID)
    {
        // Cannot be null
        if (string.IsNullOrEmpty(newStageID))
        {
            return false;
        }
        // Does path exist
        string stagePath = GridManager.GetStagePath(newStageID);
        if (File.Exists(stagePath))
        {
            return false;
        }

        // Valid
        return true;
    }

    // Get blank level
    private bool CreateNewStage(string stageID, int columns, int rows)
    {
        // Generate level
        StageData stage = new StageData();
        stage.stageID = stageID;
        stage.columns = columns;
        stage.rows = rows;

        // Add all tiles & wall props around sides
        int total = stage.columns * stage.rows;
        stage.tiles = new TileData[total];
        List<PropData> walls = new List<PropData>();
        for (int y = 0; y < stage.rows; y++)
        {
            int firstX = y * stage.columns;
            int lastX = (y + 1) * stage.columns - 1;
            for (int x = 0; x < stage.columns; x++)
            {
                // Add tile
                TileData tile = new TileData();
                tile.tileIndex = firstX + x;
                tile.tilePrefab = 0;
                tile.tilePrefabVariant = tile.tileIndex % 2 == 0 ? 0 : 1;
                if (y % 2 == 0 && stage.columns % 2 == 0)
                {
                    tile.tilePrefabVariant = 1 - tile.tilePrefabVariant;
                }
                stage.tiles[tile.tileIndex] = tile;

                // Determine tile to left
                tile.tileLeft = tile.tileIndex - 1;
                if (tile.tileLeft < firstX)
                {
                    tile.tileLeft = lastX;
                }
                // Determine tile to right
                tile.tileRight = tile.tileIndex + 1;
                if (tile.tileRight > lastX)
                {
                    tile.tileRight = firstX;
                }
                // Determine tile up
                tile.tileUp = tile.tileIndex - stage.columns;
                if (tile.tileUp < 0)
                {
                    tile.tileUp += total;
                }
                // Determine tile down
                tile.tileDown = tile.tileIndex + stage.columns;
                if (tile.tileDown >= total)
                {
                    tile.tileDown -= total;
                }

                // Add walls to border
                if (x == 0 || y == 0 || x == stage.columns - 1 || y == stage.rows - 1)
                {
                    PropData wall = new PropData();
                    wall.tileIndex = tile.tileIndex;
                    wall.direction = Direction.Down;
                    wall.propPrefab = 0;
                    wall.propPrefabVariant = 0;
                    walls.Add(wall);
                }
            }
        }
        // Add win prop
        PropData tv = new PropData();
        tv.tileIndex = Mathf.FloorToInt((float)stage.rows / 2f) * stage.columns + Mathf.FloorToInt((float)stage.columns / 2f);
        tv.direction = Direction.Down;
        tv.propPrefab = 1;
        tv.propPrefabVariant = 0;
        tv.isWinProp = true;
        walls.Add(tv);
        stage.props = walls.ToArray();

        // Add blank players list
        stage.players = new PlayerData[_players.Length - 1];
        for (int p = 0; p < stage.players.Length; p++)
        {
            PlayerData player = new PlayerData();
            player.tileIndex = columns + 1 + p;
            player.direction = Direction.Down;
            stage.players[p] = player;
        }

        // Save to json
        string error = SaveStage(stage);
        if (!string.IsNullOrEmpty(error))
        {
            EditorUtility.DisplayDialog("New Stage", "Could not save new stage '" + _stage.stageID + " '.  " + error, "Okay");
            return false;
        }

        // Success
        return true;
    }
#endregion

#region EDIT STAGE
    // The stage itself
    private StageData _stage;
    // Stage changed
    private bool _stageChanged = false;
    // Stage edit
    private EditType _stageEdit = EditType.PLAYER;
    private int _type = 0;
    private int _variant;
    private string[] _variantIDs;
    private Direction _direction;
    private bool _isWin;
    // Scrolling
    private Vector2 _stageScroll;

    // Various edit options
    public enum EditType
    {
        PLAYER,
        PROPS,
        TILE
    }

    // Edit a specific stage
    private void StageUI()
    {
        // Ensure exists
        if (_stage == null)
        {
            return;
        }

        // Title
        GUILayout.Space(MARGIN);
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Edit Stage");
        // Delete Stage
        GUILayout.FlexibleSpace();
        GUI.color = Color.red;
        if (GUILayout.Button("-", GUILayout.Width(MINI_SIZE), GUILayout.Height(MINI_SIZE)))
        {
            GUI.color = Color.white;
            if (EditorUtility.DisplayDialog("Delete Stage", "Are you sure you would like to delete the stage '" + _stage.stageID + " '", "Okay", "Cancel"))
            {
                string stagePath = GridManager.GetStagePath(_stage.stageID);
                if (File.Exists(stagePath))
                {
                    File.Delete(stagePath);
                }
                SetStage(-1);
            }
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;

        // Stage ID
        string newID = EditorGUILayout.TextField("Stage ID:", _stage.stageID);
        if (!string.Equals(newID, _stage.stageID))
        {
            _stage.stageID = newID;
            _stageChanged = true;
        }

        // Edit type
        EditType newEdit = (EditType)EditorGUILayout.EnumPopup("Edit Type:", _stageEdit);
        if (_stageEdit != newEdit)
        {
            _stageEdit = newEdit;
            _type = 0;
            _variant = 0;
            _variantIDs = null;
            _isWin = false;
            _direction = _stageEdit == EditType.PLAYER ? Direction.Down : Direction.None;
        }
        // Edit type index
        int newType = _type;
        switch (_stageEdit)
        {
            case EditType.TILE:
                newType = EditorGUILayout.Popup("Tile:", newType, _tiles);
                break;
            case EditType.PLAYER:
                newType = EditorGUILayout.Popup("Player:", newType, _players);
                if (_type != 0)
                {
                    _direction = (Direction)EditorGUILayout.Popup("Direction:", (int)_direction, _directions);
                }
                break;
            case EditType.PROPS:
                newType = EditorGUILayout.Popup("Prop:", newType, _props);
                if (_type != 0)
                {
                    _direction = (Direction)EditorGUILayout.Popup("Direction:", (int)_direction, _directions);
                    if (_variantIDs != null)
                    {
                        _variant = EditorGUILayout.Popup("Variant:", _variant, _variantIDs);
                    }
                    _isWin = EditorGUILayout.Toggle("Winning Prop:", _isWin);
                }
                break;
        }
        if (_type != newType)
        {
            _type = newType;
            _isWin = _type == 2 && _stageEdit == EditType.PROPS;
            _variant = 0;
            if (_stageEdit == EditType.PROPS && HasPropVariants(_type - 1))
            {
                GridVariant[] variants = _manager.propPrefabs[_type - 1].variants;
                _variantIDs = new string[variants.Length];
                for (int v = 0; v < variants.Length; v++)
                {
                    _variantIDs[v] = variants[v].variantID.ToUpper();
                }
            }
            else
            {
                _variantIDs = null;
            }
        }

        // Buttons
        GUILayout.Space(MARGIN);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(MARGIN);
        /*
        // View Stage
        if (GUILayout.Button("LAYOUT", GUILayout.Width(BTN_WIDTH), GUILayout.Height(BTN_HEIGHT)))
        {

        }
        */
        // Randomize props
        if (GUILayout.Button("RANDOMIZE\nPROPS", GUILayout.Width(BTN_WIDTH), GUILayout.Height(BTN_HEIGHT)))
        {
            // All props changed
            _stageChanged = true;

            // Iterate props
            foreach (PropData prop in _stage.props)
            {
                // Randomize
                if (prop.propPrefab >= 2)
                {
                    prop.propPrefab = UnityEngine.Random.Range(2, _manager.propPrefabs.Length);
                    prop.propPrefabVariant = 0;
                    if (HasPropVariants(prop.propPrefab))
                    {
                        int max = _manager.propPrefabs[prop.propPrefab].variants.Length;
                        prop.propPrefabVariant = UnityEngine.Random.Range(0, max);
                    }
                }
            }
        }
        // Space
        GUILayout.FlexibleSpace();
        // Delete Stage
        GUI.enabled = _stageChanged;
        GUI.color = Color.white;
        if (GUILayout.Button("CLEAR CHANGES", GUILayout.Width(BTN_WIDTH), GUILayout.Height(BTN_HEIGHT)))
        {
            SetStage(_stageIndex);
        }
        // Save Stage
        GUI.color = Color.green;
        if (GUILayout.Button("SAVE CHANGES", GUILayout.Width(BTN_WIDTH), GUILayout.Height(BTN_HEIGHT)))
        {
            GUI.enabled = true;
            GUI.color = Color.white;
            if (EditorUtility.DisplayDialog("Save Stage", "Are you sure you would like to save the stage '" + _stage.stageID + " '", "Okay", "Cancel"))
            {
                // Log save error
                string error = SaveStage(_stage);
                if (!string.IsNullOrEmpty(error))
                {
                    EditorUtility.DisplayDialog("Save Stage", "Could not save stage '" + _stage.stageID + " '.  " + error, "Okay");
                }
                else
                {
                    _stageChanged = false;
                }
            }
        }
        GUI.color = Color.white;
        GUI.enabled = true;
        GUILayout.Space(MARGIN);
        EditorGUILayout.EndHorizontal();

        // Layout grid
        GUILayout.Space(MARGIN);
        _stageScroll = EditorGUILayout.BeginScrollView(_stageScroll);
        for (int r = 0; r < _stage.rows; r++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < _stage.columns; c++)
            {
                int tileIndex = GridManager.GetTileIndex(_stage, c, r);
                if (_stageEdit == EditType.TILE)
                {
                    GridTileUI(tileIndex);
                }
                else
                {
                    GridItemUI(tileIndex);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        GUI.color = Color.white;
        EditorGUILayout.EndScrollView();

        // End
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
    }

    // Save stage to json
    public static string SaveStage(StageData stageData)
    {
        // Invalid stage
        string error = CheckStageValid(stageData);
        if (!string.IsNullOrEmpty(error))
        {
            return error;
        }

        // Encode to json
        string jsonString = "";
        try
        {
            jsonString = JsonUtility.ToJson(stageData, true);
        }
        catch (Exception e)
        {
            return "Failed to encode stage into json!\n\nException:\n" + e.Message;
        }
        if (string.IsNullOrEmpty(jsonString))
        {
            return "Failed to encode stage into json!";
        }

        // Save to file
        string stagePath = GridManager.GetStagePath(stageData.stageID);
        try
        {
            // Add directory if needed
            string dir = GridManager.GetStageDirectory();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Save path
            File.WriteAllText(stagePath, jsonString);
        }
        catch (Exception e)
        {
            return "Failed to save stage json to path!\nPath: " + stagePath + "\n\nException:\n" + e.Message;
        }

        // Success
        AssetDatabase.Refresh();
        return "";
    }

    // Ensure stage is valid
    private static string CheckStageValid(StageData stageData)
    {
        // Null error
        if (stageData == null)
        {
            return "Stage data is null.";
        }

        // Check props
        if (stageData.props == null)
        {
            return "Stage props are null.";
        }
        bool foundWinner = false;
        foreach (PropData prop in stageData.props)
        {
            // Check for winner
            if (prop.isWinProp)
            {
                foundWinner = true;
            }
            // Invalid tile index
            if (prop.tileIndex < 0 || prop.tileIndex >= stageData.rows * stageData.columns)
            {
                return "Invalid prop tile index: " + prop.tileIndex;
            }
            // Check for players
            int playerIndex = HasPlayer(stageData, prop.tileIndex);
            if (playerIndex != -1)
            {
                return "Stage prop shares tile " + prop.tileIndex + " with Player " + (playerIndex + 1);
            }

        }
        if (!foundWinner)
        {
            return "Stage has no win prop.";
        }

        // Ensure all players are set to existing tiles
        if (stageData.players == null)
        {
            return "Stage players are null.";
        }
        for (int p = 0; p < stageData.players.Length; p++)
        {
            // Player
            PlayerData player = stageData.players[p];
            // Invalid tile index
            if (player.tileIndex < 0 || player.tileIndex >= stageData.rows * stageData.columns)
            {
                return "Player " + (p + 1) + " is not set to a valid tile.";
            }
        }

        // Success
        return "";
    }
#endregion

#region TILES
    // Layout tiles
    public void GridTileUI(int tileIndex)
    {
        // Get item id & color
        string tileID = "#" + tileIndex;
        Color tileColor = emptyTileColor;
        int index = GridManager.HasTile(_stage, tileIndex);
        if (index == -1)
        {
            tileColor = noTileColor;
        }

        // Button
        GUI.color = tileColor;
        bool wasPressed = GUILayout.Button(tileID, GUILayout.Width(TILE_WIDTH), GUILayout.Height(TILE_HEIGHT));

        // Drawing
        Rect inRect = GUILayoutUtility.GetLastRect();
        if (_isDrawing && Event.current.type == EventType.Repaint && inRect.Contains(Event.current.mousePosition))
        {
            wasPressed = true;
        }

        // Handle press
        if (wasPressed)
        {
            // Needs save
            _stageChanged = true;
            // Remove
            index = GridManager.HasTile(_stage, tileIndex);
            if (_type == 0 && index != -1)
            {
                // Remove items
                RemoveItems(_stage, tileIndex);
                // Remove tile
                List<TileData> tiles = new List<TileData>(_stage.tiles);
                tiles.RemoveAt(index);
                _stage.tiles = tiles.ToArray();
            }
            // Add/Change Type
            else if (_type > 0)
            {
                // Add new
                if (index == -1)
                {
                    TileData newTile = new TileData();
                    newTile.tileIndex = tileIndex;
                    newTile.tilePrefab = _type - 1;
                    newTile.tilePrefabVariant = 0;
                    List<TileData> tiles = new List<TileData>(_stage.tiles);
                    tiles.Add(newTile);
                    _stage.tiles = tiles.ToArray();
                }
                // Simply set
                else
                {
                    _stage.tiles[index].tilePrefab = _type - 1;
                }
            }
            // Recalculat Adjacents
            RecalculateAdjacents(_stage, tileIndex);
        }
        // Add adjacent border overlay
        else if (index != -1)
        {
            TileData tile = _stage.tiles[index];
            AddSideText(tile.tileUp.ToString(), inRect, Direction.Up);
            AddSideText(tile.tileDown.ToString(), inRect, Direction.Down);
            AddSideText(tile.tileLeft.ToString(), inRect, Direction.Left);
            AddSideText(tile.tileRight.ToString(), inRect, Direction.Right);       
        }
    }
    // Add text to rect
    private static void AddSideText(string text, Rect inRect, Direction direction)
    {
        // Get size
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));

        // Get new rect
        Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);

        // X middle
        if (direction == Direction.Up || direction == Direction.Down)
        {
            rect.x += (inRect.width - size.x) / 2f;
        }
        // X right
        else if (direction == Direction.Right)
        {
            rect.x += inRect.width - size.x;
        }

        // Y middle
        if (direction == Direction.Left || direction == Direction.Right)
        {
            rect.y += (inRect.height - size.y) / 2f;
        }
        // Y bottom
        else if (direction == Direction.Down)
        {
            rect.y += inRect.height - size.y;
        }

        // Shown label
        GUI.Label(rect, text);
    }
    // Recalculate all adjacents
    private static void RecalculateAdjacents(StageData stageData, int tileIndex)
    {
        // Get path
        int column;
        int row;
        GridManager.GetPath(stageData, tileIndex, out column, out row);
        if (column == -1 || row == -1)
        {
            Debug.LogError("COULD NOT CALCULATE ADJACENTS");
            return;
        }

        // Recalculate row
        RecalculateAdjacents(stageData, true, row);

        // Recalculate column
        RecalculateAdjacents(stageData, false, column);
    }
    // Iterate through in a specific direction
    private static void RecalculateAdjacents(StageData stageData, bool horizontal, int v)
    {
        // Total
        int total = horizontal ? stageData.columns : stageData.rows;

        // Iterate through all
        for (int i = 0; i < total; i++)
        {
            // Get this tile
            int thisTileIndex = GridManager.GetTileIndex(stageData, horizontal ? i : v, horizontal ? v : i);
            int thisLookup = GridManager.HasTile(stageData, thisTileIndex);
            if (thisLookup == -1)
            {
                continue;
            }
            TileData thisTile = stageData.tiles[thisLookup];

            // Get next tile
            int i2 = Iterate(i, total, true);
            int nextTileIndex = GridManager.GetTileIndex(stageData, horizontal ? i2 : v, horizontal ? v : i2);
            int nextLookup = GridManager.HasTile(stageData, nextTileIndex);
            // Find open in previous
            int i3 = i;
            while (nextLookup == -1)
            {
                i3 = Iterate(i3, total, false);
                int checkTileIndex = GridManager.GetTileIndex(stageData, horizontal ? i3 : v, horizontal ? v : i3);
                int checkLookup = GridManager.HasTile(stageData, checkTileIndex);
                if (checkLookup == -1)
                {
                    i3 = Iterate(i3, total, true);
                    nextTileIndex = GridManager.GetTileIndex(stageData, horizontal ? i3 : v, horizontal ? v : i3);
                    nextLookup = GridManager.HasTile(stageData, nextTileIndex);
                }
            }
            TileData nextTile = stageData.tiles[nextLookup];

            // Set horizontal items
            if (horizontal)
            {
                thisTile.tileRight = nextTileIndex;
                nextTile.tileLeft = thisTileIndex;
            }
            // Set vertical items
            else
            {
                thisTile.tileDown = nextTileIndex;
                nextTile.tileUp = thisTileIndex;
            }
        }
    }
    // Iterate
    private static int Iterate(int original, int total, bool forward)
    {
        int result = original;
        if (forward)
        {
            result++;
            if (result >= total)
            {
                result = 0;
            }
        }
        else
        {
            result--;
            if (result < 0)
            {
                result = total - 1;
            }
        }
        return result;
    }
#endregion

#region ITEMS
    // Layout players & props
    public void GridItemUI(int tileIndex)
    {
        // Get tile index
        string itemID = "#" + tileIndex;
        Color itemColor = emptyTileColor;

        // No tile
        if (GridManager.HasTile(_stage, tileIndex) == -1)
        {
            GUI.enabled = false;
            GUI.color = Color.clear;
            if (GUILayout.Button(itemID, GUILayout.Width(TILE_WIDTH), GUILayout.Height(TILE_HEIGHT)))
            {

            }
            GUI.enabled = true;
            return;
        }

        // Has prop
        int propIndex = HasProp(_stage, tileIndex);
        if (propIndex != -1)
        {
            PropData prop = _stage.props[propIndex];
			itemID += " (" + _directions[((int)prop.direction)] + ")";
			itemID += "\n" + _manager.propPrefabs[prop.propPrefab].gameObject.name;
            if (HasPropVariants(prop.propPrefab))
            {
                itemID += "\n(" + _manager.propPrefabs[prop.propPrefab].variants[prop.propPrefabVariant].variantID.ToUpper() + ")";
            }
            if (prop.propPrefab == 0)
            {
                itemColor = wallColor;
            }
            else if (prop.isWinProp)
            {
                itemColor = winColor;
            }
            else
            {
                itemColor = propColor;
            }
        }
        // Has player
        int playerIndex = HasPlayer(_stage, tileIndex);
        if (playerIndex != -1)
        {
            PlayerData player = _stage.players[playerIndex];
			itemID += " (" + _directions[((int)player.direction)] + ")";
			itemID += "\nPLAYER " + (playerIndex + 1);
            itemColor = _colors[playerIndex];
        }

        // Button
        GUI.color = itemColor;
        bool wasPressed = GUILayout.Button(itemID, GUILayout.Width(TILE_WIDTH), GUILayout.Height(TILE_HEIGHT));

        // Drawing
        Rect inRect = GUILayoutUtility.GetLastRect();
        if (_isDrawing && Event.current.type == EventType.Repaint && inRect.Contains(Event.current.mousePosition))
        {
            wasPressed = true;
        }

        // Pressed or dragged
        if (wasPressed)
        {
            _stageChanged = true;
            // Clear
            if (_type == 0)
            {
                RemoveItems(_stage, tileIndex);
            }
            // Add player
            else if (_stageEdit == EditType.PLAYER)
            {
                AddPlayer(_stage, tileIndex, _direction, _type - 1);
            }
            // Add prop
            else if (_stageEdit == EditType.PROPS)
            {
                AddProp(_stage, tileIndex, _direction, _type - 1, _variant, _isWin);
            }
        }
    }

    // Determine if a player is on a space
    private static int HasPlayer(StageData level, int tileIndex)
    {
        for (int p = 0; p < level.players.Length; p++)
        {
            PlayerData playerDatum = level.players[p];
            if (playerDatum.tileIndex == tileIndex)
            {
                return p;
            }
        }
        return -1;
    }

    // Add player
    private void AddPlayer(StageData stage, int tileIndex, Direction direction, int playerIndex)
    {
        // Remove prop/player
        RemoveItems(stage, tileIndex);

        // Set player
        PlayerData player = stage.players[playerIndex];
        player.tileIndex = tileIndex;
        player.direction = direction;
    }

    // Determine if a prop is on a space
    private static int HasProp(StageData stage, int tileIndex)
    {
        for (int p = 0; p < stage.props.Length; p++)
        {
            PropData propDatum = stage.props[p];
            if (propDatum.tileIndex == tileIndex)
            {
                return p;
            }
        }
        return -1;
    }

    // Get prefab
    private bool HasPropVariants(int propPrefabIndex)
    {
        if (_manager != null && _manager.propPrefabs != null && propPrefabIndex >= 0 && propPrefabIndex < _manager.propPrefabs.Length)
        {
            GridProp prefab = _manager.propPrefabs[propPrefabIndex];
            if (prefab != null && prefab.variants != null && prefab.variants.Length > 1)
            {
                return true;
            }
        }
        return false;
    }

    // Add prop
    private void AddProp(StageData stage, int tileIndex, Direction direction, int propPrefab, int propVariant, bool isWinner)
    {
        // Remove prop/player
        RemoveItems(stage, tileIndex);

        // Create new
        PropData prop = new PropData();
        prop.tileIndex = tileIndex;
        prop.propPrefab = propPrefab;
        prop.direction = direction;
        prop.isWinProp = isWinner;
        prop.propPrefabVariant = propVariant;

        // Add
        List<PropData> props = new List<PropData>();
        if (stage.props != null)
        {
            props.AddRange(stage.props);
        }
        props.Add(prop);
        stage.props = props.ToArray();
    }

    // Remove player & grid
    private void RemoveItems(StageData stage, int tileIndex)
    {
        // Remove player
        int playerIndex = HasPlayer(stage, tileIndex);
        if (playerIndex != -1)
        {
            PlayerData player = stage.players[playerIndex];
            player.tileIndex = -1;
        }

        // Remove prop
        int propIndex = HasProp(stage, tileIndex);
        if (propIndex != -1)
        {
            List<PropData> props = new List<PropData>();
            if (stage.props != null)
            {
                props.AddRange(stage.props);
            }
            props.RemoveAt(propIndex);
            stage.props = props.ToArray();
        }
    }
#endregion
}
