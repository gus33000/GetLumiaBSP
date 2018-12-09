using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetLumiaBSP
{
    class MbbInfHandler
    {
        public static void GenInfProperly(string QCMbbReg, string QCMBB)
        {
            Console.WriteLine("(mbbCare) Finding informations about the Mobile broadband device...");
            
            string ID = "QCOMHWID";

            foreach (var line in QCMbbReg.Split('\n'))
            {
                if (line.ToLower().Contains("[hkey_local_machine\\rtsystem\\driverdatabase\\deviceids\\qcms\\"))
                    ID = line.Split('\\').Last().Replace("]", "").Replace("\n", "").Replace("\r", "");
            }

            Console.WriteLine("(mbbCare) Generating INF...");
            string inf = GetPrefilledInf(ID, QCMBB);

            Console.WriteLine("(mbbCare) Copying files...");

            Directory.CreateDirectory("Mbb");
            File.Move(QCMBB, @"Mbb\" + QCMBB);

            File.WriteAllText(@"Mbb\qcmbb.inf", inf);

            Console.WriteLine("(mbbCare) Done.");
        }

        public static string GetPrefilledInf(string ACPIID, string QCMBB)
        {
            var lines = File.ReadAllText(@"Care\MBBCARE\qcmbb.inf");

            lines = lines.Replace("!!HWID!!", ACPIID);
            lines = lines.Replace("!!MBBSYS!!", QCMBB);

            return lines;
        }
    }
}
