using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MechNodeSerializable
{
    public string prefabID;
    public string type;
    public string joinIdParent;
    public List<MechNodeSerializable> childs = new();

    public MechNodeSerializable(MechNode node)
    {
        prefabID = node.piece.pieceName;
        type = node.piece.pieceType.ToString();
        joinIdParent = node.joinIdParent;

        foreach (var child in node.childs)
        {
            childs.Add(new MechNodeSerializable(child));
        }
    }
}