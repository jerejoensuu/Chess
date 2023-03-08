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
            boardManager.MovePieceToSquare(_selectedSquare.gameObject, boardManager.cursor);
        }
        
        private void OnMouseUp()
        {
            boardManager.CalculateCursorPosition();
            if (_selectedSquare == null) return;
            _targetSquare = boardManager.GetSquareUnderCursor();

            if (_targetSquare == null || _targetSquare == _selectedSquare)
            {
                ResetCursor();
                return;
            }
            
            boardManager.MovePieceToSquare(boardManager.cursor, _targetSquare.gameObject);
        }
        
        private void ResetCursor()
        {
            if (boardManager.cursor.transform.childCount == 0) return;
            boardManager.MovePieceToSquare(boardManager.cursor, _selectedSquare.gameObject);
            
            _selectedSquare = null;
            _targetSquare = null;
        }
    }
}