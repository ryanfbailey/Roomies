using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

// Game state
public enum GameState
{
    Title = 0, // Intro screen
    PlayerSetup = 1, // Player add/remove, input map, user select, character select, stage select
    GameLoad = 7, // Handles load
    GameIntro = 2, // Simple countdown pre-game & possibly tutorial
    GamePlay = 3, // Handles game actually occuring
    GamePause = 4, // Handles game pause menu
    InputFix = 5, // When controllers disconnect, handles fixing input (once fixed, resume current section)
    RoundComplete = 6, // Shows who won most recent game & scores
    MatchComplete = 8 // Shows who won most recent game & scores
}

// Game data
[Serializable]
public class GameData
{
    // For testing specific stage ids
    public string debugStage;

    // Min player count
    [Range(1, 16)]
    public int minPlayers = 1;
    // Max player count
    [Range(1, 16)]
    public int maxPlayers = 4;
    // Rounds
    public int roundsPerMatch = 3;
    // Colors
    public GameColor[] colors;
    // Character options
    public GameCharacter[] characters;
    // Stage options
    public GameStage[] stages;

    // Setup
    public void Setup()
    {
        if (colors != null)
        {
            foreach (GameColor colorData in colors)
            {
                Color color;
                if (ColorUtility.TryParseHtmlString(colorData.colorHex, out color))
                {
                    colorData.color = color;
                }
                else
                {
                    colorData.color = Color.black;
                }
            }
        }
    }
}

// Game color
[Serializable]
public class GameColor
{
    // Name
    public string colorName;
    // Hex
    public string colorHex;
    // Color
    public Color color;
    // Texture
    public Texture2D colorBorderTexture;
}

// Game manager
public class GameManager : MonoBehaviour
{
    #region SETUP
    // Instance
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }
    private static GameManager _instance;

    [Header("Game Data")]
    // Content override
    public string gameDataPath;
    private bool _isDataLoaded = false;
    // Game data used for game setup
    public GameData gameData;

    // Awake
    protected virtual void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(_instance.gameObject);

        // Get button press callback
        //ReInput.

        // Create player list
        players = new List<GamePlayer>();

        // Load override
        if (!string.IsNullOrEmpty(gameDataPath))
        {
            _isDataLoaded = false;
            FileManager.LoadJson<GameData>(Application.streamingAssetsPath + "/" + gameDataPath + ".json", delegate (GameData gd)
            {
                _isDataLoaded = true;
                if (gd != null)
                {
                    // Use original debug if not empty
                    if (!string.IsNullOrEmpty(gameData.debugStage))
                    {
                        gd.debugStage = gameData.debugStage;
                    }
                    // Set game data
                    gd.Setup();
                    gameData = gd;
                }
            });
        }
        // Use original
        else
        {
            _isDataLoaded = true;
        }
    }
    // Start by setting state
    protected virtual IEnumerator Start()
    {
        // Wait for data load
        while (!_isDataLoaded)
        {
            yield return new WaitForEndOfFrame();
        }
        // Load textures
        LoadGameTextures();
        while (texturesLoading > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        // Set title
        SetState(GameState.Title, false);
    }
    // Load game textures
    private void LoadGameTextures()
    {
        if (gameData != null)
        {
            if (gameData.characters != null)
            {
                for (int c = 0; c < gameData.characters.Length; c++)
                {
                    GameCharacter character = gameData.characters[c];
                    if (!string.IsNullOrEmpty(character.characterIconPath) && character.characterIconTexture == null)
                    {
                        LoadTexture(character.characterIconPath, delegate (Texture2D texture)
                        {
                            character.characterIconTexture = texture;
                        });
                    }
                }
            }
            if (gameData.colors != null)
            {
                for (int c = 0; c < gameData.colors.Length; c++)
                {
                    GameColor color = gameData.colors[c];
                    if (color.colorBorderTexture == null)
                    {
                        LoadTexture(color.colorName + ".png", delegate (Texture2D texture)
                        {
                            color.colorBorderTexture = texture;
                        });
                    }
                }
            }
        }
    }
    // Load texture
    private int texturesLoading = 0;
    public void LoadTexture(string textureName, Action<Texture2D> onTextureLoaded)
    {
        string path = Application.streamingAssetsPath + "/Images/" + textureName;
        texturesLoading++;
        FileManager.LoadTexture(path, delegate (Texture2D texture)
        {
            if (onTextureLoaded != null)
            {
                onTextureLoaded(texture);
            }
            texturesLoading--;
        });
    }
    // Add test player
    protected virtual void Update()
    {
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.P))
		{
			int index = players.Count > 0 ? players[0].inputIndex : 16; // Default to Arrows
			AddPlayer(index);
		}
