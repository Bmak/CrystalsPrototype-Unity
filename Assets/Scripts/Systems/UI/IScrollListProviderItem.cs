using UnityEngine;
using System.Collections;

/// <summary>
/// The ScrolLListProvider uses this interface to allow any dynamically instantiated objects
/// to initialize themselves or provide their own shutdown logic.
/// </summary>
public interface IScrollListProviderItem
{
	void Initialize();

	void Shutdown();
}
