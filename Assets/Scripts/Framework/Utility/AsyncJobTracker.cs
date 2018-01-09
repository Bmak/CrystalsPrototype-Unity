using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Utility class which will track a set number of asynchronous operations
/// and notify a specified success and failure callback once those operations
/// are completed.
/// 
/// The Success and Failure properties on this class should be used for the
/// success and failure callbacks on asynchronous operations that should
/// be tracked.
/// 
/// If one of the asynchronous jobs fails, the failure callback will be
/// notified and the tracker will stop tracking the remaining asynchronous
/// jobs.
/// </summary>
public class AsyncJobTracker
{
    /// <summary>
    /// Should be registered as the success callback for each
    /// asynchronous job that is being tracked.
    /// </summary>
    private Action _success;
    public Action Success
    {
        get { return _success; }
    }

    /// <summary>
    /// Should be registered as the failure callback for each
    /// asynchronous job that is being tracked.
    /// </summary>
    private Action _failure;
    public Action Failure
    {
        get { return _failure; }
    }

    private Action _failureProxy;

    private Action _successCallback;
    private Action _failureCallback;

    private bool _jobsFailed;
    private int _jobsRemaining;

    /// <summary>
    /// Constructs a new async job tracker instance given the number of jobs to track.
    /// </summary>
    /// 
    /// <param name="numberOfJobs">The number of jobs to track. Upon
    /// receiving this number of success callbacks, the tracker will call
    /// it's success callback.</param>
    /// <param name="successCallback">Called once all tracked jobs succeed.</param>
    /// <param name="failureCallback">Called at the first failed tracked
    /// job. May be null.</param>
    public AsyncJobTracker(int numberOfJobs, Action successCallback, Action failureCallback)
    {
        _successCallback = successCallback;
        _failureCallback = failureCallback;

        _jobsRemaining = numberOfJobs;

        _success = OnSuccess;
        _failure = OnFailure;
    }

    private void OnSuccess()
    {
        if (_jobsFailed)
        {
            return;
        }

        _jobsRemaining--;
        if (_jobsRemaining == 0)
        {
            _successCallback();
        }
    }

    private void OnFailure()
    {
        if (_jobsFailed)
        {
            return;
        }

        _jobsFailed = true;
        
        if (_failureCallback != null)
        {
            _failureCallback();
        }
    }
}
