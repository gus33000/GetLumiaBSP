using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetLumiaBSP
{
    class UartInfHandler
    {
        public static void GenInfProperly(string QCUARTReg, string QCUART)
        {
            Console.WriteLine("(uartCare) Finding informations about the UART device...");
            
            string ID = "QCOMHWID";
            
            foreach (var line in QCUARTReg.Split('\n'))
            {
                if (line.ToLower().Contains("[hkey_local_machine\\rtsystem\\driverdatabase\\deviceids\\acpi\\"))
                    ID = line.Split('\\').Last().Replace("]", "").Replace("\n", "").Replace("\r", "");
            }

            Console.WriteLine("(uartCare) Generating INF...");
            string inf = GetPrefilledInf(ID, QCUART);

            Console.WriteLine("(uartCare) Copying files...");

            Directory.CreateDirectory("UART");
            File.Move(QCUART, @"UART\" + QCUART);

            File.WriteAllText(@"UART\qcuart.inf", inf);

            Console.WriteLine("(uartCare) Done.");
        }

        public static string GetPrefilledInf(string ACPIID, string QCUART)
        {
            var lines = File.ReadAllText(@"Care\UARTCARE\qcUART.inf");

            lines = lines.Replace("!!HWID!!", ACPIID);
            lines = lines.Replace("!!UARTSYS!!", QCUART);

            return lines;
        }
    }
}
