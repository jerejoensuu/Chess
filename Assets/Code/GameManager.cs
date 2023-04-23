using System;
using Code.Board;
using Code.UI;
using UnityEngine;

namespace Code
{
    public class GameManager : MonoBehaviour
    {
        // [SerializeField] private GameSettings gameSettings;
        private BoardManager _boardManager;
        private UIManager _uiManager;
        private string _activeFenString;
        
        private void Awake()
        {
            _boardManager ??= FindObjectOfType<BoardManager>();
            _uiManager ??= FindObjectOfType<UIManager>();
        }

        private void Start()
        {
            _boardManager.SetupBoard();
        }
        
        public void LoadNewGame()
        {
            _boardManager.SetupBoard(_uiManager.gameSettingsInspector.FenString);
        }
        
        public void UpdateFenString(string fenString)
        {
            throw new NotImplementedException();
        }
    }
}