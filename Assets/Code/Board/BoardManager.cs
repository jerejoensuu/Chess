using System;
using System.Collections;
using System.Collections.Generic;
using Code.AI;
using Code.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Board
{
    public class BoardManager : MonoBehaviour
    {
        [InspectorLabel("References")] public GameManager gameManager;
        public UserInputs userInputs;
        public BoardBuilder boardBuilder;
        public UIManager uiManager;
        public GameSettingsInspector gameSettingsInspector;
        [SerializeField] private GameObject piecePrefab;
        [SerializeField] private Sprite[] pieceSprites;
        private Camera _mainCamera;

        [InspectorLabel("Colors")] public Color lightColor;
        public Color darkColor;
        public Color pieceOriginColor;
        public Color moveIndicatorColor;
        public Color legalMoveColor;

        [SerializeField] private float squareSize = 1f;

        public Square[] squares;

        public GameObject cursor;
        public GameObject heldPiece;

        private Square _highlightedSquare;

        // AI
        private bool _aiIsThinking;
        [NonSerialized] public bool AutoPlay = true;
        [NonSerialized] public bool Paused = false;


        private void Start()
        {
            _mainCamera = Camera.main;
            cursor = new GameObject("Cursor");
        }

        public void SetupBoard(string fenString = "default")
        {
            squares = boardBuilder.BuildBoard(lightColor, darkColor, fenString, squareSize);
            try
            {
                boardBuilder.SetPiecesFromFenString(fenString, ref squares);
            }
            catch (Exception)
            {
                Debug.LogError("Invalid FEN string:");
                boardBuilder.SetPiecesFromFenString("default", ref squares);
            }

            InstantiatePieces();
        }

        private void Update()
        {
            CalculateCursorPosition();

            if (!Paused)
            {
                if (!gameManager.FenString.WhiteToMove) StartCoroutine(MakeAIMove(Piece.Black));
                // else StartCoroutine(MakeAIMove(Piece.White));
                
                if (!AutoPlay) Paused = true;
            }

            if (gameManager.FenString.HalfmoveClock >= 100)
            {
                Debug.Log("Draw by 50 move rule");
                // stop unity editor
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }

        private IEnumerator MakeAIMove(int color)
        {
            if (_aiIsThinking) yield break;
            _aiIsThinking = true;

            Move move = gameManager.ai.GetMove(squares, gameManager.FenString, color);
            Square originSquare = squares[move.From];
            Square targetSquare = squares[move.To];
            Debug.Log(color == Piece.White
                ? "White"
                : "Black" + " is moving " + originSquare.GetNotation() + " to " + targetSquare.GetNotation());

            PickUpPiece(originSquare.GetPiece());
            MovePieceTo(originSquare, targetSquare);
            gameManager.FenString.SetWhiteToMove(color == Piece.Black);

            yield return new WaitForSeconds(0.01f);
            _aiIsThinking = false;
        }

        public void CalculateCursorPosition()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10;
            cursor.transform.position = _mainCamera.ScreenToWorldPoint(mousePosition);
            cursor.transform.Translate(Vector3.back);

            Square square = GetSquareAtPosition(cursor.transform.position);
            if (square == null) return;

            if (heldPiece != null)
                HighlightSquare(square);

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

            return squares[index];
        }

        private void InstantiatePieces()
        {
            foreach (Square square in squares)
            {
                if (square.GetPieceHolderTransform().childCount > 0)
                {
                    Destroy(square.GetPiece().gameObject);
                }

                if (square.pieceValue == 0) continue;

                GameObject piece = Instantiate(piecePrefab, square.GetPieceHolderTransform());
                SetSprite(piece, square.pieceValue);
            }
        }
        
        private void SetSprite(GameObject piece, int pieceValue)
        {
            SpriteRenderer spriteRenderer = piece.GetComponentInChildren<SpriteRenderer>();
            int spriteIndex = Piece.GetColor(pieceValue) == 16 ? 6 : 0;
            spriteIndex += Piece.GetType(pieceValue) - 1;

            spriteRenderer.sprite = pieceSprites[spriteIndex];
            piece.name = pieceSprites[spriteIndex].name;
        }

        public void PickUpPiece(GameObject piece)
        {
            heldPiece = piece;
            heldPiece.transform.parent = cursor.transform;
            heldPiece.transform.localPosition = Vector3.zero;
        }

        public bool MovePieceTo(Square origin, Square target)
        {
            if (!Rules.IsMoveLegal(origin, target, squares, gameManager.FenString)) return false;
            gameManager.FenString.IncrementHalfmoveClock();

            // En passant
            if (Piece.GetType(origin.pieceValue) == Piece.Pawn && Math.Abs(target.index - origin.index) == 16)
            {
                gameManager.FenString.SetEnPassantIndex(origin.index +
                                                        (Piece.GetColor(origin.pieceValue) == Piece.White ? 8 : -8));
            }
            else if (Piece.GetType(origin.pieceValue) == Piece.Pawn)
            {
                if (target.index == gameManager.FenString.EnPassantIndex)
                {
                    CapturePieceOn(squares[target.index + (Piece.GetColor(origin.pieceValue) == Piece.White ? -8 : 8)]);
                }

                gameManager.FenString.SetEnPassantIndex(-1);
            }
            else gameManager.FenString.SetEnPassantIndex(-1);

            // Capturing
            if (target.GetPieceHolderTransform().childCount > 0)
            {
                CapturePieceOn(target);
                gameManager.FenString.ResetHalfmoveClock();
            }
            
            if (Piece.GetType(origin.pieceValue) == Piece.Pawn) gameManager.FenString.ResetHalfmoveClock();

            // Castling
            if (Piece.GetType(origin.pieceValue) == Piece.King)
            {
                if (Piece.GetColor(origin.pieceValue) == Piece.White)
                {
                    gameManager.FenString.SetWhiteCanCastleShort(false);
                    gameManager.FenString.SetWhiteCanCastleLong(false);

                    if (target.index == 6 && gameManager.FenString.WhiteCanCastleKingside)
                    {
                        StaticMovePieceTo(squares[7], squares[5]);
                    }
                    else if (target.index == 2 && gameManager.FenString.WhiteCanCastleQueenside)
                    {
                        StaticMovePieceTo(squares[0], squares[3]);
                    }
                }
                else
                {
                    gameManager.FenString.SetBlackCanCastleShort(false);
                    gameManager.FenString.SetBlackCanCastleLong(false);

                    if (target.index == 62 && gameManager.FenString.BlackCanCastleKingside)
                    {
                        StaticMovePieceTo(squares[63], squares[61]);
                    }
                    else if (target.index == 58 && gameManager.FenString.BlackCanCastleQueenside)
                    {
                        StaticMovePieceTo(squares[56], squares[59]);
                    }
                }
            }

            if (Piece.GetType(origin.pieceValue) == Piece.Rook)
            {
                if (Piece.GetColor(origin.pieceValue) == Piece.White)
                {
                    if (origin.index == 7)
                    {
                        gameManager.FenString.SetWhiteCanCastleShort(false);
                    }
                    else if (origin.index == 0)
                    {
                        gameManager.FenString.SetWhiteCanCastleLong(false);
                    }
                }
                else
                {
                    if (origin.index == 63)
                    {
                        gameManager.FenString.SetBlackCanCastleShort(false);
                    }
                    else if (origin.index == 56)
                    {
                        gameManager.FenString.SetBlackCanCastleLong(false);
                    }
                }
            }

            if (Piece.GetType(origin.pieceValue) == Piece.King)
            {
                if (Piece.GetColor(origin.pieceValue) == Piece.White)
                {
                    gameManager.FenString.SetWhiteCanCastleShort(false);
                    gameManager.FenString.SetWhiteCanCastleLong(false);
                }
                else
                {
                    gameManager.FenString.SetBlackCanCastleShort(false);
                    gameManager.FenString.SetBlackCanCastleLong(false);
                }
            }
            
            // Promotion
            bool promoted = false;
            if (Piece.GetType(origin.pieceValue) == Piece.Pawn)
            {
                if (Piece.GetColor(origin.pieceValue) == Piece.White && target.index >= 56)
                {
                    origin.pieceValue = Piece.White | Piece.Queen;
                    promoted = true;
                }
                else if (Piece.GetColor(origin.pieceValue) == Piece.Black && target.index <= 7)
                {
                    origin.pieceValue = Piece.Black | Piece.Queen;
                    promoted = true;
                }
            }

            target.pieceValue = origin.pieceValue;
            origin.pieceValue = 0;

            heldPiece.transform.parent = target.GetPieceHolderTransform();
            heldPiece.transform.SetSiblingIndex(0);
            heldPiece.transform.localPosition = Vector3.zero;
            heldPiece = null;
            
            if (promoted) SetSprite(target.GetPiece(), target.pieceValue);

            int opponentColor = Piece.GetColor(target.pieceValue) == Piece.White ? Piece.Black : Piece.White;
            int[] pieceValues = Rules.CopyPieceValuesFromSquares(squares);
            if (Rules.IsKingInMate(pieceValues, gameManager.FenString, opponentColor))
            {
                Debug.Log("Checkmate!");
            }
            else if (Rules.IsKingInCheck(pieceValues, gameManager.FenString, opponentColor))
            {
                Debug.Log("Check!");
            }

            ResetSquareColors();
            ColorSquare(origin.index, pieceOriginColor);
            ColorSquare(target.index, moveIndicatorColor);
            
            if (!gameManager.FenString.WhiteToMove) gameManager.FenString.IncrementFullmoveNumber();
            gameManager.FenString.SetWhiteToMove(!gameManager.FenString.WhiteToMove);
            return true;

            void CapturePieceOn(Square pieceSquare)
            {
                Destroy(pieceSquare.GetPiece().gameObject);
                pieceSquare.pieceValue = 0;
            }
        }

        private void StaticMovePieceTo(Square origin, Square target)
        {
            target.pieceValue = origin.pieceValue;
            origin.pieceValue = 0;

            GameObject movedPiece = origin.GetPiece().gameObject;
            movedPiece.transform.parent = target.GetPieceHolderTransform();
            movedPiece.transform.localPosition = Vector3.zero;
        }

        public void ResetPieceToOrigin(Square origin)
        {
            heldPiece.transform.parent = origin.GetPieceHolderTransform();
            heldPiece.transform.localPosition = Vector3.zero;
            heldPiece = null;
        }

        public void ColorSquare(int squareIndex, Color color)
        {
            squares[squareIndex].SetColor(color);
        }

        public void ColorSquares(List<int> squareIndices, Color color)
        {
            foreach (int square in squareIndices)
            {
                squares[square].SetColor(color);
            }
        }

        public void ResetSquareColors()
        {
            foreach (Square square in squares)
            {
                square.ResetColor();
            }
        }

        public void SetSquareMarks(List<Move> squareIndices)
        {
            foreach (Move square in squareIndices)
            {
                squares[square.To].SetMarkerCircle(true);
            }
        }

        public void ResetSquareMarks()
        {
            foreach (Square square in squares)
            {
                square.SetMarkerCircle(false);
                square.SetHighlight(false);
            }
        }

        private void HighlightSquare(Square square)
        {
            square.SetHighlight(true);
            if (square != _highlightedSquare)
            {
                if (_highlightedSquare != null)
                {
                    _highlightedSquare.SetHighlight(false);
                }

                _highlightedSquare = square;
            }
        }
    }
}