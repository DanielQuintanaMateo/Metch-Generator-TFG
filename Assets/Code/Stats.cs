using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    //Atributes
   #region public atributes
   
   public int lvl;
   public int attack;
   public int defense;
   public int speed;

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

   public Stats(int lvl, int attack, int defense, int speed)
   {
       this.lvl = lvl;
       this.attack = attack;
       this.defense = defense;
       this.speed = speed;
   }
   
   #endregion
}
