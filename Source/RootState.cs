namespace RootPattern
{
    /// <summary>
    /// Describes the lifecycle state of a root.
    /// </summary>
    public enum RootState
    {
        Created,
        Initializing,
        Initialized,
        InitializationFailed,
        Disposing,
        Disposed
    }
}
