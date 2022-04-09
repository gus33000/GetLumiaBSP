using System;

namespace LibSxS.Delta
{
    [Flags]
    public enum DeltaInputFlags : ulong
    {
        DELTA_FLAG_NONE = 0x0,
        DELTA_APPLY_FLAG_ALLOW_PA19 = 0x1
    }
}
