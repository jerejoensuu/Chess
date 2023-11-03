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
        private Camera _mainCamera;

        [InspectorLabel("Colors")] public Color lightColor;
        public Color darkColor;
        public Color pieceOriginColor;
        public Color moveIndicatorColor;
        public Color legalMoveColor;

        [SerializeField] private float squareSize = 1f;

        public Piece[] pieces = new Piece[64];
        public Square[] squares;

        public GameObject cursor;
        public GameObject heldPiece;

        private Square _highlightedSquare;

        // AI
        private bool _aiIsThinking;
        [NonSerialized] public bool AutoPlay = true;
        [NonSerialized] public bool Paused = false;
        
        private bool _tilesCreated = false;


        private void Start()
        {
            _mainCamera = Camera.main;
            cursor = new GameObject("Cursor");
        }

        public void SetupBoard(string fenString = "default")
        {
            if (!_tilesCreated)
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    pieces[i] = new Piece();
                }
                    
                squares = boardBuilder.BuildBoard(lightColor, darkColor, fenString, squareSize);
                
                for (int i = 0; i < pieces.Length; i++)
                {
                    pieces[i].OnValueChanged.AddListener(squares[i].SetSprite);
                }

                _tilesCreated = true;
            }
            
            try
            {
                boardBuilder.SetPiecesFromFenString(fenString, ref pieces);
            }
            catch (Exception)
            {
                Debug.LogError("Invalid FEN string:");
                boardBuilder.SetPiecesFromFenString("default", ref pieces);
            }
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

            Move move = gameManager.ai.GetMove(pieces, gameManager.FenString, color);
            Square originSquare = squares[move.From];
            Square targetSquare = squares[move.To];

            string playerColor = color == Piece.White ? "White" : "Black";
            Debug.Log(playerColor + " is moving " + originSquare.GetNotation() + " to " + targetSquare.GetNotation());

            PickUpPiece(originSquare.GetPiece());
            MovePieceTo(move.From, move.To);
            gameManager.FenString.SetWhiteToMove(color == Piece.Black);

            yield return new WaitForSeconds(0.01f);
            _aiIsThinking = false;
        }

        public void CalculateCursorPosition()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10;
            cursor.transform.position = _mainCamera.ScreenToWorldPoint(mousePosition) + Vector3.back;

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

        public void PickUpPiece(GameObject piece)
        {
            // heldPiece = piece;
            // heldPiece.transform.SetParent(cursor.transform, false);
        }

        public bool MovePieceTo(int originIndex, int targetIndex)
        {
            int pieceValue = pieces[originIndex].Value;
            int color = Piece.GetColor(pieceValue);
            int type = Piece.GetType(pieceValue);

            if (!Rules.IsMoveLegal(originIndex, targetIndex, pieces, gameManager.FenString))
                return false;

            gameManager.FenString.IncrementHalfmoveClock();

            HandleEnPassant(originIndex, targetIndex);

            // Capturing
            if (pieces[targetIndex].Value != 0) 
            { 
                CapturePieceOn(targetIndex); 
                gameManager.FenString.ResetHalfmoveClock(); 
            } 

            HandleCastling(originIndex, targetIndex);

            HandlePromotion(originIndex, targetIndex);

            MovePieceToTarget(originIndex, targetIndex);

            int opponentColor = color == Piece.White ? Piece.Black : Piece.White;
            int[] pieceValues = Rules.CopyPieceValues(pieces);

            HandleCheckAndMate(pieceValues, opponentColor);

            ResetSquareColors();
            ColorSquare(originIndex, pieceOriginColor);
            ColorSquare(targetIndex, moveIndicatorColor);

            HandleFullmoveNumber();

            return true;
        }

        private void HandleEnPassant(int originIndex, int targetIndex)
        {
            int type = Piece.GetType(pieces[originIndex].Value);
            int color = Piece.GetColor(pieces[originIndex].Value);

            gameManager.FenString.SetEnPassantIndex(-1);

            if (type == Piece.Pawn && Mathf.Abs(targetIndex - originIndex) == 16)
            {
                gameManager.FenString.SetEnPassantIndex(originIndex + (color == Piece.White ? 8 : -8));
            }
            else if (type == Piece.Pawn && targetIndex == gameManager.FenString.EnPassantIndex)
            {
                CapturePieceOn(targetIndex + (color == Piece.White ? -8 : 8));
            }
        }

        private void HandleCastling(int originIndex, int targetIndex)
        {
            int type = Piece.GetType(pieces[originIndex].Type);
            int color = Piece.GetColor(pieces[originIndex].Color);
            
            if (type == Piece.King)
            {
                if (color == Piece.White)
                {
                    gameManager.FenString.SetWhiteCanCastleShort(false);
                    gameManager.FenString.SetWhiteCanCastleLong(false);

                    if (targetIndex == 6 && gameManager.FenString.WhiteCanCastleKingside)
                    {
                        StaticMovePieceTo(7, 5);
                    }
                    else if (targetIndex == 2 && gameManager.FenString.WhiteCanCastleQueenside)
                    {
                        StaticMovePieceTo(0, 3);
                    }
                }
                else
                {
                    gameManager.FenString.SetBlackCanCastleShort(false);
                    gameManager.FenString.SetBlackCanCastleLong(false);

                    if (targetIndex == 62 && gameManager.FenString.BlackCanCastleKingside)
                    {
                        StaticMovePieceTo(63, 61);
                    }
                    else if (targetIndex == 58 && gameManager.FenString.BlackCanCastleQueenside)
                    {
                        StaticMovePieceTo(56, 59);
                    }
                }
            }

            if (type == Piece.Rook)
            {
                if (color == Piece.White)
                {
                    if (originIndex == 7)
                        gameManager.FenString.SetWhiteCanCastleShort(false);
                    else if (originIndex == 0)
                        gameManager.FenString.SetWhiteCanCastleLong(false);
                }
                else
                {
                    if (originIndex == 63)
                        gameManager.FenString.SetBlackCanCastleShort(false);
                    else if (originIndex == 56)
                        gameManager.FenString.SetBlackCanCastleLong(false);
                }
            }
        }

        // TODO: Implement option to choose promotion piece
        private void HandlePromotion(int originIndex, int targetIndex)
        {
            int type = Piece.GetType(pieces[originIndex].Type);
            int color = Piece.GetColor(pieces[originIndex].Color);
            
            bool promoted = false;
            if (type == Piece.Pawn)
            {
                if (color == Piece.White && targetIndex >= 56)
                {
                    pieces[originIndex].Value = Piece.White | Piece.Queen;
                    promoted = true;
                }
                else if (color == Piece.Black && targetIndex <= 7)
                {
                    pieces[originIndex].Value = Piece.Black | Piece.Queen;
                    promoted = true;
                }
            }

            // TODO: Implement UI
            // if (promoted)
            //     SetSprite(target.GetPiece(), target.pieceValue);
        }

        private void MovePieceToTarget(int originIndex, int targetIndex)
        {
            pieces[targetIndex].Value = pieces[originIndex].Value;
            pieces[originIndex].Value = 0;

            // TODO: Implement UI
            // Transform pieceHolderTransform = target.GetPieceHolderTransform();
            // heldPiece.transform.SetParent(pieceHolderTransform, false);
            // heldPiece.transform.localPosition = Vector3.zero;
            // heldPiece = null;
        }

        private void HandleCheckAndMate(int[] pieceValues, int opponentColor)
        {
            if (Rules.IsKingInMate(pieceValues, gameManager.FenString, opponentColor))
                Debug.Log("Checkmate!");
            else if (Rules.IsKingInCheck(pieceValues, gameManager.FenString, opponentColor))
                Debug.Log("Check!");
        }

        private void HandleFullmoveNumber()
        {
            if (!gameManager.FenString.WhiteToMove)
                gameManager.FenString.IncrementFullmoveNumber();

            gameManager.FenString.SetWhiteToMove(!gameManager.FenString.WhiteToMove);
        }

        private void CapturePieceOn(int index)
        {
            // TODO: Implement UI
            // Destroy(pieceSquare.GetPiece().gameObject);
            // pieceSquare.GetPiece().gameObject.transform.parent = null;
            pieces[index].Value = 0;
        }

        private void StaticMovePieceTo(int originIndex, int targetIndex)
        {
            pieces[targetIndex].Value = pieces[originIndex].Value;
            pieces[originIndex].Value = 0;

            // TODO: Implement UI
            // GameObject movedPiece = origin.GetPiece().gameObject;
            // movedPiece.transform.parent = target.GetPieceHolderTransform();
            // movedPiece.transform.localPosition = Vector3.zero;
        }

        public void ResetPieceToOrigin(int originIndex)
        {
            // heldPiece.transform.parent = origin.GetPieceHolderTransform();
            // heldPiece.transform.localPosition = Vector3.zero;
            // heldPiece = null;
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
                squares[square.To].SetMarkerCircle(true, pieces[square.To].Value == 0);
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

        // private void OnDrawGizmos()
        // {
        //     // Piece value over each square
        //     
        //     for (int i = 0; i < pieces.Length; i++)
        //     {
        //         if (pieces[i].Value == 0) continue;
        //
        //         Handles.color = Color.black;
        //         // Handles.Label(squares[i].transform.position + (Vector3.down * .4f), pieces[i].Value.ToString());
        //     }
        // }
    }
}