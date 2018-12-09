using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetLumiaBSP
{
    class WlanInfHandler
    {
        public static void GenInfProperly(string QCWLANSYS, string QCWLANDAT)
        {
            Console.WriteLine("(wlanCare) Copying files...");

            Directory.CreateDirectory("Wlan");
            File.Move(QCWLANSYS, @"Wlan\" + QCWLANSYS);
            File.Move(QCWLANDAT, @"Wlan\" + QCWLANDAT);

            if (QCWLANSYS.Contains("8974"))
                File.Copy(@"Care\WLANCare\qcwlan8974.inf", @"Wlan\qcwlan8974.inf");
            else
                File.Copy(@"Care\WLANCare\qcwlan8626.inf", @"Wlan\qcwlan8974.inf");

            Console.WriteLine("(wlanCare) Done.");
        }
    }
}
