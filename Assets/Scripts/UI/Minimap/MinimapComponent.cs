using UnityEngine;

// In the future, this class may not be needed, as the minimap may be part of
// the UI where it cannot be shown and hidden.
public class MinimapComponent : MonoBehaviour
{

	public bool GetIsShowing()
	{
		return gameObject.activeSelf;
	}

    public void Show()
	{
		gameObject.SetActive(true);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public void Toggle()
	{
		if (GetIsShowing())
			Hide();
		else
			Show();
	}
}
