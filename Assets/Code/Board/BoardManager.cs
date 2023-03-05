using Code.UI;
using UnityEngine;

namespace Code.Board
{
    public class BoardManager : MonoBehaviour
    {
        public BoardBuilder boardBuilder;
        public UIManager uiManager;
        [SerializeField] private GameObject piecePrefab;
        [SerializeField] private Sprite[] pieceSprites;
        
        private Camera _mainCamera;
        
        [SerializeField] private float squareSize = 1f;

        private Square[] _squares;

        [HideInInspector] public Vector2 cursorPosition;

        private void Start()
        {
            _mainCamera = Camera.main;
            _squares = boardBuilder.BuildBoard();
            PlacePieces();
        }

        private void Update()
        {
            CalculateCursorPosition();
        }

        private void CalculateCursorPosition()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10;
            cursorPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
            
            Square square = GetSquareAtPosition(cursorPosition);
            if (square == null) return;
            uiManager.SetNotationText(square.GetNotation());
        }

        private Square GetSquareAtPosition(Vector2 position)
        {
            int file = (int)(position.x / squareSize + 4);
            int rank = (int)(position.y / squareSize + 4);

            int index = rank * 8 + file;
            
            if (index < 0 || index > 63)
            {
                return null;
            }
            
            return _squares[index];
        }
        
        private void PlacePieces()
        {
            foreach (Square square in _squares)
            {
                if (square.pieceValue == 0) continue;
                
                GameObject piece = Instantiate(piecePrefab, square.transform);
                
                SpriteRenderer spriteRenderer = piece.GetComponentInChildren<SpriteRenderer>();
                int spriteIndex = Piece.GetColor(square.pieceValue) == 16 ? 6 : 0;
                spriteIndex += Piece.GetType(square.pieceValue) - 1;
                
                spriteRenderer.sprite = pieceSprites[spriteIndex];
            }
        }
    }
}