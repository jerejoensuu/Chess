using Code.Data;
using UnityEngine;

namespace Code.Board
{
    // A square on the chess board
    public class Square : MonoBehaviour
    {
        [SerializeField] private GameObject pieceHolder;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Color _defaultColor;
        
        [SerializeField] private AssetLib assetLib;

        [SerializeField] private GameObject highlight;
        [SerializeField] private GameObject moveMarker;
        [SerializeField] private GameObject captureMarker;

        public int index; // The index of the square on the board, 0-63

        public Square(int index)
        {
            this.index = index;
        }

        public void SetColor(Color color)
        {
            spriteRenderer.color = color;
        }

        public void SetDefaultColor(Color color)
        {
            _defaultColor = color;
            spriteRenderer.color = color;
        }

        public void ResetColor()
        {
            spriteRenderer.color = _defaultColor;
        }

        // public Transform GetPieceHolderTransform()
        // {
        //     return pieceHolder.transform;
        // }

        public GameObject GetPiece()
        {
            return pieceHolder.transform.childCount == 0 ? null : pieceHolder.transform.GetChild(0).gameObject;
        }

        public void SetHighlight(bool active)
        {
            highlight.SetActive(active);
        }

        public void SetMarkerCircle(bool active, bool isEmpty = false)
        {
            if (active)
            {
                if (isEmpty)
                    moveMarker.SetActive(true);
                else
                    captureMarker.SetActive(true);
            }
            else
            {
                moveMarker.SetActive(false);
                captureMarker.SetActive(false);
            }
        }

        public Vector2 GetCenter()
        {
            return transform.position;
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

        public string GetNotation()
        {
            return GetNotationForIndex(index);
        }
        
        public static string GetNotationForIndex(int index)
        {
            int fileIndex = index % 8;
            int rankIndex = index / 8;
            
            string file = fileIndex switch
            {
                0 => "A",
                1 => "B",
                2 => "C",
                3 => "D",
                4 => "E",
                5 => "F",
                6 => "G",
                7 => "H",
                _ => ""
            };
            
            string rank = (rankIndex + 1).ToString();
            
            return $"{file}{rank}";
        }
        
        public void SetSprite(int pieceValue)
        {
            SpriteRenderer pieceSpriteRenderer = pieceHolder.GetComponentInChildren<SpriteRenderer>();
            
            if (pieceValue == 0)
            {
                pieceSpriteRenderer.enabled = false;
                return;
            }
            else if (!pieceSpriteRenderer.enabled)
            {
                pieceSpriteRenderer.enabled = true;
            }
            
            int color = Piece.GetColor(pieceValue);
            int type = Piece.GetType(pieceValue);

            int spriteIndex = color == 16 ? 6 : 0;
            spriteIndex += type - 1;

            Sprite newSprite = assetLib.pieceSprites[spriteIndex];

            pieceSpriteRenderer.sprite = newSprite;
            // pieceObject.name = newSprite.name;
        }
    }
}