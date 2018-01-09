using System;
using System.Collections.Generic;

/// <summary>
/// The enumerated set of features that can be locked
/// Note "feature" in this case is an abstract concept, not related necessarily to a FeatureController
/// </summary>
public enum LockableFeature
{
    STORE,
    CARDS,
    CLANS,
    REPLAYS,
	UNDEFINED
}

/// <summary>
/// Provides utility methods to check if a given feature is "unlocked" for the user
/// </summary>
public class FeatureLockController : ILoggable
{
    [Inject]
    private Config _config;

	// TODO: We should consider having this controller use the requirements controller if we have features
	// that are not just using player level or campaign related requirements
    public bool IsFeatureLocked(LockableFeature feature, bool showDialog = false)
    {
		bool featureLocked = false;
/*
        if (showDialog && featureLocked) {
            _messageController.ShowMessage(
                _localizationManager.Localize(_lc.GetFeatureLockedTitle(feature)),
                localizationMessage
                );
        }
*/
        return featureLocked;
    }
/*
	public List<LockableFeature> GetFeaturesUnlockedAtLevel(int level)
	{
		List<LockableFeature> unlockedAtLevel = new List<LockableFeature>();
		foreach (LockableFeature feature in System.Enum.GetValues(typeof(LockableFeature)))
		{
			if (_config.GetFeatureUnlockLevel(feature) == level)
			{
				unlockedAtLevel.Add(feature);
			}
		}

		return unlockedAtLevel;
	}
*/
}
