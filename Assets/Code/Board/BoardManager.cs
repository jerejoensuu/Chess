using System;
using Code.UI;
using UnityEngine;

namespace Code.Board
{
    public class BoardManager : MonoBehaviour
    {
        public UserInputs userInputs;
        public BoardBuilder boardBuilder;
        public UIManager uiManager;
        [SerializeField] private GameObject piecePrefab;
        [SerializeField] private Sprite[] pieceSprites;
        
        private Camera _mainCamera;
        
        [SerializeField] private float squareSize = 1f;

        private Square[] _squares;

        public GameObject cursor;

        private void Start()
        {
            _mainCamera = Camera.main;
            
            // cursor = new GameObject("Cursor");
            cursor.transform.parent = transform;
        }
        
        public void SetupBoard(string fenString = "default")
        {
            _squares ??= boardBuilder.BuildBoard(fenString, squareSize);
            try
            {
                boardBuilder.SetPiecesFromFenString(fenString, ref _squares);
            }
            catch (Exception)
            {
                Debug.LogError("Invalid FEN string:");
                boardBuilder.SetPiecesFromFenString("default", ref _squares);
            }
            InstantiatePieces();
        }

        private void Update()
        {
            CalculateCursorPosition();
        }

        public void CalculateCursorPosition()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10;
            cursor.transform.position = _mainCamera.ScreenToWorldPoint(mousePosition);
            
            Square square = GetSquareAtPosition(cursor.transform.position);
            if (square == null) return;
            uiManager.SetNotationText(square.GetNotation());
        }
        
        public Square GetSquareUnderCursor()
        {
            return GetSquareAtPosition(cursor.transform.position);
        }

        private Square GetSquareAtPosition(Vector2 position)
        {
            if (position.x < -4 * squareSize || position.x > 4 * squareSize) return null;
            if (position.y < -4 * squareSize || position.y > 4 * squareSize) return null;
            
            int file = (int)(position.x / squareSize + 4);
            int rank = (int)(position.y / squareSize + 4);

            int index = rank * 8 + file;
            
            return _squares[index];
        }
        
        private void InstantiatePieces()
        {
            foreach (Square square in _squares)
            {
                if (square.transform.childCount > 0)
                {
                    Destroy(square.transform.GetChild(0).gameObject);
                }
                if (square.pieceValue == 0) continue;
                
                GameObject piece = Instantiate(piecePrefab, square.transform);
                
                SpriteRenderer spriteRenderer = piece.GetComponentInChildren<SpriteRenderer>();
                int spriteIndex = Piece.GetColor(square.pieceValue) == 16 ? 6 : 0;
                spriteIndex += Piece.GetType(square.pieceValue) - 1;
                
                spriteRenderer.sprite = pieceSprites[spriteIndex];
                piece.name = pieceSprites[spriteIndex].name;
            }
        }
        
        public void MovePieceToSquare(GameObject origin, GameObject target)
        {
            Transform piece = origin.gameObject.transform.GetChild(0);
            piece.parent = target.transform;
            piece.localPosition = Vector3.zero;
        }
    }
}