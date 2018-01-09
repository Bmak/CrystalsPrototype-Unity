using System;
using System.Text.RegularExpressions;

/// <summary>
/// Utility class for common LanguageType manipulation and translation.
/// </summary>
public class LanguageUtil
{
	/// <summary>
	/// Maps language and country code to a language type.
	/// </summary>
	/// <returns>The language from codes.</returns>
	public static LanguageType GetLanguageFromCodes () 
	{
		return GetLanguageFromCodes (DeviceUtil.GetDeviceLanguageCode (), DeviceUtil.GetDeviceCountryCode ());
	}

	private static LanguageType GetLanguageFromCodes (string langCode, string countryCode)
	{
		langCode = langCode.ToLower();
		countryCode = countryCode.ToLower();

		// English
		if (langCode.StartsWith ("en")) {
			return LanguageType.ENG_US;
		// French
		} else if (langCode.StartsWith ("fr")) {
			return LanguageType.FRE_FR;
		// Italian
		} else if (langCode.StartsWith ("it")) {
			return LanguageType.ITA_IT;
		// German
		} else if (langCode.StartsWith ("de")) {
			return LanguageType.GER_DE;
		// Spanish
		} else if (langCode.StartsWith ("es")) {
			return LanguageType.SPA_XM;
		// Portugese
		} else if (langCode.StartsWith ("pt")) {
			return LanguageType.POR_BR;
		// Japanese
		} else if (langCode.StartsWith ("ja")) {
			return LanguageType.JPN_JP;
		// Korean
		} else if (langCode.StartsWith ("ko")) {
			return LanguageType.KOR_KR;
		// Russian
		} else if (langCode.StartsWith ("ru")) {
			return LanguageType.RUS_RU;
		// Thai
		} else if (langCode.StartsWith ("th")) {
			return LanguageType.THA_TH;
		// Indonesian
		} else if (langCode.StartsWith ("id")
			|| langCode.StartsWith ("in")) {
			return LanguageType.IND_ID;
		// Turkish
		} else if (langCode.StartsWith ("tr")) {
			return LanguageType.TUR_TR;
		// Simplified Chinese
		} else if (langCode.StartsWith ("zh-hans")
			|| langCode.StartsWith ("zh-sg")
			|| langCode.StartsWith ("zh-cn")
			|| langCode.StartsWith ("zh-chs")) {
			return LanguageType.CHS_CN;
		// Traditional Chinese
		} else if (langCode.StartsWith ("zh-hant")
			|| langCode.StartsWith ("zh-hk")
			|| langCode.StartsWith ("zh-tw")
			|| langCode.StartsWith ("zh-mo")
			|| langCode.StartsWith ("zh-cht")) {
			return LanguageType.CHT_CN;
		// "Generic" Chinese (Backwards Compatibility)
		} else if (langCode.StartsWith ("zh")) {
			// Traditional
			if (countryCode == "tw"
				|| countryCode == "hk"
				|| countryCode == "mo") {
				return LanguageType.CHT_CN;
			// Simplified
			} else {
				return LanguageType.CHS_CN;
			}
		// Unknown
		} else {
			// If type not resolved, default to English.
			return LanguageType.ENG_US;
		}
	}

    public static string GetURLIdentifierForLanguageType()
    {
        return GetURLIdentifierForLanguageType(GetLanguageFromCodes());
    }

    /// <summary>
    /// Maps language types to identifiers used to
    /// generate URLs for EA's support websites.
    /// </summary>
    public static string GetURLIdentifierForLanguageType(LanguageType language)
    {
        switch (language)
        {
            case LanguageType.FRE_FR: // French
                return "fr";
            case LanguageType.ITA_IT: // Italian
                return "it";
            case LanguageType.GER_DE: // German
                return "de";
            case LanguageType.SPA_XM: // Spanish
                return "es";
            case LanguageType.POR_BR: // Portugese
                return "pt";
            case LanguageType.JPN_JP: // Japanese
                return "jp";
            case LanguageType.KOR_KR: // Korean
                return "kr";
            case LanguageType.RUS_RU: // Russian
                return "ru";
            case LanguageType.TUR_TR: // Turkish
                return "tr";
            case LanguageType.CHT_CN: // Traditional Chinese
                return "tw";

        }

        // Default to English if no mapping is specified - including Thai, Indonesian, and Simplified Chinese
        return "en";
    }

    /// <summary>
    /// Convenience function to format the LanguageType for display
    /// </summary>
    /// <param name="language">The LanguageType to format</param>
    /// <returns>The string representing the given LanguageType in plain text English</returns>
    public static string LanguageTypeToPlainText (LanguageType language)
    {
        switch (language) {
            case LanguageType.ENG_US: return "English";
            case LanguageType.FRE_FR: return "French";
            case LanguageType.ITA_IT: return "Italian";
            case LanguageType.GER_DE: return "German";
            case LanguageType.SPA_XM: return "Spanish";
            case LanguageType.POR_BR: return "Portuguese";
            case LanguageType.JPN_JP: return "Japanese";
			case LanguageType.CHT_CN: return "Chinese (Traditional)";
            case LanguageType.CHS_CN: return "Chinese (Simplified)";
            case LanguageType.KOR_KR: return "Korean";
            case LanguageType.RUS_RU: return "Russian";
			case LanguageType.IND_ID: return "Indonesian";
			case LanguageType.TUR_TR: return "Turkish";
			case LanguageType.THA_TH: return "Thai";
            default: return language.ToString();
        }
    }
}

