using UnityEngine;
using UnityEngine.Video;

namespace AstralAvarice.Backend
{
	[CreateAssetMenu(fileName = "HowToPlayTip", menuName = "Scriptable Objects/HowToPlayTip")]
	public class HowToPlayTip : ScriptableObject
    {
		public string title;
		public string description;
		public VideoClip videoClip;
    }
}
