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
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GetLumiaBSP
{
    public class CLIOptions
    {
        /*[VerbOption("reg_enosw", HelpText = "Extract a reg install package")]
        public RegOptions RegEnoSWVerb { get; set; }*/

        [VerbOption("reg_mountedfs", HelpText = "Extract a reg install package")]
        public RegOptions RegMountedFsVerb { get; set; }

        /*[VerbOption("reg_hybrid", HelpText = "Extract a reg install package")]
        public RegOptions2 RegHybridVerb { get; set; }

        [VerbOption("inf_enosw", HelpText = "Extract an inf install package")]
        public InfOptions InfEnoSWVerb { get; set; }

        [VerbOption("inf_mountedfs", HelpText = "Extract an inf install package")]
        public InfOptions InfMountedFsVerb { get; set; }*/

        /*[VerbOption("secwim2wim", HelpText = "Convert secwim to wim")]
        public secwim2wimOptions secwim2wimVerb { get; set; }*/

        /*[VerbOption("enoswdl", HelpText = "Download ENOSW image")]
        public enoswdlOptions enoswdlVerb { get; set; }*/

        /*[VerbOption("unbsp", HelpText = "Remove BSP from image")]
        public unbspOptions unbspVerb { get; set; }*/

        public CLIOptions()
        {
            //RegEnoSWVerb = new RegOptions();
            RegMountedFsVerb = new RegOptions();
            /*RegHybridVerb = new RegOptions2();
            InfMountedFsVerb = new InfOptions();
            InfEnoSWVerb = new InfOptions();*/
            //secwim2wimVerb = new secwim2wimOptions();
            //unbspVerb = new unbspOptions();
        }

        [HelpOption]
        public string GetUsage()
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(ass.Location);

            HelpText ht = new HelpText
            {
                Heading = new HeadingInfo(fvi.FileDescription, ass.GetName().Version.ToString()),
                Copyright = new CopyrightInfo(fvi.CompanyName, DateTime.Today.Year),
                AddDashesToOption = true
            };

            ht.AddOptions(this);
            return ht;
        }

        [HelpVerbOption]
        public string GetUsage(string verb) => HelpText.AutoBuild(this, verb);
    }

    public class secwim2wimOptions
    {
        [Option('p', "path", HelpText = "Path to secwim", Required = true)]
        public string path { get; set; }

        [Option('o', "output", HelpText = "Path to output wim", Required = true)]
        public string output { get; set; }
    }

    public class enoswdlOptions
    {
        [Option('r', "rm", HelpText = "Device RM product code", Required = true)]
        public string rmcode { get; set; }

        [Option('o', "output", HelpText = "Path to output secwim", Required = true)]
        public string path { get; set; }
    }

    public class InfOptions
    {
        [Option('p', "path", HelpText = "Path to wim/secwim/mountedfs", Required = true)]
        public string path { get; set; }

        [Option('s', "signingcert", HelpText = "Path to signing certificate", Required = false)]
        public string signingcert { get; set; }

        [Option('o', "output", HelpText = "Path to output", Required = true)]
        public string output { get; set; }
    }

    public class unbspOptions
    {
        [Option('p', "path", HelpText = "Path to mounted read/write fs", Required = true)]
        public string path { get; set; }
    }

    public class RegOptions
    {
        [Option('p', "path", HelpText = "Path to wim/secwim/mountedfs", Required = true)]
        public string path { get; set; }

        [Option('s', "signingcert", HelpText = "Path to signing certificate", Required = false)]
        public string signingcert { get; set; }

        /*[Option('o', "output", HelpText = "Path to output", Required = true)]
        public string output { get; set; }*/

        [Option('m', "mergeno", HelpText = "Disable auto merge of components", Required = false, DefaultValue = false)]
        public bool mergeno { get; set; }

        [Option('a', "patcherno", HelpText = "Disable auto patch of components (PhoneNT checks)", Required = false, DefaultValue = false)]
        public bool patcherno { get; set; }

        [Option('d', "dxcareno", HelpText = "Disable DX Care (should be used for non RT 8.1 applications)", Required = false, DefaultValue = false)]
        public bool dxcareno { get; set; }

        [Option('b', "mbbcareno", HelpText = "Disable MBB Care (should be used for non Desktop applications)", Required = false, DefaultValue = false)]
        public bool mbbcareno { get; set; }

        [Option('u', "uartcareno", HelpText = "Disable UART Care (should be used for non Desktop applications)", Required = false, DefaultValue = false)]
        public bool uartcareno { get; set; }

        [Option('w', "wlancareno", HelpText = "Disable WLAN Care (should be used for non Desktop applications)", Required = false, DefaultValue = false)]
        public bool wlancareno { get; set; }

        [Option('r', "rtcareno", HelpText = "Disable RT Care (should be used for non RT 8.1 applications)", Required = false, DefaultValue = false)]
        public bool rtcareno { get; set; }
    }

    public class RegOptions2
    {
        [Option('p', "path", HelpText = "Path to mountedfs", Required = true)]
        public string path { get; set; }

        [Option('e', "path", HelpText = "Path to EnoSW wim/secwim", Required = true)]
        public string enopath { get; set; }

        [Option('s', "signingcert", HelpText = "Path to signing certificate", Required = false)]
        public string signingcert { get; set; }

        [Option('o', "output", HelpText = "Path to output", Required = true)]
        public string output { get; set; }

        [Option('m', "mergeno", HelpText = "Disable auto merge of components", Required = false, DefaultValue = false)]
        public bool mergeno { get; set; }

        [Option('a', "patcherno", HelpText = "Disable auto patch of components (PhoneNT checks)", Required = false, DefaultValue = false)]
        public bool patcherno { get; set; }

        [Option('d', "dxcareno", HelpText = "Disable DX Care (should be used for non RT 8.1 applications)", Required = false, DefaultValue = false)]
        public bool dxcareno { get; set; }

        [Option('b', "mbbcareno", HelpText = "Disable MBB Care (should be used for non Desktop applications)", Required = false, DefaultValue = false)]
        public bool mbbcareno { get; set; }

        [Option('u', "uartcareno", HelpText = "Disable UART Care (should be used for non Desktop applications)", Required = false, DefaultValue = false)]
        public bool uartcareno { get; set; }

        [Option('w', "wlancareno", HelpText = "Disable WLAN Care (should be used for non Desktop applications)", Required = false, DefaultValue = false)]
        public bool wlancareno { get; set; }

        [Option('r', "rtcareno", HelpText = "Disable RT Care (should be used for non RT 8.1 applications)", Required = false, DefaultValue = false)]
        public bool rtcareno { get; set; }
    }
}
