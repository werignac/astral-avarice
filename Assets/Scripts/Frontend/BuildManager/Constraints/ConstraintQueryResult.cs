using UnityEngine;

namespace AstralAvarice.Frontend
{
	public class ConstraintQueryResult
	{
		/// <summary>
		/// If true, prevents the current build state from being applied
		/// (applying = performing the main action of the build state).
		/// </summary>
		public bool BlocksApply { get; private set; }
		public bool HasWarning { get; private set; }
		public BuildWarning Warning { get; private set; }
		
		/// <summary>
		/// Constructor to use when the constraint is not preventing a structure
		/// from being built and there is no warning.
		/// </summary>
		public ConstraintQueryResult()
		{
			BlocksApply = false;
			HasWarning = false;
		}

		public ConstraintQueryResult(bool blocksApply, BuildWarning warning)
		{
			BlocksApply = blocksApply;
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
