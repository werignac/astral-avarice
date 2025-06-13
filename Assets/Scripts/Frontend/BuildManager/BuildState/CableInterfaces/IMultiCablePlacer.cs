using UnityEngine;

namespace AstralAvarice.Frontend
{
	/// <summary>
	/// Interface for BuildStates that place multiple cables at a time.
	/// </summary>
    public interface IMultiCablePlacer
    {
		ICablePlacer[] GetCablePlacers();
    }
}