#endif
		RefreshInputPlayers();
    }
    // Destroy
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    #endregion

    #region PLAYERS
    // Players
    public List<GamePlayer> players { get; private set; }
    // Last winning player
    public int lastWinner { get; private set; }
    // Last match ended
    public bool isEndOfMatch { get; private set; }

    // Player button click
    public static event Action<List<GamePlayer>> onPlayersUpdated;

    // Remove player
    public void RemovePlayer(int playerIndex)
    {
        // Cannot remove
        if (gameState != GameState.Title && gameState != GameState.PlayerSetup)
        {
            return;
        }
		// Invalid index
		if (playerIndex < 0 || playerIndex >= players.Count)
		{
			return;
		}

        // Remove input delegate
        Player inputPlayer = GetInputPlayer(playerIndex);
        if (inputPlayer != null && IsUniqueInput(playerIndex))
        {
            inputPlayer.RemoveInputEventDelegate(OnPlayerButtonEvent, InputActionEventType.ButtonJustReleased);
            inputPlayer.RemoveInputEventDelegate(OnPlayerButtonEvent, InputActionEventType.NegativeButtonJustReleased);
        }

        // Remove player index
        players.RemoveAt(playerIndex);

        // Players
        if (onPlayersUpdated != null)
        {
            onPlayersUpdated(players);
        }
    }
    // Add player
    private GamePlayer AddPlayer(int inputIndex)
    {
        if (players.Count < gameData.maxPlayers)
        {
            // Generate player
            GamePlayer player = new GamePlayer();
            player.userName = "";
            player.ready = false;
            player.inputIndex = inputIndex;
            player.characterIndex = players.Count % GameManager.instance.gameData.characters.Length;
            player.colorIndex = GetUnusedColor();
            player.score = 0;
            players.Add(player);

			// Add delegate
			int playerIndex = players.Count - 1;
            Player inputPlayer = GetInputPlayer(playerIndex);
			if (IsUniqueInput(playerIndex))
			{
				inputPlayer.AddInputEventDelegate(OnPlayerButtonEvent, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased);
				inputPlayer.AddInputEventDelegate(OnPlayerButtonEvent, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustReleased);
			}

            // Update players
            if (onPlayersUpdated != null)
            {
                onPlayersUpdated(players);
            }

            // Return
            return player;
        }
        return null;
    }
    // Get player by index
    public GamePlayer GetPlayer(int playerIndex)
    {
        if (players != null && playerIndex >= 0 && playerIndex < players.Count)
        {
            return players[playerIndex];
        }
        return null;
    }
    // Color data
    private GameColor GetColorData(int playerIndex)
    {
        if (players != null && playerIndex >= 0 && playerIndex < instance.players.Count && gameData != null && gameData.colors != null)
        {
            int colorIndex = instance.players[playerIndex].colorIndex;
            if (colorIndex >= 0 && colorIndex < gameData.colors.Length)
            {
                return gameData.colors[colorIndex];
            }
        }
        return null;
    }
    // Player color
    public Color GetPlayerColor(int playerIndex)
    {
        GameColor colorData = GetColorData(playerIndex);
        if (colorData != null)
        {
            return colorData.color;
        }
        return Color.black;
    }
    // Player color
    public Texture2D GetPlayerBorder(int playerIndex)
    {
        GameColor colorData = GetColorData(playerIndex);
        if (colorData != null)
        {
            return colorData.colorBorderTexture;
        }
        return null;
    }
    // Find unused color
    private int GetUnusedColor()
    {
        List<int> availableColors = new List<int>();
        for (int c = 0; c < gameData.colors.Length; c++)
        {
            availableColors.Add(c);
        }
        foreach (GamePlayer player in instance.players)
        {
            if (availableColors.Contains(player.colorIndex))
            {
                availableColors.Remove(player.colorIndex);
            }
        }
        if (availableColors.Count > 0)
        {
            return availableColors[0];
        }
        else
        {
            return -1;
        }
    }
    #endregion

    #region INPUT
    // Input Keys
    public const string SUBMIT_INPUT_KEY = "Submit";
    public const string CANCEL_INPUT_KEY = "Cancel";
    public const string PAUSE_INPUT_KEY = "Pause";
    public const string HORIZONTAL_INPUT_KEY = "Horizontal";
    public const string HORIZONTAL_POS_INPUT_KEY = "Right";
    public const string HORIZONTAL_NEG_INPUT_KEY = "Left";
    public const string VERTICAL_INPUT_KEY = "Vertical";
    public const string VERTICAL_POS_INPUT_KEY = "Up";
    public const string VERTICAL_NEG_INPUT_KEY = "Down";

    // Player button click callback
    public static event Action<int, string> onPlayerButtonClick;

    // Refresh input players
    private void RefreshInputPlayers()
    {
        // Ignore
        if (!ReInput.isReady)
        {
            return;
        }

        // Add first player to input
        if (gameState == GameState.Title || gameState == GameState.PlayerSetup)
        {
            for (int p = 0; p < ReInput.players.playerCount; p++)
            {
                GamePlayer player = GetPlayerByInput(p);
                if (player == null)
                {
                    Player inputPlayer = ReInput.players.GetPlayer(p);
                    if (inputPlayer.GetAnyButtonUp())
                    {
						player = AddPlayer(p);
                    }
                }
            }
        }
    }
    // Get input player
    private Player GetInputPlayer(int playerIndex)
    {
        GamePlayer player = GetPlayer(playerIndex);
        if (player != null)
        {
            return ReInput.players.GetPlayer(player.inputIndex);
        }
        return null;
    }
	// Check if unique
	private bool IsUniqueInput(int playerIndex)
	{
		int inputIndex = players[playerIndex].inputIndex;
		List<int> playerIndices = GetPlayerIndicesByInput(inputIndex);
		return playerIndices.Count == 1;
	}
	// Get all indices
	private List<int> GetPlayerIndicesByInput(int inputIndex)
	{
		List<int> playerIndices = new List<int>();
		if (players != null)
		{
			for (int p = 0; p < players.Count; p++)
			{
				if (players[p].inputIndex == inputIndex)
				{
					playerIndices.Add(p);
				}
			}
		}
		return playerIndices;
	}
    // Get player by controller
    private int GetPlayerIndexByInput(int inputIndex)
    {
        if (players != null)
        {
            for (int p = 0; p < players.Count; p++)
            {
                if (players[p].inputIndex == inputIndex)
                {
                    return p;
                }
            }
        }
        return -1;
    }
    // Get player by controller
    private GamePlayer GetPlayerByInput(int inputIndex)
    {
        int playerIndex = GetPlayerIndexByInput(inputIndex);
        if (playerIndex != -1)
        {
            return players[playerIndex];
        }
        return null;
    }
    // Player action event delegate
    private void OnPlayerButtonEvent(InputActionEventData actionEvent)
    {
        //Debug.Log("INPUT: " + actionEvent.actionName + "\nPLAYER: " + actionEvent.playerId + "\nEVENT: " + actionEvent.eventType.ToString());
		List<int> playerIndices = GetPlayerIndicesByInput(actionEvent.playerId);
        if (playerIndices.Count > 0)
        {
            // Get action id
            string actionID = actionEvent.actionName;
            if (string.Equals(actionID, HORIZONTAL_INPUT_KEY))
            {
                actionID = actionEvent.eventType == InputActionEventType.NegativeButtonJustReleased ? HORIZONTAL_NEG_INPUT_KEY : HORIZONTAL_POS_INPUT_KEY;
            }
            else if (string.Equals(actionID, VERTICAL_INPUT_KEY))
            {
                actionID = actionEvent.eventType == InputActionEventType.NegativeButtonJustReleased ? VERTICAL_NEG_INPUT_KEY : VERTICAL_POS_INPUT_KEY;
            }

            // Call delegate
            if (onPlayerButtonClick != null)
            {
				foreach (int playerIndex in playerIndices)
				{
					onPlayerButtonClick(playerIndex, actionID);
				}
            }
        }
    }
    // Get player axis input
    public float GetPlayerAxis(int playerIndex, string axisID)
    {
        Player inputPlayer = GetInputPlayer(playerIndex);
        if (inputPlayer != null)
        {
            return Mathf.Clamp(inputPlayer.GetAxis(axisID), -1f, 1f);
        }
        return 0f;
    }
    // Get player button input
    public bool GetPlayerButton(int playerIndex, string buttonID)
    {
        Player inputPlayer = GetInputPlayer(playerIndex);
        if (inputPlayer != null)
        {
            if (string.Equals(buttonID, HORIZONTAL_POS_INPUT_KEY))
            {
                return inputPlayer.GetButton(HORIZONTAL_INPUT_KEY);
            }
            else if (string.Equals(buttonID, HORIZONTAL_NEG_INPUT_KEY))
            {
                return inputPlayer.GetNegativeButton(HORIZONTAL_INPUT_KEY);
            }
            else if (string.Equals(buttonID, VERTICAL_POS_INPUT_KEY))
            {
                return inputPlayer.GetButton(VERTICAL_INPUT_KEY);
            }
            else if (string.Equals(buttonID, VERTICAL_NEG_INPUT_KEY))
            {
                return inputPlayer.GetNegativeButton(VERTICAL_INPUT_KEY);
            }
            return inputPlayer.GetButton(buttonID);
        }
        return false;
    }
