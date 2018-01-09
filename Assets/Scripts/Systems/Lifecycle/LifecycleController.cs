using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lifecycle controller, manages object lifecycle and client boot/reboot sequence.
/// This class subscribes to the OnInstantiate event fired by IInstantiator. When
/// an object implementing ILifeycleAware is instantiated, it will be stored for
/// later lifecycle operations. Upon client Reboot(), this class will invoke
/// Reset() on each ILifecycleAware instance, in reverse instantiation order.
/// </summary>
public class LifecycleController : MonoBehaviour, IInitializable, ILoggable {
	
	private static readonly string GAME_OBJECT_NAME_PREFIX = "_"; 
	
	// Only the IInstantiator and IInjector should be injected as dependencies as
	// IInstantiator.OnInstantiate must be bound to track other objects for reset purposes
	[Inject]
	private IInstantiator _instantiator;
	
	[Inject]
	private IInjector _injector;
	
	// The following dependencies will be assigned just in time via providers
	[Inject]
	private IProvider<Client> _clientProvider;

	[Inject]
	private IProvider<Config> _configProvider;   

	[Inject]
	private IProvider<CoroutineCreator> _coroutineCreatorProvider;

	[Inject]
	private IProvider<StateController> _stateControllerProvider;
/*
	[Inject]
	private IProvider<SystemMessageController> _systemMessageControllerProvider;
*/
	// Event fired immediately before application reset, to allow subscribers to
	// prepare for Reset() ( analogy: Awake() vs Start() )
	// 
	// (OnReset invoke order across subscriber instances is not deterministic, whereas the Reset()
	// message sent to all ILifecycleAware instances is invoked on each instance in exact
	// reverse order of registration, essentially "unwinding" the application flow in time)
	// 
	private event Action _onReset;
	public event Action OnReset { // Event handler subscription should be idempotent
		add     { _onReset -= value;  _onReset += value; }
		remove  { _onReset -= value; }
	}
	
	private event Action _onSoftReset;
	public event Action OnSoftReset { // Event handler subscription should be idempotent
		add     { _onSoftReset -= value;  _onSoftReset += value; }
		remove  { _onSoftReset -= value; }
	}
	
	// Event fired immediately before application exit
	private event Action _onExit;
	public event Action OnExit { // Event handler subscription should be idempotent
		add     { _onExit -= value;  _onExit += value; }
		remove  { _onExit -= value; }
	}
	
	private bool _resetInProgress;
	public bool ResetInProgress {
		get { return _resetInProgress; }
	}
	
	private bool _clientInitializing = true;
	private long _clientStartupTimestamp = 0L;
	
	// Instances registered for lifecycle management
	private LinkedHashSet<ILifecycleAware> _registeredInstances = new LinkedHashSet<ILifecycleAware>();
	
	[PostConstruct]
	void PostConstruct() {
		// Set defaults
		_resetInProgress = false;
		
		// Register the Injector first for lifecycle management. It will be the last instance Reset().
		Register( _injector );    
		
		// Subscribe to OnInstantiate event. Instantiations that occur after this
		// subscription will will be monitored for lifecycle management
		_instantiator.Subscribe( HandleInstantiateEvent );

		_clientInitializing = true;
		_clientStartupTimestamp = 0L;
		
	}
	
	public void Reset() {
		// Clear event subscriptions
		_onReset = null;
		_onExit = null;
		// Forget all instances that have been previously registered
		_registeredInstances.Clear();
	}
	
	public void Initialize( InstanceInitializedCallback initializedCallback = null ) {
		this.LogTrace("Initialize()", LogCategory.INITIALIZATION);
		_stateControllerProvider.Get().Boot();
		// Currently unused, but provided for consistency if future refactoring requires async initialization
		if ( initializedCallback != null ) initializedCallback( this );
	}
	
	private void ResetGlobalState() {
		Resources.UnloadUnusedAssets(); //Force unity to unload assets
		GC.Collect(); // Now is a good time for garbage collection
	}
	
