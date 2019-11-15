using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour
{
    #region STATE
    // Animator
    private Animator _animator;
    // Character movement
    private GridCharacter _character;

    // On awake, grab
    private void Awake()
    {
        // Grab Animator at start
        _animator = gameObject.GetComponent<Animator>();
        CharacterStateChanged(GridCharacterState.Idle);
        // Grab character
        _character = gameObject.GetComponent<GridCharacter>();
        if (_character != null)
        {
            _character.onStageChange += CharacterStateChanged;
        }
    }
    // On destroy
    private void OnDestroy()
    {
        if (_character != null)
        {
            _character.onStageChange -= CharacterStateChanged;
        }
    }

    // State change
    private void CharacterStateChanged(GridCharacterState state)
    {
        _animator.SetBool("Idling", state == GridCharacterState.Idle || state == GridCharacterState.Stunned);
        _animator.SetBool("Walking", state == GridCharacterState.Walking || state == GridCharacterState.Pushing);
    }
    #endregion

    #region MESH
    public Transform idleCasterLeft;
    public Transform idleCasterRight;
    public float idleForce = 10f;
    public float idleForceOffset = 0.1f;

    public Transform walkingCasterLeft;
    public Transform walkingCasterRight;
    public float walkingForce = 25f;
    public float walkingForceOffset = 0.1f;

    public Transform loseCaster;
    public float loseForce;
    public float loseForceOffset = 0.1f;

    public void IdleCastLeft()
    {
        RaycastHit hit;
        Debug.DrawLine(idleCasterLeft.position, -idleCasterLeft.right * 20, Color.magenta);
        if (Physics.Raycast(idleCasterLeft.position, -idleCasterLeft.right * 20, out hit))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if (deformer)
            {
                Vector3 point = hit.point;
                point += hit.normal * idleForceOffset;
                deformer.AddDeformingForce(point, idleForce);
            }
        }
    }

    public void IdleCastRight()
    {
        RaycastHit hit;
        Debug.DrawLine(idleCasterRight.position, idleCasterRight.right * 20, Color.magenta);
        if (Physics.Raycast(idleCasterRight.position, idleCasterRight.right * 20, out hit))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if (deformer)
            {
                Vector3 point = hit.point;
                point += hit.normal * idleForceOffset;
                deformer.AddDeformingForce(point, idleForce);
            }
        }
    }

    public void WalkingCast()
    {
        RaycastHit hit;
        Debug.DrawLine(walkingCasterLeft.position, -walkingCasterLeft.right * 20, Color.magenta);
        if (Physics.Raycast(walkingCasterLeft.position, -walkingCasterLeft.right * 20, out hit))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if (deformer)
            {
                Vector3 point = hit.point;
                point += hit.normal * walkingForceOffset;
                deformer.AddDeformingForce(point, walkingForce);
            }
        }
        Debug.DrawLine(walkingCasterRight.position, walkingCasterRight.right * 20, Color.magenta);
        if (Physics.Raycast(walkingCasterRight.position, walkingCasterRight.right * 20, out hit))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if (deformer)
            {
                Vector3 point = hit.point;
                point += hit.normal * walkingForceOffset;
                deformer.AddDeformingForce(point, walkingForce);
            }
        }
    }

    public void LoseCast()
    {
        RaycastHit hit;
        // Debug.DrawLine(loseCaster.position, -loseCaster.up * 20, Color.magenta);
        if (Physics.Raycast(loseCaster.position, -loseCaster.up * 20, out hit))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if (deformer)
            {
                Vector3 point = hit.point;
                point += hit.normal * loseForceOffset;
                deformer.AddDeformingForce(point, loseForce);
            }
        }
    }

    public void Nothing()
    {

    }
    #endregion
}
