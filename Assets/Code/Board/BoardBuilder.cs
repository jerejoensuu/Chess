using System;
using UnityEngine;

namespace Code.Board
{
    public class BoardBuilder : MonoBehaviour
    {
        
        [SerializeField] private GameObject squarePrefab;

        [SerializeField] private Color lightColor;
        [SerializeField] private Color darkColor;

        public Square[] BuildBoard(string fenString = "default", float squareSize = 1f)
        {
            Square[] squares = new Square[64];
            
            for (int i = 0; i < 64; i++)
            {
                GameObject square = Instantiate(squarePrefab, transform);
                square.transform.position = GetTilePositionForIndex(i, squareSize);
                square.transform.localScale = new Vector3(squareSize, squareSize, 1);
                
                Square squareScript = square.GetComponent<Square>();
                squareScript.index = i;
                square.name = squareScript.GetNotationForIndex(i);
                
                bool isLight = (i / 8 + i % 8) % 2 == 0;
                squareScript.SetColor(isLight ? lightColor : darkColor);
                
                squares[i] = squareScript;
            }
            
            if (fenString == "default")
                fenString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            
            TranslateFenString(fenString, squares);
            
            return squares;
        }
        
        private void TranslateFenString(string fenString, Square[] squares)
        {
            string[] fenFields = fenString.Split(' ');
            string[] fenRows = fenFields[0].Split('/');
            
            int index = 0;
            foreach (string fenRow in fenRows)
            {
                foreach (char c in fenRow)
                {
                    if (char.IsDigit(c))
                    {
                        index += int.Parse(c.ToString());
                    }
                    else
                    {
                        squares[index].pieceValue = Piece.GetValueForFenChar(c);
                        index++;
                    }
                }
            }
        }
        
        private Vector3 GetTilePositionForIndex(int index, float squareSize)
        {
            float offset = squareSize * 4 - squareSize / 2;

            int file = index % 8;
            int rank = index / 8;
            
            float x = file * squareSize - offset;
            float y = -rank * squareSize + offset;
            
            return new Vector3(x, y, 0);
        }
    }
}
