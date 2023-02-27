using System;
using System.Runtime.InteropServices;

namespace Internal.Windows.Calls.PhoneOm
{
    /// <summary>
    /// Callback function to be used with PhoneAddListener.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint PH_CHANGE_EVENT_NOTIFY_FUNCTION(IntPtr phoneListener, IntPtr userData, ref PH_CHANGEEVENT eventType);
}
