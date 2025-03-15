using UnityEngine;

public struct BuildWarning
{
	private readonly string message;
	private readonly bool isFatal;

	public BuildWarning(string message, bool isFatal)
	{
		this.message = message;
		this.isFatal = isFatal;
	}

	public bool GetIsFatal()
	{
		return isFatal;
	}

	public string GetMessage()
	{
		return message;
	}

}
