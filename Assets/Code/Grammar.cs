using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grammar : MonoBehaviour
{
    //Atributes
   #region public atributes

   #endregion
   
   
   #region private atributes
   
   private List<GrammarRule> rules;
   
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

   public void AddRule(GrammarRule rule)
   {
       rules.Add(rule);
   }

   public bool validateRules(List<Piece> pieces)
   {
       foreach (var rule in rules)
       {
           if (!rule.ValidateRule(pieces))
           {
               return false;
           }
       }
       return true;
   }
   
   #endregion
}
