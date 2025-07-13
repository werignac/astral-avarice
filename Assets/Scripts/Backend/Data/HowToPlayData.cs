using UnityEngine;
using UnityEngine.Video;

namespace AstralAvarice.Backend
{
    [CreateAssetMenu(fileName = "HowToPlayData", menuName = "Scriptable Objects/HowToPlayData")]
    public class HowToPlayData : ScriptableObject
    {
		public HowToPlayTip[] tips;
    }
}
