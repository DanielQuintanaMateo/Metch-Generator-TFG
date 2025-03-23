using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mech Piece", menuName = "Mech/Piece", order = 1)]
public class Piece : ScriptableObject
{
    #region Public Attributes

    [Header("Identification")]
    [ReadOnly] public string pieceName;
    public PieceType pieceType;

    [SerializedDictionary("Stat Name", "Value")] public SerializedDictionary<Stats, float> stats;

    [Header("Visual and Prefabs")]
    public GameObject prefab;

    [Header("Join Parameters")]
    public List<JoinPoint> joinPoints;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Assign this piece as owner to all join points.
        foreach (var join in joinPoints)
        {
            if (join != null)
            {
                join.ownerPiece = this;
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the join point with the specified ID from the stored joinPoints.
    /// </summary>
    public JoinPoint GetJoinByID(string id)
    {
        return joinPoints.FirstOrDefault(j => j.id == id);
    }

    #endregion
}
