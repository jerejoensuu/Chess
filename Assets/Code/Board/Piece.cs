using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Board
{
    public class Piece
    {
        // The piece is notated with a binary string where the first 2 bits are the color and the next 3 bits are the type

        public const int None = 0;
        public const int Pawn = 1;
        public const int Knight = 2;
        public const int Bishop = 3;
        public const int Rook = 4;
        public const int Queen = 5;
        public const int King = 6;
        
        public const int White = 8;
        public const int Black = 16;

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                try
                {
                    OnValueChanged?.Invoke(value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        private int _value;

        public int Type => GetType(Value);
        public int Color => GetColor(Value);
        
        public UnityEvent<int> OnValueChanged = new UnityEvent<int>();


        public static int GetColor(int piece)
        {
            return piece & 24;
        }
        
        public static int GetType(int piece)
        {
            return piece & 7;
        }
        
        public static int GetValueForFenChar(char c)
        {
            return c switch
            {
                'p' => Black | Pawn,
                'n' => Black | Knight,
                'b' => Black | Bishop,
                'r' => Black | Rook,
                'q' => Black | Queen,
                'k' => Black | King,
                'P' => White | Pawn,
                'N' => White | Knight,
                'B' => White | Bishop,
                'R' => White | Rook,
                'Q' => White | Queen,
                'K' => White | King,
                _ => None
            };
        }
        
        public static bool IsWhite(int piece)
        {
            return GetColor(piece) == White;
        }
        
        public static string GetPieceNotation(int piece)
        {
            string notation = "";
            switch (GetType(piece))
            {
                case Pawn:
                    notation += "P";
                    break;
                case Knight:
                    notation += "N";
                    break;
                case Bishop:
                    notation += "B";
                    break;
                case Rook:
                    notation += "R";
                    break;
                case Queen:
                    notation += "Q";
                    break;
                case King:
                    notation += "K";
                    break;
            }

            return IsWhite(piece) ? notation : notation.ToLower();
        }
    }
}