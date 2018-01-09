using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;

/// <summary>
/// Controls the backdrop sprite for the global backdrop camera prefab.
/// </summary>
public class BackdropCamera : MonoBehaviour
{
    [Inject]
    LocalizationManager _lm;
    [Inject]
    LocalizationConfig _lc;

    [SerializeField]
	private UIProgressBar _progressBarLoading;
    [SerializeField]
    private UILabel _labelLoadingPercentage;
    [SerializeField]
    private UIButton _buttonProceed;
    [SerializeField]
    private UILabel _labelProceed;
    [SerializeField]
    private UISprite _fadeSprite;
    [SerializeField]
    private UISprite _flashSprite;

    public Action OnProceedClicked;

    private Tweener tweener;

    public void Init()
    {
        Injector.VerifyInject(ref _lm);
        Injector.VerifyInject(ref _lc);

        _buttonProceed.gameObject.SetActive(false);
        _labelProceed.gameObject.SetActive(false);
    }

    public void WireWidgets()
    {
        EventDelegate.Add(_buttonProceed.onClick, ProceedClicked);
    }

    public void OnRelease()
    {
        EventDelegate.Remove(_buttonProceed.onClick, ProceedClicked);
    }

    private void ProceedClicked()
    {
		if (OnProceedClicked != null)
		{
			OnProceedClicked();
		}
    }

    public void UpdateProgressValue(float value)
    {
        if (tweener == null)
        {
            tweener = DOTween.To(GetCurrentValue, UpdateProgress, value, 0.10f);
        }
        else
        {
            tweener.ChangeEndValue(value, 1.0f);
        }
    }

    public float GetCurrentValue()
    {
        return _progressBarLoading.value;
    }

    public void UpdateProgress(float value)
    {
        _progressBarLoading.value = value;
		_labelLoadingPercentage.text = string.Format("Loading {0}", value.ToString("P0"));

        if (value >= 1.0f)
        {
            _labelLoadingPercentage.gameObject.SetActive(false);
            _buttonProceed.gameObject.SetActive(true);
            _labelProceed.gameObject.SetActive(true);
        }
        else
        {
            _labelLoadingPercentage.gameObject.SetActive(true);
            _buttonProceed.gameObject.SetActive(false);
            _labelProceed.gameObject.SetActive(false);
        }
    }
}
