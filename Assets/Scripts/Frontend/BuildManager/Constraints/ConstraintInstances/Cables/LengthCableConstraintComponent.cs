using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Restricts the length of a cable to be under the max determined by build settings.
	/// </summary>
	public class LengthCableConstraintComponent : CableConstraintComponent
	{
		public override ConstraintQueryResult QueryConstraint(CableConstraintData state)
		{
			float cableLength = state.cableState.Length;
			float remainingCableLength = GlobalBuildingSettings.GetOrCreateSettings().MaxCableLength - cableLength;
			bool cableIsTooLong = remainingCableLength < 0;
			string formatLength = cableLength.ToString("0.00");


			string postfix = "";
			BuildWarning.WarningType warningType = BuildWarning.WarningType.GOOD;
			bool constraintTriggered = false;

			if (cableIsTooLong)
			{
				string formatRemainingLength = Mathf.Abs(remainingCableLength).ToString("0.00");
				postfix = $" ({formatRemainingLength} over limit)";
				warningType = BuildWarning.WarningType.FATAL;
				constraintTriggered = true;
			}

			BuildWarning warning = new BuildWarning($"Cable Length {formatLength}{postfix}.", warningType);

			return new ConstraintQueryResult(constraintTriggered, warning);
		}
	}
}
