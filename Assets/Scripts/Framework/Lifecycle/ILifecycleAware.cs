
/// <summary>
/// Implementers of this interface should sufficiently reset their internal state upon Reset().
/// Used to put objects back in a known clean state, e.g. for purposes of client restart.
/// </summary>

public interface ILifecycleAware {
    void Reset();
}

public enum ResetLevel {
    FULL = 0, // Entirely reset state, e.g. for a full reboot of the game
    REAUTH = 1 // Reset state for re-auth/account change
}

public static class LifecycleExtensions {

    public static string GetName( this ILifecycleAware caller ) {
        return caller.GetType().Name;
    }

}