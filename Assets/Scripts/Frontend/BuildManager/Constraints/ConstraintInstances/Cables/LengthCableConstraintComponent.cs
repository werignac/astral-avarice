using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Restricts the length of a cable to be under the max determined by build settings.
	/// </summary>
	public class LengthCableConstraintComponent : CablePlacerConstraintComponent
	{
		protected override ConstraintQueryResult QueryConstraint_Internal(ICablePlacer state)
		{
			ConstraintQueryResult result = new ConstraintQueryResult();

			// Don't show the warning until the player has clicked on the first building.
			if (!state.GetFromAttachment().GetIsSetAndNonVolatile())
				return result;

			// If we're moving and not showing the building we're moving,
			// we're not showing the cables either. Hide the warnings.
			if (state.GetFromAttachment().GetIsAttachedToHiddenBuildingCursor() || state.GetToAttachment().GetIsAttachedToHiddenBuildingCursor())
				return result;

			float cableLength = state.Length;
			float remainingCableLength = GlobalBuildingSettings.GetOrCreateSettings().MaxCableLength - cableLength;
			bool cableIsTooLong = remainingCableLength < 0;
			string formatLength = cableLength.ToString("0.00");

			string postfix = "";
			BuildWarning.WarningType warningType = BuildWarning.WarningType.GOOD;

			if (cableIsTooLong)
			{
				string formatRemainingLength = Mathf.Abs(remainingCableLength).ToString("0.00");
				postfix = $" ({formatRemainingLength} over limit)";
				warningType = BuildWarning.WarningType.FATAL;
			}

			BuildWarning warning = new BuildWarning($"Cable Length {formatLength}{postfix}.", warningType);
			result.AddWarning(warning);

			return result;
		}

		
	}
}
