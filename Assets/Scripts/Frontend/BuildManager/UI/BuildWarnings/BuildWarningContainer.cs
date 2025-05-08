using System.Collections.Generic;
using UnityEngine;

public class BuildWarningContainer
{
	private List<BuildWarning> buildingWarnings = new List<BuildWarning>();
	private List<BuildWarning> cableWarnings = new List<BuildWarning>();
	
	public void AddBuildingWarning(BuildWarning toAdd)
	{
		buildingWarnings.Add(toAdd);
	}

	public void AddCableWarning(BuildWarning toAdd)
	{
		cableWarnings.Add(toAdd);
	}

	public IEnumerable<BuildWarning> GetBuildingWarnings()
	{
		return buildingWarnings;
	}

	public IEnumerable<BuildWarning> GetCableWarnings()
	{
		return cableWarnings;
	}

	public IEnumerator<BuildWarning> GetAllWarnings()
	{
		foreach (BuildWarning warning in buildingWarnings)
		{
			yield return warning;
		}

		foreach (BuildWarning warning in cableWarnings)
		{
			yield return warning;
		}
	}
}
