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
using System.Reflection;

namespace GetLumiaBSP
{
    internal class Program
    {
        private static readonly string[] directx = new string[]
        {
            "qcvss",
            "direct3d",
            "qcdx",
            "qcvid"
        };

        public static object Options;
        public static string OptionVerb;

        private static void PrintLogo()
        {
            Console.WriteLine($"GetLumiaBSP {Assembly.GetExecutingAssembly().GetName().Version} - Get Lumia BSP Command Line Interface");
            Console.WriteLine("Copyright (c) Gustave Monce and Contributors");
            Console.WriteLine("https://github.com/gus33000/GetLumiaBSP");
            Console.WriteLine();
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY.");
            Console.WriteLine("This is free software, and you are welcome to redistribute it under certain conditions.");
            Console.WriteLine();
        }

        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<SecWim2WimOptions, ENOSWDownloadOptions, InfOptions, UnBSPOptions, RegExtractionMainOSOptions, RegExtractionWimOptions>(args).MapResult(
              (SecWim2WimOptions arg) =>
              {
                  PrintLogo();
                  if (File.Exists(arg.path))
                  {
                      ENOSWPackageDownloader.ConvertSECWIM2WIM(arg.path, arg.output);
                  }

                  return 0;
              },
              (ENOSWDownloadOptions arg) =>
              {
                  PrintLogo();
                  ENOSWPackageDownloader.DownloadENOSWPackage(arg.rmcode, arg.path + "\\");
                  return 0;
              },
              (InfOptions arg) =>
              {
                  PrintLogo();
                  Console.WriteLine("Coming soon!");
                  return 0;
              },
              (UnBSPOptions arg) =>
              {
                  PrintLogo();
                  UnBSP(arg.path);
                  return 0;
              },
              (RegExtractionMainOSOptions arg) =>
              {
                  PrintLogo();
                  RegMounted(arg.path, !arg.patcherno, !arg.dxcareno, !arg.wlancareno, !arg.mbbcareno, !arg.uartcareno, !arg.rtcareno, arg.signingcert, !arg.mergeno);
                  return 0;
              },
              (RegExtractionWimOptions arg) =>
              {
                  PrintLogo();
                  RegWIM(arg.path, !arg.patcherno, !arg.dxcareno, !arg.wlancareno, !arg.mbbcareno, !arg.uartcareno, !arg.rtcareno, arg.signingcert, !arg.mergeno);
                  return 0;
              },
              errs => 1);
        }

