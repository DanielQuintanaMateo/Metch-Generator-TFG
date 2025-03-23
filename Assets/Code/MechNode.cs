using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MechNode
{
    #region Public Attributes

    public Piece piece;
    public string joinIdParent;
    public List<MechNode> childs = new();

    #endregion

    #region Constructors

    public MechNode(Piece piece, string joinIdParent = null)
    {
        this.piece = piece;
        this.joinIdParent = joinIdParent;
    }

    #endregion

    #region Public Methods

    public void AddChild(MechNode child)
    {
        childs.Add(child);
    }

    #endregion
}