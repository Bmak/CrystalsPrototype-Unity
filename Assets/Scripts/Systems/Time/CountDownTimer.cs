using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Timer utility class that tracks the remaining time between the current
/// time and when the timer finishes
/// </summary>
public class CountDownTimer
{
    [Inject]
    private CoroutineCreator _coroutineCreator;

    private IEnumerator _countDownCoroutine = null;

    private bool _fixedTimer = false;

    private long _remainingTime;

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
        long endTime = CurrentTime() + numSeconds;
        StartTimer(endTime, updateCallback, finishedCallback);
    }

	public void StartFixedTimer(int numSeconds, Action<long> updateCallback, Action finishedCallback)
    {
    	_fixedTimer = true;
        long endTime = CurrentTime() + numSeconds;
        StartTimer(endTime, updateCallback, finishedCallback);
    }

    /// <summary>
    /// Starts the count down timer
    /// </summary>
    /// <param name="endTime">When the timer should finish</param>
    /// <param name="updateCallback">Called once a second, providing the remaining time to the callback</param>
    /// <param name="finishedCallback">Called when the timer finishes</param>
    public void StartTimer(long endTime, Action<long> updateCallback, Action finishedCallback)
    {
        StopTimer();
        _countDownCoroutine = CountDown(endTime, updateCallback, finishedCallback);
        _coroutineCreator.StartCoroutine(_countDownCoroutine);
    }

    public void StopTimer()
    {
        if(_countDownCoroutine != null) {
            _coroutineCreator.StopCoroutine(_countDownCoroutine);
            _countDownCoroutine = null;
        }
    }

    public bool IsFinished
    {
        get { return _remainingTime <= 0;  }
    }

	private long CurrentTime()
	{
		return (long)Time.time;
	}

    private IEnumerator CountDown(long endTime, Action<long> updateCallback, Action finishedCallback)
    {
        //required to use _PlayerDC.GetServerTimeWithDebugOffset so the Timeshift cheat works correctly
        _remainingTime = endTime - CurrentTime();
        
        while (_remainingTime > 0) {
            if(updateCallback != null) {
                updateCallback(_remainingTime);
            }
            if (_fixedTimer) {
            	yield return new WaitForFixedUpdate();
            } else {
				yield return new WaitForSeconds(1);
            }
            _remainingTime = endTime - CurrentTime();
        }

        _countDownCoroutine = null;
        if(finishedCallback != null) {
            finishedCallback();
        }
    }
}
