using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Internal.Windows.Calls
{
    internal static class Utils
    {
        public static unsafe IntPtr AllocateAndClear(int cb, byte fill = 0)
        {
            IntPtr result = Marshal.AllocHGlobal(cb);
            byte* cleaning = (byte*)result.ToPointer();
            for (int i0 = 0; i0 < cb; i0++)
            {
                cleaning[i0] = fill;
            }
            return result;
        }

        public static unsafe string ByteDump(void* ptr, int count, int split = 0)
        {
            StringBuilder result = new();
            byte* dumping = (byte*)ptr;
            for (int i0 = 0; i0 < count; i0++)
            {
                _ = result.Append(dumping[i0]);
                _ = result.Append(' ');
                if (split != 0 && split == i0)
                {
                    _ = result.Append('|');
                }
            }
            return result.ToString();
        }

        public static uint CallStateToOrder(this CallState state)
        {
            return state switch
            {
                CallState.Incoming => 0,
                CallState.Dialing => 1,
                CallState.ActiveTalking => 2,
                CallState.OnHold => 3,
                CallState.Transferring => 4,
                _ => 5,
            };
        }
    }
}
