using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Container for a set of animations that can be played together
/// </summary>
[Serializable]
public class NguiAnimGroup
{
    [SerializeField]
    private List<UITweener> _tweeners = new List<UITweener>();

    // plays all anim type things referenced in the lists above, returns the total duration (longest anim)
    public float Play(MonoBehaviour owner, bool reverse, Action finishCallback = null)
    {
        float longestAnim = 0.0f;

        for (int i = 0; i < _tweeners.Count; ++i) {
            UITweener tweener = _tweeners[i];
            if (tweener == null) {
                continue;
            }
            longestAnim = Mathf.Max(longestAnim, tweener.duration + tweener.delay);
            if (reverse) {
				tweener.PlayReverse();
            } else {
				tweener.PlayForward();
            }
            tweener.ResetToBeginning();
        }

        if (finishCallback != null) {
            // Don't wait a frame if no animations will happen
            if (longestAnim == 0.0f) {
                finishCallback();
            } else {
                owner.Invoke(finishCallback, longestAnim);
            }
        }

        return longestAnim;
    }
}
