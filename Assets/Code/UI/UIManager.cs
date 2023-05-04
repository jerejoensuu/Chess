using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class UIManager : MonoBehaviour
    {
        public GameSettingsInspector gameSettingsInspector;
        [SerializeField] private TextMeshProUGUI notationText;
        
        public void SetNotationText(string notation)
        {
            notationText.text = notation;
        }
        
    }
}