using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Piece : MonoBehaviour
{
    //Atributes
   #region public atributes
   
   

   #endregion
   
   
   #region private atributes

   private string pieceName;
   private PieceType pieceType;
   private Stats stats;
   private List<JoinPoint> joinPoints;
   
   #endregion
   
   //Unity Methods
   #region Unity methods
   
   private void Awake()
   {
      
   }

   void Start()
   {
        
   }

   void Update()
   {
        
   }
   
   #endregion
   
   //My Methods
   #region private methods
   
   #endregion

   #region public methods

   public Piece(string pieceName, PieceType pieceType, Stats stats)
   {
       this.pieceName = pieceName;
       this.pieceType = pieceType;
       this.stats = stats;
   }
   
   #endregion
}
