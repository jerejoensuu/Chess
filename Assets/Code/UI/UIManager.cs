using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class UIManager : MonoBehaviour
    {
        public GameSettings gameSettings;
        [SerializeField] private TextMeshProUGUI notationText;
        
        public void SetNotationText(string notation)
        {
            notationText.text = notation;
        }
        
    }
}