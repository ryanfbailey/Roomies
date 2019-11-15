using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridOverlayPanel : Panel
{
    [Header("Players Overlay")]
    // Player offset
    public Vector2 playerOffset = Vector2.zero;
    // Player prefab
    public PlayerOverlayView playerPrefab;
    // Player overlays
    private RectTransform[] _players;

    [Header("Win Props Overlay")]
    // Win offset
    public Vector2 winOffset = Vector2.zero;
    // Win prefab
    public RectTransform winPrefab;
    // Win overlays
    private RectTransform[] _wins;

    // Other
    public float offsetY = 0.8f;
    private Camera _camera;
    private int[] _propLookup;

    // Add delegates
    protected override void Awake()
    {
        base.Awake();
        GameManager.onGameStateChange += OnGameStateChanged;
        GridManager.onGridLoaded += GridLoaded;
        GridCharacter.onHit += CharacterCollision;
    }
    // Remove delegates
    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.onGameStateChange -= OnGameStateChanged;
        GridManager.onGridLoaded -= GridLoaded;
        GridCharacter.onHit -= CharacterCollision;
    }

    // State changed
    protected virtual void OnGameStateChanged(GameState newState, bool immediately)
    {
        // Show
        if (newState == GameState.GameIntro || newState == GameState.GamePlay || newState == GameState.GamePause)
        {
            Transition(true, immediately);
        }
        // Hide
        else
        {
            Transition(false, immediately);
        }
    }

    // Grid loaded, setup
    private void GridLoaded(GridManager manager)
    {
        // Unload
        if (_players != null)
        {
            foreach (RectTransform playerRect in _players)
            {
                Pool.instance.Unload(playerRect.gameObject);
            }
            _players = null;
        }
        if (_wins != null)
        {
            foreach (RectTransform winRect in _wins)
            {
                Pool.instance.Unload(winRect.gameObject);
            }
            _wins = null;
        }

        // Get cam
        _camera = Camera.main;

        // Load players & win props
        _players = new RectTransform[manager.characters.Length];
        for (int i = 0; i < manager.characters.Length; i++)
        {
            RectTransform playerRect = Pool.instance.Load(playerPrefab.gameObject).GetComponent<RectTransform>();
            playerRect.SetParent(transform);
            playerRect.localPosition = Vector3.zero;
            playerRect.localRotation = Quaternion.identity;
            playerRect.localScale = Vector3.one;
            PlayerOverlayView overlay = playerRect.GetComponent<PlayerOverlayView>();
            overlay.SetPlayer(i);
            _players[i] = playerRect;
            FollowTransform(manager.characters[i].transform, playerRect, playerOffset);
        }
        List<RectTransform> winners = new List<RectTransform>();
        List<int> winProp = new List<int>();
        for (int i = 0; i < manager.props.Length; i++)
        {
            if (manager.props[i].data.isWinProp)
            {
                RectTransform winRect = Pool.instance.Load(winPrefab.gameObject).GetComponent<RectTransform>();
                winRect.SetParent(transform);
                winRect.localPosition = Vector3.zero;
                winRect.localRotation = Quaternion.identity;
                winRect.localScale = Vector3.one;
                winners.Add(winRect);
                winProp.Add(i);
                FollowTransform(manager.props[i].transform, winRect, playerOffset);
            }
        }
        _wins = winners.ToArray();
        _propLookup = winProp.ToArray();
    }

    // Index
    private int GetCharacterIndex(GridCharacter character)
    {
        if (GridManager.instance.characters != null)
        {
            for (int c = 0; c < GridManager.instance.characters.Length; c++)
            {
                if (GridManager.instance.characters[c] == character)
                {
                    return c;
                }
            }
        }
        return -1;
    }
    // Collision
    private void CharacterCollision(GridCharacter hitCharacter, GridCharacter byCharacter)
    {
        int hitIndex = GetCharacterIndex(hitCharacter);
        _players[hitIndex].GetComponent<PlayerOverlayView>().WasHit();
        int byIndex = GetCharacterIndex(byCharacter);
        _players[byIndex].GetComponent<PlayerOverlayView>().DidHit();
    }
    // Update
    private void Update()
    {
        if (_players == null || _wins == null || GridManager.instance.characters == null)
        {
            return;
        }

        // Follow players & win props
        for (int i = 0; i < GridManager.instance.characters.Length; i++)
        {
            GridCharacter character = GridManager.instance.characters[i];
            FollowTransform(character.transform, _players[i], playerOffset);
        }
        for (int i = 0; i < _wins.Length; i++)
        {
            int prefabIndex = _propLookup[i];
            FollowTransform(GridManager.instance.props[prefabIndex].transform, _wins[i], winOffset);
        }
    }

    // Follow transform
    private void FollowTransform(Transform target, RectTransform follower, Vector2 offset)
    {
        if (_camera != null)
        {
            Vector3 screenPoint = _camera.WorldToScreenPoint(target.position + new Vector3(0f, offsetY, 0f));
            Vector2 final = new Vector2(screenPoint.x, screenPoint.y);
            final *= 1080f / (float)Screen.height;
            final += offset;
            follower.anchoredPosition = final;
        }
    }
}
