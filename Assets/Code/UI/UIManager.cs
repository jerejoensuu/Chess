using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class UIManager : MonoBehaviour
    {
        
        [SerializeField] private TextMeshProUGUI notationText;
        
        public void SetNotationText(string notation)
        {
            notationText.text = notation;
        }
        
    }
}