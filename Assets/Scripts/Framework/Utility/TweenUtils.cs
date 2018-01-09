using UnityEngine;
using System.Collections;

/// <summary>
/// Tween utils. Class creates and returns the hashtable args used by iTween
/// </summary>
public class TweenUtils 
{
	public static Hashtable TweenMoveToArguments(Vector3 position,bool isLocal,string onComplete,GameObject onCompleteGO,float time = 1.0f)
	{
		Hashtable arguments = new Hashtable();
		arguments.Add("easetype",iTween.EaseType.linear);
		arguments.Add("position",position);
		arguments.Add("islocal",isLocal);
        if (onCompleteGO != null) {
            arguments.Add("oncomplete", onComplete);
            arguments.Add("oncompletetarget", onCompleteGO);
        }
		arguments.Add("time",time);

		return arguments;
	}

	public static Hashtable TweenScaleToArguments(Vector3 scale,bool isLocal,float time = 1.0f,string onComplete = "",GameObject onCompleteGO = null)
	{
		Hashtable arguments = new Hashtable();
		arguments.Add("easetype",iTween.EaseType.linear);
		arguments.Add("scale",scale);
		arguments.Add("islocal",true);
		arguments.Add("time",time);

		if(!string.IsNullOrEmpty(onComplete) && onCompleteGO != null) {
			arguments.Add("oncomplete",onComplete);
			arguments.Add("oncompletetarget",onCompleteGO);
		}
		
		return arguments;
	}

	public static Hashtable TweenMoveByArguments(Vector3 amount,string onComplete,GameObject onCompleteGO,float time = 1.0f)
	{
		Hashtable arguments = new Hashtable();
		arguments.Add("easetype",iTween.EaseType.linear);
		arguments.Add("amount",amount);
		if(onCompleteGO != null) {
			arguments.Add("oncomplete",onComplete);
			arguments.Add("oncompletetarget",onCompleteGO);
		}
		arguments.Add("time",time);
		
		return arguments;
	}

	public static Hashtable TweenFadeToArguments(float alpha,string onComplete,GameObject onCompleteGO,float time)
	{
		Hashtable arguments = new Hashtable();
		arguments.Add("easetype",iTween.EaseType.easeInSine);
		arguments.Add("alpha",alpha);
		if(onCompleteGO != null) {
			arguments.Add("oncomplete",onComplete);
			arguments.Add("oncompletetarget",onCompleteGO);
		}
		arguments.Add("time",time);
		
		return arguments;
	}

	public static void GameObjectFadeTo(GameObject gameObject,float startAlpha,float alpha,string onComplete,GameObject onCompleteGO,float time)
	{
		Hashtable args = TweenFadeToArguments(alpha,onComplete,onCompleteGO,time);
		Hashtable args1 = TweenFadeToArguments(alpha,"",null,time);

		int i = 0;
		foreach(Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
		{
			Color color = renderer.material.color;
			color.a = startAlpha;
			renderer.material.color = color;

			if(i==0) {
				iTween.FadeTo(renderer.gameObject,args);
			}
			else {
				iTween.FadeTo(renderer.gameObject,args1);
			}
			++i;
		}
	}
}
