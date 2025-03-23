using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "Piece Database", menuName = "Mech/Piece Database", order = 2)]
public class MechPieceDatabase : ScriptableObject
{
    #region Public Attributes

    [Tooltip("Complete list of pieces available to assemble mechs.")]
    public List<Piece> pieces;

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        Debug.Log("[Database] Activated.");
        ValidateData();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateData();
    }
#endif

    #endregion

    #region Public Methods

    public void ValidateData()
    {
        if (pieces == null || pieces.Count == 0)
        {
            Debug.LogWarning("[Database] The piece list is empty.");
            return;
        }

        HashSet<string> ids = new HashSet<string>();
        foreach (var piece in pieces)
        {
            if (piece == null)
            {
                Debug.LogError("[Database] A piece in the list is null.");
                continue;
            }

            if (string.IsNullOrEmpty(piece.pieceName))
            {
                string path = AssetDatabase.GetAssetPath(piece);
                string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                piece.pieceName = filename;
                Debug.Log($"[Database] Renamed piece to: {piece.pieceName}");
            }

            if (!ids.Add(piece.pieceName))
            {
                Debug.LogWarning($"[Database] Duplicate detected: {piece.pieceName}");
            }

            if (piece.prefab == null)
            {
                Debug.LogWarning($"[Database] {piece.pieceName} does not have a prefab assigned.");
            }

            if (piece.joinPoints == null || piece.joinPoints.Count <= 0)
            {
                piece.joinPoints = new List<JoinPoint>();
                JoinPoint[] prefabJoinPoints = piece.prefab.GameObject().GetComponentsInChildren<JoinPoint>();
                for (int i = 0; i < prefabJoinPoints.Length; i++)
                {
                    prefabJoinPoints[i].ownerPiece = piece;
                    piece.joinPoints.Add(prefabJoinPoints[i]);
                }

                Debug.Log($"[Database] Loaded {piece.joinPoints.Count} join points for {piece.pieceName}.");
            }
        }
        Debug.Log($"[Database] Validation complete. {pieces.Count} pieces loaded.");
    }

    public List<Piece> TypeFiler(PieceType tipo)
    {
        return pieces.FindAll(p => p.pieceType == tipo);
    }

    public Piece GetPieceByID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[Database] Piece ID is null or empty.");
            return null;
        }

        foreach (var piece in pieces)
        {
            if (piece != null && piece.pieceName == id)
            {
                return piece;
            }
        }
        Debug.LogWarning($"[Database] No piece found with ID: {id}");
        return null;
    }

    #endregion
}
