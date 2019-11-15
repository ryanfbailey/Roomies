using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePlayPanel : GameStatePanel
{
    [Header("Play UI")]
    // Pause ui
    public RoomiesButton pauseButton;
    public TextMeshProUGUI pauseLabel;

    // Add btn delegates
    protected override void Awake()
    {
        base.Awake();
        pauseButton.onSubmit += Pause;
    }
    // Remove btn delegates
    protected override void OnDestroy()
    {
        base.OnDestroy();
        pauseButton.onSubmit -= Pause;
    }
    // Show if game panel
    protected override bool ShouldShow()
    {
        return GameManager.instance.gameState == GameState.GamePlay;
    }
    // Update
    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    // Go to pause menu
    protected override void OnPlayerPause(int playerIndex)
    {
        GamePausePanel.playerIndex = playerIndex;
        Pause();
    }
    // Pause
    private void Pause()
    {
        GameManager.instance.SetState(GameState.GamePause);
    }

    // Reload
    private const char DELIMITER = '|';
    private void OnEnable()
    {
        // Set button text
        if (GameManager.instance.players != null && pauseLabel != null)
        {
            // Pause name list
            List<string> pauseNames = new List<string>();
            pauseNames.Add("Esc");
            pauseNames.Add("+");
            pauseNames.Add("-");

            // Set pause text
            string pauseText = "";
            pauseNames.Sort(delegate(string s1, string s2)
            {
                return string.Compare(s2, s1, System.StringComparison.CurrentCultureIgnoreCase);
            });
            foreach (string pauseID in pauseNames)
            {
                if (!string.IsNullOrEmpty(pauseText))
                {
                    pauseText += " / ";
                }
                pauseText += pauseID;
            }
            pauseLabel.text = pauseText;
        }
    }
}
