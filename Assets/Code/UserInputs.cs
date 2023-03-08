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
            _selectedSquare = boardManager.GetSquareUnderCursor();
            if (_selectedSquare == null) return;
            _selectedSquare.gameObject.transform.GetChild(0).parent = boardManager.cursor.transform;
        }
        
        private void OnMouseUp()
        {
            if (_selectedSquare == null) return;
            _targetSquare = boardManager.GetSquareUnderCursor();

            if (_targetSquare == null || _targetSquare == _selectedSquare)
                ResetCursor();
            
            boardManager.cursor.transform.GetChild(0).parent = _targetSquare.gameObject.transform;
        }
        
        private void ResetCursor()
        {
            if (boardManager.cursor.transform.childCount == 0) return;
            boardManager.cursor.transform.GetChild(0).parent = _selectedSquare.gameObject.transform;
            
            _selectedSquare = null;
            _targetSquare = null;
        }
    }
}