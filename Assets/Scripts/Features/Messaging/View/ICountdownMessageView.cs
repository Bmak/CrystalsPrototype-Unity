using System;

/// <summary>
/// An interface which allows interaction with a message view that has an update timer on it
/// </summary>
public interface ICountdownMessageView
{
    /// <summary>
    /// Kicks off a coroutine which will call the passed in function.
    /// The passed in function should return a bool for whether the coroutine should continue firing
    /// </summary>
    void StartUpdateFunction(Func<ICountdownMessageView, bool> updateAction, Action timerCompleteAction = null);

    void SetMessage(string message);

    void SetTitle(string title);
}