#endregion

#region STAGES
    // Current round
    public int currentRound { get; private set; }
    // Set stage
    public string currentStage { get; private set; }
    private List<string> _playedStages = new List<string>();
    // Handle stage load
    public static event Action<string> onGameStageLoad;

    // New match
    public void PlayNewMatch()
    {
        // Reset round
        currentRound = 0;
        _playedStages.Clear();

        // Reset scores
        if (players != null)
        {
            foreach (GamePlayer player in players)
            {
                player.score = 0;
            }
        }

        // Play random stage
        PlayRandomStage();
    }
    // New Round
    public void PlayNewRound()
    {
        // Iterate round
        currentRound++;

        // TODO: Reset same stage
        //GridManager.instance.ResetRound();

        // Play random stage
        PlayRandomStage();
    }
    // Play a random stage
    protected void PlayRandomStage()
    {
        // Start with first
        string stageID = gameData.stages[0].stageID;

        // Use debug
        if (!string.IsNullOrEmpty(gameData.debugStage))
        {
            stageID = gameData.debugStage;
        }
        // Use random
        else
        {
            // Get possible stage list minus used
            List<string> unplayedStages = GetUnplayedStages();

            // Played all stages
            if (unplayedStages.Count == 0)
            {
                _playedStages.Clear();
                unplayedStages = GetUnplayedStages();
            }

            // Get random from list
            if (unplayedStages.Count >= 1)
            {
                int index = UnityEngine.Random.Range(0, unplayedStages.Count);
                stageID = unplayedStages[index];
            }
        }

        // Play stage
        PlayStage(stageID);
    }
    // Get list of available stages
    private List<string> GetUnplayedStages()
    {
        List<string> possibleStages = new List<string>();
        foreach (GameStage stage in gameData.stages)
        {
            if (!_playedStages.Contains(stage.stageID) && currentRound >= stage.stageComplexity)
            {
                possibleStages.Add(stage.stageID);
            }
        }
        return possibleStages;
    }
    // Play a specific stage
    protected void PlayStage(string stageID)
    {
        // Set stage id & call delegate
        currentStage = stageID;
        lastWinner = -1;
        isEndOfMatch = false;
        _playedStages.Add(currentStage);

        // Load stage
        if (onGameStageLoad != null)
        {
            onGameStageLoad(currentStage);
        }

        // Perform set
        PerformSetState(GameState.GameLoad, false);
    }

    // Wins a stage
    public void WinStage(int playerIndex)
    {
        if (gameState != GameState.GamePlay)
        {
            return;
        }
        if (playerIndex < 0 || playerIndex >= players.Count)
        {
            return;
        }

        // Set last winner & add to score
        lastWinner = playerIndex;
        players[playerIndex].score++;
        isEndOfMatch = players[playerIndex].score == GameManager.instance.gameData.roundsPerMatch;

        // Round Complete
        PerformSetState(GameState.RoundComplete, false);
    }
#endregion

#region STATES
    // Game state
    public GameState gameState { get; private set; }
    // Game state change callback (bool is for immediately
    public static event Action<GameState, bool> onGameStateChange;

    // Set state
    public void SetState(GameState newState, bool immediately = false)
    {
        // Not allowed
        if (newState == GameState.GameLoad)
        {
            Debug.LogError("GAME MANAGER - SET STATE FAILED\nCannot set to 'GameLoad' state via set state, use PlayStage instead");
            return;
        }
        if (newState == GameState.RoundComplete)
        {
            Debug.LogError("GAME MANAGER - SET STATE FAILED\nCannot set to 'RoundComplete' state via set state, use WinStage instead");
            return;
        }

        // Perform set
        PerformSetState(newState, immediately);
    }

    // Set state
    private void PerformSetState(GameState newState, bool immediately)
    {
        // Set game state
        gameState = newState;

        // Clear current players & stage
        if (gameState == GameState.Title)
        {
            while (players.Count > 0)
            {
                RemovePlayer(0);
            }
            currentStage = "";
        }

        // Call delegate
        if (onGameStateChange != null)
        {
            onGameStateChange(gameState, immediately);
        }
    }
#endregion
}
