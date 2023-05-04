using System;
using UnityEngine;

namespace Code.Data
{
    public static class PrecomputedData
    {
        public static readonly int[] DirectionOffsets = { 1, -1, 8, -8, 9, -9, 7, -7 };
        public static readonly int[][] NumSquaresToEdge;

        static PrecomputedData()
        {
            NumSquaresToEdge = new int[64][];
            
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    int numForward = 7 - rank;
                    int numBackward = rank;
                    int numRight = 7 - file;
                    int numLeft = file;

                    int index = rank * 8 + file;
                    
                    NumSquaresToEdge[index] = new[]
                    {
                        numForward,
                        numBackward,
                        numRight,
                        numLeft,
                        Math.Min(numForward, numRight),
                        Math.Min(numBackward, numLeft),
                        Math.Min(numForward, numLeft),
                        Math.Min(numBackward, numRight)
                    };
                }
            }
        }
    }
}