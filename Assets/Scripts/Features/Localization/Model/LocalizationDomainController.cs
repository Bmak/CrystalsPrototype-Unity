using System;
using UnityEngine;

/// <summary>
/// Localization domain controller provides operations to be done on LocalizationDO. 
/// </summary>
public class LocalizationDomainController : ILoggable, IDomainController {

	[Inject]
	private LocalizationService _localizationService;

    [Inject]
    private IProvider<LocalizationDO> _localizationDOProvider;

	public LocalizationDO Localize { get; private set; }

	void IDomainController.Reset ()
	{
		Init();
	}

	private void Init ()
	{
		Localize = _localizationDOProvider.Get();
	}
    
    [PostConstruct]
    private void PostConstruct()
    {
		Localize = _localizationDOProvider.Get();
    }

	public void GetLocalizeRPC( Action success = null, Action<ResponseCode>  failed = null )
	{
		_localizationService.GetLocalization( success, failed );
	}
}
