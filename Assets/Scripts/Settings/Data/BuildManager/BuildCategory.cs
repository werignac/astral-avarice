using UnityEngine;

namespace AstralAvarice.VisualData
{
	[CreateAssetMenu(fileName = "BuildingCategory", menuName = "Visual Info/Building Category", order = 2)]
    public class BuildCategory : ScriptableObject, IBuildUIMenuElement
	{
		[SerializeField, InspectorName("Name")] private string categoryName;
		[SerializeField, InspectorName("Icon")] private Sprite categoryIcon;
		[SerializeField, InspectorName("Priority")] private int categoryPriority;
		[SerializeField, InspectorName("Description"), Multiline(lines: 10)] private string categoryDescription;

		// Getters.
		public string Name { get => categoryName; }
		public Sprite Icon { get => categoryIcon; }
		public int Priority { get => categoryPriority; }
		public string Description { get => categoryDescription; }

		public override bool Equals(object other)
		{
			// Non-null is not equal to null.
			// this!=null might not ever be true.
			if (other == null && this != null)
				return false;

			// Only compare build categories.
			if (!(other is BuildCategory))
			return false;			

			BuildCategory otherCategory = other as BuildCategory;

			// If the names are different, the categories are different.
			if (categoryName != otherCategory.categoryName)
				return false;

#if UNITY_EDITOR
			// Check that the rest of the parameters are the same. Should not be needed in final build.
			AssertSharesFields(otherCategory);
#endif

			return true;
		}

		/// <summary>
		/// Assets that this BuildCategory shares the same fields as the passed parameter.
		/// </summary>
		/// <param name="otherCategory">The BuildCategory to compare to.</param>
		private void AssertSharesFields(BuildCategory otherCategory)
		{
			Debug.Assert(
				categoryName == otherCategory.categoryName,
				$"Expected BuildCategory name {categoryName} to equal {otherCategory.categoryName} from two equivalent categories."
				);
			Debug.Assert(
				categoryIcon == otherCategory.categoryIcon,
				$"Two BuildCategories with the name {categoryName} have different icons {categoryIcon} and {otherCategory.categoryIcon}."
				);
			Debug.Assert(
				categoryPriority == otherCategory.categoryPriority,
				$"Two BuildCategories with the name {categoryName} have different priorities {categoryPriority} and {otherCategory.categoryPriority}."
				);
			Debug.Assert(
				categoryDescription == otherCategory.categoryDescription,
				$"Two BuildCategories with the name {categoryName} have different descriptions {categoryDescription} and {otherCategory.categoryDescription}."
				);

		}

		/// <summary>
		/// Required since overriding equals.
		/// </summary>
		public override int GetHashCode()
		{
			return name.GetHashCode();
		}
	}
}
