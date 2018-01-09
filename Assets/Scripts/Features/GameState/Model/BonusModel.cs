using System;

public class BonusModel
{
    public enum BonusState
    {
        OnField = 0,
        Await = 1,
        Activated = 2
    }

    public BonusType Type { get; set; }
    public BonusState State { get; set; }
    public Action<BonusModel> OnFinishAction { get; set; }

    private SpecificCountDownTimer _timer;

    public BonusModel(SpecificCountDownTimer timer, BonusType type)
    {
        Type = type;
        _timer = timer;
    }

    public void Start(int time)
    {
        _timer.StopTimer();
        _timer.StartTimer(time, null, Finish);
    }

    public void Stop()
    {
        _timer.StopTimer();
    }

    public void Destroy()
    {
        _timer.StopTimer();
        _timer = null;
    }

    private void Finish()
    {
        if (OnFinishAction != null)
        {
            OnFinishAction(this);
        }
    }

}
