namespace Internal.Windows.Calls
{
    public sealed class CallStateChangedEventArgs
    {
        public CallState OldState
        {
            get;
        }
        public CallState NewState
        {
            get;
        }
        public CallStateReason StateReason
        {
            get;
        }

        public CallStateChangedEventArgs(CallState old, CallState @new, CallStateReason reason)
        {
            OldState = old;
            NewState = @new;
            StateReason = reason;
        }
    }
}
