using System;

public enum LanguageType
{
	ENG_US, FRE_FR, ITA_IT, GER_DE, SPA_XM, POR_BR, JPN_JP, CHT_CN, CHS_CN, KOR_KR, RUS_RU, IND_ID, THA_TH, TUR_TR
};

public static class LanguageTypeExtensions
{
	public static string[] AllLanguageCodes
	{ 
		get { return Enum.GetNames(typeof(LanguageType)); }
	} 
}