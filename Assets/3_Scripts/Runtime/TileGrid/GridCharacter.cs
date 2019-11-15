using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridCharacterState
{
    Idle,
    Walking,
    Pushing,
    Stunned
}

public class GridCharacter : GridItem
{
    // Player index
    [Range(0, 8)]
    public int playerIndex = 0;

    // Can move & can control
    public bool canMove = true;
    public bool canControl = true;

    // Input threshold
    public float axisMin = 0.3f;

    // Move speed
    public float acceleration = 0.5f;
    public float maxVelocity = 6f;

    // Stun
    public float stunTime = 3f;
    public float stunInvulnerable = 1f;
    public AudioClip stunClip;
    private float _stunElapsed = 0f;

    // Dizzy
    public Vector2 dizzyEffectOffset = new Vector2(0f, 1.5f);
    public GameObject dizzyEffect;
    private Transform _curDizzy;

    // Hit immovable
    public float immovableSpawnTime = 0.5f;
    private float _immovableElapsed = 0f;
    public Vector2 immovableOffset = new Vector2(-0.5f, 0.5f);
    public GameObject immovableEffect;

    // Input, direction, & velocity
    public Direction direction { get; private set; }
    public float velocity { get; private set; }

    // Character state
    public GridCharacterState characterState { get; private set; }
    // State changed
    public Action<GridCharacterState> onStageChange;

    // On hit callback
    public static Action<GridCharacter, GridCharacter> onHit;

    // Create controller & set direction
    private void Awake()
    {
        // Update direction
        _stunElapsed = 0f;
        characterState = (GridCharacterState)(-1);
        UpdateDirection(Direction.None);
    }

    // Place character with variant, tile, and direction
    public void Place(int newVariant, int newTile, Direction newDirection)
    {
        // Set variant
        SetVariant(newVariant);

        // Set tile index
        SetTileIndex(newTile, true);

        // Ready to be stunned
        _stunElapsed = stunTime + stunInvulnerable;
        if (_curDizzy != null)
        {
            Pool.instance.Unload(_curDizzy.gameObject);
            _curDizzy = null;
        }

        // Set direction
        UpdateDirection(Direction.None);
        Direction direction = newDirection;
        if (direction == Direction.None)
        {
            int random = UnityEngine.Random.Range(0, 5) + 1;
            direction = (Direction)random;
        }
        UpdateDirection(direction);
    }

    // Set state & call delegate
    private void SetState(GridCharacterState newState)
    {
        if (characterState != newState)
        {
            characterState = newState;
            if (onStageChange != null)
            {
                onStageChange(characterState);
            }
        }
    }

    // Not pushable
    public override bool CanPush(Direction direction, int startTile)
    {
        // Cannot be pushed except while stunned
        if (characterState != GridCharacterState.Stunned)
        {
            return false;
        }

        // Use base
        return base.CanPush(direction, startTile);
    }

