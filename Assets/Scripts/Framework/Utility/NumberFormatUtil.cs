using System;
using UnityEngine;

/// <summary>
/// Lightweight injectable that provides number formatting support.
/// </summary>
public class NumberFormatUtil
{
	[Inject]
	private LocalizationManager _lm;

	[Inject]
	private LocalizationConfig _lc;

    public enum FormatType
    {
        STANDARD_TRUNCATE,
        COMMA_NO_DECIMAL,
    }

    public string Format (FormatType type, long number)
    {
        switch (type)
        {
            case FormatType.COMMA_NO_DECIMAL:
                return GetCommaNoDecimalFormat (number);
            case FormatType.STANDARD_TRUNCATE:
                return GetStandardTruncateFormat (number);
        }
        return number.ToString (); // fall-through
    }

	private string GetStandardTruncateFormat (long number)
	{				
		if (Mathf.Abs(number) >= 100000000) {	// hundreds millions (e.g., 100M, 100.5M)
			return _lm.LocalizeAndFormat (_lc.GetNumberFormatUtilUnitFormatMillions (), Math.Truncate ((double)number/1000/1000*10)/10);
		} else if (Mathf.Abs(number) >= 1000000) { // millions (e.g., 1M, 1.5M, 10.25M)
			return _lm.LocalizeAndFormat (_lc.GetNumberFormatUtilUnitFormatMillions (), Math.Truncate ((double)number/1000/1000*10*10)/10/10);
		} else if (Mathf.Abs(number) >= 100000) { // hundreds of thousands (e.g., 100K, 100.5K)
			return _lm.LocalizeAndFormat (_lc.GetNumberFormatUtilUnitFormatThousands (), Math.Truncate ((double)number/1000*10)/10);
		} 
        return GetCommaNoDecimalFormat (number); // default (e.g., 99,999)
    }

    private string GetCommaNoDecimalFormat (long number)
    {
        // "Prettify" number by adding commas (but no decimal point) to it
        string formattedString = string.Format("{0:n0}", number);
        if (!string.IsNullOrEmpty(formattedString)) {
            formattedString = formattedString.Replace(",", _lm.Localize(_lc.GetSharedNumberSeparatorThousands()));
        }
        return formattedString;
    }
}
