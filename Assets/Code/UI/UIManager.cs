using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class UIManager : MonoBehaviour
    {
        public GameSettingsInspector gameSettingsInspector;
        [SerializeField] private TextMeshProUGUI notationText;
        
        private void Awake()
        {
            gameSettingsInspector ??= FindObjectOfType<GameSettingsInspector>();
        }
        
        public void SetNotationText(string notation)
        {
            notationText.text = notation;
        }
        
    }
}