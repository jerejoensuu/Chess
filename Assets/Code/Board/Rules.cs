using System;
using System.Collections.Generic;
using System.Linq;
using Code.Data;
using UnityEngine;

namespace Code.Board
{
    public static class Rules
    {
        private static int _index;
        private static int[] _pieceValues;
        private static FenString _fenString;
        private static int _color;
        
        public static bool IsMoveLegal(Square selectedSquare, Square targetSquare, Square[] squares, FenString fenString)
        {
            _index = selectedSquare.index;
            _pieceValues = CopyPieceValuesFromSquares(squares);
            _fenString = fenString;
            _color = Piece.GetColor(selectedSquare.pieceValue);
            
            List<int> moves = GetMovesForPiece(selectedSquare.pieceValue);
            return moves.Contains(targetSquare.index);
        }

        public static List<int> GetMovesForPiece(Square square, Square[] squares, FenString fenString)
        {
            _index = square.index;
            _pieceValues = CopyPieceValuesFromSquares(squares);
            _fenString = fenString;
            _color = Piece.GetColor(square.pieceValue);
            
            return GetMovesForPiece(square.pieceValue);
        }
        
        public static int[] CopyPieceValuesFromSquares(Square[] squares)
        {
            int[] pieceValues = new int[squares.Length];
            for (int i = 0; i < squares.Length; i++)
            {
                pieceValues[i] = squares[i].pieceValue;
            }

            return pieceValues;
        }

        public static List<int> GetMovesForPiece(int pieceValue)
        {
            List<int> moves = new List<int>();

            int piece = pieceValue;
            if (_color != (_fenString.WhiteToMove ? Piece.White : Piece.Black)) return moves;

            switch (Piece.GetType(piece))
            {
                case Piece.Pawn:
                    moves = GetMovesForPawn();
                    break;
                case Piece.Knight:
                    moves = GetMovesForKnight();
                    break;
                case Piece.Bishop:
                    moves = GetMovesForBishop();
                    break;
                case Piece.Rook:
                    moves = GetMovesForRook();
                    break;
                case Piece.Queen:
                    moves = GetMovesForQueen();
                    break;
                case Piece.King:
                    moves = GetMovesForKing();
                    break;
            }

            return moves;
        }

        private static List<int> GetMovesForPawn()
        {
            List<int> moves = new List<int>();

            int currentRow = _index / 8;
            int currentCol = _index % 8;

            int direction = _color == Piece.White ? 1 : -1;

            // Check if the pawn can move forward one square
            int targetIndex = (currentRow + direction) * 8 + currentCol;
            if (targetIndex >= 0 && targetIndex < 64 && _pieceValues[targetIndex] == 0)
            {
                moves.Add(targetIndex);
            }

            // Check if the pawn can move forward two squares from its starting position
            if ((currentRow == 1 && _color == Piece.White) || (currentRow == 6 && _color == Piece.Black))
            {
                targetIndex = (currentRow + 2 * direction) * 8 + currentCol;
                if (_pieceValues[targetIndex] == 0 &&
                    _pieceValues[(currentRow + direction) * 8 + currentCol] == 0)
                {
                    moves.Add(targetIndex);
                }
            }

            // Check if the pawn can capture diagonally to the left
            targetIndex = (currentRow + direction) * 8 + currentCol - 1;
            if (targetIndex is >= 0 and < 64
                && currentCol > 0 && _pieceValues[targetIndex] != 0
                && Piece.GetColor(_pieceValues[targetIndex]) != _color)
            {
                moves.Add(targetIndex);
            }

            // Check if the pawn can capture diagonally to the right
            targetIndex = (currentRow + direction) * 8 + currentCol + 1;
            if (targetIndex is >= 0 and < 64
                && currentCol < 7 && _pieceValues[targetIndex] != 0
                && Piece.GetColor(_pieceValues[targetIndex]) != _color)
            {
                moves.Add(targetIndex);
            }

            // Check for en passant capture to the left
            targetIndex = (currentRow + direction) * 8 + currentCol - 1;
            if (targetIndex == _fenString.EnPassantIndex)
            {
                moves.Add(targetIndex);
            }

            // Check for en passant capture to the right
            targetIndex = (currentRow + direction) * 8 + currentCol + 1;
            if (targetIndex == _fenString.EnPassantIndex)
            {
                moves.Add(targetIndex);
            }

            return moves;
        }


        private static List<int> GetMovesForKnight()
        {
            int[] rowOffsets = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] colOffsets = { 1, 2, 2, 1, -1, -2, -2, -1 };
            List<int> moves = new List<int>();

            int currentRow = _index / 8;
            int currentCol = _index % 8;

            for (int i = 0; i < 8; i++)
            {
                int newRow = currentRow + rowOffsets[i];
                int newCol = currentCol + colOffsets[i];

                if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    int targetIndex = newRow * 8 + newCol;
                    int targetPiece = _pieceValues[targetIndex];

                    if (targetPiece == 0 || Piece.GetColor(targetPiece) != _color)
                    {
                        moves.Add(targetIndex);
                    }
                }
            }

            return moves;
        }

        private static List<int> GetMovesForBishop()
        {
            return GetSlidingMoves(Piece.Bishop);
        }

        private static List<int> GetMovesForRook()
        {
            return GetSlidingMoves(Piece.Rook);
        }

        private static List<int> GetMovesForQueen()
        {
            return GetSlidingMoves(Piece.Queen);
        }

        private static List<int> GetMovesForKing()
        {
            return GetSlidingMoves(Piece.King);
        }

        private static List<int> GetSlidingMoves(int piece)
        {
            int[] offsets = { 8, -8, 1, -1, 9, -9, 7, -7 };
            List<int> moves = new List<int>();

            int startingDirection = Piece.GetType(piece) == Piece.Bishop ? 4 : 0;
            int endingDirection = Piece.GetType(piece) == Piece.Rook ? 4 : 8;

            for (int direction = startingDirection; direction < endingDirection; direction++)
            {
                for (int i = 0; i < PrecomputedData.NumSquaresToEdge[_index][direction]; i++)
                {
                    int offset = offsets[direction] * (i + 1);
                    int targetIndex = _index + offset;

                    int targetPiece = _pieceValues[targetIndex];

                    if (targetPiece != 0 && Piece.GetColor(targetPiece) == Piece.GetColor(piece)) break;

                    moves.Add(targetIndex);

                    if (targetPiece != 0 && Piece.GetColor(targetPiece) != Piece.GetColor(piece)) break;

                    if (Piece.GetType(piece) == Piece.King) break;
                }
            }
            
            // Castling
            if (Piece.GetType(piece) == Piece.King)
            {
                if (_color == Piece.White)
                {
                    if (_fenString.WhiteCanCastleKingside && _pieceValues[5] == 0 && _pieceValues[6] == 0)
                    {
                        moves.Add(6);
                    }

                    if (_fenString.WhiteCanCastleQueenside && _pieceValues[1] == 0 && _pieceValues[2] == 0 && _pieceValues[3] == 0)
                    {
                        moves.Add(2);
                    }
                }
                else
                {
                    if (_fenString.BlackCanCastleKingside && _pieceValues[61] == 0 && _pieceValues[62] == 0)
                    {
                        moves.Add(62);
                    }

                    if (_fenString.BlackCanCastleQueenside && _pieceValues[57] == 0 && _pieceValues[58] == 0 && _pieceValues[59] == 0)
                    {
                        moves.Add(58);
                    }
                }
            }

            return moves;
        }
    }
}