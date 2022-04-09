using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace LibSxS.Delta
{
    public static class DeltaAPI
    {
        [DllImport("msdelta.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ApplyDeltaB(
           DeltaInputFlags ApplyFlags,
           DeltaInput Source,
           DeltaInput Delta,
           out DeltaOutput lpTarget);

        [DllImport("msdelta.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ApplyDeltaW(
           DeltaInputFlags ApplyFlags,
           string lpSourceName,
           string lpDeltaName,
           string lpTargetName);

        [DllImport("msdelta.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetDeltaInfoB(
           DeltaInput Delta,
           out DeltaHeaderInfo lpHeaderInfo);

        [DllImport("msdelta.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetDeltaInfoW(
           string lpDeltaName,
           out DeltaHeaderInfo lpHeaderInfo);

        [DllImport("msdelta.dll", SetLastError = true)]
        public static extern bool DeltaFree(IntPtr lpMemory);

        [DllImport("kernel32.dll")]
        private static extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, int NumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr lpOverlapped);

        public static unsafe DeltaHeaderInfo GetDeltaFileInformation(string path)
        {
            byte[] delta;
            DeltaHeaderInfo info;

            using (FileStream fStr = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream mStr = new MemoryStream((int)fStr.Length))
            {
                fStr.Position = 4;
                fStr.CopyTo(mStr);
                delta = mStr.ToArray();
            }

            fixed (byte* deltaPtr = delta)
            {
                DeltaInput deltaData = new DeltaInput()
                {
                    lpStart = new IntPtr(deltaPtr),
                    uSize = (IntPtr)delta.Length,
                    Editable = false
                };

                bool success = GetDeltaInfoB(deltaData, out info);
                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            return info;
        }

        public static string wcpBasePath;
        public static unsafe string GetManifest(string path)
        {
            byte[] source, delta, output;
            bool success = false;

            using (FileStream fStr = new FileStream(wcpBasePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream mStr = new MemoryStream((int)fStr.Length))
            {
                fStr.CopyTo(mStr);
                source = mStr.ToArray();
            }

            using (FileStream fStr = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream mStr = new MemoryStream((int)fStr.Length))
            {
                //fStr.Position = 4;
                byte[] compTest = new byte[4];
                fStr.Read(compTest, 0, 4);
                uint headerInt = BitConverter.ToUInt32(compTest, 0);
                if (headerInt == 0x3CBFBBEF || headerInt == 0x6D783F3C) //decompressed XML starts
                {
                    if (headerInt == 0x3CBFBBEF)
                        fStr.Position = 3;
                    else
                        fStr.Position = 0;
                    fStr.CopyTo(mStr);
                    mStr.Position = 0x69;
                    mStr.WriteByte(0x33);
                    mStr.Position = 0;
                    StreamReader reader = new StreamReader(mStr);
                    return reader.ReadToEnd();
                    //return null;
                }
                fStr.CopyTo(mStr);
                delta = mStr.ToArray();
            }

            fixed (byte* sourcePtr = source)
            fixed (byte* deltaPtr = delta)
            {
                DeltaInput sourceData = new DeltaInput()
                {
                    lpStart = new IntPtr(sourcePtr),
                    uSize = (IntPtr)source.Length,
                    Editable = false
                };

                DeltaInput deltaData = new DeltaInput()
                {
                    lpStart = new IntPtr(deltaPtr),
                    uSize = (IntPtr)delta.Length,
                    Editable = false
                };

                success = ApplyDeltaB(DeltaInputFlags.DELTA_FLAG_NONE, sourceData, deltaData, out DeltaOutput outData);
                if (!success) throw new Win32Exception(Marshal.GetLastWin32Error());

                output = new byte[outData.cbBuf.ToInt32()];
                Marshal.Copy(outData.pBuf, output, 0, output.Length);
                //for (int i = 0; i < output.Length; i++)
                //{
                //    output[i] = (byte)Marshal.PtrToStructure(outData.lpStart + i, typeof(byte));
                //}

                success = DeltaFree(outData.pBuf);
                if (!success) throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            using (MemoryStream mStr = new MemoryStream(output))
            {
                StreamReader reader = new StreamReader(mStr);
                return reader.ReadToEnd();
            }
        }

        public static unsafe void ApplyDelta(string basisPath, string patchPath, long patchOffset, int patchSize, string outputPath, bool allowPA19 = false)
        {
            byte[] source, delta;
            bool success = false;

            using (FileStream fStr = new FileStream(basisPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream mStr = new MemoryStream((int)fStr.Length))
            {
                fStr.CopyTo(mStr);
                source = mStr.ToArray();
            }

            using (FileStream fStr = new FileStream(patchPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            //using (MemoryStream mStr = new MemoryStream((int)fStr.Length))
            {
                //fStr.Position = 4;
                //fStr.CopyTo(mStr);
                //delta = mStr.ToArray();
                fStr.Position = patchOffset;
                delta = new byte[patchSize];
                fStr.Read(delta, 0, patchSize);
            }

            fixed (byte* sourcePtr = source)
            fixed (byte* deltaPtr = delta)
            {
                DeltaInput sourceData = new DeltaInput()
                {
                    lpStart = new IntPtr(sourcePtr),
                    uSize = (IntPtr)source.Length,
                    Editable = false
                };

                DeltaInput deltaData = new DeltaInput()
                {
                    lpStart = new IntPtr(deltaPtr),
                    uSize = (IntPtr)delta.Length,
                    Editable = false
                };

                success = ApplyDeltaB(allowPA19 ? DeltaInputFlags.DELTA_APPLY_FLAG_ALLOW_PA19 : DeltaInputFlags.DELTA_FLAG_NONE, sourceData, deltaData, out DeltaOutput outData);
                if (!success)
                {
                    sourceData = new DeltaInput()
                    {
                        lpStart = IntPtr.Zero,
                        uSize = IntPtr.Zero,
                        Editable = false
                    };
                    success = ApplyDeltaB(allowPA19 ? DeltaInputFlags.DELTA_APPLY_FLAG_ALLOW_PA19 : DeltaInputFlags.DELTA_FLAG_NONE, sourceData, deltaData, out outData);
                    if (!success)
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                //output = new byte[outData.cbBuf.ToInt32()];
                //Marshal.Copy(outData.pBuf, output, 0, output.Length);
                //for (int i = 0; i < output.Length; i++)
                //{
                //    output[i] = (byte)Marshal.PtrToStructure(outData.lpStart + i, typeof(byte));
                //}
                using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    if (!WriteFile(fs.Handle, outData.pBuf, outData.cbBuf.ToInt32(), out int written, IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                success = DeltaFree(outData.pBuf);
                if (!success) throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            //File.WriteAllBytes(outputPath, output);
        }

        public static unsafe DeltaHeaderInfo GetDeltaFilePartInformation(string path, long patchOffset, int patchSize)
        {
            byte[] delta;
            DeltaHeaderInfo info;

            using (FileStream fStr = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream mStr = new MemoryStream((int)fStr.Length))
            {
                fStr.Position = patchOffset;
                delta = new byte[patchSize];
                fStr.Read(delta, 0, patchSize);
            }

            fixed (byte* deltaPtr = delta)
            {
                DeltaInput deltaData = new DeltaInput()
                {
                    lpStart = new IntPtr(deltaPtr),
                    uSize = (IntPtr)delta.Length,
                    Editable = false
                };

                bool success = GetDeltaInfoB(deltaData, out info);
                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            return info;
        }
    }
}
