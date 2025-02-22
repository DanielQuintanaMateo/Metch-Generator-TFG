using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechGenerator : MonoBehaviour
{
    //Atributes
   #region public atributes

   public List<Piece> piecePool;

   #endregion
   
   
   #region private atributes

   private List<Grammar> grammars = new List<Grammar>();
   private List<Piece> mechPieces = new List<Piece>();
   
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

   // public Mech GenerateMech(Stats stats)
   // {
   //     return new Mech();
   // }
   
   #endregion
}
