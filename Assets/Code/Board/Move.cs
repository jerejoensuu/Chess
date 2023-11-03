using UnityEngine;

namespace Code.Board
{
    public class Move
    {
        public int Type;
        public int From;
        public int To;
        public int CapturedType;
        public int Turn;

        public readonly string Notation;

        public Move(int from, int to, int type, int capturedType)
        {
            Type = type;
            From = from;
            To = to;
            CapturedType = capturedType;
            
            Notation = GetNotation();
        }
        
        private string GetNotation()
        {
            string from = Square.GetNotationForIndex(From);
            string to = Square.GetNotationForIndex(To);
            string captured = CapturedType == Piece.None ? "" : "x";
            string piece = Piece.GetPieceNotation(Type);
            string promotion = "";
            // if (Rules.IsPawn(Squares[From]) && (To < 8 || To > 55))
            // {
            //     promotion = "=Q";
            // }
        
            return $"{piece}{from}{captured}{to}{promotion}";
        }
    }
}