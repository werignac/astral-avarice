using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
    public class VersionUIComponent : MonoBehaviour
    {
		private const string VERSION_LABEL_NAME = "VersionLabel";

		[SerializeField] private UIDocument _versionDocument;

		private void Start()
		{
			Label versionLabel = _versionDocument.rootVisualElement.Q<Label>(VERSION_LABEL_NAME);
			versionLabel.text = Application.version;
		}
	}
}
