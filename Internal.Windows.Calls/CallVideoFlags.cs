namespace Internal.Windows.Calls
{
    public enum CallVideoFlags : uint
    {
        None = 0,
        Transmit = 1,
        Receive = 2,
        Paused = 4,
        TemporarilyUnavailable = 8
    }
}