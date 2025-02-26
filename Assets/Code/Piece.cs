using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
public class Piece : MonoBehaviour
{
    //Atributes
   #region public atributes
   
   [SerializedDictionary("Stat Name", "Value")] public SerializedDictionary<Stats, float> stats;

   #endregion
   
   
   #region private atributes

   [SerializeField] private string pieceName;
   [SerializeField] private PieceType pieceType;
   [SerializeField] private List<JoinPoint> joinPoints;
   
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

   public Piece(string pieceName, PieceType pieceType, SerializedDictionary<Stats, float> stats)
   {
       this.pieceName = pieceName;
       this.pieceType = pieceType;
       this.stats = stats;
   }
   
   #endregion
}
