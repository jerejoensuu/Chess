using System.Collections.Generic;
using Code.Data;

namespace Code.Board
{
    public static class Rules
    {
        public static bool IsMoveLegal(Square selectedSquare, Square targetSquare, Square[] squares,
            FenString fenString)
        {
            List<int> moves = GetMovesForPiece(selectedSquare, squares, fenString);
            return moves.Contains(targetSquare.index);
        }

        public static List<int> GetMovesForPiece(Square square, Square[] squares, FenString fenString)
        {
            int[] pieceValues = CopyPieceValuesFromSquares(squares);

            List<int> moves = new List<int>();
            if (Piece.GetColor(square.pieceValue) != (fenString.WhiteToMove ? Piece.White : Piece.Black)) return moves;
            
            moves = GetMovesForPiece(square.pieceValue, square.index, pieceValues, fenString);
            moves = FilterMovesForCheck(square.pieceValue, square.index, moves, pieceValues, fenString);
            return moves;
        }
        
        private static List<int> FilterMovesForCheck(int piece, int index, List<int> moves, int[] pieceValues,
            FenString fenString)
        {
            List<int> filteredMoves = new List<int>();
            int color = Piece.GetColor(piece);

            foreach (int move in moves)
            {
                int oldPiece = pieceValues[move];
                pieceValues[move] = piece; // Move the piece to the new position
                pieceValues[index] = 0; // Remove the piece from its original position
                if (!IsKingInCheck(pieceValues, fenString, color)) // Check if the king is in check after the move
                {
                    filteredMoves.Add(move); // Add the move to the list of legal moves
                }

                pieceValues[index] = piece; // Move the piece back to its original position
                pieceValues[move] = oldPiece; // Restore the old piece
            }

            return filteredMoves;
        }

        public static bool IsKingInCheck(int[] pieceValues, FenString fenString, int color)
        {
            int kingIndex = GetKingIndex(pieceValues, color);
            int opponentColor = color == Piece.White ? Piece.Black : Piece.White;
            List<int> opponentMoves = GetMovesForColor(pieceValues, fenString, opponentColor);
            return opponentMoves.Contains(kingIndex);
        }
        
        public static bool IsKingInMate(int[] pieceValues, FenString fenString, int color)
        {
            List<int> moves = GetMovesForColor(pieceValues, fenString, color, true);
            return moves.Count == 0 && IsKingInCheck(pieceValues, fenString, color);
        }
        
        private static List<int> GetMovesForColor(int[] pieceValues, FenString fenString, int color, bool filterForCheck = false)
        {
            List<int> moves = new List<int>();

            for (int i = 0; i < 64; i++)
            {
                if ((pieceValues[i] & color) == color)
                {
                    List<int> m = GetMovesForPiece(pieceValues[i], i, pieceValues, fenString);
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


        private static List<int> GetMovesForPiece(int piece, int index, int[] pieceValues, FenString fenString)
        {
            List<int> moves = Piece.GetType(piece) switch
            {
                Piece.Pawn => GetMovesForPawn(piece, index, pieceValues, fenString),
                Piece.Knight => GetMovesForKnight(piece, index, pieceValues),
                _ => GetSlidingMoves(piece, index, pieceValues, fenString)
            };

            return moves;
        }

        private static List<int> GetMovesForPawn(int piece, int index, int[] pieceValues, FenString fenString)
        {
            List<int> moves = new List<int>();
            int color = Piece.GetColor(piece);

            int currentRow = index / 8;
            int currentCol = index % 8;

            int direction = color == Piece.White ? 1 : -1;

            // Check if the pawn can move forward one square
            int targetIndex = (currentRow + direction) * 8 + currentCol;
            if (targetIndex is >= 0 and < 64 && pieceValues[targetIndex] == 0)
            {
                moves.Add(targetIndex);
            }

            // Check if the pawn can move forward two squares from its starting position
            if ((currentRow == 1 && color == Piece.White) || (currentRow == 6 && color == Piece.Black))
            {
                targetIndex = (currentRow + 2 * direction) * 8 + currentCol;
                if (pieceValues[targetIndex] == 0 &&
                    pieceValues[(currentRow + direction) * 8 + currentCol] == 0)
                {
                    moves.Add(targetIndex);
                }
            }

            // Check if the pawn can capture diagonally to the left
            targetIndex = (currentRow + direction) * 8 + currentCol - 1;
            if (targetIndex is >= 0 and < 64
                && currentCol > 0 && pieceValues[targetIndex] != 0
                && Piece.GetColor(pieceValues[targetIndex]) != color)
            {
                moves.Add(targetIndex);
            }

            // Check if the pawn can capture diagonally to the right
            targetIndex = (currentRow + direction) * 8 + currentCol + 1;
            if (targetIndex is >= 0 and < 64
                && currentCol < 7 && pieceValues[targetIndex] != 0
                && Piece.GetColor(pieceValues[targetIndex]) != color)
            {
                moves.Add(targetIndex);
            }

            // Check for en passant capture to the left
            targetIndex = (currentRow + direction) * 8 + currentCol - 1;
            if (targetIndex == fenString.EnPassantIndex)
            {
                moves.Add(targetIndex);
            }

            // Check for en passant capture to the right
            targetIndex = (currentRow + direction) * 8 + currentCol + 1;
            if (targetIndex == fenString.EnPassantIndex)
            {
                moves.Add(targetIndex);
            }

            return moves;
        }


        private static List<int> GetMovesForKnight(int piece, int index, int[] pieceValues)
        {
            int[] rowOffsets = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] colOffsets = { 1, 2, 2, 1, -1, -2, -2, -1 };
            List<int> moves = new List<int>();
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
                        moves.Add(targetIndex);
                    }
                }
            }

            return moves;
        }

        private static List<int> GetSlidingMoves(int piece, int index, int[] pieceValues, FenString fenString)
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

                    int targetPiece = pieceValues[targetIndex];

                    if (targetPiece != 0 && Piece.GetColor(targetPiece) == Piece.GetColor(piece)) break;

                    moves.Add(targetIndex);

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
                        moves.Add(6);
                    }

                    if (fenString.WhiteCanCastleQueenside && pieceValues[1] == 0 && pieceValues[2] == 0 &&
                        pieceValues[3] == 0)
                    {
                        moves.Add(2);
                    }
                }
                else
                {
                    if (fenString.BlackCanCastleKingside && pieceValues[61] == 0 && pieceValues[62] == 0)
                    {
                        moves.Add(62);
                    }

                    if (fenString.BlackCanCastleQueenside && pieceValues[57] == 0 && pieceValues[58] == 0 &&
                        pieceValues[59] == 0)
                    {
                        moves.Add(58);
                    }
                }
            }

            return moves;
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
    }
}