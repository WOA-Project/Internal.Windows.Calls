using System.Runtime.InteropServices;

namespace Internal.Windows.Calls.PhoneOm
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct OLITEMID
    {
        [FieldOffset(0x0)]
        public int field_0;
        [FieldOffset(0x4)]
        public int field_4;
        [FieldOffset(0x8)]
        public int field_8;
    }
}
