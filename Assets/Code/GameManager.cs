using System;
using Code.Board;
using Code.Data;
using Code.UI;
using UnityEngine;

namespace Code
{
    public class GameManager : MonoBehaviour
    {
        // [SerializeField] private GameSettings gameSettings;
        private BoardManager _boardManager;
        private UIManager _uiManager;
        public FenString FenString { get; private set; }
        
        public AI.AI ai;
        
        private void Awake()
        {
            _boardManager ??= FindObjectOfType<BoardManager>();
            _boardManager.gameManager ??= this;
            
            _uiManager ??= FindObjectOfType<UIManager>();
            FenString ??= new FenString();
            
            ai ??= new AI.PseudoSmartAIv1();
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
        
        public void UnpauseGame()
        {
            _boardManager.Paused = false;
        }
        
        public void EnableAutoPlay(bool enable)
        {
            _boardManager.AutoPlay = enable;
        }
    }
}