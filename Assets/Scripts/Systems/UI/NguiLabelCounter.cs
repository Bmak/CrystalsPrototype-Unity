using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// This class will increment a numeric value over time and set the associated label's text with that value
/// </summary>
public abstract class NguiLabelCounter<T> where T : struct
{
    public static readonly Func<string, T, string> DEFAULT_FORMATTER = (format, value) => string.Format(format, value);

    private struct AnimationData
    {
        public T finalAmount;
        public float animateTime;
        public Action finishedCallback;
		public Action<T> updatedCallback;

		public AnimationData(T finalAmount, float animateTime, Action finishedCallback, Action<T> updatedCallback)
        {
            this.finalAmount = finalAmount;
            this.animateTime = animateTime;
            this.finishedCallback = finishedCallback;
            this.updatedCallback = updatedCallback;
        }
    }

    protected UILabel _label;
    private T _counterValue;
    protected T _cachedFinalValue;
    private Func<T, string> _formatter = (value) => DEFAULT_FORMATTER("{0}", value);
    private AnimationData _data;
    private IEnumerator _countUpRoutine;

    protected abstract T LerpValue(T from, T to, float time);
    protected abstract void AddDeltaToCachedAmount(T deltaAmount);

    public T GetCurrentValue ()
	{
		return _counterValue;
	}

    public void SetTextColor(Color c)
    {
        if (_label != null) {
            _label.color = c;
        }
    }

    // convenience access to just set label text
    public void SetTextDirectly(string text)
    {
        if (_label == null) { return; }

        // kill any count up in prgoress since we're hard setting the text        
        if(_countUpRoutine != null) {
            _label.StopCoroutine(_countUpRoutine);
        }

        _label.text = text;
    }

    //This function will be used to set the Initial Counter value
    public void SetInitialCounterValue(T value)
    {
        if (_label == null) { return; }

        // kill any count up in prgoress since we're setting a new starting value        
        if(_countUpRoutine != null) {
            _label.StopCoroutine(_countUpRoutine);
        }

        UpdateCounter(value);
        _cachedFinalValue = value;
    }

    /// <summary>
	/// Method that will be used to format the current counter value.
	/// </summary>
	/// <param name="formatter">Formatter.</param>
	public void SetStringFormatter(Func<T, string> formatter)
    {
        _formatter = formatter;
        UpdateCounter(_counterValue);
    }

    public void AnimateByDeltaAmount(T deltaAmount, float animateTime, Action<T> updatedCallback = null, Action finishedCallback = null)
    {
        if (_label == null) { return; }

        AddDeltaToCachedAmount(deltaAmount);
        _data = new AnimationData(_cachedFinalValue, animateTime, finishedCallback, updatedCallback);
        StartCountUp();
    }

    public void AnimateToAmount(T newCounterValue, float animateTime, Action<T> updatedCallback = null, Action finishedCallback = null)
    {
        if (_label == null) { return; }

        _cachedFinalValue = newCounterValue;
        _data = new AnimationData(_cachedFinalValue, animateTime, finishedCallback, updatedCallback);
        StartCountUp();
    }

    private void StartCountUp()
    {
        // if the label isn't active, coroutine won't work, and we won't see the anim anyway
        // just set the value instead
        if (!_label.gameObject.activeInHierarchy) {
            SetInitialCounterValue(_cachedFinalValue);

            if (_data.finishedCallback != null) {
                _data.finishedCallback();
            }
            return;
        }

        // We only ever want one Coroutine running to count up the numbers so that
        // multiple coroutines aren't trying to edit the label's text field at once.
        if(_countUpRoutine != null) {
            _label.StopCoroutine(_countUpRoutine);
        }

        _countUpRoutine = CountUp();
        _label.StartCoroutine(_countUpRoutine);
    }

    private IEnumerator CountUp()
    {
        AnimationData data = _data;
        T startValue = _counterValue;
        T endValue = data.finalAmount;

        if (!startValue.Equals(endValue)) {
            float elapsedTime = 0f;
            while (elapsedTime < data.animateTime) {
                T currentValue = LerpValue(startValue, endValue, elapsedTime / data.animateTime);
                UpdateCounter(currentValue);
                if (data.updatedCallback != null)
                {
                    data.updatedCallback(currentValue);
                }
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        UpdateCounter(endValue);

        if (data.finishedCallback != null)
        {
            data.finishedCallback();
        }
    }

    private void UpdateCounter(T value)
    {
        if (_label == null) { return; }

        _label.text = _formatter (value);
        _counterValue = value;
    }
}

/// <summary>
/// Specific implementation of NguiLabelCounter for integers
/// </summary>
public class NguiLabelCounterInt : NguiLabelCounter<int>
{
    public NguiLabelCounterInt(UILabel label)
    {
        _label = label;
    }

    protected override int LerpValue(int from, int to, float time)
    {
        return (int)Mathf.Lerp(from, to, time);
    }

    protected override void AddDeltaToCachedAmount(int deltaAmount)
    {
        _cachedFinalValue = deltaAmount + _cachedFinalValue;
    }
}

/// <summary>
/// Specific implementation of NguiLabelCounter for floats
/// </summary>
public class NguiLabelCounterFloat : NguiLabelCounter<float>
{
    public NguiLabelCounterFloat(UILabel label)
    {
        _label = label;
        // By default, format float counters to two decimals
        SetStringFormatter((value) => string.Format("{0:0.00}", value));
    }

    protected override float LerpValue(float from, float to, float time)
    {
        return Mathf.Lerp(from, to, time);
    }

    protected override void AddDeltaToCachedAmount(float deltaAmount)
    {
        _cachedFinalValue = deltaAmount + _cachedFinalValue;
    }
}