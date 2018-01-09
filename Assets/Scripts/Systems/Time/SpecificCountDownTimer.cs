using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Timer utility class that tracks the remaining time between the current
/// time and when the timer finishes
/// </summary>
public class SpecificCountDownTimer
{
    [Inject]
    private CoroutineCreator _coroutineCreator;

    private IEnumerator _countDownCoroutine = null;

    private bool _fixedTimer = false;

    private long _remainingTime;
    private long _endTime;

    public string Name { get; set; }

    /// <summary>
    /// Starts the count down timer
    /// </summary>
    /// <param name="numSeconds">Timer finishes at current time plus numSeconds</param>
    /// <param name="updateCallback">Called once a second, providing the remaining time to the callback</param>
    /// <param name="finishedCallback">Called when the timer finishes</param>
    public void StartTimer(int numSeconds, Action<long> updateCallback, Action finishedCallback)
    {
        _fixedTimer = false;
        _endTime = CurrentTime() + numSeconds;
        StartTimer(updateCallback, finishedCallback);
    }

    public void StartFixedTimer(int numSeconds, Action<long> updateCallback, Action finishedCallback)
    {
        _fixedTimer = true;
        _endTime = CurrentTime() + numSeconds;
        StartTimer(updateCallback, finishedCallback);
    }

    /// <summary>
    /// Starts the count down timer
    /// </summary>
    /// <param name="endTime">When the timer should finish</param>
    /// <param name="updateCallback">Called once a second, providing the remaining time to the callback</param>
    /// <param name="finishedCallback">Called when the timer finishes</param>
    public void StartTimer(Action<long> updateCallback, Action finishedCallback)
    {
        StopTimer();
        _countDownCoroutine = CountDown(updateCallback, finishedCallback);
        _coroutineCreator.StartCoroutine(_countDownCoroutine);
    }

    public void StopTimer()
    {
        if (_countDownCoroutine != null)
        {
            _coroutineCreator.StopCoroutine(_countDownCoroutine);
            _countDownCoroutine = null;
        }
    }

    public long EndTime
    {
        get { return _endTime; }
        set
        {
            _endTime = value;
            _remainingTime = _endTime - CurrentTime();
        }
    }

    public long RemainingTime { get { return _remainingTime; } }

    private long CurrentTime()
    {
        return (long)Time.time;
    }

    private IEnumerator CountDown(Action<long> updateCallback, Action finishedCallback)
    {
        //required to use _PlayerDC.GetServerTimeWithDebugOffset so the Timeshift cheat works correctly
        _remainingTime = _endTime - CurrentTime();

        while (_remainingTime > 0)
        {
            if (updateCallback != null)
            {
                updateCallback(_remainingTime);
            }
            if (_fixedTimer)
            {
                yield return new WaitForFixedUpdate();
            }
            else
            {
                yield return new WaitForSeconds(1);
            }
            _remainingTime = _endTime - CurrentTime();
        }

        _countDownCoroutine = null;
        if (finishedCallback != null)
        {
            finishedCallback();
        }
    }
}
