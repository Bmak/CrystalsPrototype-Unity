class UIUnhoverButtonAfterClickOnEvent : UIButton
{
    private bool isClicked;
    private bool isUnhovered;

    protected override void OnClick()
    {
        if (isUnhovered)
        {
            return;
        }

        isClicked = true;

        base.SetState(State.Pressed, true);
        base.UpdateColor(true);

        base.OnClick();
    }

    protected override void OnPress(bool isPressed)
    {
        if (isPressed)
        {
            isUnhovered = false;
        }
        base.OnPress(isPressed);
    }

    public override void SetState(State state, bool immediate)
    {
        if (!isClicked)
        {
            base.SetState(state, immediate);
        }
    }

    public void Unhover()
    {
        base.SetState(State.Normal, true);
        base.UpdateColor(true);
        isClicked = false;
        isUnhovered = true;
    }
}

