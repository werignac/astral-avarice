using System.Collections.Generic;
using UnityEngine;

public class BuildWarningContext
{
	private List<BuildWarning> buildingWarnings = new List<BuildWarning>();
	private List<BuildWarning> cableWarnings = new List<BuildWarning>();
	
	public void AddBuildingWarning(BuildWarning toAdd)
	{
		buildingWarnings.Add(toAdd);
	}

	public void AddBuildingWarnings(IEnumerable<BuildWarning> toAdd)
	{
		buildingWarnings.AddRange(toAdd);
	}

	public void AddCableWarning(BuildWarning toAdd)
	{
		cableWarnings.Add(toAdd);
	}

	public void AddCableWarnings(IEnumerable<BuildWarning> toAdd)
	{
		cableWarnings.AddRange(toAdd);
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