    // Move player if possible
    protected override void Update()
    {
        // Cant move
        if (GameManager.instance == null || GameManager.instance.gameState != GameState.GamePlay)
        {
            return;
        }
        if (tile == null)
        {
            return;
        }

        // Update input
        Vector2 input = Vector2.zero;
        if (canControl)
        {
            input.x = GameManager.instance.GetPlayerAxis(playerIndex, GameManager.HORIZONTAL_INPUT_KEY);
            if (Mathf.Abs(input.x) < axisMin)
            {
                input.x = 0f;
            }
            else
            {
                input.x = input.x > 0f ? 1f : -1f;
            }
            input.y = GameManager.instance.GetPlayerAxis(playerIndex, GameManager.VERTICAL_INPUT_KEY);
            if (Mathf.Abs(input.y) < axisMin)
            {
                input.y = 0f;
            }
            else
            {
                input.y = input.y > 0f ? 1f : -1f;
            }
        }

        // Still stunned
        if (characterState == GridCharacterState.Stunned)
        {
            input = Vector2.zero;
            _stunElapsed += Time.deltaTime;
            if (_stunElapsed >= stunTime)
            {
                if (_curDizzy != null)
                {
                    Pool.instance.Unload(_curDizzy.gameObject);
                    _curDizzy = null;
                }
                SetState(GridCharacterState.Idle);
            }
        }
        // Invulnerable
        else if (_stunElapsed < stunTime + stunInvulnerable)
        {
            _stunElapsed += Time.deltaTime;
        }

        // Get direction
        Direction newDirection = direction;
        if (direction == Direction.Left || direction == Direction.Right)
        {
            if (input.x == 0f)
            {
                if (input.y != 0f)
                {
                    newDirection = input.y > 0 ? Direction.Up : Direction.Down;
                }
            }
            else if (direction == Direction.Left && input.x > 0)
            {
                newDirection = Direction.Right;
            }
            else if (direction == Direction.Right && input.x < 0)
            {
                newDirection = Direction.Left;
            }
        }
        else if (direction == Direction.Up || direction == Direction.Down)
        {
            if (input.y == 0f)
            {
                if (input.x != 0f)
                {
                    newDirection = input.x > 0 ? Direction.Right : Direction.Left;
                }
            }
            else if (direction == Direction.Down && input.y > 0)
            {
                newDirection = Direction.Up;
            }
            else if (direction == Direction.Up && input.y < 0)
            {
                newDirection = Direction.Down;
            }
        }
        UpdateDirection(newDirection);

        // Moving
        float inputVal = direction == Direction.Left || direction == Direction.Right ? input.x : input.y;
        if (Mathf.Abs(inputVal) > 0f)
        {
            // Can push next
            GridTile next = tile.GetAdjacentTile(direction);
            if (next.movable != null && characterState != GridCharacterState.Pushing)
            {
                next.movable.PrePush(this, direction);
            }
            // Cannot move
            if (next.movable != null && !next.movable.CanPush(direction, tileIndex))
            {
                if (_immovableElapsed > 0)
                {
                    _immovableElapsed -= Time.deltaTime;
                }
                else
                {
                    _immovableElapsed = immovableSpawnTime;
                    InstantiateEffect(immovableEffect, transform.position, immovableOffset, direction);
                }
            }
            // Can move
            else
            {
                // Update velocity
                velocity += inputVal * acceleration;
                // Clamp max
                if (maxVelocity > 0f)
                {
                    velocity = Mathf.Clamp(velocity, -maxVelocity, maxVelocity);
                }

                // Add velocity
                float offset = 0f;
                if (direction == Direction.Left || direction == Direction.Right)
                {
                    offset = pushOffset.x;
                }
                else if (direction == Direction.Up || direction == Direction.Down)
                {
                    offset = pushOffset.y;
                }
                offset += velocity * Time.deltaTime;

                // Start push
                if (characterState != GridCharacterState.Pushing && next.movable != null)
                {
                    if (offset > 0f && (direction == Direction.Up || direction == Direction.Right))
                    {
                        SetState(GridCharacterState.Pushing);
                    }
                    else if (offset < 0f && (direction == Direction.Down || direction == Direction.Left))
                    {
                        SetState(GridCharacterState.Pushing);
                    }
                }

                // Push by offset
                Push(direction, offset, characterState == GridCharacterState.Pushing);
            }
        }
        else
        {
            velocity = 0f;
        }

        // Begin walking
        bool isWalking = Mathf.Abs(velocity) > 0f;
        if (isWalking && characterState == GridCharacterState.Idle)
        {
            SetState(GridCharacterState.Walking);
        }
        // Stop walking
        else if (!isWalking && (characterState == GridCharacterState.Walking || characterState == GridCharacterState.Pushing))
        {
            SetState(GridCharacterState.Idle);
        }

        // Update
        base.Update();
    }

    // Update direction
    private void UpdateDirection(Direction newDirection)
    {
        if (direction != newDirection)
        {
            // Change of direction
            velocity = 0f;
            // Immovable change
            _immovableElapsed = 0f;
            // Set direction
            direction = newDirection;
            // Rotate
            transform.localRotation = GridManager.GetRotation(newDirection);
            // Set state
            SetState(GridCharacterState.Idle);
        }
    }

    // Check
    public static bool IsDirectionHorizontal(Direction direction)
    {
        return direction == Direction.Left || direction == Direction.Right;
    }

    // Be stunned
    public override void PrePush(GridItem byItem, Direction inDirection)
    {
        base.PrePush(byItem, inDirection);
        if (byItem.GetType() == typeof(GridCharacter) && byItem != this)
        {
            if (characterState != GridCharacterState.Stunned && _stunElapsed >= stunTime + stunInvulnerable)
            {
                _stunElapsed = 0f;
                MusicManager.PlaySFX(stunClip);
                _curDizzy = InstantiateEffect(dizzyEffect, tile.transform.position, dizzyEffectOffset, inDirection, false);
                _curDizzy.SetParent(transform);

                // Hit
                if (onHit != null)
                {
                    onHit(this, (GridCharacter)byItem);
                }

                // Set state
                SetState(GridCharacterState.Stunned);
            }
        }
    }
    // Instantiate effect
    private Transform InstantiateEffect(GameObject effect, Vector3 position, Vector2 offset, Direction inDirection, bool waitForEnd = true)
    {
        if (effect != null)
        {
            Transform instance = Pool.instance.Load(effect).transform;
            Vector3 rot = Vector3.zero;
            Vector3 final = position;
            final.y += offset.y;
            switch(inDirection)
            {
                case Direction.Left:
                    final.x -= offset.x;
                    rot.y = 90f;
                    break;
                case Direction.Right:
                    final.x += offset.x;
                    rot.y = -90f;
                    break;
                case Direction.Up:
                    final.z += offset.x;
                    rot.y = 180f;
                    break;
                case Direction.Down:
                    final.z -= offset.x;
                    rot.y = 0f;
                    break;
            }
            instance.position = final;
            instance.eulerAngles = rot;
            if (waitForEnd)
            {
                StartCoroutine(WaitForEnd(instance));
            }
            return instance;
        }
        return null;
    }
    // Unload when done
    private IEnumerator WaitForEnd(Transform effect)
    {
        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        while (effect.gameObject.activeInHierarchy && ps != null)
        {
            yield return new WaitForSeconds(0.1f);
            if (!ps.IsAlive(true))
            {
                Pool.instance.Unload(effect.gameObject);
                break;
            }
        }
    }
}
