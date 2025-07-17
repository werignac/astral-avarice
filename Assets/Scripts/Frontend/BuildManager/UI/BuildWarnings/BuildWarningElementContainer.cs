using System.Collections.Generic;
using UnityEngine;

namespace AstralAvarice.Frontend
{
    public class BuildWarningElementContainer
    {
		private List<BuildWarningElement> children = new List<BuildWarningElement>();

		public void AddChild(BuildWarningElement child)
		{
			children.Add(child);
		}

		public void AddChildren<T>(ICollection<T> newChildren) where T : BuildWarningElement
		{
			foreach (BuildWarningElement child in newChildren)
				AddChild(child);
		}

		public List<BuildWarningElement> GetChildren()
		{
			return children;
		}

		public bool GetHasChildren()
		{
			return children.Count > 0;
		}
	}
}