        public static void UnBSP(string imagepath)
        {
            //RegMounted(imagepath, false, false, false, false, false, false, null, true);

            foreach (string? item in Directory.EnumerateFiles("root", "*", SearchOption.AllDirectories))
            {
                // Delete files
                string? todelete = imagepath + string.Join("", item.Skip(4));
                Console.WriteLine(todelete);
                File.Delete(todelete);
            }

            string? text = File.ReadAllText("import.reg").Replace(",\\\r\n", ",");
            List<string>? lines = text.Replace("\r", "").Split('\n').ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains('='))
                {
                    lines[i] = lines[i].Split('=')[0] + "=-";
                }
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
                IEnumerable<string>? filesall = Directory.EnumerateFiles(@"out\Files", "*.*", SearchOption.AllDirectories);
                foreach (string? stuff in filesall)
                {
                    byte[]? read = RTInstaller.HandleFile.GetProperBytes(stuff, out bool patched);
                    if (patched)
                    {
                        File.WriteAllBytes(stuff, read);
                    }
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

            if (DXCareRT)
            {
                DXCare();
            }

            //
            // WlanCare section
            //

            if (WlanCareRT)
            {
                WlanCare();
            }

            //
            // MbbCare section
            //

            if (MbbCareRT)
            {
                MbbCare();
            }

            //
            // UartCare section
            //

            if (UartCareRT)
            {
                UartCare();
            }

            //
            // Merging section
            //

            if (merge)
            {
                IEnumerable<string>? dirsfi = Directory.EnumerateDirectories(@"out\Files");
                foreach (string? dir in dirsfi)
                {
                    Console.WriteLine("(processing) Merging " + dir + "...");
                    CopyDir(dir, "root");
                    Directory.Delete(dir, true);
                }

                Directory.Delete(@"out\Files", true);

                IEnumerable<string>? regsfi = Directory.EnumerateFiles(@"out\Registry");
                foreach (string? reg in regsfi)
                {
                    Console.WriteLine("(processing) Merging " + reg + "...");
                    string? lines = string.Join("\r\n", File.ReadLines(reg).Skip(1).ToArray()) + "\r\n";
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
                IEnumerable<string>? filesall = Directory.EnumerateFiles(@"out\Files", "*.*", SearchOption.AllDirectories);
                foreach (string? stuff in filesall)
                {
                    byte[]? read = RTInstaller.HandleFile.GetProperBytes(stuff, out bool patched);
                    if (patched)
                    {
                        File.WriteAllBytes(stuff, read);
                    }
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

            if (DXCareRT)
            {
                DXCare();
            }

            //
            // WlanCare section
            //

            if (WlanCareRT)
            {
                WlanCare();
            }

            //
            // MbbCare section
            //

            if (MbbCareRT)
            {
                MbbCare();
            }

            //
            // UartCare section
            //

            if (UartCareRT)
            {
                UartCare();
            }

            if (RTCare)
            {
                RtCare();
            }

            //
            // Merging section
            //

            if (merge)
            {
                IEnumerable<string>? dirsfi = Directory.EnumerateDirectories(@"out\Files");
                foreach (string? dir in dirsfi)
                {
                    Console.WriteLine("(processing) Merging " + dir + "...");
                    CopyDir(dir, "root");
                    Directory.Delete(dir, true);
                }

                Directory.Delete(@"out\Files", true);

                IEnumerable<string>? regsfi = Directory.EnumerateFiles(@"out\Registry");
                foreach (string? reg in regsfi)
                {
                    Console.WriteLine("(processing) Merging " + reg + "...");
                    string? lines = string.Join("\r\n", File.ReadLines(reg).Skip(1).ToArray()) + "\r\n";
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
            IEnumerable<string>? dirs = Directory.EnumerateDirectories(@"out\Files");
            IEnumerable<string>? regs = Directory.EnumerateFiles(@"out\Registry");

            foreach (string? dir in dirs)
            {
                if (dir.ToLower().Contains(banned.ToLower()))
                {
                    Console.WriteLine("(rtCare) Deleting " + dir + "...");
                    Directory.Delete(dir, true);
                }
            }

            foreach (string? reg in regs)
            {
                if (reg.ToLower().Contains(banned.ToLower()))
                {
                    Console.WriteLine("(rtCare) Deleting " + reg + "...");
                    File.Delete(reg);
                }
            }
        }

        public static void WlanCare()
        {
            string? WlanFile = "";
            string? WlanDatFile = "";

            IEnumerable<string>? dirs = Directory.EnumerateDirectories(@"out\Files");
            foreach (string? dir in dirs)
            {
                bool wlanrel = false;
                if (dir.ToLower().Contains("wlan"))
                {
                    wlanrel = true;
                }
                if (wlanrel)
                {
                    IEnumerable<string>? wlan8974 = Directory.EnumerateFiles(dir, "qcwlan8*74.sys", SearchOption.AllDirectories);
                    IEnumerable<string>? wlan8626 = Directory.EnumerateFiles(dir, "qcwlan8*26.sys", SearchOption.AllDirectories);
                    IEnumerable<string>? wlandat = Directory.EnumerateFiles(dir, "qcwlan*cfg.dat", SearchOption.AllDirectories);

                    if (wlan8974 != null)
                    {
                        foreach (string? itm in wlan8974)
                        {
                            WlanFile = itm.Split('\\').Last();
                        }
                    }

                    if (wlan8626 != null)
                    {
                        foreach (string? itm in wlan8626)
                        {
                            WlanFile = itm.Split('\\').Last();
                        }
                    }

                    if (wlandat != null)
                    {
                        foreach (string? itm in wlandat)
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

            foreach (string? dir in dirs)
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
            string? MbbFile = "";
            string MbbReg = "";

            IEnumerable<string>? dirs = Directory.EnumerateDirectories(@"out\Files");
            foreach (string? dir in dirs)
            {
                bool mbbrel = false;
                if (dir.ToLower().Contains("mbb") && !dir.ToLower().Contains("mbbuio"))
                {
                    mbbrel = true;
                }
                if (mbbrel)
                {
                    IEnumerable<string>? mbb = Directory.EnumerateFiles(dir, "qcmbb*", SearchOption.AllDirectories);

                    if (mbb != null)
                    {
                        foreach (string? itm in mbb)
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
                IEnumerable<string>? regs = Directory.EnumerateFiles(@"out\Registry");
                foreach (string? reg in regs)
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
            string? UartFile = "";
            string UartReg = "";

            IEnumerable<string>? dirs = Directory.EnumerateDirectories(@"out\Files");
            foreach (string? dir in dirs)
            {
                bool uartrel = false;
                if (dir.ToLower().Contains("qualcomm_uart"))
                {
                    uartrel = true;
                }
                if (uartrel)
                {
                    IEnumerable<string>? uart = Directory.EnumerateFiles(dir, "qcuart*", SearchOption.AllDirectories);

                    if (uart != null)
                    {
                        foreach (string? itm in uart)
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
                IEnumerable<string>? regs = Directory.EnumerateFiles(@"out\Registry");
                foreach (string? reg in regs)
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
            string? dxfile = "";
            string? vssfile = "";

            IEnumerable<string>? dirs = Directory.EnumerateDirectories(@"out\Files");
            foreach (string? dir in dirs)
            {
                bool dxrel = false;
                foreach (string? itm in directx)
                {
                    if (dir.ToLower().Contains(itm.ToLower()))
                    {
                        dxrel = true;
                        break;
                    }
                }

                if (dxrel)
                {
                    IEnumerable<string>? vss = Directory.EnumerateFiles(dir, "qcvss*.mbn", SearchOption.AllDirectories);
                    IEnumerable<string>? dx = Directory.EnumerateFiles(dir, "qcdxkm*.sys", SearchOption.AllDirectories);

                    if (vss != null)
                    {
                        foreach (string? itm in vss)
                        {
                            vssfile = itm.Split('\\').Last();
                            Console.WriteLine("(dxCare) Moving QCVSS...");
                            File.Copy(itm, itm.Split('\\').Last());
                        }
                    }

                    if (dx != null)
                    {
                        foreach (string? itm in dx)
                        {
                            dxfile = itm.Split('\\').Last();
                            Console.WriteLine("(dxCare) Moving QCDXKM...");
                            File.Copy(itm, itm.Split('\\').Last());
                        }
                    }
                }
            }

            IEnumerable<string>? regs = Directory.EnumerateFiles(@"out\Registry");
            foreach (string? reg in regs)
            {
                bool dxrel = false;
                foreach (string? itm in directx)
                {
                    if (reg.ToLower().Contains(itm.ToLower()))
                    {
                        dxrel = true;
                        break;
                    }
                }

                if (dxrel)
                {
                    if (reg.ToLower().Contains("qcdxdriver"))
                    {
                        DXReg += File.ReadAllText(reg);
                    }
                }
            }

            foreach (string? dir in dirs)
            {
                bool dxrel = false;
                foreach (string? itm in directx)
                {
                    if (dir.ToLower().Contains(itm.ToLower()))
                    {
                        dxrel = true;
                        break;
                    }
                }

                if (dxrel)
                {
                    Console.WriteLine("(dxCare) Deleting " + dir + "...");
                    Directory.Delete(dir, true);
                }
            }

            foreach (string? reg in regs)
            {
                bool dxrel = false;
                foreach (string? itm in directx)
                {
                    if (reg.ToLower().Contains(itm.ToLower()))
                    {
                        dxrel = true;
                        break;
                    }
                }

                if (dxrel)
                {
                    Console.WriteLine("(dxCare) Deleting " + reg + "...");
                    File.Delete(reg);
                }
            }

            if (DXReg != "" && dxfile != "" && vssfile != "")
            {
                DXInfHandler.GenInfProperly(DXReg, dxfile, vssfile);
            }
        }

        public static void CopyDir(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

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
