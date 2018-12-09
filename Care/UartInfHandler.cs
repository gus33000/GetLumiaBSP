// Copyright (c) 2018, Gustave M. - gus33000.me - @gus33000
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

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
