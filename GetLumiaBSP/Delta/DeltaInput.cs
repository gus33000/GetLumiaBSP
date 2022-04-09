using System;

namespace LibSxS.Delta
{
    public struct DeltaInput
    {
        public IntPtr lpStart { get; set; }
        public IntPtr uSize { get; set; }
        public bool Editable { get; set; }
    }
}
