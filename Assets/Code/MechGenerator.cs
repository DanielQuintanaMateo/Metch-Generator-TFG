using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MechGenerator : MonoBehaviour
{
    #region Public Attributes

    public MechPieceDatabase mechPieceDatabase;
    public PieceType[] cachedTypes;
    public List<GameObject> piecePool;
    Dictionary<PieceType, List<Piece>> piecesByType = new Dictionary<PieceType, List<Piece>>();

    #endregion

    #region Private Attributes

    private List<Grammar> grammars = new List<Grammar>();
    private List<Piece> mechPieces = new List<Piece>();

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Prepopulate the piece pool with a few inactive instances for each piece
        foreach (var piece in mechPieceDatabase.pieces)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject instance = Instantiate(piece.prefab, Vector3.zero, Quaternion.identity);
                instance.SetActive(false);
                foreach (var join in instance.GetComponentsInChildren<JoinPoint>())
                {
                    join.ownerPiece = piece;
                }
                piecePool.Add(instance);
            }
        }
        cachedTypes = (PieceType[])System.Enum.GetValues(typeof(PieceType));
    }

    private void Start()
    {
        GenerateMech();
    }

    #endregion

    #region Public Methods

    public void GenerateMech()
    {
        // Clear the dictionary before rebuilding it
        piecesByType.Clear();
        
        // Build dictionary of pieces by type
        foreach (var type in cachedTypes)
        {
            piecesByType.Add(type, TypeFiler(type));
        }

        // Select a torso randomly as the root
        List<Piece> torsoPieces = ObtainPieces(PieceType.Torso);
        if (torsoPieces == null || torsoPieces.Count == 0)
        {
            Debug.LogError("No torso pieces available.");
            return;
        }
        Piece selectedTorso = torsoPieces[Random.Range(0, torsoPieces.Count)];
        MechNode mainNode = new MechNode(selectedTorso);

        // Build the mech tree (with a max depth of 5)
        string jsonString = SelectPiece(mainNode, 0, 5);
        // Build the mech from the JSON and instantiate it in the scene
        GameObject mechGO = BuildFromJson(jsonString);
        if (mechGO != null)
        {
            // Optionally, set mechGO active or parent it to a scene container
            mechGO.SetActive(true);
        }
    }

    public List<Piece> TypeFiler(PieceType tipo)
    {
        return mechPieceDatabase.pieces.FindAll(p => p.pieceType == tipo);
    }

    public List<Piece> ObtainPieces(PieceType tipo)
    {
        return piecesByType.ContainsKey(tipo) ? piecesByType[tipo] : new List<Piece>();
    }

    /// <summary>
    /// Recursively builds the mech tree by connecting compatible pieces.
    /// Returns the JSON representation if called at the root.
    /// </summary>
    public string SelectPiece(MechNode node, int depth = 0, int maxDepth = 5)
{
    if (depth >= maxDepth)
        return "";

    foreach (JoinPoint parentJoin in node.piece.joinPoints)
    {
        // Skip join points that are already occupied
        if (parentJoin.ocuped)
            continue;

        if (parentJoin.compatiblePieces == null || parentJoin.compatiblePieces.Length == 0)
            continue;

        // Shuffle the list of compatible piece types for this join
        List<PieceType> shuffledTypes = parentJoin.compatiblePieces.OrderBy(x => Random.value).ToList();

        foreach (PieceType candidateType in shuffledTypes)
        {
            List<Piece> candidates = ObtainPieces(candidateType);
            if (candidates == null || candidates.Count == 0)
                continue;

            List<Piece> shuffledCandidates = candidates.OrderBy(x => Random.value).ToList();

            foreach (Piece candidate in shuffledCandidates)
            {
                // Use the candidate's stored join points (loaded during database validation)
                JoinPoint[] childJoins = candidate.joinPoints.ToArray();
                // Find a compatible join point in the candidate that is not occupied
                JoinPoint childJoin = childJoins.FirstOrDefault(j => parentJoin.IsCompatible(j) && !j.ocuped);
                if (childJoin == null)
                    continue;

                // Valid connection found; create a new node for the candidate piece
                MechNode childNode = new MechNode(candidate, parentJoin.id);
                
                // Mark both join points as occupied
                parentJoin.ocuped = true;
                childJoin.ocuped = true;
                
                // Recursively select pieces for the child node
                SelectPiece(childNode, depth + 1, maxDepth);
                node.AddChild(childNode);
                break; // exit candidate loop for this join
            }

            // Once a valid connection is made for this join, break out of candidate type loop
            if (node.childs.Any(c => c.joinIdParent == parentJoin.id))
                break;
        }
    }

    // Only serialize if it's the root call (depth 0)
    if (depth == 0)
    {
        MechNodeSerializable rootSerializable = new MechNodeSerializable(node);
        return JsonUtility.ToJson(rootSerializable, true);
    }

    return "";
}


    public GameObject BuildFromJson(string jsonString)
    {
        MechNodeSerializable root = JsonUtility.FromJson<MechNodeSerializable>(jsonString);
        return RecursiveBuild(root, null);
    }

    /// <summary>
    /// Recursively builds the mech by retrieving pieces from the pool and aligning them according to join points.
    /// </summary>
    public GameObject RecursiveBuild(MechNodeSerializable node, Transform parent)
{
    // Retrieve the piece from the database using the node's prefabID
    Piece piece = mechPieceDatabase.GetPieceByID(node.prefabID);
    if (piece == null)
    {
        Debug.LogError($"[Assembler] Piece not found: {node.prefabID}");
        return null;
    }
    
    // Retrieve a GameObject from the pool (or instantiate if needed)
    GameObject instance = GetFromPool(piece);
    instance.name = piece.pieceName;
    
    // Reassign ownerPiece for each JoinPoint in the instance
    JoinPoint[] instanceJoins = instance.GetComponentsInChildren<JoinPoint>();
    foreach (JoinPoint j in instanceJoins)
    {
        j.ownerPiece = piece;
    }
    
    // Reset scale to match the prefab exactly.
    instance.transform.localScale = piece.prefab.transform.localScale;
    
    // If a parent and a join ID are provided, align the instance accordingly.
    if (parent != null && !string.IsNullOrEmpty(node.joinIdParent))
    {
        // Find the parent's join point using the join ID stored in the node
        JoinPoint parentJoin = parent.GetComponentsInChildren<JoinPoint>()
                                     .FirstOrDefault(j => j.id == node.joinIdParent);
        
        if (parentJoin == null)
        {
            Debug.LogError($"[Assembler] JoinPoint '{node.joinIdParent}' not found in parent");
            instance.transform.SetParent(parent, false);
        }
        else if (parentJoin.ocuped)
        {
            Debug.LogError($"[Assembler] Parent join '{parentJoin.id}' is already occupied. Skipping connection for {piece.pieceName}.");
            return null;
        }
        else
        {
            // Use the candidate's stored join points (already assigned above)
            JoinPoint childJoin = piece.joinPoints.FirstOrDefault(j => parentJoin.IsCompatible(j) && !j.ocuped);
            if (childJoin == null)
            {
                Debug.LogError($"[Assembler] No compatible join in {piece.pieceName} for parent join '{parentJoin.id}'.");
                return null;
            }
            
            // Find the corresponding join on the instance
            JoinPoint instanceChildJoin = instance.GetComponentsInChildren<JoinPoint>()
                                                .FirstOrDefault(j => j.id == childJoin.id);
            
            if (instanceChildJoin == null)
            {
                Debug.LogError($"[Assembler] Could not find join '{childJoin.id}' on instance of {piece.pieceName}");
                return null;
            }
            
            // Detach instance temporarily to compute world positions correctly.
            instance.transform.SetParent(null, true);
            
            // Compute world offset so that the child's join aligns with the parent's join.
            Vector3 worldOffset = parentJoin.transform.position - instanceChildJoin.transform.position;
            instance.transform.position += worldOffset;
            
            // Compute the rotation difference to align the child's join with the parent's join.
            Quaternion rotationDiff = parentJoin.transform.rotation * Quaternion.Inverse(instanceChildJoin.transform.rotation);
            instance.transform.rotation = rotationDiff * instance.transform.rotation;
            
            // Reapply the original scale from the prefab.
            instance.transform.localScale = piece.prefab.transform.localScale;
            
            // Reparent the instance under the parent.
            instance.transform.SetParent(parent, true);
            
            // Mark both join points as occupied.
            parentJoin.ocuped = true;
            instanceChildJoin.ocuped = true;
        }
    }
    else
    {
        // For the root node or if no join is specified, attach the instance directly.
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
    }
    
    // Recursively build and attach all child nodes.
    foreach (var child in node.childs)
    {
        GameObject childInstance = RecursiveBuild(child, instance.transform);
        if(childInstance == null)
        {
            Debug.LogWarning($"[Assembler] Failed to build child for {piece.pieceName}");
        }
    }
    
    return instance;
}


    
    /// <summary>
    /// Retrieves a GameObject for the given piece from the pool. If none is available, it instantiates a new one.
    /// </summary>
    private GameObject GetFromPool(Piece piece)
    {
        // Find an inactive GameObject in the pool with a matching name
        GameObject pooledObj = piecePool.Find(obj => !obj.activeInHierarchy && obj.name == piece.pieceName);
        if (pooledObj != null)
        {
            pooledObj.SetActive(true);
            return pooledObj;
        }
        else
        {
            // Instantiate a new instance, add it to the pool, and return it
            GameObject newObj = Instantiate(piece.prefab);
            newObj.name = piece.pieceName;
            piecePool.Add(newObj);
            return newObj;
        }
    }

    public MechNodeSerializable ConvertToNodeSerializable(MechNode node)
    {
        var s = new MechNodeSerializable(node)
        {
            prefabID = node.piece.pieceName,
            type = node.piece.pieceType.ToString(),
            joinIdParent = node.joinIdParent
        };

        foreach (var child in node.childs)
        {
            s.childs.Add(ConvertToNodeSerializable(child));
        }

        return s;
    }

    #endregion
}
