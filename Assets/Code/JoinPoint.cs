using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JoinPoint : MonoBehaviour
{
    #region Public Attributes

    [ReadOnly] public Piece ownerPiece;

    [Tooltip("Unique ID for this join point within the piece")]
    [ReadOnly] public string id;

    [Tooltip("List of piece types that can connect here (e.g., Torso, Leg, Arm, Weapon, Head)")]
    public PieceType[] compatiblePieces;

    [Tooltip("Indicates whether this join point is currently occupied")]
    public bool ocuped = false;

    #endregion

    #region Unity Methods

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.05f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.05f, id);
    }
#endif

    #endregion

    #region Public Methods

    /// <summary>
    /// Checks if this join point is mutually compatible with another.
    /// Each join point must accept the piece type of the other.
    /// </summary>
    public bool IsCompatible(JoinPoint other)
    {
        if (ownerPiece == null || other.ownerPiece == null)
        {
            Debug.LogWarning("One of the join points does not have an associated piece.");
            return false;
        }

        PieceType myType = ownerPiece.pieceType;
        PieceType otherType = other.ownerPiece.pieceType;

        bool iAccept = compatiblePieces.Contains(otherType);
        bool otherAccept = other.compatiblePieces.Contains(myType);

        return iAccept && otherAccept;
    }

    #endregion
}