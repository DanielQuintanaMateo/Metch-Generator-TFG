using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GrammarRule : MonoBehaviour
{
    //Atributes
   #region public atributes
   
   

   #endregion
   
   
   #region private atributes
   
   
   
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

   public abstract bool ValidateRule(List<Piece> pieces);


   #endregion
}
