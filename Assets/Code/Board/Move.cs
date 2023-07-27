using UnityEngine;

namespace Code.Board
{
    public class Move
    {
        public int From;
        public int To;
        public int CapturedType;
        public int Turn;

        public Move(int from, int to, int capturedType)
        {
            From = from;
            To = to;
            CapturedType = capturedType;
        }
    }
}