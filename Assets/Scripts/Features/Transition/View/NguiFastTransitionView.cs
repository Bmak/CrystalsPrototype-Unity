
// This transition screen is intended to be used any time you think an async operation would be fast enough to not need one.
// Contains an empty widget with a time delayed spinner, so if the operation takes longer than a second or two, a wait image will come up
public class NguiFastTransitionView : NguiView
{
    public override DepthEnum InitialDepth { get { return DepthEnum.BusyOverlay; } }
}
