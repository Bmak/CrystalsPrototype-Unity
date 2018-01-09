using UnityEngine;
using System.Collections;

/// <summary>
/// Defines a common pattern for managing the lifecycle of
/// a feature controller. Feature controllers are top level
/// controllers which manage a game feature (i.e. BattleController,
/// StoreController, etc). Feature controllers do not necessarily
/// have a global lifecycle - if they don't have a global lifecycle,
/// their lifecycle will usually be controlled by a game state.
/// 
/// Feature controllers may be created using their constructor so
/// they are responsible for doing their own dependency injection if
/// they have injected fields.
/// </summary>
public interface IFeatureController
{
    /// <summary>
    /// Called at the start of the feature controller's lifecycle and
    /// contains any necessary startup logic.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Called at the end of the feature controller's lifecycle and
    /// contains any necessary cleanup logic.
    /// </summary>
	void Shutdown();
}
