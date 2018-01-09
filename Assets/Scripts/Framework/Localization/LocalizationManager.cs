using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class LocalizationManager : IInitializable, ILoggable 
{  
	public const string OVERRIDE_LANGUAGE_PREFS_KEY = "OverrideLanguage";
	
	[Inject]
	private LocalizationDomainController _localizationDC;
	
	[Inject]
	private LocalPrefs _localPrefs;

    [Inject]
    private Client _client;
	
	// Static to allow reading language code in a static context, before initialization
	private static LanguageType? _language = null;
	
	private LocStringDatabase _locDatabase;

    public event System.Action<string, LanguageType> LookupFailedHard;  // Key wasn't found, caller requires it so report it as an error
    public event System.Action<string, LanguageType> LookupFailedSoft;  // Key wasn't found, caller was merely asking if key exists, not an error
	
	
	// TODO: Prevent double-loading of loc db when it is already cached locally
	public void Initialize( InstanceInitializedCallback initializedCallback = null )
	{
		_locDatabase = _localizationDC.Localize.LoadLocDatabase();
		LoadSelectedLanguage();
		
		if ( initializedCallback != null )
			initializedCallback( this );
	}
	
	public IEnumerable<string> GetAllCountries()
	{
		if (_locDatabase == null) {
			this.LogWarning("The localization string database is null");
		}
		
		return _locDatabase != null ? _locDatabase.GetAllCountries() : new string[] { };
	}
	
	private void LoadSelectedLanguage()
	{
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( GetType().Name + ":LoadSelectedLanguage" );
		#endif	

		LoadLanguage( GetSelectedLanguage() );

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( GetType().Name + ":LoadSelectedLanguage" );
		#endif	
	}
	
	private bool HasOverrideLanguage()
	{
#if PRODUCTION
        return false;
#else
        return _localPrefs.HasSharedKey(OVERRIDE_LANGUAGE_PREFS_KEY);
#endif
	}
	
	public LanguageType GetOverrideLanguage()
	{
		// try to set the language from the override
		string saveLang = _localPrefs.GetSharedString (OVERRIDE_LANGUAGE_PREFS_KEY);    
		return EnumUtil.Parse<LanguageType> (saveLang);
	}
	
	public LanguageType GetSelectedLanguage()
	{
		if ( _language != null ) { return ( LanguageType )_language; }
		
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( GetType().Name + ":GetSelectedLanguage" );
		#endif	

        _language = LanguageUtil.GetLanguageFromCodes ();
		
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( GetType().Name + ":GetSelectedLanguage" );
		#endif	

		return (LanguageType)_language;
	}
	

	
	public void LoadLanguage( LanguageType language, bool enableOverride = true )
	{
		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.Start( GetType().Name + ":LoadLanguage" );
		#endif
		
		if ( enableOverride && HasOverrideLanguage() )	{
			language = GetOverrideLanguage();
		}

		_language = language;
		_locDatabase.LoadLanguage(language);

		#if METRICS_ENABLED && INCLUDE_DEV_METRICS
		Metrics.End( GetType().Name + ":LoadLanguage" );
		#endif
	}
	
	public string Localize(string key, bool ignoreMissingKey = false)
	{
		string result = string.Empty;

		try{
		    if (!_locDatabase.TryLocalize(key, out result)) {
                if (ignoreMissingKey) {
                    if (LookupFailedSoft != null)
                        LookupFailedSoft(key, GetSelectedLanguage());
		        } else {
                    this.LogError("No localization value found when trying to localize " + key);
                    if(LookupFailedHard != null)
                        LookupFailedHard(key, GetSelectedLanguage());
		        }
			}
		} catch (NullReferenceException nre){
			this.LogError(string.Format("NullReferenceException in LocalizationManager.Localize(): {0}, Stack:\n {1}", nre.Message, nre.StackTrace));
        } catch (Exception e){
			this.LogError(string.Format("Exception in LocalizationManager.Localize(): {0}, Stack:\n {1}", e.Message, e.StackTrace));
		}
		
		return result;
	}
	
	public string LocalizeAndFormat (string key, params object[] param)
	{
		try
		{
			return string.Format(Localize(key), param);
		}
		catch (FormatException)
		{
			return key;
		}
	}


	/// **** Time methods moved below, because their reliance upon LocalizationManager creates 
	/// **** dependency loops requiring much framework code to be located in the main assembly instead of
	/// **** the Plugins assembly. We may wish to move this to another class at some point.


    /// <summary>
    /// Converts a time in seconds to a relative time string, describing how long ago something occurred in human-readable terms
    /// </summary>
    /// <param name="seconds">The length of time to describe in seconds</param>
    /// <param name="localizationKeySuffix">The suffix for the localization key used as a template for the construted string</param>
    /// <param name="substituteMomentsAgo">Flag indicating that small times should be replaced by "moments ago"</param>
    /// <returns>Human-readable string describing how long ago something occurred (assuming seconds ended now)</returns>
    public string RelativeTimeString(long seconds, string localizationKeySuffix = "", bool substituteMomentsAgo = true)
    {
        long timeValue = TimeUtil.RelativeTimeFloorValue(seconds);
        TimeType timeType = TimeUtil.RelativeTimeType(seconds, substituteMomentsAgo);

        // Moment type times do not have a value and are always plural
        if (timeType == TimeType.Moment && substituteMomentsAgo)
        {
            return Localize("MOMENTS_AGO");
        }

        // Singular form
        string timeTypeLocalizationString = timeType.ToString().ToUpper();

        // Use plural form
        if (timeValue != 1)
        {
            timeTypeLocalizationString += "S";
        }

        // Gets the appropriate singular or plural form (example: SECONDS_AGO) and replaces the number string with the actual value
        return Localize(timeTypeLocalizationString + localizationKeySuffix).Replace("[number]", timeValue.ToString(CultureInfo.InvariantCulture));
    }

    //  Gets Future Tense localized time strings for short time formats
    //  at most 2 significant time discriptors will be present
    //  i.e: "in 1d 4h 4m" would be "in 1d 4h"  
    //  i.e  "in 4m" minutes being the smallest time format displayed
    //  i.e  "in 30s" would be "in 1m"
    //  user needs to specify if time format is for future or past tense
    //  future is formatted for hour and minutes only as per design
    public string RelativeTimeStringFutureTenseShortFormatDayHourMinute(long seconds)
    {
        long timeValueWin = TimeUtil.RelativeTimeFloorValue(seconds);
        TimeType timeTypeWin = TimeUtil.RelativeTimeType(seconds, false);

        long remainderTime = TimeUtil.RelativeTimeFloorValueRemainder(seconds);
        long timeValuePlace = TimeUtil.RelativeTimeFloorValue(remainderTime);
        TimeType timeTypePlace = TimeUtil.RelativeTimeType(remainderTime);

        if ( timeTypeWin == TimeType.Second )
        {
            timeTypeWin = TimeType.Minute;
            timeValueWin = 1;
            timeValuePlace = 0;
        }

        if ( timeTypeWin == TimeType.Minute)
        {
            timeValuePlace = 0;
        }

        if ( timeTypePlace == TimeType.Minute && timeTypeWin == TimeType.Day )
        {
            timeTypePlace = TimeType.Hour;
            timeValuePlace = 1;
        }

        string timeTypeWinLocalizedString = GetTimeSuffixForTimeType(timeTypeWin);
        string timeTypePlaceLocalizedString = GetTimeSuffixForTimeType(timeTypePlace);
        string shortTimeString = timeValueWin + timeTypeWinLocalizedString;

        if ( timeValuePlace > 0)       
        {
            shortTimeString += " " + timeValuePlace + timeTypePlaceLocalizedString;
        }

        return shortTimeString;           

   }

	/// <summary>
	/// Returns the suffix to use at the end of a time component to denote the magnitude of time.
	/// </summary>
	/// <param name="type">The TimeType component to return the suffix for.</param>
	/// <returns>Suffix for the time component.</returns>
	public string GetTimeSuffixForTimeType(TimeType type)
	{
		switch(type)
		{
			case TimeType.Day:
				return Localize("DAY_SHORT");//"d";
			case TimeType.Hour:
				return Localize("HOUR_SHORT");//"h";
			case TimeType.Minute:
	        	return Localize("MINUTE_SHORT");//"m";
			case TimeType.Second:
			case TimeType.Moment:
				return Localize("SECONDS_SHORT");//"s";
		}
		
		//TimeType.Moment does not need a suffix, hence return an empty string.
        return "";
	}


    //  Gets Past Tense localized time strings for medium time formats
    //  i.e "17 days ago",
    public string RelativeTimeStringPastTenseMediumFormat( long seconds, bool useAbbreviated = true )
    {
        long timeValue;
        // use medium localizations
        timeValue = TimeUtil.RelativeTimeFloorValue(seconds);
        TimeType timeType = TimeUtil.RelativeTimeType(seconds);
        // we don't want to display seconds so coerce to 1 minute
        if ( timeType == TimeType.Second )
        {
            timeValue = 1;
            timeType = TimeType.Minute;
        }
        string localizationKey = GetLocalizationMediumKey(timeType, timeValue > 1 ? true : false, useAbbreviated );
        return Localize(localizationKey).Replace("[number]", timeValue.ToString(CultureInfo.InvariantCulture));
    }

	
    // returns the time localization key for Medium format time
    private string GetLocalizationMediumKey( TimeType timeType, bool plural, bool useAbbreviated = true )
    {
        if ( plural ) 
        {
            switch (timeType)
            {
                case TimeType.Moment:
                    return "MOMENTS_AGO";

                case TimeType.Minute:
                    return (useAbbreviated ? "MINUTES_AGO_ABBREVIATED" : "MINUTES_AGO");

                case TimeType.Hour:
                    return (useAbbreviated ? "HOURS_AGO_ABBREVIATED" : "HOURS_AGO");

                case TimeType.Day:
                    return "DAYS_AGO";

                case TimeType.Second:
                    return (useAbbreviated ? "SECONDS_AGO_ABBREVIATED" : "SECONDS_AGO");

                default:
                    UnityEngine.Debug.LogWarning("Unknown TimeType: " + timeType.ToString());
                    return "";
            }
        }
        else
        {
            switch (timeType)
            {
                case TimeType.Moment:
                    return "MOMENTS_AGO";

                case TimeType.Minute:
                    return (useAbbreviated ? "MINUTE_AGO_ABBREVIATED" : "MINUTE_AGO");

                case TimeType.Hour:
                    return (useAbbreviated ? "HOUR_AGO_ABBREVIATED" : "HOUR_AGO");

                case TimeType.Day:
                    return "DAY_AGO";

                case TimeType.Second:
                    return (useAbbreviated ? "SECOND_AGO_ABBREVIATED" : "SECOND_AGO");

                default:
                    UnityEngine.Debug.LogWarning("Unknown TimeType: " + timeType.ToString());
                    return "";
            }
        }
    }

	/// <summary>
	/// Converts a time in seconds to a relative time string comprising of a maximum of 2 components of time in decreasing order of magnitude.
	/// </summary>
	/// <param name="seconds">The length of time to describe in seconds</param>
	/// <returns>Human-readable string describing the time to maximum of 2 levels of magnitude eg. 1d 2h or 6h 32m</returns>
	public string ShortTimeString(long seconds)
	{
		long timeValue = TimeUtil.RelativeTimeFloorValue(seconds);
        TimeType timeType = TimeUtil.RelativeTimeType(seconds, false);
		
		long remainderTime = TimeUtil.RelativeTimeFloorValueRemainder(seconds);
		long timeValueSecond = TimeUtil.RelativeTimeFloorValue(remainderTime);
        TimeType timeTypeSecond = TimeUtil.RelativeTimeType(remainderTime);
		
        string timeTypeLocalizedString = GetTimeSuffixForTimeType(timeType);
		string timeTypeSecondLocalizedString = GetTimeSuffixForTimeType(timeTypeSecond);
		
		string shortTimeString = timeValue + timeTypeLocalizedString;
		
		if(timeType != TimeType.Second && 0 != timeValueSecond)
		{
			shortTimeString += " " + timeValueSecond + timeTypeSecondLocalizedString;
		}
		
		return shortTimeString;
	}

    /// <summary>
    /// Gets a localized short date format, e.g. 10/10/2010
    /// </summary>
    public string ShortDateString(DateTime date)
    {
        return LocalizeAndFormat("DATE_FORMAT_SHORT_MDY", date.Month, date.Day, date.Year);
    }

}
