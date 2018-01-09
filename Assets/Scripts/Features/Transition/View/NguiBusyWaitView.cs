using UnityEngine;

public class NguiBusyWaitView : NguiView
{
    public override DepthEnum InitialDepth { get { return DepthEnum.BusyOverlay; } }

    private const int PANEL_DEPTH = 10000;

    // On start, set the depth of this view to be really high instead of the default depth that was calculated
    // because the default depth could be lower than a panel depth in a different view, which would cause some widgets
    // to appear above this scrim view
    void Start()
    {
        _setDepth(PANEL_DEPTH);
    }
}
