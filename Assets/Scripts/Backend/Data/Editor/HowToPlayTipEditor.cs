using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace AstralAvarice.Backend.Editor
{
	/// <summary>
	/// Editor that draws a special UI for the how to play tip scriptable objects.
	/// </summary>
	[CustomEditor(typeof(HowToPlayTip))]
    public class HowToPlayTipEditor : UnityEditor.Editor
    {
		private const string INSPECTOR_PATH = "Assets/Editor/UI/HowToPlay/HowToPlayTipInspector.uxml";

		public override VisualElement CreateInspectorGUI()
		{
			VisualElement container = new VisualElement();

			VisualTreeAsset inspectorXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(INSPECTOR_PATH);
			container.Add(inspectorXML.CloneTree());

			return container;
		}
	}
}
