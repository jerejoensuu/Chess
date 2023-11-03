using UnityEngine;

namespace Code.Data
{
    [CreateAssetMenu(fileName = "AssetLib", menuName = "ScriptableObjects/AssetLib")]
    public class AssetLib : ScriptableObject
    {
        public Sprite[] pieceSprites;
    }
}
