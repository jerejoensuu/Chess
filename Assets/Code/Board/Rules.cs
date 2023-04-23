using System;
using System.Collections.Generic;
using Code.Data;
using UnityEngine;

namespace Code.Board
{
    public class Rules : MonoBehaviour
    {
        public bool isWhiteTurn = true;

        public static bool IsMoveLegal(Square selectedSquare, Square targetSquare, Square[] squares, int enPassantIndex = -1)
        {
            List<int> moves = GetMovesForPiece(selectedSquare, squares, enPassantIndex);
            return moves.Contains(targetSquare.index);
        }

        public static List<int> GetMovesForPiece(Square square, Square[] squares, int enPassantIndex = -1)
        {
            List<int> moves = new List<int>();

            int piece = square.pieceValue;
            int index = square.index;

            switch (Piece.GetType(piece))
            {
                case Piece.Pawn:
                    moves = GetMovesForPawn(index, Piece.GetColor(piece), squares, enPassantIndex);
                    break;
                case Piece.Knight:
                    moves = GetMovesForKnight(index, Piece.GetColor(piece), squares);
                    break;
                case Piece.Bishop:
                    moves = GetMovesForBishop(index, Piece.GetColor(piece), squares);
                    break;
                case Piece.Rook:
                    moves = GetMovesForRook(index, Piece.GetColor(piece), squares);
                    break;
                case Piece.Queen:
                    moves = GetMovesForQueen(index, Piece.GetColor(piece), squares);
                    break;
                case Piece.King:
                    moves = GetMovesForKing(index, Piece.GetColor(piece), squares);
                    break;
            }

            return moves;
        }

        private static List<int> GetMovesForPawn(int index, int color, Square[] squares, int enPassantIndex = -1)
        {
            List<int> moves = new List<int>();

            int currentRow = index / 8;
            int currentCol = index % 8;

            int direction = color == Piece.White ? 1 : -1;

            // Check if the pawn can move forward one square
            int targetIndex = (currentRow + direction) * 8 + currentCol;
            if (targetIndex >= 0 && targetIndex < 64 && squares[targetIndex].pieceValue == 0)
            {
                moves.Add(targetIndex);
            }

            // Check if the pawn can move forward two squares from its starting position
            if ((currentRow == 1 && color == Piece.White) || (currentRow == 6 && color == Piece.Black))
            {
                targetIndex = (currentRow + 2 * direction) * 8 + currentCol;
                if (squares[targetIndex].pieceValue == 0 &&
                    squares[(currentRow + direction) * 8 + currentCol].pieceValue == 0)
                {
                    moves.Add(targetIndex);
                }
            }

            // Check if the pawn can capture diagonally to the left
            targetIndex = (currentRow + direction) * 8 + currentCol - 1;
            if (targetIndex >= 0 && targetIndex < 64 && squares[targetIndex].pieceValue != 0 &&
                Piece.GetColor(squares[targetIndex].pieceValue) != color)
            {
                moves.Add(targetIndex);
            }

            // Check if the pawn can capture diagonally to the right
            targetIndex = (currentRow + direction) * 8 + currentCol + 1;
            if (targetIndex >= 0 && targetIndex < 64 && squares[targetIndex].pieceValue != 0 &&
                Piece.GetColor(squares[targetIndex].pieceValue) != color)
            {
                moves.Add(targetIndex);
            }

            // Check for en passant capture to the left
            targetIndex = (currentRow + direction) * 8 + currentCol - 1;
            if (targetIndex == enPassantIndex)
            {
                moves.Add(targetIndex);
            }

            // Check for en passant capture to the right
            targetIndex = (currentRow + direction) * 8 + currentCol + 1;
            if (targetIndex == enPassantIndex)
            {
                moves.Add(targetIndex);
            }

            return moves;
        }


        private static List<int> GetMovesForKnight(int index, int color, Square[] squares)
        {
            int[] rowOffsets = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] colOffsets = { 1, 2, 2, 1, -1, -2, -2, -1 };
            List<int> moves = new List<int>();

            int currentRow = index / 8;
            int currentCol = index % 8;

            for (int i = 0; i < 8; i++)
            {
                int newRow = currentRow + rowOffsets[i];
                int newCol = currentCol + colOffsets[i];

                if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    int targetIndex = newRow * 8 + newCol;
                    int targetPiece = squares[targetIndex].pieceValue;

                    if (targetPiece == 0 || Piece.GetColor(targetPiece) != color)
                    {
                        moves.Add(targetIndex);
                    }
                }
            }

            return moves;
        }

        private static List<int> GetMovesForBishop(int index, int color, Square[] squares)
        {
            return GetSlidingMoves(index, Piece.Bishop | color, squares);
        }

        private static List<int> GetMovesForRook(int index, int color, Square[] squares)
        {
            return GetSlidingMoves(index, Piece.Rook | color, squares);
        }

        private static List<int> GetMovesForQueen(int index, int color, Square[] squares)
        {
            return GetSlidingMoves(index, Piece.Queen | color, squares);
        }

        private static List<int> GetMovesForKing(int index, int color, Square[] squares)
        {
            return GetSlidingMoves(index, Piece.King | color, squares);
        }

        private static List<int> GetSlidingMoves(int index, int piece, Square[] squares)
        {
            int[] offsets = { 8, -8, 1, -1, 9, -9, 7, -7 };
            List<int> moves = new List<int>();

            int startingDirection = Piece.GetType(piece) == Piece.Bishop ? 4 : 0;
            int endingDirection = Piece.GetType(piece) == Piece.Rook ? 4 : 8;

            for (int direction = startingDirection; direction < endingDirection; direction++)
            {
                for (int i = 0; i < PrecomputedData.NumSquaresToEdge[index][direction]; i++)
                {
                    int offset = offsets[direction] * (i + 1);
                    int targetIndex = index + offset;

                    int targetPiece = squares[targetIndex].pieceValue;

                    if (targetPiece != 0 && Piece.GetColor(targetPiece) == Piece.GetColor(piece)) break;

                    moves.Add(targetIndex);

                    if (targetPiece != 0 && Piece.GetColor(targetPiece) != Piece.GetColor(piece)) break;

                    if (Piece.GetType(piece) == Piece.King) break;
                }
            }

            return moves;
        }
    }
}