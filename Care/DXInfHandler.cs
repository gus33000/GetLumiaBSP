using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetLumiaBSP
{
    class DXInfHandler
    {
        public static void GenInfProperly(string QCDXKMReg, string QCDXKM, string QCVSS)
        {
            Console.WriteLine("(dxCare) Finding informations about the Adreno GPU...");

            string desc = "Qualcomm Adreno UNKNOWN";
            string ID = "QCOMHWID";

            foreach (var line in QCDXKMReg.Split('\n'))
            {
                if (line.ToLower().Contains("\"=\"qualcomm adreno "))
                    desc = "Qualcomm Adreno " + line.Split(' ').Last().Replace("\"", "").Replace("\n", "").Replace("\r", "");

                if (line.ToLower().Contains("[hkey_local_machine\\rtsystem\\driverdatabase\\deviceids\\acpi\\"))
                    ID = line.Split('\\').Last().Replace("]", "").Replace("\n", "").Replace("\r", "");
            }

            Console.WriteLine("(dxCare) Generating INF...");
            string inf = GetPrefilledInf(desc, ID, QCDXKM, QCVSS);

            Console.WriteLine("(dxCare) Copying files...");

            Directory.CreateDirectory("DirectX");
            File.Copy(@"Care\DXCare\qca3xxcompiler8974.DLL", @"DirectX\qca3xxcompiler8974.DLL");
            File.Copy(@"Care\DXCare\qcdx9um8974.dll", @"DirectX\qcdx9um8974.dll");
            File.Copy(@"Care\DXCare\qcmcumd8974.dll", @"DirectX\qcmcumd8974.dll");
            File.Copy(@"Care\DXCare\qcviddecmft8974.dll", @"DirectX\qcviddecmft8974.dll");
            File.Copy(@"Care\DXCare\qcvidencmfth2648974.dll", @"DirectX\qcvidencmfth2648974.dll");
            File.Copy(@"Care\DXCare\QcVidEncMftVC18974.dll", @"DirectX\QcVidEncMftVC18974.dll");
            File.Copy(@"Care\DXCare\qcvidum8974.DLL", @"DirectX\qcvidum8974.DLL");
            File.Move(QCDXKM, @"DirectX\" + QCDXKM);
            File.Move(QCVSS, @"DirectX\" + QCVSS);

            File.WriteAllText(@"DirectX\qcdx8974.inf", inf);

            Console.WriteLine("(dxCare) Done.");
        }

        public static string GetPrefilledInf(string DeviceDesc, string ACPIID, string QCDXKM, string QCVSS)
        {
            var lines = File.ReadAllText(@"Care\DXCARE\qcdx8974.inf");

            lines = lines.Replace("!!HWID!!", ACPIID);
            lines = lines.Replace("!!DXKM!!", QCDXKM);
            lines = lines.Replace("!!VSS!!", QCVSS);
            lines = lines.Replace("!!DEVDESC!!", DeviceDesc);

            return lines;
        }
    }
}
