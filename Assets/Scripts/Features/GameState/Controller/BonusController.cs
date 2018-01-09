using System;
using System.Collections.Generic;

public class BonusController : ILoggable
{
    private IProvider<SpecificCountDownTimer> _timerProvider;

    public delegate bool SetBonus(BonusType type);
    private SetBonus SetBonusAction;
    private Action<BonusType, Action> _removeBonusAction;

    
    private bool _bonusPause;
    private SpecificCountDownTimer _bonusPauseTimer;

    private const int COOL_DOWN = 8;
    private const int WAIT_COOL_DOWN = 2;
    private const int BONUS_PAUSE = 1;


    private List<BonusModel> _bonusModels = new List<BonusModel>();
    private List<BonusModel> _stack = new List<BonusModel>();

    public void Initialize(IProvider<SpecificCountDownTimer>  timerProvider, List<BonusType> bonusTypes, SetBonus setBonusAction, Action<BonusType, Action> removeBonusAction)
    {
        _timerProvider = timerProvider;

        SetBonusAction = setBonusAction;
        _removeBonusAction = removeBonusAction;
        _bonusPauseTimer = _timerProvider.Get();

        foreach (BonusType type in bonusTypes)
        {
            BonusModel model = new BonusModel(_timerProvider.Get(), type);
            _bonusModels.Add(model);
        }
    }

    public void Start()
    {
        foreach (BonusModel model in _bonusModels)
        {
            model.State = BonusModel.BonusState.Activated;
            model.OnFinishAction = OnFinishTimer;
            model.Start(COOL_DOWN);
        }
    }

    private void OnFinishTimer(BonusModel model)
    {
        _stack.Add(model);
        ShowBonus();
    }

    private void ShowBonus()
    {
        if (_bonusPause) return;

        if (_stack.Count > 0)
        {
            if (SetBonusAction != null)
            {
                if (SetBonusAction(_stack[0].Type))
                {
                    _stack[0].State = BonusModel.BonusState.OnField;
                    _stack[0].OnFinishAction = RemoveBonus;
                    _stack[0].Start(WAIT_COOL_DOWN);
                    _stack.RemoveAt(0);
                }
            }

            _bonusPause = true;

            _bonusPauseTimer.StartTimer(BONUS_PAUSE, null, PauseFinished);
        }
    }
    private void PauseFinished()
    {
        _bonusPause = false;
        ShowBonus();
    }

    public void StartBonus(BonusType type, bool wait = false)
    {
        BonusModel model = _bonusModels.Find(b => b.Type == type);
        model.State = wait ? BonusModel.BonusState.Await : BonusModel.BonusState.Activated;
        model.OnFinishAction = OnFinishTimer;
        model.Start(wait ? WAIT_COOL_DOWN : COOL_DOWN);
    }

    private void RemoveBonus(BonusModel model)
    {
        if (_removeBonusAction != null)
        {
            _removeBonusAction(model.Type, ()=>
            {
                model.State = BonusModel.BonusState.Await;
                model.OnFinishAction = OnFinishTimer;
                model.Start(WAIT_COOL_DOWN);
            });
        }
    }

    public void Stop()
    {
        foreach (BonusModel model in _bonusModels)
        {
            model.Stop();
            if (_removeBonusAction != null)
            {
                _removeBonusAction(model.Type, null);
            }
        }
    }

    public void Destroy()
    {
        _bonusPauseTimer.StopTimer();

        foreach (BonusModel model in _bonusModels)
        {
            model.Stop();
        }

        _bonusModels.Clear();
        _bonusModels = null;

        _stack.Clear();
        _stack = null;
    }
}
