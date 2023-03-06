using Code.Board;
using UnityEngine;

namespace Code.UI
{
    public class GameSettings : MonoBehaviour
    {
        [SerializeField]
        private BoardManager boardManager;
        
        [HideInInspector]
        public string FenString { get; private set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        
        public void SetFenString(string fenString)
        {
            FenString = fenString;
        }
    }
}