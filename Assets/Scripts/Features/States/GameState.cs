using UnityEngine;
using System.Collections;

public class GameState : State
{
    [Inject]
    private Config _config;

    [Inject]
    private GameController _gameController;

    public override bool SC_Enter(object transitionInfo, SC_Callback onCompleteCallback = null)
    {
        DoGlobalCleanUp();

        GameTransitionInfo data = transitionInfo as GameTransitionInfo;
        _gameController.Initialize(data);

        return base.SC_Enter(transitionInfo, onCompleteCallback);
    }

    public override void SC_Exit(bool result, SC_Callback onCompleteCallback)
    {
        _gameController.Shutdown();

        DoGlobalCleanUp();

        base.SC_Exit(result, onCompleteCallback);
    }

    // Normally Unity would do this sort of work on a scene load, but since we only use one scene,
    // we'll run this manually in and out of battle
    private void DoGlobalCleanUp()
    {
        if (_config.GetUnloadUnusedAssetsEachStateFrequency() > 0)
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}
