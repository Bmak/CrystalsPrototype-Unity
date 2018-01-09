using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Time info provides access to periodically updated timestamp
/// values, instead of computing them repeatedly. Note that values
/// returned by these methods may be inaccurate by TIMESTAMP_UPDATE_INTERVAL
/// seconds.
/// The non-cached values are also exposed through "immediate" values.
/// </summary>
public class TimeInfo : MonoBehaviour, ILifecycleAware, ILoggable {

    private static readonly int UNSET = -1;
    private static readonly float TIMESTAMP_UPDATE_INTERVAL = 1.0f;

    private TimeSpan _currentTimeSpan;

    private long _startupTimestamp =                UNSET;
    private long _currentTimestamp =                UNSET;
    private long _currentTimestampMilliseconds =    UNSET;
    private float _realTimeSinceStartup =           UNSET;
    private long _ServerTimeDrift =                 default(long);
    
    public void Reset() {
        this.StopAllCoroutines();
        this.DestroyAll();
    }

    public void Initialize() {
        // Guard against repeated initialization
        if ( _startupTimestamp > UNSET ) return;

        StartCoroutine( UpdateValues() );
    }

    public void UpdateServerTimestamp(long timestamp)
    {
        _ServerTimeDrift = timestamp - GetImmediateCurrentTimestamp();
    }

    /// <summary>
    /// This returns the immediate timestamp of the server, not the cached version.
    /// </summary>
    /// <returns></returns>
    public long GetImmediateServerAdjustedCurrentTime()
    {
        return GetImmediateCurrentTimestamp() + _ServerTimeDrift;
    }

    /// <summary>
    /// This returns the cached copy of the current server's Unix timestamp in seconds (integer),
    /// updated each second.
    /// </summary>
    /// <returns></returns>
    public long GetServerAdjustedCurrentTime()
    {
        return GetCurrentTimestamp() + _ServerTimeDrift;
    }

    /// <summary>
    /// Returns the current Unix UTC timestamp in seconds (integer)
    /// </summary>
    /// <returns>Unix UTC timestamp as long</returns>
    private long GetImmediateCurrentTimestamp()
    {
        return (long)TimeUtil.GetCurrentTimeSpan().TotalSeconds;
    }

    /// <summary>
    /// Returns cached copy of the current Unix UTC timestamp in seconds (integer),
    /// updated each second.
    /// </summary>
    /// <returns>Unix UTC timestamp as long</returns>
    public long GetCurrentTimestamp()
    {
        return _currentTimestamp;
    }

    /// <summary>
    /// Returns cached copy of the current Unix UTC timestamp in milliseconds (integer),
    /// updated each second.
    /// </summary>
    /// <returns>Unix UTC timestamp as long</returns>
    public long GetCurrentTimestampMS()
    {
        return _currentTimestampMilliseconds;
    }

    public float GetRealTimeSinceStartup() {
        return _realTimeSinceStartup;
    }

    public long GetStartupTimestamp() {
        return _startupTimestamp;
    }

    /// <summary>
    /// Returns the total seconds until DateTime, negative value if time has passed
    /// </summary>
    public long GetTimeUntilInSeconds(long destinationTimestamp)
    {
        return (destinationTimestamp - GetServerAdjustedCurrentTime());
    }

    private IEnumerator UpdateValues() {

        _currentTimeSpan = TimeUtil.GetCurrentTimeSpan();
        _startupTimestamp = (long)_currentTimeSpan.TotalSeconds;

        while ( true ) {
            // Update the cached time, accurate to TIMESTAMP_UPDATE_INTERVAL seconds
            _currentTimestamp = (long)_currentTimeSpan.TotalSeconds;
            _currentTimestampMilliseconds = (long)_currentTimeSpan.TotalMilliseconds;
            _realTimeSinceStartup = (float)( _currentTimestamp - _startupTimestamp );

            yield return new WaitForSeconds( TIMESTAMP_UPDATE_INTERVAL );
            // Update timeSpan for next iteration            
            _currentTimeSpan = TimeUtil.GetCurrentTimeSpan();
        }
    }

}
