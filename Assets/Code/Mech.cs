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
   
   private MechNode mainNode;
   private Dictionary<PieceType, float> stats;
   
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

   public Mech(MechNode mainNode)
   {
       this.mainNode = mainNode;
   }
   
   #endregion
}
