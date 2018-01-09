using System;

/// <summary>
/// Providers that implement this interface are expected to create/load T through asynchronous means
/// and once T has been created/loaded will return T to the requestor through the finish callback
/// </summary>
public interface IProviderAsync<T>
{
    void Get(string name = null, string objectName = null, Action<T> finishCallback = null);
}
