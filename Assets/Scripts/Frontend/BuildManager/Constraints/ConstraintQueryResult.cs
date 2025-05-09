using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class ConstraintQueryResult
	{
		public bool ConstraintTriggered { get; private set; }
		public bool HasWarning { get; private set; }
		public BuildWarning Warning { get; private set; }
		
		/// <summary>
		/// Constructor to use when the constraint is not preventing a structure
		/// from being built and there is no warning.
		/// </summary>
		public ConstraintQueryResult()
		{
			ConstraintTriggered = false;
			HasWarning = false;
		}

		public ConstraintQueryResult(bool constraintTriggered, BuildWarning warning)
		{
			ConstraintTriggered = constraintTriggered;
			HasWarning = true;
			Warning = warning;
		}

		public bool TryGetWarning(out BuildWarning warning)
		{
			warning = Warning;
			return HasWarning;
		}
	}
}
