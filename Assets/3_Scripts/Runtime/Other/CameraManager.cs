using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    // Camera
    public Camera camera;
    // Play time positioning
    public Vector2 gameMargin;
    public CinemachineVirtualCamera gameCamPos;
    public Vector2 pauseMargin;
    public CinemachineVirtualCamera pauseCamPos;
    // Intro timeline
    public PlayableDirector introTimeline;
    public CinemachineVirtualCamera introWinProp;
    // Pause/Resume
    public PlayableDirector pauseTimeline;
    public PlayableDirector resumeTimeline;

    // Game Y
    public float gameY { get; private set; }

    // Add delegate
    private void Awake()
    {
        GridManager.onGridLoaded += OnGridLoaded;
        GameManager.onGameStateChange += OnStateChanged;
    }
    // Remove delegate
    private void OnDestroy()
    {
        GridManager.onGridLoaded -= OnGridLoaded;
        GameManager.onGameStateChange -= OnStateChanged;
    }

    // Grid loaded
    private void OnGridLoaded(GridManager manager)
    {
        // Setup cameras that follow cam
        Transform win = null;
        foreach (GridProp prop in manager.props)
        {
            if (prop.data.isWinProp)
            {
                win = prop.transform;
                break;
            }
        }
        if (win != null)
        {
            introWinProp.Follow = win;
            introWinProp.LookAt = win;
        }
        introTimeline.Stop();
        pauseTimeline.Stop();
        resumeTimeline.Stop();

        // Set game distance
        CinemachineTransposer gameT = gameCamPos.GetCinemachineComponent<CinemachineTransposer>();
        Vector3 gameFollow = gameT.m_FollowOffset;
        gameY = GetBestFitDistance(manager.data.rows, manager.data.columns, manager.tileSize, camera.fieldOfView, gameMargin);
        gameFollow.y = gameY;
        gameT.m_FollowOffset = gameFollow;
        // Set pause distance
        CinemachineTransposer pauseT = pauseCamPos.GetCinemachineComponent<CinemachineTransposer>();
        Vector3 pauseFollow = pauseT.m_FollowOffset;
        pauseFollow.y = GetBestFitDistance(manager.data.rows, manager.data.columns, manager.tileSize, camera.fieldOfView, pauseMargin);
        pauseT.m_FollowOffset = pauseFollow;
    }

    // State changed
    private GameState _state = (GameState)(-1);
    private void OnStateChanged(GameState gameState, bool immediate)
    {
        // Intro
        if (gameState == GameState.GameIntro)
        {
            introTimeline.Stop();
            introTimeline.Play();
        }
        // Pause
        else if (gameState == GameState.GamePause)
        {
            resumeTimeline.Stop();
            pauseTimeline.Play();
        }
        // Resume
        else if (gameState == GameState.GamePlay && _state == GameState.GamePause)
        {
            pauseTimeline.Stop();
            resumeTimeline.Play();
        }
        /*
        // Results
        else if (gameState == GameState.Results)
        {
            //GridCharacter character = GridManager.instance.characters[GameManager.instance.lastWinner];
            //resultsWinPlayer.Follow = character.transform;
            //resultsWinPlayer.LookAt = character.transform;
            resultsTimeline.Play();
        }
        // Stop Results
        else if (gameState == GameState.GameLoad || gameState == GameState.Title)
        {
            resultsTimeline.Stop();
        }
        */
        _state = gameState;
    }
    // Determine best fit, given sizes
    private float GetBestFitDistance(int rows, int columns, float tileSize, float fovY, Vector2 margin)
    {
        // Get y best fit
        float worldSizeY = ((rows + 1) * tileSize / 2f) + margin.y;
        float bestFitY = worldSizeY / Mathf.Tan(Mathf.Deg2Rad * fovY / 2f);
        // Determine fovX
        float aspect = (float)Screen.width / (float)Screen.height;
        float fovX = Mathf.Atan((worldSizeY * aspect) / bestFitY);
        // Get x best fit
        float worldSizeX = ((columns + 1) * tileSize / 2f) + margin.x;
        float bestFitX = worldSizeX / Mathf.Tan(fovX);
        // Use biggest
        return Mathf.Max(bestFitX, bestFitY);
    }
}
