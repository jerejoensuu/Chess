using System;
using System.Collections.Generic;
using Code.Data;
using UnityEngine;

namespace Code.Board
{
    public static class Rules
    {
        public static bool IsMoveLegal(int originIndex, int targetIndex, Piece[] pieces,
            FenString fenString)
        {
            List<Move> moves = GetMovesForPiece(originIndex, pieces, fenString);
            return moves.Exists(move => move.To == targetIndex);
        }

        public static List<Move> GetMovesForPiece(int index, Piece[] pieces, FenString fenString)
        {
            int[] pieceValues = CopyPieceValues(pieces);

            List<Move> moves = new List<Move>();
            if (Piece.GetColor(pieces[index].Value) != (fenString.WhiteToMove ? Piece.White : Piece.Black)) return moves;

            moves = GetMovesForPiece(pieces[index].Value, index, pieceValues, fenString);
            moves = FilterMovesForCheck(pieces[index].Value, index, moves, pieceValues, fenString);
            return moves;
        }

        private static List<Move> FilterMovesForCheck(int piece, int index, List<Move> moves, int[] pieceValues,
            FenString fenString)
        {
            List<Move> filteredMoves = new List<Move>();
            int color = Piece.GetColor(piece);

            try
            {
                foreach (Move move in moves)
                {
                    int oldPiece = pieceValues[move.To];
                    pieceValues[move.To] = piece; // Move the piece to the new position
                    pieceValues[index] = 0; // Remove the piece from its original position
                    if (!IsKingInCheck(pieceValues, fenString, color)) // Check if the king is in check after the move
                    {
                        filteredMoves.Add(move); // Add the move to the list of legal moves
                    }

                    pieceValues[index] = piece; // Move the piece back to its original position
                    pieceValues[move.To] = oldPiece; // Restore the old piece
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            return filteredMoves;
        }

        public static bool IsKingInCheck(int[] pieceValues, FenString fenString, int color)
        {
            int kingIndex = GetKingIndex(pieceValues, color);
            int opponentColor = color == Piece.White ? Piece.Black : Piece.White;
            List<Move> opponentMoves = GetMovesForColor(pieceValues, fenString, opponentColor);
            return opponentMoves.Exists(move => move.To == kingIndex);
        }

        public static bool IsKingInMate(int[] pieceValues, FenString fenString, int color)
        {
            List<Move> moves = GetMovesForColor(pieceValues, fenString, color, true);
            return moves.Count == 0 && IsKingInCheck(pieceValues, fenString, color);
        }

        public static List<Move> GetMovesForColor(int[] pieceValues, FenString fenString, int color,
            bool filterForCheck = false)
        {
            List<Move> moves = new List<Move>();

            for (int i = 0; i < 64; i++)
            {
                if ((pieceValues[i] & color) == color)
                {
                    List<Move> m = GetMovesForPiece(pieceValues[i], i, pieceValues, fenString);
                    if (filterForCheck) m = FilterMovesForCheck(pieceValues[i], i, m, pieceValues, fenString);
                    moves.AddRange(m);
                }
            }

            return moves;
        }

        private static int GetKingIndex(int[] pieceValues, int color)
        {
            int colorMask = color | Piece.King;

            for (int i = 0; i < 64; i++)
            {
                if ((pieceValues[i] & colorMask) == colorMask)
                {
                    return i;
                }
            }

            return -1;
        }


        private static List<Move> GetMovesForPiece(int piece, int index, int[] pieceValues, FenString fenString)
        {
            List<Move> moves = Piece.GetType(piece) switch
            {
                Piece.Pawn => GetMovesForPawn(piece, index, pieceValues, fenString),
                Piece.Knight => GetMovesForKnight(piece, index, pieceValues),
                _ => GetSlidingMoves(piece, index, pieceValues, fenString)
            };

            return moves;
        }

        private static List<Move> GetMovesForPawn(int piece, int index, int[] pieceValues, FenString fenString)
        {
            List<Move> moves = new List<Move>();
            int color = Piece.GetColor(piece);

            int currentRow = index / 8;
            int currentCol = index % 8;

            int direction = color == Piece.White ? 1 : -1;

            // Check if the pawn can move forward one square
            int targetIndex = (currentRow + direction) * 8 + currentCol;
            if (targetIndex is >= 0 and < 64 && pieceValues[targetIndex] == 0)
            {
                moves.Add(new Move(index, targetIndex, Piece.Pawn & color, 0));
            }

            // Check if the pawn can move forward two squares from its starting position
            if ((currentRow == 1 && color == Piece.White) || (currentRow == 6 && color == Piece.Black))
            {
                targetIndex = (currentRow + 2 * direction) * 8 + currentCol;
                if (pieceValues[targetIndex] == 0 &&
                    pieceValues[(currentRow + direction) * 8 + currentCol] == 0)
                {
                    moves.Add(new Move(index, targetIndex, Piece.Pawn & color, 0));
                    if (targetIndex == -1) Debug.Log("");
                }
            }

            // Check if the pawn can capture diagonally to the left
            targetIndex = (currentRow + direction) * 8 + currentCol - 1;
            if (targetIndex is >= 0 and < 64
                && currentCol > 0 && pieceValues[targetIndex] != 0
                && Piece.GetColor(pieceValues[targetIndex]) != color)
            {
                moves.Add(new Move(index, targetIndex, Piece.Pawn & color, Piece.GetType(pieceValues[targetIndex])));
            }

            // Check if the pawn can capture diagonally to the right
            targetIndex = (currentRow + direction) * 8 + currentCol + 1;
            if (targetIndex is >= 0 and < 64
                && currentCol < 7 && pieceValues[targetIndex] != 0
                && Piece.GetColor(pieceValues[targetIndex]) != color)
            {
                moves.Add(new Move(index, targetIndex, Piece.Pawn & color, Piece.GetType(pieceValues[targetIndex])));
            }

            // Check for en passant capture to the left
            targetIndex = (currentRow + direction) * 8 + currentCol - 1;
            if (targetIndex == fenString.EnPassantIndex && targetIndex > 0)
            {
                moves.Add(new Move(index, targetIndex, Piece.Pawn & color, Piece.GetType(pieceValues[targetIndex])));
                if (targetIndex == -1) Debug.Log("");
            }

            // Check for en passant capture to the right
            targetIndex = (currentRow + direction) * 8 + currentCol + 1;
            if (targetIndex == fenString.EnPassantIndex && targetIndex > 0)
            {
                moves.Add(new Move(index, targetIndex, Piece.Pawn & color, Piece.GetType(pieceValues[targetIndex])));
                if (targetIndex == -1) Debug.Log("");
            }

            return moves;
        }


        private static List<Move> GetMovesForKnight(int piece, int index, int[] pieceValues)
        {
            int[] rowOffsets = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] colOffsets = { 1, 2, 2, 1, -1, -2, -2, -1 };
            List<Move> moves = new List<Move>();
            int color = Piece.GetColor(piece);

            int currentRow = index / 8;
            int currentCol = index % 8;

            for (int i = 0; i < 8; i++)
            {
                int newRow = currentRow + rowOffsets[i];
                int newCol = currentCol + colOffsets[i];

                if (newRow is >= 0 and < 8 && newCol is >= 0 and < 8)
                {
                    int targetIndex = newRow * 8 + newCol;
                    int targetPiece = pieceValues[targetIndex];

                    if (targetPiece == 0 || Piece.GetColor(targetPiece) != color)
                    {
                        moves.Add(new Move(index, targetIndex, Piece.Knight & color, Piece.GetType(pieceValues[targetIndex])));
                    }
                }
            }

            return moves;
        }

        private static List<Move> GetSlidingMoves(int piece, int index, int[] pieceValues, FenString fenString)
        {
            int[] offsets = { 8, -8, 1, -1, 9, -9, 7, -7 };
            List<Move> moves = new List<Move>();

            int startingDirection = Piece.GetType(piece) == Piece.Bishop ? 4 : 0;
            int endingDirection = Piece.GetType(piece) == Piece.Rook ? 4 : 8;

            for (int direction = startingDirection; direction < endingDirection; direction++)
            {
                for (int i = 0; i < PrecomputedData.NumSquaresToEdge[index][direction]; i++)
                {
                    int offset = offsets[direction] * (i + 1);
                    int targetIndex = index + offset;

                    int targetPiece = pieceValues[targetIndex];

                    if (targetPiece != 0 && Piece.GetColor(targetPiece) == Piece.GetColor(piece)) break;

                    moves.Add(new Move(index, targetIndex, piece, Piece.GetType(pieceValues[targetIndex])));

                    if (targetPiece != 0 && Piece.GetColor(targetPiece) != Piece.GetColor(piece)) break;

                    if (Piece.GetType(piece) == Piece.King) break;
                }
            }

            // Castling
            if (Piece.GetType(piece) == Piece.King)
            {
                if (Piece.GetColor(piece) == Piece.White)
                {
                    if (fenString.WhiteCanCastleKingside && pieceValues[5] == 0 && pieceValues[6] == 0)
                    {
                        moves.Add(new Move(index, 6, piece, 0));
                    }

                    if (fenString.WhiteCanCastleQueenside && pieceValues[1] == 0 && pieceValues[2] == 0 &&
                        pieceValues[3] == 0)
                    {
                        moves.Add(new Move(index, 2, piece, 0));
                    }
                }
                else
                {
                    if (fenString.BlackCanCastleKingside && pieceValues[61] == 0 && pieceValues[62] == 0)
                    {
                        moves.Add(new Move(index, 62, piece, 0));
                    }

                    if (fenString.BlackCanCastleQueenside && pieceValues[57] == 0 && pieceValues[58] == 0 &&
                        pieceValues[59] == 0)
                    {
                        moves.Add(new Move(index, 58, piece, 0));
                    }
                }
            }

            return moves;
        }

        public static int[] CopyPieceValues(Piece[] pieces)
        {
            int[] pieceValues = new int[pieces.Length];
            for (int i = 0; i < pieces.Length; i++)
            {
                pieceValues[i] = pieces[i].Value;
            }

            return pieceValues;
        }
    }
}