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

namespace GetLumiaBSP
{
    internal class MbbInfHandler
    {
        public static void GenInfProperly(string QCMbbReg, string QCMBB)
        {
            Console.WriteLine("(mbbCare) Finding informations about the Mobile broadband device...");

            string ID = "QCOMHWID";

            foreach (string? line in QCMbbReg.Split('\n'))
            {
                if (line.ToLower().Contains("[hkey_local_machine\\rtsystem\\driverdatabase\\deviceids\\qcms\\"))
                {
                    ID = line.Split('\\').Last().Replace("]", "").Replace("\n", "").Replace("\r", "");
                }
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
            string? lines = File.ReadAllText(@"Care\MBBCARE\qcmbb.inf");

            lines = lines.Replace("!!HWID!!", ACPIID);
            lines = lines.Replace("!!MBBSYS!!", QCMBB);

            return lines;
        }
    }
}
