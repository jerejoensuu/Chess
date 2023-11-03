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
            if (_selectedSquare == null || boardManager.pieces[_selectedSquare.index].Value == 0) return;
            boardManager.PickUpPiece(_selectedSquare.GetPiece());
            boardManager.SetSquareMarks(Rules.GetMovesForPiece(_selectedSquare.index, boardManager.pieces, boardManager.gameManager.FenString));
            boardManager.ColorSquare(_selectedSquare.index, boardManager.pieceOriginColor);
        }

        private void OnMouseUp()
        {
            boardManager.CalculateCursorPosition();
            if (_selectedSquare == null) return;
            _targetSquare = boardManager.GetSquareUnderCursor();

            if (_targetSquare == null || _targetSquare == _selectedSquare ||
                !boardManager.MovePieceTo(_selectedSquare.index, _targetSquare.index))
            {
                ResetCursor();
            }

            boardManager.ResetSquareColors();
            boardManager.ResetSquareMarks();
        }

        private void ResetCursor()
        {
            if (boardManager.cursor.transform.childCount == 0) return;
            boardManager.ResetPieceToOrigin(_selectedSquare.index);
            boardManager.ResetSquareColors();

            _selectedSquare = null;
            _targetSquare = null;
        }
    }
}