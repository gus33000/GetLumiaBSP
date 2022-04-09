namespace LibSxS.Delta
{
    public unsafe struct DeltaHash
    {
        uint HashSize;
        fixed byte HashValue[32];
    }
}