	public void BroadcastReset() {        
		this.LogTrace("BroadcastReset() start");
		
		// Block instantiation and registration that may inadvertently
		// be attempted by Reset() in one of our registered instances
		_resetInProgress = true;
		_instantiator.SetResetInProgress( _resetInProgress );
		_coroutineCreatorProvider.Get().SetResetInProgress( _resetInProgress );
		
		if ( _onReset != null ) {
			this.LogTrace("Firing OnReset() ...");
			_onReset();
		}
		
		// Reset the registered instances in the reverse order they were registered.
		// This returns a copy of the underlying LinkedList, so we can modify the original
		// list without issue.
		foreach( ILifecycleAware targetInstance in _registeredInstances.GetListReversed() ) {
			this.LogTrace("Reset() " + targetInstance.GetName() );			
			try { // We make a best effort to reset all instances, even if Reset() throws an exception in a registered instance
				targetInstance.Reset();
			} catch ( Exception e ) {
				this.LogError("Exception in Reset() on '" + targetInstance.GetName() + "': " + e.ToString() );
			}
		} 
		
		// Apply any global unloading logic        
		ResetGlobalState();
		
		_resetInProgress = false;
		_instantiator.SetResetInProgress( _resetInProgress );
		
		this.LogTrace("BroadcastReset() end");
		this.LogTrace("~".Repeat(80));  
	}
	
	public void Info() {
		foreach( ILifecycleAware instance in _registeredInstances ) {
			string instanceName = instance.GetName();
			UnityEngine.Object unityObject = instance as UnityEngine.Object;
			if ( unityObject != null ) { instanceName += " @ " + unityObject.GetInstanceID(); }
			this.LogTrace( instanceName );
		}
		this.LogTrace(_registeredInstances.Count + " registered instance(s)");
	}
	
	private bool RegisterInternal<T>( T instance ) where T : ILifecycleAware {
		return _registeredInstances.Add( instance );
	}
	
	public T Register<T> ( T instance ) {
		// Registration is ignored during reset
		if ( _resetInProgress ) return instance;
		
		ILifecycleAware resettableInstance = instance as ILifecycleAware;
		// If instance does not implement ILifecycleAware, bail.
		if ( resettableInstance == null ) return instance; 
		
		RegisterInternal( resettableInstance );
		return instance;
	}
	
	public T Add<T>( string gameObjectName ) where T : MonoBehaviour, new() {
		return _instantiator.New<T>( gameObjectName );
	}
	
	public T Add<T>() where T : MonoBehaviour, new() {
		return _instantiator.New<T>();
	}   
	
	public T New<T>() where T : new() {
		return _instantiator.New<T>();
	}
	
	/// <summary>
	/// Destroy the specified target. Note that this internally calls
	/// UnityEngine.Object.Destroy(), which delays the actual destroy
	/// until the end of the frame.
	/// </summary>
	/// <param name="target">Target.</param>
	new public void Destroy( UnityEngine.Object target ) {	
		try {
			if ( target is MonoBehaviour ) { // Destroy parent GameObject, which will also destroy all sibling components            
				UnityEngine.Object.Destroy( ((MonoBehaviour)target).gameObject );
			} else { // Else, destroy the object itself            
				UnityEngine.Object.Destroy( target );
			}
		} catch (Exception e) {
			Log.Error("Destroy() exception on target '" + target + "': " + e.ToString() );
		}
	}
	
	private static string GameObjectNameForType<T>() {
		return GAME_OBJECT_NAME_PREFIX + typeof(T).ToString();  
	}
	
	private void HandleInstantiateEvent( object sender, OnInstantiateArgs args ) {
		Register( args.GetInstance() );
	}
	
	public void SubscribeOnReset( Action handler ) {
		_onReset += handler;
	}
	
	public void UnsubscribeOnReset( Action handler ) {
		_onReset -= handler;
	}
	
