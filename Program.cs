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

using CommandLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GetLumiaBSP
{
    class Program
    {
        private static string[] directx = new string[]
        {
            "qcvss",
            "direct3d",
            "qcdx",
            "qcvid"
        };

        public static object Options;
        public static string OptionVerb;

        static void Main(string[] args)
        {
            Console.Title = "Get Lumia BSP Command Line Interface";

            CLIOptions baseOptions = new CLIOptions();
            if (!Parser.Default.ParseArguments(args,
               baseOptions,
               (s, o) =>
               {
                   OptionVerb = s;
                   Options = o;
               }))
            {
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            Assembly ass = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(ass.Location);

            Console.WriteLine("");

            Console.WriteLine(fvi.FileDescription + " " + ass.GetName().Version.ToString());
            Console.WriteLine(fvi.LegalCopyright);

            Console.WriteLine("");

            switch (OptionVerb)
            {
                case "secwim2wim":
                    {
                        var arg = (secwim2wimOptions)Options;

                        if (File.Exists(arg.path))
                            ENOSWPackageDownloader.ConvertSECWIM2WIM(arg.path, arg.output);

                        break;
                    }
                case "enoswdl":
                    {
                        var arg = (enoswdlOptions)Options;

                        ENOSWPackageDownloader.DownloadENOSWPackage(arg.rmcode, arg.path + "\\");

                        break;
                    }
                case "unbsp":
                    {
                        var arg = (unbspOptions)Options;

                        UnBSP(arg.path);

                        break;
                    }
                case "reg_enosw":
                    {
                        var arg = (RegOptions)Options;

                        RegWIM(arg.path, !arg.patcherno, !arg.dxcareno, !arg.wlancareno, !arg.mbbcareno, !arg.uartcareno, !arg.rtcareno, arg.signingcert, !arg.mergeno);

                        break;
                    }
                case "reg_mountedfs":
                    {
                        var arg = (RegOptions)Options;

                        RegMounted(arg.path, !arg.patcherno, !arg.dxcareno, !arg.wlancareno, !arg.mbbcareno, !arg.uartcareno, !arg.rtcareno, arg.signingcert, !arg.mergeno);

                        break;
                    }
                case "reg_hybrid":
                    {
                        var arg = (RegOptions2)Options;

                        Console.WriteLine("Comming soon!");

                        break;
                    }
                case "inf_enosw":
                    {
                        var arg = (InfOptions)Options;

                        Console.WriteLine("Comming soon!");

                        break;
                    }
                case "inf_mountedfs":
                    {
                        var arg = (InfOptions)Options;

                        Console.WriteLine("Comming soon!");

                        break;
                    }
            }

            Console.ReadKey();


        }

        public static void UnBSP(string imagepath)
        {
            //RegMounted(imagepath, false, false, false, false, false, false, null, true);

            foreach (var item in Directory.EnumerateFiles("root", "*", SearchOption.AllDirectories))
            {
                // Delete files
                var todelete = imagepath + string.Join("", item.Skip(4));
                Console.WriteLine(todelete);
                File.Delete(todelete);
            }

            var text = File.ReadAllText("import.reg").Replace(",\\\r\n", ",");
            var lines = text.Replace("\r", "").Split('\n').ToList();
            
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("="))
                    lines[i] = lines[i].Split('=')[0] + "=-";
            }

            File.Delete("import.reg");
            File.WriteAllLines("import.reg", lines);

            // Mount SOFTWARE and SYSTEM as RTSOFTWARE and RTSYSTEM
            // Import import.reg
            // Unmount

            Console.ReadKey();

            Directory.Delete("root", true);
            File.Delete("import.reg");
        }

        public static void RegWIM(string file, bool PatchCare, bool DXCareRT, bool WlanCareRT, bool MbbCareRT, bool UartCareRT, bool RTCare, string cert, bool merge)
        {
            ENOSWPackageDownloader.ConvertSECWIM2WIM(file, file + ".wim");

            Console.WriteLine("(preProcessing) Mounting ENOSW...");
            Directory.CreateDirectory("mount");
            System.Diagnostics.Process.Start("cmd.exe", "/c dism.exe /Mount-Image /ImageFile:" + file + ".wim /Index:1 /Mountdir:mount").WaitForExit();
            BSPExtractor.Program.HandlePackages("mount", "out");
            Console.WriteLine("(preProcessing) Unmounting ENOSW...");
            System.Diagnostics.Process.Start("cmd.exe", "/c dism.exe /Unmount-Image /Mountdir:mount /discard").WaitForExit();
            Directory.Delete("mount");
            File.Delete(file + ".wim");
            Console.WriteLine("(preProcessing) Done.");

            //
            // Patches
            //

            string regcontent = "Windows Registry Editor Version 5.00\r\n\r\n";

            if (PatchCare)
            {
                Console.WriteLine("(patcher) Starting enumeration...");
                var filesall = Directory.EnumerateFiles(@"out\Files", "*.*", SearchOption.AllDirectories);
                foreach (var stuff in filesall)
                {
                    bool patched;
                    var read = RTInstaller.HandleFile.GetProperBytes(stuff, out patched);
                    if (patched) File.WriteAllBytes(stuff, read);
                }

                Console.WriteLine("(patcher) Adding AnotherSuite patches to Registry...");

                regcontent += @"[HKEY_LOCAL_MACHINE\RTSYSTEM\ControlSet001\Control\ProductOptions]";
                regcontent += "\r\n";
                regcontent += "\"AnotherSuite\"=hex(7):50,00,68,00,6f,00,6e,00,65,00,4e,00,54,00,00,00,00,00";
                regcontent += "\r\n\r\n";
            }

            //
            // DXCare section
            //

            if (DXCareRT) DXCare();

            //
            // WlanCare section
            //

            if (WlanCareRT) WlanCare();

            //
            // MbbCare section
            //

            if (MbbCareRT) MbbCare();

            //
            // UartCare section
            //

            if (UartCareRT) UartCare();

            //
            // Merging section
            //

            if (merge)
            {
                var dirsfi = Directory.EnumerateDirectories(@"out\Files");
                foreach (var dir in dirsfi)
                {
                    Console.WriteLine("(processing) Merging " + dir + "...");
                    CopyDir(dir, "root");
                    Directory.Delete(dir, true);
                }

                Directory.Delete(@"out\Files", true);

                var regsfi = Directory.EnumerateFiles(@"out\Registry");
                foreach (var reg in regsfi)
                {
                    Console.WriteLine("(processing) Merging " + reg + "...");
                    var lines = String.Join("\r\n", File.ReadLines(reg).Skip(1).ToArray()) + "\r\n";
                    regcontent += lines;
                    File.Delete(reg);
                }

                Console.WriteLine("(processing) Committing Registry file...");
                File.WriteAllText("import.reg", regcontent);
                Directory.Delete(@"out", true);

            }

            Console.WriteLine("(processing) Done.");

            // Prepackage RT missing drivers (RTCare)
            // Presign files
        }

        public static void RegMounted(string firmwarepath, bool PatchCare, bool DXCareRT, bool WlanCareRT, bool MbbCareRT, bool UartCareRT, bool RTCare, string cert, bool merge)
        {
            BSPExtractor.Program.HandlePackages(firmwarepath, "out");
            Console.WriteLine("(preProcessing) Done.");

            //
            // Patches
            //

            string regcontent = "Windows Registry Editor Version 5.00\r\n\r\n";

            if (PatchCare)
            {
                Console.WriteLine("(patcher) Starting enumeration...");
                var filesall = Directory.EnumerateFiles(@"out\Files", "*.*", SearchOption.AllDirectories);
                foreach (var stuff in filesall)
                {
                    bool patched;
                    var read = RTInstaller.HandleFile.GetProperBytes(stuff, out patched);
                    if (patched) File.WriteAllBytes(stuff, read);
                }

                Console.WriteLine("(patcher) Adding AnotherSuite patches to Registry...");

                regcontent += @"[HKEY_LOCAL_MACHINE\RTSYSTEM\ControlSet001\Control\ProductOptions]";
                regcontent += "\r\n";
                regcontent += "\"AnotherSuite\"=hex(7):50,00,68,00,6f,00,6e,00,65,00,4e,00,54,00,00,00,00,00";
                regcontent += "\r\n\r\n";
            }

            //
            // DXCare section
            //

            if (DXCareRT) DXCare();

            //
            // WlanCare section
            //

            if (WlanCareRT) WlanCare();

            //
            // MbbCare section
            //

            if (MbbCareRT) MbbCare();

            //
            // UartCare section
            //

            if (UartCareRT) UartCare();

            if (RTCare) RtCare();

            //
            // Merging section
            //

            if (merge)
            {
                var dirsfi = Directory.EnumerateDirectories(@"out\Files");
                foreach (var dir in dirsfi)
                {
                    Console.WriteLine("(processing) Merging " + dir + "...");
                    CopyDir(dir, "root");
                    Directory.Delete(dir, true);
                }

                Directory.Delete(@"out\Files", true);

                var regsfi = Directory.EnumerateFiles(@"out\Registry");
                foreach (var reg in regsfi)
                {
                    Console.WriteLine("(processing) Merging " + reg + "...");
                    var lines = String.Join("\r\n", File.ReadLines(reg).Skip(1).ToArray()) + "\r\n";
                    regcontent += lines;
                    File.Delete(reg);
                }

                Console.WriteLine("(processing) Committing Registry file...");
                File.WriteAllText("import.reg", regcontent);
                Directory.Delete(@"out", true);

            }

            Console.WriteLine("(processing) Done.");

            // Prepackage RT missing drivers (RTCare)
            // Presign files
        }

        public static void RtCare()
        {
            string banned = "Customizations";
            var dirs = Directory.EnumerateDirectories(@"out\Files");
            var regs = Directory.EnumerateFiles(@"out\Registry");

            foreach (var dir in dirs)
                if (dir.ToLower().Contains(banned.ToLower()))
                {
                    Console.WriteLine("(rtCare) Deleting " + dir + "...");
                    Directory.Delete(dir, true);
                }
            foreach (var reg in regs)
                if (reg.ToLower().Contains(banned.ToLower()))
                {
                    Console.WriteLine("(rtCare) Deleting " + reg + "...");
                    File.Delete(reg);
                }
        }

        public static void WlanCare()
        {
            var WlanFile = "";
            var WlanDatFile = "";

            var dirs = Directory.EnumerateDirectories(@"out\Files");
            foreach (var dir in dirs)
            {
                bool wlanrel = false;
                if (dir.ToLower().Contains("wlan"))
                {
                    wlanrel = true;
                }
                if (wlanrel)
                {
                    var wlan8974 = Directory.EnumerateFiles(dir, "qcwlan8*74.sys", SearchOption.AllDirectories);
                    var wlan8626 = Directory.EnumerateFiles(dir, "qcwlan8*26.sys", SearchOption.AllDirectories);
                    var wlandat = Directory.EnumerateFiles(dir, "qcwlan*cfg.dat", SearchOption.AllDirectories);

                    if (wlan8974 != null)
                    {
                        foreach (var itm in wlan8974)
                        {
                            WlanFile = itm.Split('\\').Last();
                        }
                    }

                    if (wlan8626 != null)
                    {
                        foreach (var itm in wlan8626)
                        {
                            WlanFile = itm.Split('\\').Last();
                        }
                    }

                    if (wlandat != null)
                    {
                        foreach (var itm in wlandat)
                        {
                            WlanDatFile = itm.Split('\\').Last();
                        }
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(WlanFile) && !string.IsNullOrEmpty(WlanDatFile))
            {
                File.Copy(WlanFile, WlanFile.Split('\\').Last());
                File.Copy(WlanDatFile, WlanDatFile.Split('\\').Last());
            }

            foreach (var dir in dirs)
            {
                bool wlanrel = false;
                if (dir.ToLower().Contains("wlan"))
                {
                    wlanrel = true;
                }
                if (wlanrel)
                {
                    Console.WriteLine("(wlanCare) Deleting " + dir + "...");
                    Directory.Delete(dir, true);
                }
            }

            if (!string.IsNullOrEmpty(WlanFile) && !string.IsNullOrEmpty(WlanDatFile))
            {
                WlanInfHandler.GenInfProperly(WlanFile, WlanDatFile);
            }
        }

        public static void MbbCare()
        {
            var MbbFile = "";
            string MbbReg = "";

            var dirs = Directory.EnumerateDirectories(@"out\Files");
            foreach (var dir in dirs)
            {
                bool mbbrel = false;
                if (dir.ToLower().Contains("mbb") && !dir.ToLower().Contains("mbbuio"))
                {
                    mbbrel = true;
                }
                if (mbbrel)
                {
                    var mbb = Directory.EnumerateFiles(dir, "qcmbb*", SearchOption.AllDirectories);

                    if (mbb != null)
                    {
                        foreach (var itm in mbb)
                        {
                            MbbFile = itm.Split('\\').Last();
                            Console.WriteLine("(mbbCare) Copying QCMBB...");
                            File.Copy(itm, itm.Split('\\').Last());
                        }
                    }
                    Console.WriteLine("(mbbCare) Deleting " + dir + "...");
                    Directory.Delete(dir, true);
                }
            }

            if (!string.IsNullOrEmpty(MbbFile))
            {
                var regs = Directory.EnumerateFiles(@"out\Registry");
                foreach (var reg in regs)
                {
                    bool mbbrel = false;
                    if (reg.ToLower().Contains("mbb") && !reg.ToLower().Contains("mbbuio"))
                    {
                        mbbrel = true;
                    }
                    if (mbbrel)
                    {
                        MbbReg += File.ReadAllText(reg);
                        Console.WriteLine("(mbbCare) Deleting " + reg + "...");
                        File.Delete(reg);
                    }
                }

                MbbInfHandler.GenInfProperly(MbbReg, MbbFile);
            }
        }

        public static void UartCare()
        {
            var UartFile = "";
            string UartReg = "";

            var dirs = Directory.EnumerateDirectories(@"out\Files");
            foreach (var dir in dirs)
            {
                bool uartrel = false;
                if (dir.ToLower().Contains("qualcomm_uart"))
                {
                    uartrel = true;
                }
                if (uartrel)
                {
                    var uart = Directory.EnumerateFiles(dir, "qcuart*", SearchOption.AllDirectories);

                    if (uart != null)
                    {
                        foreach (var itm in uart)
                        {
                            UartFile = itm.Split('\\').Last();
                            Console.WriteLine("(uartCare) Copying QCUART...");
                            File.Copy(itm, itm.Split('\\').Last());
                        }
                    }
                    Console.WriteLine("(uartCare) Deleting " + dir + "...");
                    Directory.Delete(dir, true);
                }
            }

            if (!string.IsNullOrEmpty(UartFile))
            {
                var regs = Directory.EnumerateFiles(@"out\Registry");
                foreach (var reg in regs)
                {
                    bool uartrel = false;
                    if (reg.ToLower().Contains("qualcomm_uart"))
                    {
                        uartrel = true;
                    }
                    if (uartrel)
                    {
                        UartReg += File.ReadAllText(reg);
                        Console.WriteLine("(uartCare) Deleting " + reg + "...");
                        File.Delete(reg);
                    }
                }

                UartInfHandler.GenInfProperly(UartReg, UartFile);
            }
        }

        public static void DXCare()
        {
            string DXReg = "";
            var dxfile = "";
            var vssfile = "";

            var dirs = Directory.EnumerateDirectories(@"out\Files");
            foreach (var dir in dirs)
            {
                bool dxrel = false;
                foreach (var itm in directx)
                    if (dir.ToLower().Contains(itm.ToLower()))
                    {
                        dxrel = true;
                        break;
                    }
                if (dxrel)
                {
                    var vss = Directory.EnumerateFiles(dir, "qcvss*.mbn", SearchOption.AllDirectories);
                    var dx = Directory.EnumerateFiles(dir, "qcdxkm*.sys", SearchOption.AllDirectories);

                    if (vss != null)
                    {
                        foreach (var itm in vss)
                        {
                            vssfile = itm.Split('\\').Last();
                            Console.WriteLine("(dxCare) Moving QCVSS...");
                            File.Copy(itm, itm.Split('\\').Last());
                        }
                    }

                    if (dx != null)
                    {
                        foreach (var itm in dx)
                        {
                            dxfile = itm.Split('\\').Last();
                            Console.WriteLine("(dxCare) Moving QCDXKM...");
                            File.Copy(itm, itm.Split('\\').Last());
                        }
                    }
                }
            }

            var regs = Directory.EnumerateFiles(@"out\Registry");
            foreach (var reg in regs)
            {
                bool dxrel = false;
                foreach (var itm in directx)
                    if (reg.ToLower().Contains(itm.ToLower()))
                    {
                        dxrel = true;
                        break;
                    }
                if (dxrel)
                {
                    if (reg.ToLower().Contains("qcdxdriver"))
                        DXReg += File.ReadAllText(reg);
                }
            }

            foreach (var dir in dirs)
            {
                bool dxrel = false;
                foreach (var itm in directx)
                    if (dir.ToLower().Contains(itm.ToLower()))
                    {
                        dxrel = true;
                        break;
                    }
                if (dxrel)
                {
                    Console.WriteLine("(dxCare) Deleting " + dir + "...");
                    Directory.Delete(dir, true);
                }
            }

            foreach (var reg in regs)
            {
                bool dxrel = false;
                foreach (var itm in directx)
                    if (reg.ToLower().Contains(itm.ToLower()))
                    {
                        dxrel = true;
                        break;
                    }
                if (dxrel)
                {
                    Console.WriteLine("(dxCare) Deleting " + reg + "...");
                    File.Delete(reg);
                }
            }

            if (DXReg != "" && dxfile != "" && vssfile != "")
                DXInfHandler.GenInfProperly(DXReg, dxfile, vssfile);
        }

        public static void CopyDir(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            // Get Files & Copy
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);

                // ADD Unique File Name Check to Below!!!!
                string dest = Path.Combine(destFolder, name);

                File.Copy(file, dest);
            }

            // Get dirs recursively and copy files
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyDir(folder, dest);
            }
        }
    }
}
