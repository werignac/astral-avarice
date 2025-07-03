using UnityEngine;
using AstralAvarice.Frontend;

public struct BuildWarning : BuildWarningElement
{
	/// <summary>
	/// Warning types are sorted so that when doing greater than operations,
	/// FATAL will override ALERT which will override GOOD. FATAL also overrides GOOD.
	/// </summary>
	public enum WarningType
	{
		GOOD, ALERT, FATAL
	}

	private readonly string message;
	private readonly WarningType warningType;

	public BuildWarning(string message, WarningType type)
	{
		this.message = message;
		warningType = type;
	}

	public WarningType GetWarningType()
	{
		return warningType;
	}

	public string GetMessage()
	{
		return message;
	}
}
