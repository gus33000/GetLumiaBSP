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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace BSPExtractor
{
    class Program
    {
        private static string[] oems = new string[]
        {
            "MMO",
            "NOKIA",
            "Qualcomm",
            "OEM",
            "MSAsOEM"
        };

        private static string[] parts = new string[]
        {
            "MainOS"
        };
        
        private static string SystemKeyName = "RTSYSTEM";
        private static string SoftwareKeyName = "RTSOFTWARE";

        static string[] GetPackages(string sourcedrive)
        {
            var lst = new List<string>();
            if (Directory.Exists(sourcedrive + @"\Windows\Packages\DsmFiles\"))
            {
                var files1 = Directory.EnumerateFiles(sourcedrive + @"\Windows\Packages\DsmFiles\");

                foreach (var file in files1)
                {
                    var name = file.Replace(sourcedrive + @"\Windows\Packages\DsmFiles\", "");
                    foreach (var oem in oems)
                    {
                        if (name.ToLower().StartsWith(oem.ToLower()))
                        {
                            foreach (var part in parts)
                            {
                                if (name.ToLower().Contains(part.ToLower()) || name.Count(x => x == '.') <= 4)
                                {
                                    lst.Add(name);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            if (Directory.Exists(sourcedrive + @"\Windows\servicing\Packages\"))
            {
                var files2 = Directory.EnumerateFiles(sourcedrive + @"\Windows\servicing\Packages\", "*.mum");

                foreach (var file in files2)
                {
                    var name = file.Replace(sourcedrive + @"\Windows\servicing\Packages\", "");
                    foreach (var oem in oems)
                    {
                        if (name.ToLower().StartsWith(oem.ToLower()))
                        {
                            foreach (var part in parts)
                            {
                                if (name.ToLower().Contains(part.ToLower()) || name.Count(x => x == '.') <= 6)
                                {
                                    if (!lst.Any(x => name.ToLower().Contains(x.ToLower())))
                                        lst.Add(name);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            return lst.ToArray();
        }

        static void HandleReg(string NewDrive, string Output)
        {
            var packages = GetPackages(NewDrive);

            Console.WriteLine("(bspExtractor) " + "Preparing files for registry import file generation...");
            foreach (var packagename in packages)
            {
                if (File.Exists(NewDrive + @"\Windows\servicing\Packages\" + packagename))
                {
                    string path = NewDrive + @"\Windows\servicing\Packages\" + packagename;

                    Stream stream = File.OpenRead(path);

                    XmlSerializer serializer = new XmlSerializer(typeof(XmlMum.Assembly));
                    XmlMum.Assembly package = (XmlMum.Assembly)serializer.Deserialize(stream);

                    stream.Close();

                    var entries = package.Package.CustomInformation.File.Where(x => x.Name.ToLower().EndsWith(".manifest") && !x.Name.ToLower().Contains("deployment"));

                    foreach (var entry in entries)
                    {
                        var filepath = entry.Name.Replace("$(runtime.bootdrive)", "").Replace("$(runtime.system32)", @"Windows\System32").Replace("$(runtime.systemroot)", "Windows").Replace("$(runtime.drivers)", @"Windows\System32\Drivers").Replace("$(runtime.programfiles)", "Program Files");

                        if (filepath == @"\update.mum")
                        {
                            filepath = @"\Windows\servicing\Packages\" + packagename;
                        }

                        if (filepath.EndsWith(@"\update.cat"))
                        {
                            filepath = filepath.Replace(@"\update.cat", @"\" + packagename.Replace(".mum", ".cat"));
                        }

                        if (filepath.EndsWith(".manifest") && !filepath.Contains(@"\"))
                        {
                            filepath = @"\Windows\WinSxS\Manifests\" + filepath;
                        }

                        if (!Directory.Exists(Output + @"\Registry\cbs\"))
                            Directory.CreateDirectory(Output + @"\Registry\cbs\");
                        try
                        {
                            File.Copy(NewDrive + @"\" + filepath, Output + @"\Registry\cbs\" + entry.Name);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("(bspExtractor) " + ex.Message);
                            Console.ResetColor();
                        }
                    }
                }
                else if (File.Exists(NewDrive + @"\Windows\Packages\DsmFiles\" + packagename))
                {
                    string path = NewDrive + @"\Windows\Packages\DsmFiles\" + packagename;

                    var proc = new Process();
                    proc.StartInfo = new ProcessStartInfo("7za.exe", "x " + path) { WindowStyle = ProcessWindowStyle.Hidden };
                    proc.Start();
                    proc.WaitForExit();

                    Stream stream = File.OpenRead(packagename.Replace(".xml", "")); //File.OpenRead(path);//

                    XmlSerializer serializer = new XmlSerializer(typeof(XmlDsm.Package));
                    XmlDsm.Package package = (XmlDsm.Package)serializer.Deserialize(stream);

                    stream.Close();

                    File.Delete(packagename.Replace(".xml", ""));

                    var entries = package.Files.FileEntry.Where(x => x.DevicePath.ToLower().Contains(@"windows\packages\registryfiles\"));

                    foreach (var entry in entries)
                    {
                        if (!Directory.Exists(Output + @"\Registry\spkg\"))
                            Directory.CreateDirectory(Output + @"\Registry\spkg\");
                        try
                        {
                            File.Copy(NewDrive + entry.DevicePath, Output + @"\Registry\spkg\" + entry.DevicePath.Split('\\').Last());
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("(bspExtractor) " + ex.Message);
                            Console.ResetColor();
                        }
                    }
                }
            }

            if (Directory.Exists(Output + @"\Registry\cbs\"))
            {
                Directory.CreateDirectory(Output + @"\Registry\cbs\out");
                Console.WriteLine("(bspExtractor) " + "Processing registry files...");
                foreach (var file in Directory.EnumerateFiles(Output + @"\Registry\cbs\"))
                {
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo("SxSExpand.exe", file + " " + Output + @"\Registry\cbs\out")
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };
                    proc.Start();
                    proc.WaitForExit();
                    File.Delete(file);
                }

                if (Directory.Exists(Output + @"\Registry\cbs\out"))
                {
                    foreach (var file in Directory.EnumerateFiles(Output + @"\Registry\cbs\out\"))
                    {
                        HandleCbsFile(file, Output + @"\Registry\");
                    }
                    Console.WriteLine("(bspExtractor) " + "Cleaning up...");
                    Directory.Delete(Output + @"\Registry\cbs\", true);
                }
            }

            if (Directory.Exists(Output + @"\Registry\spkg\"))
            {
                Console.WriteLine("(bspExtractor) " + "Processing registry files...");
                Directory.CreateDirectory(Output + @"\Registry\spkg\out\");
                foreach (var file in Directory.EnumerateFiles(Output + @"\Registry\spkg\"))
                {
                    var proc = new Process();
                    proc.StartInfo = new ProcessStartInfo("7za.exe", "x " + file + " -o" + Output + @"\Registry\spkg\out -aou") { WindowStyle = ProcessWindowStyle.Hidden };
                    proc.Start();
                    proc.WaitForExit();
                    File.Delete(file);

                    //File.Copy(file, Output + @"\Registry\spkg\out\" + file.Split('\\').Last());
                }

                Console.WriteLine("(bspExtractor) " + "Reading files...");
                if (Directory.Exists(Output + @"\Registry\spkg\out"))
                {
                    foreach (var file in Directory.EnumerateFiles(Output + @"\Registry\spkg\"))
                    {
                        //var file = string.Join("\\", file2.Split('\\').Reverse().Skip(1).Reverse()) + @"\out\" + string.Join("\\", file2.Split('\\').Last());
                        var content = KeyNameReplace(File.ReadAllText(file));
                        File.WriteAllText(file, content);

                        if (content.Contains("; RegistrySource="))
                        {
                            foreach (var part in content.Split(new string[] { "; RegistrySource=" }, StringSplitOptions.None).Skip(1))
                            {
                                var src = part.Split('\n').First().Replace("\r", "").Replace("_1", "");

                                var newcontent = "; RegistrySource=" + file.Split('\\').Last().Replace("_1", "") + ".reg" + string.Join("\n", part.Split('\n').Skip(1));

                                if (File.Exists(Output + @"\Registry\" + src))
                                {
                                    File.AppendAllText(Output + @"\Registry\" + src, newcontent, Encoding.ASCII);
                                    continue;
                                }
                                newcontent = "Windows Registry Editor Version 5.00" + "\r\n" + newcontent;
                                File.WriteAllText(Output + @"\Registry\" + src, newcontent, Encoding.ASCII);
                            }
                        }
                        else
                        {
                            if (!File.Exists(Output + @"\Registry\" + file.Split('\\').Last().Replace("_1", "") + ".reg"))
                                File.Move(file, Output + @"\Registry\" + file.Split('\\').Last().Replace("_1", "") + ".reg");
                            else
                                File.AppendAllText(Output + @"\Registry\" + file.Split('\\').Last().Replace("_1", "") + ".reg", File.ReadAllText(file).Replace("Windows Registry Editor Version 5.00", "; RegistrySource=" + file.Split('\\').Last().Replace("_1", "") + ".reg"), Encoding.ASCII);
                        }
                    }
                    Console.WriteLine("(bspExtractor) " + "Cleaning up...");
                    Directory.Delete(Output + @"\Registry\spkg\", true);
                }
            }
        }

        static void HandleCbsFile(string file, string outputFolder)
        {
            Stream stream = File.OpenRead(file);

            var name = file.Split('\\').Last();

            XmlSerializer serializer = new XmlSerializer(typeof(XmlMum.Assembly));
            XmlMum.Assembly cbs = (XmlMum.Assembly)serializer.Deserialize(stream);

            name = cbs.AssemblyIdentity.Name.Substring(0, cbs.AssemblyIdentity.Name.Length - 1);

            if (cbs.RegistryKeys != null && cbs.RegistryKeys.Count() > 0)
            {
                CbsToReg regExporter = new CbsToReg();

                foreach (var regKey in cbs.RegistryKeys)
                {
                    var keyValues = new List<RegistryValue>(regKey.RegistryValues.Count);
                    foreach (var keyValue in regKey.RegistryValues)
                    {
                        keyValues.Add(new RegistryValue(keyValue.Name, keyValue.Value, keyValue.ValueType, 
                                                        keyValue.Mutable, keyValue.OperationHint));
                    }

                    regExporter.Add(new RegistryCollection(regKey.KeyName, keyValues));
                }

                regExporter.Comment = "; RegistrySource=" + name;

                string reg = regExporter.Build(SoftwareKeyName, SystemKeyName);
                File.WriteAllText(outputFolder + name + ".reg", reg, Encoding.Unicode);
            }

            stream.Close();
        }

        static void HandleFiles(string NewDrive, string Output)
        {
            Console.WriteLine("(bspExtractor) " + "Processing files...");
            var packages = GetPackages(NewDrive);

            foreach (var packagename in packages)
            {
                var othername = packagename.Replace(".dsm.xml", "");

                if (File.Exists(NewDrive + @"\Windows\servicing\Packages\" + packagename))
                {
                    string path = NewDrive + @"\Windows\servicing\Packages\" + packagename;
                    
                    othername = packagename.Split('~').First();

                    Stream stream = File.OpenRead(path);

                    XmlSerializer serializer = new XmlSerializer(typeof(XmlMum.Assembly));
                    XmlMum.Assembly package = (XmlMum.Assembly)serializer.Deserialize(stream);

                    stream.Close();

                    var entries = package.Package.CustomInformation.File;
                    var catpath = (@"\Windows\servicing\Packages\" + packagename).Replace(".mum", ".cat");
                    var catfilepath = catpath.Replace("$(runtime.bootdrive)", "").Replace("$(runtime.system32)", @"Windows\System32").Replace("$(runtime.systemroot)", "Windows").Replace("$(runtime.drivers)", @"Windows\System32\Drivers").Replace("$(runtime.programfiles)", "Program Files");

                    if (catfilepath == @"\update.mum")
                    {
                        catfilepath = @"\Windows\servicing\Packages\" + packagename;
                    }

                    if (catfilepath.EndsWith(@"\update.cat"))
                    {
                        catfilepath = catfilepath.Replace(@"\update.cat", @"\" + packagename.Replace(".mum", ".cat"));
                    }

                    if (catfilepath.EndsWith(".manifest") && !catfilepath.Contains(@"\"))
                    {
                        if (!catfilepath.Contains(".deployment_"))
                        {
                            DirectoryCopy(NewDrive + @"\Windows\WinSxS\" + catfilepath.Replace(".manifest", ""), Output + @"\Files\" + othername + @"\Windows\WinSxS\" + catfilepath.Replace(".manifest", ""), true);
                        }
                        catfilepath = @"\Windows\WinSxS\Manifests\" + catfilepath;
                    }

                    if (!Directory.Exists(Output + @"\Files\" + othername + string.Join("\\", catfilepath.Split('\\').Reverse().Skip(1).Reverse())))
                        Directory.CreateDirectory(Output + @"\Files\" + othername + @"\" + string.Join("\\", catfilepath.Split('\\').Reverse().Skip(1).Reverse()));
                    try
                    {
                        File.Copy(NewDrive + @"\" + catfilepath, Output + @"\Files\" + othername + @"\" + catfilepath);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("(bspExtractor) " + ex.Message);
                        Console.ResetColor();
                    }

                    foreach (var entry in entries)
                    {
                        string outfolder = othername;
                        if (!string.IsNullOrEmpty(entry.SourcePackage))
                            outfolder = entry.SourcePackage;
                        
                        var filepath = entry.Name.Replace("$(runtime.bootdrive)", "").Replace("$(runtime.system32)", @"Windows\System32").Replace("$(runtime.systemroot)", "Windows").Replace("$(runtime.drivers)", @"Windows\System32\Drivers").Replace("$(runtime.programfiles)", "Program Files");

                        if (filepath == @"\update.mum")
                        {
                            filepath = @"\Windows\servicing\Packages\" + packagename;
                        }

                        if (filepath.EndsWith(@"\update.cat"))
                        {
                            filepath = filepath.Replace(@"\update.cat", @"\" + packagename.Replace(".mum", ".cat"));
                        }

                        if (filepath.EndsWith(".manifest") && !filepath.Contains(@"\"))
                        {
                            if (!filepath.Contains(".deployment_"))
                            {
                                //Console.WriteLine("(bspExtractor) " + "Copying " + @"\Windows\WinSxS\" + filepath.Replace(".manifest", "") + "...");
                                DirectoryCopy(NewDrive + @"\Windows\WinSxS\" + filepath.Replace(".manifest", ""), Output + @"\Files\" + outfolder + @"\Windows\WinSxS\" + filepath.Replace(".manifest", ""), true);
                            }
                            filepath = @"\Windows\WinSxS\Manifests\" + filepath;
                        }
                        
                        if (!Directory.Exists(Output + @"\Files\" + outfolder + @"\" + string.Join("\\", filepath.Split('\\').Reverse().Skip(1).Reverse())))
                            Directory.CreateDirectory(Output + @"\Files\" + outfolder + @"\" + string.Join("\\", filepath.Split('\\').Reverse().Skip(1).Reverse()));
                        try
                        {
                            File.Copy(NewDrive + @"\" + filepath, Output + @"\Files\" + outfolder + @"\" + filepath);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("(bspExtractor) " + ex.Message);
                            Console.ResetColor();
                        }
                    }
                }
                else if (File.Exists(NewDrive + @"\Windows\Packages\DsmFiles\" + packagename))
                {
                    string path = NewDrive + @"\Windows\Packages\DsmFiles\" + packagename;

                    var proc = new Process();
                    proc.StartInfo = new ProcessStartInfo("7za.exe", "x " + path) { WindowStyle = ProcessWindowStyle.Hidden };
                    proc.Start();
                    proc.WaitForExit();

                    Stream stream = File.OpenRead(packagename.Replace(".xml", "")); //File.OpenRead(path);//

                    XmlSerializer serializer = new XmlSerializer(typeof(XmlDsm.Package));
                    XmlDsm.Package package = (XmlDsm.Package)serializer.Deserialize(stream);

                    stream.Close();

                    File.Delete(packagename.Replace(".xml", ""));

                    var entries = package.Files.FileEntry;

                    foreach (var entry in entries)
                    {
                        string outfolder = othername;
                        if (!string.IsNullOrEmpty(entry.SourcePackage))
                            outfolder = entry.SourcePackage;

                        if (!Directory.Exists(Output + @"\Files\" + outfolder + @"\" + string.Join("\\", entry.DevicePath.Split('\\').Reverse().Skip(1).Reverse())))
                            Directory.CreateDirectory(Output + @"\Files\" + outfolder + @"\" + string.Join("\\", entry.DevicePath.Split('\\').Reverse().Skip(1).Reverse()));
                        try
                        {
                            File.Copy(NewDrive + entry.DevicePath, Output + @"\Files\" + outfolder + @"\" + entry.DevicePath);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("(bspExtractor) " + ex.Message);
                            Console.ResetColor();
                        }
                    }
                }
            }
        }

        public static void HandlePackages(string NewDrive, string Output)
        {
            Console.WriteLine("(bspExtractor) " + $"\nUsing: \t{NewDrive} as ROM path\n\t{Output} as output directory\n\t\"{SystemKeyName}\" as System keyname\n\t\"{SoftwareKeyName}\" as Software keyname\n");

            HandleFiles(NewDrive, Output);
            HandleReg(NewDrive, Output);

            Console.WriteLine("(bspExtractor) " + "Done.");
        }
        
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);
                DirectoryInfo[] dirs = dir.GetDirectories();

                // If the source directory does not exist, throw an exception.
                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                // If the destination directory does not exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }


                // Get the file contents of the directory to copy.
                FileInfo[] files = dir.GetFiles();

                foreach (FileInfo file in files)
                {
                    // Create the path to the new copy of the file.
                    string temppath = Path.Combine(destDirName, file.Name);

                    // Copy the file.
                    file.CopyTo(temppath, false);
                }

                // If copySubDirs is true, copy the subdirectories.
                if (copySubDirs)
                {

                    foreach (DirectoryInfo subdir in dirs)
                    {
                        // Create the subdirectory.
                        string temppath = Path.Combine(destDirName, subdir.Name);

                        // Copy the subdirectories.
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("(bspExtractor) " + ex.Message);
                Console.ResetColor();
            }
        }
        private static string KeyNameReplace(string content)
        {
            return Regex.Replace(Regex.Replace(content, @"^\[HKEY_LOCAL_MACHINE\\SOFTWARE", "[HKEY_LOCAL_MACHINE\\" + SoftwareKeyName, RegexOptions.IgnoreCase | RegexOptions.Multiline),
                    @"^\[HKEY_LOCAL_MACHINE\\SYSTEM", "[HKEY_LOCAL_MACHINE\\" + SystemKeyName, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }
    }
}