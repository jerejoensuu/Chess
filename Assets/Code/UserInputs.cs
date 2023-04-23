using System;
using Code.Board;
using UnityEngine;

namespace Code
{
    public class UserInputs : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;

        private Square _selectedSquare;
        private Square _targetSquare;


        private void OnMouseDown()
        {
            boardManager.CalculateCursorPosition();
            _selectedSquare = boardManager.GetSquareUnderCursor();
            if (_selectedSquare == null) return;
            boardManager.PickUpPiece(_selectedSquare.transform.GetChild(0).gameObject);
            boardManager.ColorSquares(Rules.GetMovesForPiece(_selectedSquare, boardManager.squares), boardManager.legalMoveColor);
        }

        private void OnMouseUp()
        {
            boardManager.CalculateCursorPosition();
            if (_selectedSquare == null) return;
            _targetSquare = boardManager.GetSquareUnderCursor();

            if (_targetSquare == null || _targetSquare == _selectedSquare ||
                !boardManager.MovePieceTo(_selectedSquare, _targetSquare))
            {
                ResetCursor();
                return;
            }

            boardManager.ResetSquareColors();
        }

        private void ResetCursor()
        {
            if (boardManager.cursor.transform.childCount == 0) return;
            boardManager.ResetPieceToOrigin(_selectedSquare);
            boardManager.ResetSquareColors();

            _selectedSquare = null;
            _targetSquare = null;
        }
    }
}