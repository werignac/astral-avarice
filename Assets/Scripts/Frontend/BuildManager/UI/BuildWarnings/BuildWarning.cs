using UnityEngine;

public struct BuildWarning
{
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
