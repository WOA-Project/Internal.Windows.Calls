namespace Internal.Windows.Calls
{
    public enum CallState : int
    {
        Indeterminate = 0,
        Incoming = 1,
        Dialing = 2,
        ActiveTalking = 3,
        OnHold = 4,
        Disconnected = 5,
        Transferring = 6,
        Count = 7
    }
}
