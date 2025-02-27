using UnityEngine;
using System.Collections.Generic;

namespace AstralAvarice.VisualData
{
    public interface IBuildUIMenuElement
    {
		public string Name { get; }
		public Sprite Icon { get; }
		// The order in which this category should be shown. Lower numbers are shown first, higher numbers are shown later.
		public int Priority { get; }
		// Not currently used. Could be useful as a tooltip.
		public string Description { get; }
    }

	public class BuildUIMenuElementPriorityComparer : IComparer<IBuildUIMenuElement>
	{
		public int Compare(IBuildUIMenuElement x, IBuildUIMenuElement y)
		{
			return x.Priority.CompareTo(y.Priority);
		}
	}
}
