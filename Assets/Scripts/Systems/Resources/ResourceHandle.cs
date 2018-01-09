using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceHandle : ILoggable
{
    protected readonly string _name;
    public string Name
    {
        get { return _name; }
    }

    protected readonly bool _isArchetype;
    public bool IsArchetype
    {
        get { return _isArchetype; }
    }


    /// <summary>
    /// Reference count used to determine eligibility for resource eviction. 
    /// Each time a ResourceHandle is requested and returned by the ResourceCache,
    /// its refCount will be incremented. Each time ResourceHandle.Release() is
    /// invoked, its refCount will be decremented. Resources in the cache with
    /// refCount == 0 are considered eligible for eviction. Mutable resources
    /// (prefabs/gameobjects) should never have a refCount over one. Immutable
    /// resources (textures, etc) may have a refCount that is any positive integer. 
    /// </summary>
    protected int _refCount = 0;
    public int RefCount
    {
        get { return _refCount; }
    }

    public bool Available
    {
        get { return _refCount <= 0 && _loadComplete; }
    }

    protected UnityEngine.Object _resource;

    // Raw object which may or may not be based on GameObject
    public UnityEngine.Object Resource
    {
        get { return _resource; }
    }
    // Convenience cast for those expecting GameObject based resources
    public GameObject GO
    {
        get { return _resource as GameObject; }
    }

    protected bool _loadComplete;
    public bool LoadComplete
    {
        get { return _loadComplete; }
    }

    public ResourceHandle( string name, bool isArchetype = false )
    {
        _name = name;        
        _isArchetype = isArchetype;
    }

    /// <summary>
    /// DO NOT USE OUTSIDE OF THE RESOURCE CACHE SYSTEM
    /// Increments the reference count.
    /// Used when the resource is being pulled out of the cache for use
    /// so the Resource Cache system can keep track of whether the
    /// resource can be evicted or not. refCount > 0 means it can't be evicted.
    /// </summary>
    public virtual void _acquire()
    {
        // This handle has been requested but is not currently in use.
        if ( !_isArchetype && ( _refCount <= 0 ) ) {
            // De-parent the resource on acquire if this object is not yet in use
            SetTransformParent( reset: true );
        }

        ++_refCount;
    }

    /// <summary>
    /// Reverts the resource to a known good state so that it 
    /// can be given out to another caller. This is only meaningful
    /// for mutable resources such as prefabs/gameobjects.
    /// </summary>
    public virtual void Clean() {}

    /// <summary>
    /// Disables resource. This is only meaningful for mutable resources. 
    /// This method must be idempotent.
    /// </summary>
    public virtual void Disable()
    {
        if ( GO == null ) return;
        GO.SetActive( false );
    }

    /// <summary>
    /// Enables resource. This is only meaningful for mutable resources.
    /// This method must be idempotent.
    /// </summary>
    public virtual void Enable()
    {
        if ( GO == null ) return;
        GO.SetActive( true );
    }

	/// <summary>
	/// Forces Awake to be invoked by invoking Enable() on this ResourceHandle.
	/// Disable() is then invoked, so as to not result in side effects or inadvertently enabled objects.
	/// Do not invoke this method outside of the ResourceCache system.
	/// </summary>
	public virtual void _forceAwake()
	{
		Enable();
		Disable();
	}

    /// <summary>
    /// Destroys resource and reclaims any memory allocated by the resource.
    /// The ResourceCache invokes this method when evicting a resource from the cache.
    /// NOTE: The implementation of this method may be asynchronous (e.g. Destroy()),
    /// as any object being destroying will have a refCount == 0 and hence no valid
    /// reference to the resource should exist at time of destruction.
    /// </summary>
    public virtual void _destroy()
    {
        if ( _resource == null ) return;
        GameObject.Destroy(_resource);
        // Remove local reference to resource so that it can potentially  
        // be reclaimed via Resources.UnloadUnusedAssets()
        _resource = null;
    }

    /// <summary>
    /// Assigns the backing resource reference to this ResourceHandle.
    /// This method is invoked when a resource has been successfully
    /// loaded via an AssetLoader or was cloned from another resource.
    /// </summary>
    /// <param name="resource"></param>
    public virtual void _set( UnityEngine.Object instance )
    {
        if ( _resource != null ) {
            this.LogWarning("_set() invoked on handle that has already been populated, ignoring");
            return;
        }

        _resource = instance;
        _loadComplete = true;
        // This object was just created/cloned, so ensure it is de-parented for consistency
        SetTransformParent( reset: !_isArchetype );
    }

    /// <summary>
    /// Release this ResourceHandle, decrementing its refCount.
    /// </summary>
    /// <returns><c>true</c> if this instance was released/evicted, false if could not be released/evicted.</returns>
    public virtual bool Release( bool evict = false )
    {
        // Release should be idempotent. If this resource has already been released, 
        // exit early unless we are now evicting a previously released resource.
        if ( ( _refCount <= 0 ) && !evict ) return false; 

        if ( _refCount > 0 ) --_refCount;

        // Early exit if this is an archetype, as we do not cache Archetypes in the traditional manner
        if ( _isArchetype ) return true;

        // Parent the resource to the ResourceCache upon release if no longer in use.
        // No value in resetting the transform parent if we are evicting the resource.
        if ( ( _refCount <= 0 ) && !evict ) SetTransformParent();
		GameObject.Destroy(_resource);
		return true;// _internalResourceCache._releaseResource( this, evict );
    }

    private void SetTransformParent( bool reset = false ) {
        if ( GO == null ) return;
		GO.transform.parent = null;//reset ? null : _internalResourceCache.GetResourceHandleTransformParent();
    }
}