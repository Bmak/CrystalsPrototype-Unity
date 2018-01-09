using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class UILocalizedLabel : UILabel, ILoggable
{
	[SerializeField]
	protected string _localizationKey;

	[SerializeField]
	protected bool _autoLocalizeOnStart=true;

	public string LocalizationKey { get { return _localizationKey; } }

	private string _ellipsis = "...";
	public override string Ellipsis
	{
		get { return _ellipsis; }
	}

	[Inject]
	LocalizationManager _localizationManager;

	[Inject]
	LocalizationConfig _lc;

	//private bool _fontSet = false;
	protected bool _injected = false;

	protected override void Awake()
	{
		base.Awake();

		if (!_injected && Application.isPlaying) {
			this.Inject ();
			_injected = true;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();

		if (Application.isPlaying) {
			// set actual dynamic font (if necessary)
/*
			if (!_fontSet) {
				string fontType = FontManager.BODY_FONT_TYPE;
				// use reference 'font' to resolve dynamic font
				if (bitmapFont != null) {
					fontType = bitmapFont.name;
				// falling back to default dynamic font
				} else {
					this.LogWarning (string.Format ("Label on {0} has no font reference set. Using default reference font.", NGUITools.GetHierarchy (this.gameObject)));
				}
				trueTypeFont = _fontManager.LoadLanguageFont(_localizationManager.GetSelectedLanguage(), fontType).dynamicFont;
				_fontSet = true;
			}
*/
			_ellipsis = _localizationManager.Localize(_lc.GetSharedEllipsis());

			if (_autoLocalizeOnStart && !string.IsNullOrEmpty(_localizationKey)) {
				text = _localizationManager.Localize(_localizationKey);
			}
		}
	}
}
