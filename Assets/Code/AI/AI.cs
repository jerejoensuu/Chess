using System.Collections.Generic;
using Code.Board;
using Code.Data;
using UnityEngine;

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

        private List<int> GetPiecesWithMoves(int color)
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

            return pieceIndices;
        }
        
        protected List<Move> GetAllPossibleMoves(int color)
        {
            List<int> piecesWithMoves = GetPiecesWithMoves(color);
            List<Move> moves = new List<Move>();
            foreach (int pieceIndex in piecesWithMoves)
            {
                List<Move> pieceMoves = Rules.GetMovesForPiece(pieceIndex, Pieces, FenString);
                foreach (Move move in pieceMoves)
                {
                    moves.Add(move);
                }
            }

            return moves;
        }

        public Move GetMove(Piece[] pieces, FenString fenString, int color)
        {
            Pieces = pieces;
            FenString = fenString;
            return GetMoveInternal(color);
        }

        protected abstract Move GetMoveInternal(int color);
    }
}