using System.Collections.Generic;
using Code.Board;
using Code.Data;
using UnityEngine;

namespace Code.AI
{
    public class RandomAI : AI
    {
        protected override Move GetMoveInternal(int color)
        {
            List<Move> moves = new List<Move>();

            while (moves.Count == 0)
            {
                int pieceIndex = PickRandomPiece(color);
                moves = Rules.GetMovesForPiece(pieceIndex, Pieces, FenString);
            }

            Move move = moves[Random.Range(0, moves.Count)];
            return move;
        }
    }
}