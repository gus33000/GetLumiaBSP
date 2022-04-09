using LibSxS.Native;

namespace LibSxS.Delta
{
    public struct DeltaHeaderInfo
    {
        public DeltaFileType FileTypeSet;
        public DeltaFileType FileType;
        public DeltaFlags Flags;
        public uint TargetSize;
        public FileTime TargetFileTime;
        public uint TargetHashAlgId;
        public DeltaHash TargetHash;
    }
}
