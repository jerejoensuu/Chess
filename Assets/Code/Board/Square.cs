using UnityEngine;

namespace Code.Board
{
    // A square on the chess board
    public class Square : MonoBehaviour
    {
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        public int index; // The index of the square on the board, 0-63
        
        public void SetColor(Color color)
        {
            spriteRenderer.color = color;
        }
        
        public int GetIndexForNotation(string notation)
        {
            string file = notation.Substring(0, 1);
            string rank = notation.Substring(1, 1);
            return GetIndexForNotation(file, rank);
        }

        public int GetIndexForNotation(string file, string rank)
        {
            int fileIndex = file switch
            {
                "a" => 0,
                "b" => 1,
                "c" => 2,
                "d" => 3,
                "e" => 4,
                "f" => 5,
                "g" => 6,
                "h" => 7,
                _ => -1
            };
            
            int rankIndex = int.Parse(rank) - 1;
            
            index = rankIndex * fileIndex;
            
            return index;
        }
        
        public string GetNotationForIndex(int index)
        {
            int fileIndex = index % 8;
            int rankIndex = index / 8;
            
            string file = fileIndex switch
            {
                0 => "a",
                1 => "b",
                2 => "c",
                3 => "d",
                4 => "e",
                5 => "f",
                6 => "g",
                7 => "h",
                _ => ""
            };
            
            string rank = (rankIndex + 1).ToString();
            
            return $"{file}{rank}";
        }
    }
}
