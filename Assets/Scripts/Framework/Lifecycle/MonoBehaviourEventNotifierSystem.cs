public class MonoBehaviourEventNotifierSystem : MonoBehaviourEventNotifierComponent, IInitializable, ILifecycleAware, ILoggable
{
	public void Initialize (InstanceInitializedCallback initializedCallback = null)
	{
		this.LogTrace ("Initialize()", LogCategory.INITIALIZATION);

		if (initializedCallback != null) {
			initializedCallback (this);
		}
	}

	public void Reset ()
	{
		ClearEventDelegates ();
	}
}
