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

using System.Net;
using System.Text.Json;

namespace GetLumiaBSP
{
    internal class ENOSWPackageDownloader
    {
        public static string DownloadENOSWPackage(string RMCode, string outputfolder = ".\\")
        {
            string? discoveryRequest = "{\"api-version\":\"1\",\"condition\":[\"default\"],\"query\":{ \"manufacturerHardwareModel\":\"" + RMCode + "\",\"manufacturerName\":\"Microsoft\",\"manufacturerProductLine\":\"Lumia\",\"packageClass\":\"Public\",\"packageType\":\"Test Mode\"},\"response\":null}";

            Console.WriteLine("(mmoENOSWDL) Discovering ENOSW packages...");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://api.swrepository.com/rest-api/discovery/1/package");

            req.Method = "POST";
            req.ContentType = "application/json; charset=utf-8";
            req.Accept = "application/json";
            req.UserAgent = "SoftwareRepository";

            StreamWriter streamOut = new(req.GetRequestStream(), System.Text.Encoding.ASCII);
            streamOut.Write(discoveryRequest);
            streamOut.Close();
            StreamReader streamIn = new(req.GetResponse().GetResponseStream());
            string discoveryResponse = streamIn.ReadToEnd();
            streamIn.Close();

            Discovery.RootObject disc = JsonSerializer.Deserialize<Discovery.RootObject>(discoveryResponse);

            string? id = disc.softwarePackages[0].id;

            string file = "";

            foreach (Discovery.File? filen in disc.softwarePackages[0].files)
            {
                if (filen.fileName.ToLower().EndsWith(".secwim"))
                {
                    file = filen.fileName;
                }
            }

            Console.WriteLine("(mmoENOSWDL) Accessing ENOSW packages from remote server...");

            req = (HttpWebRequest)WebRequest.Create("https://api.swrepository.com/rest-api/discovery/1/package/" + id + "/file/" + file + "/urls");

            req.Method = "GET";
            req.UserAgent = "SoftwareRepository";

            streamIn = new StreamReader(req.GetResponse().GetResponseStream());
            string fileresp = streamIn.ReadToEnd();
            streamIn.Close();

            Package.RootObject pkg = JsonSerializer.Deserialize<Package.RootObject>(fileresp);

            string? url = pkg.url;
            WebClient wc = new();
            Console.WriteLine("(mmoENOSWDL) Downloading ENOSW...");
            wc.DownloadFile(new Uri(url), outputfolder + file);
            Console.WriteLine("(mmoENOSWDL) Done.");
            return file;
        }


        public static long ToInt64LittleEndian(byte[] buffer, int offset)

        {

            return (long)ToUInt64LittleEndian(buffer, offset);

        }

        public static uint ToUInt32LittleEndian(byte[] buffer, int offset)

        {

            return (uint)(((buffer[offset + 3] << 24) & 0xFF000000U) | ((buffer[offset + 2] << 16) & 0x00FF0000U)

                | ((buffer[offset + 1] << 8) & 0x0000FF00U) | ((buffer[offset + 0] << 0) & 0x000000FFU));

        }



        public static ulong ToUInt64LittleEndian(byte[] buffer, int offset)

        {

            return (((ulong)ToUInt32LittleEndian(buffer, offset + 4)) << 32) | ToUInt32LittleEndian(buffer, offset + 0);

        }


        //
        // https://stackoverflow.com/questions/1471975/best-way-to-find-position-in-the-stream-where-given-byte-sequence-starts
        //

        public static long FindPosition(Stream stream, byte[] byteSequence)
        {
            if (byteSequence.Length > stream.Length)
            {
                return -1;
            }

            byte[] buffer = new byte[byteSequence.Length];

            BufferedStream bufStream = new(stream, byteSequence.Length);
            int i;

            while ((i = bufStream.Read(buffer, 0, byteSequence.Length)) == byteSequence.Length)
            {
                if (byteSequence.SequenceEqual(buffer))
                {
                    return bufStream.Position - byteSequence.Length;
                }
                else
                {
                    bufStream.Position -= byteSequence.Length - PadLeftSequence(buffer, byteSequence);
                }
            }

            return -1;
        }
        private static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            int i = 1;
            while (i < bytes.Length)
            {
                int n = bytes.Length - i;
                byte[] aux1 = new byte[n];
                byte[] aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                {
                    return i;
                }

                i++;
            }
            return i;
        }

        public static void ConvertSECWIM2WIM(string wimsec, string wim)
        {
            using FileStream? wimsecstream = File.OpenRead(wimsec);
            using FileStream? wimstream = File.OpenWrite(wim);
            using BinaryReader? wimsecreader = new(wimsecstream);
            using BinaryWriter? wimwriter = new(wimstream);
            byte[]? bytes = new byte[]
            {
                                            0x4D, 0x53, 0x57, 0x49, 0x4D
            };

            Console.WriteLine("(wimsec2wim) Finding Magic Bytes...");

            long start = FindPosition(wimsecstream, bytes);

            Console.WriteLine("(wimsec2wim) Found Magic Bytes at " + start);

            Console.WriteLine("(wimsec2wim) Finding WIM XML Data...");

            byte[]? endbytes = new byte[]
            {
                                0x3C, 0x00, 0x2F, 0x00, 0x57, 0x00, 0x49, 0x00, 0x4D, 0x00, 0x3E, 0x00
            };

            wimsecstream.Seek(start + 72, SeekOrigin.Begin);
            byte[] buffer = new byte[24];
            wimsecstream.Read(buffer, 0, 24);
            long may = ToInt64LittleEndian(buffer, 8);
            wimsecstream.Seek(start, SeekOrigin.Begin);

            Console.WriteLine("(wimsec2wim) Found WIM XML Data at " + start + may + 2);

            Console.WriteLine("(wimsec2wim) Writing " + may + 2 + " bytes...");

            wimwriter.Write(wimsecreader.ReadBytes((int)may + 2));

            Console.WriteLine("(wimsec2wim) Written " + may + 2 + " bytes");

            Console.WriteLine("(wimsec2wim) Writing WIM XML Data...");

            for (long i = wimsecstream.Position; i < wimsecstream.Length - endbytes.Length; i++)
            {
                if (BitConverter.ToString(wimsecreader.ReadBytes(12)) == BitConverter.ToString(endbytes))
                {
                    wimwriter.Write(endbytes);
                    break;
                }
                wimsecstream.Seek(-12, SeekOrigin.Current);
                wimwriter.Write(wimsecreader.ReadBytes(1));
            }

            Console.WriteLine("(wimsec2wim) Written WIM XML Data");
            Console.WriteLine("(wimsec2wim) Done.");
        }
    }
}