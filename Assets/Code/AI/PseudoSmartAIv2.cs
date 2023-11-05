using System;
using System.Collections.Generic;
using Code.Board;
using Code.Data;
using UnityEngine;
using UnityEngine.Profiling;

namespace Code.AI
{
    public class PseudoSmartAIv2 : AI
    {
        private GameTreeNode _rootNode;
        private int searchDepth = 2;

        protected override Move GetMoveInternal(Piece[] pieces, FenString fenString, int color)
        {
    Profiler.BeginSample("GetMoveInternal");
            Piece[] p = Rules.CopyPieces(pieces);
            _rootNode = new GameTreeNode(p, null, fenString);

            Search(_rootNode, searchDepth, int.MinValue, int.MaxValue);

    Profiler.EndSample();
            return _rootNode.BestMove;
        }

        private void CreateChildNode(GameTreeNode parentNode, Move move)
        {
    Profiler.BeginSample("CreateChildNode");
            // Create a new node for the new state and link it to the parent node
            
            // TODO: Properly implement fen string
            FenString fenString = new FenString();
            fenString.SetWhiteToMove(!parentNode.FenString.WhiteToMove);
            
            Piece[] childPieces = Rules.CopyPieces(parentNode.Pieces);
            GameTreeNode childNode = new GameTreeNode(childPieces, move, fenString);
            
            parentNode.Children.Add(childNode);
    Profiler.EndSample();
        }

        private float Search(GameTreeNode node, int depth, float alpha, float beta)
        {
            if (depth == 0)
            {
                return EvaluateBoard(node);
            }
            
            int color = node.FenString.WhiteToMove ? Piece.White : Piece.Black;
            List<Move> allPossibleMoves = GetAllPossibleMoves(node, color);
            allPossibleMoves = SortMoves(allPossibleMoves);
            foreach (Move move in allPossibleMoves)
            {
                CreateChildNode(node, move);
            }

            if (allPossibleMoves.Count == 0)
            {
                int[] pieceValues = Rules.CopyPieceValues(node.Pieces);
                if (Rules.IsKingInCheck(pieceValues, node.FenString, color))
                {
                    return int.MinValue;
                }
                return 0;
            }
        
            foreach (GameTreeNode n in node.Children)
            {
                string moveNotation = n.NodeOriginMove.Notation;
                float evaluation = -Search(n, depth - 1, -beta, -alpha);
                
                if (evaluation >= beta)
                {
                    return beta;
                }
                if (evaluation > alpha)
                {
                    alpha = evaluation;
                    node.BestMove = n.NodeOriginMove;
                }
            }
            return alpha;
        }

        /// <summary>
        /// Evaluate the board and return a value for the board using the following:
        ///
        ///  f(p) = 200(K-K')
        ///        + 9(Q-Q')
        ///        + 5(R-R')
        ///        + 3(B-B' + N-N')
        ///        + 1(P-P')
        ///        - 0.5(D-D' + S-S' + I-I')
        ///        + 0.1(M-M') + ...
        ///
        ///  KQRBNP = number of kings, queens, rooks, bishops, knights and pawns
        ///  D,S,I = doubled, blocked and isolated pawns
        ///  M = Mobility (the number of legal moves)
        /// 
        /// </summary>
        private float EvaluateBoard(GameTreeNode node)
        {
            Piece[] pieces = node.Pieces;
            int color = Piece.GetColor(node.FenString.WhiteToMove ? Piece.White : Piece.Black);
            int enemyColor = color == Piece.White ? Piece.Black : Piece.White;

            float valueOfPieces = 0;
            
    Profiler.BeginSample("EvaluateBoard.EvaluatePieceCount");
            int[] pieceCounts = GetNumberOfPieces(pieces, color);
            int[] enemyPieceCounts = GetNumberOfPieces(pieces, enemyColor);
            valueOfPieces += Piece.GetPieceValue(Piece.King) * (pieceCounts[5] - enemyPieceCounts[5]);
            valueOfPieces += Piece.GetPieceValue(Piece.Queen) * (pieceCounts[4] - enemyPieceCounts[4]);
            valueOfPieces += Piece.GetPieceValue(Piece.Rook) * (pieceCounts[3] - enemyPieceCounts[3]);
            valueOfPieces += Piece.GetPieceValue(Piece.Bishop) * (pieceCounts[2] - enemyPieceCounts[2])
                                                                + (pieceCounts[1] - enemyPieceCounts[1]);
            valueOfPieces += Piece.GetPieceValue(Piece.Pawn) * (pieceCounts[0] - enemyPieceCounts[0]);
    Profiler.EndSample();

            float valueOfPawns = 0;
            // TODO: Implement doubled, blocked and isolated pawns

    Profiler.BeginSample("EvaluateBoard.EvaluateMobility");
            float valueOfMobility = 0;
            List<Move> ownMoves = new List<Move>();
            List<Move> enemyMoves = new List<Move>();
            if (color == Piece.White)
                GetAllPossibleMoves(node, ref ownMoves, ref enemyMoves);
            else
                GetAllPossibleMoves(node, ref enemyMoves, ref ownMoves);
            
            valueOfMobility += 0.1f * ownMoves.Count - enemyMoves.Count;
    Profiler.EndSample();
            return valueOfPieces - valueOfPawns + valueOfMobility;
        }

        // Sort moves by the following criteria:
        // 1. Capture valuable pieces with less valuable pieces
        // 2. Promoting pawns
        // 3. Avoid moving piece to a square where it can be captured
        private List<Move> SortMoves(List<Move> moves)
        {
    Profiler.BeginSample("SortMoves");
            foreach (Move move in moves)
            {
                int moveScore = 0;
                int movedPieceType = Piece.GetType(move.Type);
                int capturedPieceType = Piece.GetType(move.CapturedType);
                
                // 1. Capture valuable pieces with less valuable pieces
                if (capturedPieceType != Piece.None)
                {
                    moveScore = 10 * Piece.GetPieceValue(capturedPieceType) - Piece.GetPieceValue(movedPieceType);
                }
                
                // 2. Promoting pawns
                if (movedPieceType == Piece.Pawn && (move.To < 8 || move.To > 55))
                {
                    moveScore += 10 * Piece.GetPieceValue(Piece.Queen);
                }
                
                // 3. Avoid moving piece to a square where it can be captured
                // TODO: Implement
                
                move.Score = moveScore;
            }
            
            moves.Sort((move1, move2) => move2.Score.CompareTo(move1.Score));
            
    Profiler.EndSample();
            return moves;
        }
    }
}