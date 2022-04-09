using System;

namespace LibSxS.Delta
{
    public struct DeltaOutput
    {
        public IntPtr pBuf { get; set; }
        public IntPtr cbBuf { get; set; }
    }
}
