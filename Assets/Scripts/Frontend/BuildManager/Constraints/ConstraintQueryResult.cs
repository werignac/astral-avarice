using UnityEngine;
using System.Collections.Generic;

namespace AstralAvarice.Frontend
{
	public class ConstraintQueryResult
	{
		public BuildWarning.WarningType HighestWarning { get; private set; }
		public List<BuildWarning> Warnings { get; } = new List<BuildWarning>();

		/// <summary>
		/// Constructor to use when the constraint is not preventing a structure
		/// from being built and there is no warning.
		/// </summary>
		public ConstraintQueryResult()
		{
			HighestWarning = BuildWarning.WarningType.GOOD;
		}

		public void AddWarning(BuildWarning warning)
		{
			Warnings.Add(warning);

			if (warning.GetWarningType() > HighestWarning)
				HighestWarning = warning.GetWarningType();
		}
	}
}
