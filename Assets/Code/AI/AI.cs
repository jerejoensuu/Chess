using System.Collections.Generic;
using Code.Board;
using Code.Data;
using UnityEngine;

namespace Code.AI
{
    public abstract class AI
    {
        protected Square[] Squares;
        protected FenString FenString;

        protected Square PickRandomPiece(int color)
        {
            List<Square> pieces = new List<Square>();
            foreach (Square square in Squares)
            {
                if (square.pieceValue != Piece.None && Piece.GetColor(square.pieceValue) == color)
                {
                    List<Move> moves = Rules.GetMovesForPiece(square, Squares, FenString);
                    if (moves.Count > 0)
                        pieces.Add(square);
                }
            }

            if (pieces.Count == 0)
            {
                // TODO: Checkmate
                Debug.Log("Checkmate");
                // stop unity editor
                UnityEditor.EditorApplication.isPlaying = false;
                return null;
            }
            
            return pieces[Random.Range(0, pieces.Count)];
        }

        private List<Square> GetPiecesWithMoves(int color)
        {
            List<Square> pieces = new List<Square>();
            foreach (Square square in Squares)
            {
                if (square.pieceValue != Piece.None && Piece.GetColor(square.pieceValue) == color)
                {
                    List<Move> moves = Rules.GetMovesForPiece(square, Squares, FenString);
                    if (moves.Count > 0)
                        pieces.Add(square);
                }
            }

            return pieces;
        }
        
        protected List<Move> GetAllPossibleMoves(int color)
        {
            List<Square> piecesWithMoves = GetPiecesWithMoves(color);
            List<Move> moves = new List<Move>();
            foreach (Square piece in piecesWithMoves)
            {
                List<Move> pieceMoves = Rules.GetMovesForPiece(piece, Squares, FenString);
                foreach (Move move in pieceMoves)
                {
                    moves.Add(move);
                }
            }

            return moves;
        }

        public Move GetMove(Square[] squares, FenString fenString, int color)
        {
            Squares = squares;
            FenString = fenString;
            return GetMoveInternal(color);
        }

        protected abstract Move GetMoveInternal(int color);
    }
}