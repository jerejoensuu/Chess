using System;
using UnityEngine;

namespace Code.Board
{
    public class BoardBuilder : MonoBehaviour
    {
        
        [SerializeField] private GameObject squarePrefab;
        [SerializeField] private float squareSize = 1f;
        
        [SerializeField] private Color lightColor;
        [SerializeField] private Color darkColor;
        
        private void Start()
        {
            BuildBoard();
        }
        
        private void BuildBoard()
        {
            for (int i = 0; i < 64; i++)
            {
                GameObject square = Instantiate(squarePrefab, transform);
                square.transform.position = GetPositionForIndex(i);
                square.transform.localScale = new Vector3(squareSize, squareSize, 1);
                
                Square squareScript = square.GetComponent<Square>();
                squareScript.index = i;
                square.name = squareScript.GetNotationForIndex(i);
                
                bool isLight = (i / 8 + i % 8) % 2 == 0;
                squareScript.SetColor(isLight ? lightColor : darkColor);
            }
        }
        
        private Vector3 GetPositionForIndex(int index)
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
