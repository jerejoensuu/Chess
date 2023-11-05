using System;
using System.Collections.Generic;
using Code.Board;
using Code.Data;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace Code.AI
{
    public abstract class AI
    {
        protected Piece[] Pieces;
        protected FenString FenString;

        protected int PickRandomPiece(int color)
        {
            List<int> pieceIndices = new List<int>();
            for (int i = 0; i < Pieces.Length; i++)
            {
                if (Pieces[i].Value != Piece.None && Piece.GetColor(Pieces[i].Value) == color)
                {
                    List<Move> moves = Rules.GetMovesForPiece(i, Pieces, FenString);
                    if (moves.Count > 0)
                        pieceIndices.Add(i);
                }
            }

            if (pieceIndices.Count == 0)
            {
                // TODO: Checkmate
                Debug.Log("Checkmate");
                // stop unity editor
                UnityEditor.EditorApplication.isPlaying = false;
                return 0;
            }
            
            return pieceIndices[Random.Range(0, pieceIndices.Count)];
        }
        
        private List<int> GetPiecesWithMoves(GameTreeNode gameTreeNode, int color)
        {
            List<int> pieceIndices = new List<int>();
            for (int i = 0; i < gameTreeNode.Pieces.Length; i++)
            {
                if (gameTreeNode.Pieces[i].Value != Piece.None && Piece.GetColor(gameTreeNode.Pieces[i].Color) == color)
                {
                    List<Move> moves = Rules.GetMovesForPiece(i, gameTreeNode.Pieces, gameTreeNode.FenString);
                    if (moves.Count > 0)
                        pieceIndices.Add(i);
                }
            }

            return pieceIndices;
        }
        
        [Obsolete("Used by PseudoSmartAIv1.")]
        protected List<Move> GetAllPossibleMoves(int color)
        {
            GameTreeNode gameTreeNode = new GameTreeNode(Pieces, null, FenString);
            List<Move> moves = GetAllPossibleMoves(gameTreeNode, color);

            return moves;
        }
        
        
        protected void GetAllPossibleMoves(GameTreeNode gameTreeNode, ref List<Move> whiteMoves, ref List<Move> blackMoves)
        {
    Profiler.BeginSample("GetAllPossibleMoves");
            List<Move> moves = new List<Move>();
            for (int i = 0; i < gameTreeNode.Pieces.Length; i++)
            {
                Piece piece = gameTreeNode.Pieces[i];
                if (piece.Value == Piece.None)
                    continue;
                
                List<Move> pieceMoves = Rules.GetMovesForPiece(i, gameTreeNode.Pieces, gameTreeNode.FenString);
                if (pieceMoves.Count == 0)
                    continue;
                
                foreach (Move move in pieceMoves)
                {
                    if (piece.Color == Piece.White)
                        whiteMoves.Add(move);
                    else
                        blackMoves.Add(move);
                }
            }
    Profiler.EndSample();
        }
        
        protected List<Move> GetAllPossibleMoves(GameTreeNode gameTreeNode, int color)
        {
            List<Move> moves = new List<Move>();
            for (int i = 0; i < gameTreeNode.Pieces.Length; i++)
            {
                Piece piece = gameTreeNode.Pieces[i];
                if (piece.Value == Piece.None || piece.Color != color)
                    continue;
                
                List<Move> pieceMoves = Rules.GetMovesForPiece(i, gameTreeNode.Pieces, gameTreeNode.FenString);
                if (pieceMoves.Count == 0)
                    continue;
                
                foreach (Move move in pieceMoves)
                {
                    moves.Add(move);
                }
            }
            
            return moves;
        }

        public int GetNumberOfPossibleMoves(GameTreeNode node, int color, int depth = 1)
        {
            List<Move> moves = GetAllPossibleMoves(node, color);
            return moves.Count;
        }
        
        protected int GetNumberOfPieces(Piece[] pieces, int piece, int color)
        {
            int numberOfPieces = 0;
            foreach (Piece p in pieces)
            {
                if (Piece.GetType(p.Type) == piece && Piece.GetColor(p.Color) == color)
                {
                    numberOfPieces++;
                }
            }

            return numberOfPieces;
        }
        
        // Returns the number of all piece types of a certain color.
        // Piece type = list index + 1
        protected int[] GetNumberOfPieces(Piece[] pieces, int color)
        {
            int[] numberOfPieces = new int[6];
            
            foreach (Piece p in pieces)
            {
                if (p.Value != Piece.None && Piece.GetColor(p.Color) == color)
                {
                    numberOfPieces[p.Type - 1]++;
                }
            }
            
            return numberOfPieces;
        }

        public Move GetMove(Piece[] pieces, FenString fenString, int color)
        {
            Pieces = pieces;
            FenString = fenString;
            return GetMoveInternal(pieces, fenString, color);
        }

        protected abstract Move GetMoveInternal(Piece[] pieces, FenString fenString, int color);
    }
}