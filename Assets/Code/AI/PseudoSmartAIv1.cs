using System.Collections.Generic;
using System.Linq;
using Code.Board;
using UnityEngine;

namespace Code.AI
{
    public class PseudoSmartAIv1 : AI
    {
        private List<int> _moveValues = new();

        protected override Move GetMoveInternal(int color)
        {
            _moveValues.Clear();
            List<Move> allPossibleMoves = GetAllPossibleMoves(color);
            EvaluateMoves(allPossibleMoves);

            Move bestMove = GetBestMove(allPossibleMoves);
            return bestMove;
        }
        
        private void EvaluateMoves(List<Move> moves)
        {
            foreach (Move move in moves)
            {
                int value = 0;
                if (move.CapturedType != Piece.None)
                {
                    value += move.CapturedType;
                }

                // if (move.promotion)
                // {
                //     value += Piece.GetValue(move.promotionPiece);
                // }

                _moveValues.Add(value);
            }
        }
        
        private Move GetBestMove(List<Move> moves)
        {
            List<Move> bestMoves = new List<Move>();
            int bestValue = _moveValues.Max();
            for (int i = 0; i < _moveValues.Count; i++)
            {
                if (_moveValues[i] == bestValue)
                {
                    bestMoves.Add(moves[i]);
                }
            }

            return bestMoves[Random.Range(0, bestMoves.Count)];
        }
    }
}