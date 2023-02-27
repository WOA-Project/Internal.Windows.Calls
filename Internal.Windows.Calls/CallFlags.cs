using System;

namespace Internal.Windows.Calls
{
    [Flags]
    public enum CallFlags : uint
    {
        BreakthroughCall = 0x4,
        ShowKeypadOnCall = 0x20,
        ShowAssistedDialUI = 0x80,
        CallForwarded = 0x200,
        VoicemailCall = 0x4000,
    }
}
