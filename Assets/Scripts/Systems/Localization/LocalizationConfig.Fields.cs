using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Localization config fields. This class contains the localization analog of the
/// Config/Config.Fields system. The fields below represent default loc key
/// names that may be overridden by server data. This enables us to reference
/// loc keys in a strongly-typed manner, yet still maintain flexibility, provide
/// an option for server overrides, and prevent string constants from floating
/// around the codebase.
///
/// The value of each field below is a localization *key*, which must
/// correspond identically to a loc key in our loc files/HAL.
///
/// To override the below values, specify override keys in GameData in
/// a manner analogous to game-constants.xml.
///
/// </summary>

public partial class LocalizationConfig
{
	// Shared
	private string _messageViewOk =															"MessageView_Ok";
	private string _messageViewBack =														"MessageView_Back";
	private string _messageViewYes =														"MessageView_Yes";
	private string _messageViewNo =															"MessageView_No";
	private string _messageViewBuy =														"MessageView_Buy";
	private string _messageViewCancel =														"MessageView_Cancel";

	private string _numberFormatUtilUnitFormatMillions =									"NumberFormatUtil_Unit_Format_Millions";
	private string _numberFormatUtilUnitFormatThousands =									"NumberFormatUtil_Unit_Format_Thousands";

	private string _sharedNumberSeparatorThousands =										"Shared_NumberSeparatorThousands";
	private string _sharedEllipsis	=														"Shared_Ellipsis";


    public string GetMessageViewOk()														{ return _messageViewOk; }
    public string GetMessageViewBack()														{ return _messageViewBack; }
    public string GetMessageViewYes()														{ return _messageViewYes; }
    public string GetMessageViewNo()														{ return _messageViewNo; }
	public string GetMessageViewBuy()														{ return _messageViewBuy; }
	public string GetMessageViewCancel()													{ return _messageViewCancel; }

	public string GetNumberFormatUtilUnitFormatMillions()									{ return _numberFormatUtilUnitFormatMillions; }
	public string GetNumberFormatUtilUnitFormatThousands()									{ return _numberFormatUtilUnitFormatThousands; }

	public string GetSharedNumberSeparatorThousands()										{ return _sharedNumberSeparatorThousands; }
	public string GetSharedEllipsis()														{ return _sharedEllipsis; }
}

