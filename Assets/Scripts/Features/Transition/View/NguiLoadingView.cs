using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Fully opaque loading screen
/// </summary>
public class NguiLoadingView : NguiView
{
    public override DepthEnum InitialDepth { get { return DepthEnum.BusyOverlay; } }
}
