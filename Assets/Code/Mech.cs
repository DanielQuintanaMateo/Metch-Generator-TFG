using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech : MonoBehaviour
{
    //Atributes
   #region public atributes
   
   

   #endregion
   
   
   #region private atributes
   
   private List<Piece> pieces;
   private Stats stats;
   
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

   public Mech(List<Piece> pieces, Stats stats)
   {
       this.pieces = pieces;
       this.stats = stats;
   }
   
   #endregion
}
