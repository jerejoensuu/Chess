using System;
using UnityEngine;

namespace Code.Board
{
    public class BoardBuilder : MonoBehaviour
    {
        [SerializeField] private GameObject squarePrefab;

        public Square[] BuildBoard(Color darkColor, Color lightColor, string fenString = "default", float squareSize = 1f)
        {
            GameObject tiles = new GameObject("Tiles");
            tiles.transform.parent = transform;
            
            Square[] squares = new Square[64];
            
            for (int i = 0; i < 64; i++)
            {
                GameObject square = Instantiate(squarePrefab, tiles.transform);
                square.transform.position = GetTilePositionForIndex(i, squareSize);
                square.transform.localScale = new Vector3(squareSize, squareSize, 1);
                
                Square squareScript = square.GetComponent<Square>();
                squareScript.index = i;
                square.name = squareScript.GetNotation();
                
                bool isDark = (i / 8 + i % 8) % 2 == 0;
                squareScript.SetDefaultColor(isDark ? darkColor : lightColor);
                
                squares[i] = squareScript;
            }
            
            // Create 2D collider for the board
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(8 * squareSize, 8 * squareSize);

            return squares;
        }
        
        public void SetPiecesFromFenString(string fenString, ref Piece[] pieces)
        {
            if (fenString == "default")
                fenString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
                // fenString = "4k3/7r/8/8/8/8/7P/R3K3 w - - 0 1";
            
            string[] fenFields = fenString.Split(' ');
            string[] fenRows = fenFields[0].Split('/');
            
            // Go through the board from left to right, top to bottom
            int index = 56;
            foreach (string fenRow in fenRows)
            {
                foreach (char c in fenRow)
                {
                    if (char.IsDigit(c))
                    {
                        int emptySquares = int.Parse(c.ToString());
                        for (int i = 0; i < emptySquares; i++)
                        {
                            pieces[index].Value = Piece.GetValueForFenChar('e');
                            NextIndex();
                        }
                    }
                    else
                    {
                        pieces[index].Value = Piece.GetValueForFenChar(c);
                        NextIndex();
                    }
                }
            }
            
            void NextIndex()
            {
                // Go through the board from left to right, top to bottom
                if (index % 8 == 7)
                    index -= 15;
                else
                    index++;
            }
        }
        
        private Vector3 GetTilePositionForIndex(int index, float squareSize)
        {
            float offset = squareSize * 4 - squareSize / 2;

            int file = index % 8;
            int rank = index / 8;
            
            float x = file * squareSize - offset;
            float y = rank * squareSize - offset;
            
            return new Vector3(x, y, 0);
        }
    }
}
