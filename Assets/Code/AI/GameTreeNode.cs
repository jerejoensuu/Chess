using System.Collections.Generic; 
using Code.Board; 
using Code.Data; 
using UnityEngine; 
 
namespace Code.AI 
{ 
     
    // Contains the state of the game at a given point in time 
    public class GameTreeNode 
    { 
        public Piece[] Pieces; 
        public FenString FenString; 
        public float BestEvaluation { get; set; } 
        public Move NodeOriginMove { get; set; } 
        public Move BestMove { get; set; } 
        // Other properties you might need to store, e.g., alpha, beta values, etc. 
        public List<GameTreeNode> Children { get; set; } 
 
        public GameTreeNode(Piece[] pieces, Move nodeOriginMove, FenString fenString) 
        { 
            Pieces = pieces; 
            NodeOriginMove = nodeOriginMove; 
            FenString = fenString; 
            Children = new List<GameTreeNode>(); 
            
            if (nodeOriginMove != null)
                ApplyMove(nodeOriginMove);
        } 
        
        public void ApplyMove(Move move)
        {
            Pieces[move.From].Value = 0;
            Pieces[move.To].Value = move.Type;
        }
    } 
}