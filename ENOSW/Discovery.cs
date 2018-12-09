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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetLumiaBSP
{
    class Discovery
    {
        public class Checksum
        {
            public string value { get; set; }
            public string type { get; set; }
        }

        public class File
        {
            public int fileSize { get; set; }
            public string fileType { get; set; }
            public string fileName { get; set; }
            public List<Checksum> checksum { get; set; }
        }

        public class SoftwarePackage
        {
            public List<string> manufacturerHardwareModel { get; set; }
            public object customerName { get; set; }
            public List<File> files { get; set; }
            public string packageType { get; set; }
            public object manufacturerVariantName { get; set; }
            public string packageSubRevision { get; set; }
            public object packageSubtitle { get; set; }
            public object packageDescription { get; set; }
            public object manufacturerModelName { get; set; }
            public object manufacturerPackageId { get; set; }
            public string id { get; set; }
            public object manufacturerHardwareVariant { get; set; }
            public object operatorName { get; set; }
            public string packageRevision { get; set; }
            public string packageState { get; set; }
            public string packageTitle { get; set; }
            public string manufacturerName { get; set; }
            public List<string> manufacturerPlatformId { get; set; }
            public object extendedAttributes { get; set; }
            public string manufacturerProductLine { get; set; }
            public List<string> packageClass { get; set; }
        }

        public class RootObject
        {
            public List<SoftwarePackage> softwarePackages { get; set; }
        }
    }
}
