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

namespace GetLumiaBSP
{
    [Verb("secwim2wim", HelpText = "Convert secwim to wim")]
    public class SecWim2WimOptions
    {
        [Option('p', "path", HelpText = "Path to secwim", Required = true)]
        public string path { get; set; }

        [Option('o', "output", HelpText = "Path to output wim", Required = true)]
        public string output { get; set; }
    }

    [Verb("enoswdl", HelpText = "Download ENOSW image")]
    public class ENOSWDownloadOptions
    {
        [Option('r', "rm", HelpText = "Device RM product code", Required = true)]
        public string rmcode { get; set; }

        [Option('o', "output", HelpText = "Path to output secwim", Required = true)]
        public string path { get; set; }
    }

    [Verb("inf_mountedfs", HelpText = "Extract an inf install package")]
    public class InfOptions
    {
        [Option('p', "path", HelpText = "Path to wim/secwim/mountedfs", Required = true)]
        public string path { get; set; }

        [Option('s', "signingcert", HelpText = "Path to signing certificate", Required = false)]
        public string signingcert { get; set; }

        [Option('o', "output", HelpText = "Path to output", Required = true)]
        public string output { get; set; }
    }

    [Verb("unbsp", HelpText = "Remove BSP from image")]
    public class UnBSPOptions
    {
        [Option('p', "path", HelpText = "Path to mounted read/write fs", Required = true)]
        public string path { get; set; }
    }

    [Verb("reg_mountedfs", HelpText = "Extract a reg install package")]
    public class RegExtractionMainOSOptions
    {
        [Option('p', "path", HelpText = "Path to wim/secwim/mountedfs", Required = true)]
        public string path { get; set; }

        [Option('s', "signingcert", HelpText = "Path to signing certificate", Required = false)]
        public string signingcert { get; set; }

        /*[Option('o', "output", HelpText = "Path to output", Required = true)]
        public string output { get; set; }*/

        [Option('m', "mergeno", HelpText = "Disable auto merge of components", Required = false, Default = false)]
        public bool mergeno { get; set; }

        [Option('a', "patcherno", HelpText = "Disable auto patch of components (PhoneNT checks)", Required = false, Default = false)]
        public bool patcherno { get; set; }

        [Option('d', "dxcareno", HelpText = "Disable DX Care (should be used for non RT 8.1 applications)", Required = false, Default = false)]
        public bool dxcareno { get; set; }

        [Option('b', "mbbcareno", HelpText = "Disable MBB Care (should be used for non Desktop applications)", Required = false, Default = false)]
        public bool mbbcareno { get; set; }

        [Option('u', "uartcareno", HelpText = "Disable UART Care (should be used for non Desktop applications)", Required = false, Default = false)]
        public bool uartcareno { get; set; }

        [Option('w', "wlancareno", HelpText = "Disable WLAN Care (should be used for non Desktop applications)", Required = false, Default = false)]
        public bool wlancareno { get; set; }

        [Option('r', "rtcareno", HelpText = "Disable RT Care (should be used for non RT 8.1 applications)", Required = false, Default = false)]
        public bool rtcareno { get; set; }
    }

    [Verb("reg_hybrid", HelpText = "Extract a reg install package")]
    public class RegExtractionWimOptions
    {
        [Option('p', "path", HelpText = "Path to mountedfs", Required = true)]
        public string path { get; set; }

        [Option('e', "path", HelpText = "Path to EnoSW wim/secwim", Required = true)]
        public string enopath { get; set; }

        [Option('s', "signingcert", HelpText = "Path to signing certificate", Required = false)]
        public string signingcert { get; set; }

        [Option('o', "output", HelpText = "Path to output", Required = true)]
        public string output { get; set; }

        [Option('m', "mergeno", HelpText = "Disable auto merge of components", Required = false, Default = false)]
        public bool mergeno { get; set; }

        [Option('a', "patcherno", HelpText = "Disable auto patch of components (PhoneNT checks)", Required = false, Default = false)]
        public bool patcherno { get; set; }

        [Option('d', "dxcareno", HelpText = "Disable DX Care (should be used for non RT 8.1 applications)", Required = false, Default = false)]
        public bool dxcareno { get; set; }

        [Option('b', "mbbcareno", HelpText = "Disable MBB Care (should be used for non Desktop applications)", Required = false, Default = false)]
        public bool mbbcareno { get; set; }

        [Option('u', "uartcareno", HelpText = "Disable UART Care (should be used for non Desktop applications)", Required = false, Default = false)]
        public bool uartcareno { get; set; }

        [Option('w', "wlancareno", HelpText = "Disable WLAN Care (should be used for non Desktop applications)", Required = false, Default = false)]
        public bool wlancareno { get; set; }

        [Option('r', "rtcareno", HelpText = "Disable RT Care (should be used for non RT 8.1 applications)", Required = false, Default = false)]
        public bool rtcareno { get; set; }
    }
}