	public void SubscribeOnSoftReset( Action handler ) {
		_onSoftReset += handler;
	}
	
	public void UnsubscribeOnSoftReset( Action handler ) {
		_onSoftReset -= handler;
	}
	
	public void SubscribeOnExit( Action handler ) {
		_onExit += handler;
	}
	
	public void UnsubscribeOnExit( Action handler ) {
		_onExit -= handler;
	}
	
	/// <summary>
	/// Reboots the client.
	/// </summary>
	public void Reboot() {
		this.LogTrace("~".Repeat(80)); 
		this.LogTrace("Rebooting client ...");

        string sceneName = "Main";
        if (_configProvider.Get().GetClientRebootViaEmptyScene()) {
            sceneName = "Reboot";
        }
		
		BroadcastReset();
		StartCoroutine( BootViaSceneLoad(sceneName) );
	}

	/// <summary>
	/// Boots the game via reloading the main scene.
	/// ( Async, to allow destroyed objects, if any, to be reclaimed ) 
	/// </summary>
	private IEnumerator BootViaSceneLoad(string sceneName) {
		this.LogTrace("BootViaSceneLoad()");
		// wait one frame for all Destroy()ed objects to be reclaimed
		yield return null;
		// Reset/Destroy LifecycleController state
		Reset();
		this.DestroyAll();
		// Reload main scene
		SceneManager.LoadScene( sceneName );
	}
	
	// Attempt soft reboot, else hard reboot
	public void SoftReboot() {
		if ( _stateControllerProvider.Get().CanSoftBoot() ) {
			BroadcastSoftReset();
			_stateControllerProvider.Get().SoftBoot();
			return;
		}        
		Reboot();
	}
	
	private void BroadcastSoftReset() {
		this.LogTrace("BroadcastReset() start");
		if (_onSoftReset != null) {
			_onSoftReset();
		}
	}
	
	/// <summary>
	/// Exits the client, terminating the game process.
	/// </summary>
	public void Exit() {
		this.LogTrace("ExitClient()");
		
		if ( _clientProvider.Get().GetEditorModeEnabled() ) {
			ExitUtil.ExitEditor();
			return;
		} 
		
		// Fire onExit event
		if ( _onExit != null ) _onExit();
		
		if ( ( ( _clientProvider.Get().GetPlatform() == Platform.iOS ) && _configProvider.Get().GetClientCleanExitIosEnabled() ) ||
		    ( ( _clientProvider.Get().GetPlatform() == Platform.Android ) && _configProvider.Get().GetClientCleanExitAndroidEnabled() ) ) {
			// If clean exit is enabled, allow Unity to cleanly shut down.
			ExitUtil.CleanExit();
			return;
		} 
		
		// On fall-through, we hard exit by killing the process
		ExitUtil.HardExit();
	}
	
	public void Restart() {
        //_systemMessageControllerProvider.Get().ShowClientRestartMessage();
		Reboot();
	}
	
	/// <summary>
	/// Quit game with confirmation, or exit, depending on configuration
	/// </summary>
	public void Quit() {
		Exit();
/*
		if ( _configProvider.Get().GetClientQuitConfirmationEnabled() ) {
			_systemMessageControllerProvider.Get().ShowQuitConfirmMessage(
				Exit, 	// yes
				null 	// no
				);
		} else {
			Exit();
		}
*/
	}
	
	public void ClientInitComplete()
	{
		_clientInitializing = false;
	}
	
	public bool IsClientInitializing()
	{
		return _clientInitializing;
	}
	
	// Startup timestamp should be zero during initialization to guarantee that the server will accept the requests.
	public long GetClientStartupTimestamp()
	{
		return _clientInitializing ? 0 : _clientStartupTimestamp;
	}
	
	public void SetClientStartupTimestamp( long clientStartupTimestamp )
	{
		_clientStartupTimestamp = clientStartupTimestamp;
	}
}


