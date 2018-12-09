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

using System.Collections.Generic;
using System.Xml.Serialization;

namespace BSPExtractor
{
    public static class XmlMum
	{
		[XmlRoot(ElementName="assemblyIdentity", Namespace="urn:schemas-microsoft-com:asm.v3")]
		public class AssemblyIdentity {
			[XmlAttribute(AttributeName="name")]
			public string Name { get; set; }
			[XmlAttribute(AttributeName="version")]
			public string Version { get; set; }
			[XmlAttribute(AttributeName="language")]
			public string Language { get; set; }
			[XmlAttribute(AttributeName="processorArchitecture")]
			public string ProcessorArchitecture { get; set; }
			[XmlAttribute(AttributeName="publicKeyToken")]
			public string PublicKeyToken { get; set; }
			[XmlAttribute(AttributeName="buildType")]
			public string BuildType { get; set; }
			[XmlAttribute(AttributeName="versionScope")]
			public string VersionScope { get; set; }
		}

		[XmlRoot(ElementName="phoneInformation", Namespace="urn:schemas-microsoft-com:asm.v3")]
		public class PhoneInformation {
			[XmlAttribute(AttributeName="phoneRelease")]
			public string PhoneRelease { get; set; }
			[XmlAttribute(AttributeName="phoneOwnerType")]
			public string PhoneOwnerType { get; set; }
			[XmlAttribute(AttributeName="phoneOwner")]
			public string PhoneOwner { get; set; }
			[XmlAttribute(AttributeName="phoneComponent")]
			public string PhoneComponent { get; set; }
			[XmlAttribute(AttributeName="phoneSubComponent")]
			public string PhoneSubComponent { get; set; }
			[XmlAttribute(AttributeName="phoneGroupingKey")]
			public string PhoneGroupingKey { get; set; }
		}

		[XmlRoot(ElementName="file", Namespace="urn:schemas-microsoft-com:asm.v3")]
		public class File {
			[XmlAttribute(AttributeName="name")]
			public string Name { get; set; }
			[XmlAttribute(AttributeName="size")]
			public string Size { get; set; }
			[XmlAttribute(AttributeName="staged")]
			public string Staged { get; set; }
			[XmlAttribute(AttributeName="compressed")]
			public string Compressed { get; set; }
			[XmlAttribute(AttributeName="sourcePackage")]
			public string SourcePackage { get; set; }
			[XmlAttribute(AttributeName="embeddedSign")]
			public string EmbeddedSign { get; set; }
			[XmlAttribute(AttributeName="cabpath")]
			public string Cabpath { get; set; }
		}

		[XmlRoot(ElementName="customInformation", Namespace="urn:schemas-microsoft-com:asm.v3")]
		public class CustomInformation {
			[XmlElement(ElementName="phoneInformation", Namespace="urn:schemas-microsoft-com:asm.v3")]
			public PhoneInformation PhoneInformation { get; set; }
			[XmlElement(ElementName="file", Namespace="urn:schemas-microsoft-com:asm.v3")]
			public List<File> File { get; set; }
		}

		[XmlRoot(ElementName="component", Namespace="urn:schemas-microsoft-com:asm.v3")]
		public class Component {
			[XmlElement(ElementName="assemblyIdentity", Namespace="urn:schemas-microsoft-com:asm.v3")]
			public AssemblyIdentity AssemblyIdentity { get; set; }
		}

		[XmlRoot(ElementName="update", Namespace="urn:schemas-microsoft-com:asm.v3")]
		public class Update {
			[XmlElement(ElementName="component", Namespace="urn:schemas-microsoft-com:asm.v3")]
			public Component Component { get; set; }
			[XmlAttribute(AttributeName="name")]
			public string Name { get; set; }
		}

		[XmlRoot(ElementName="package", Namespace="urn:schemas-microsoft-com:asm.v3")]
		public class Package {
			[XmlElement(ElementName="customInformation", Namespace="urn:schemas-microsoft-com:asm.v3")]
			public CustomInformation CustomInformation { get; set; }
			[XmlElement(ElementName="update", Namespace="urn:schemas-microsoft-com:asm.v3")]
			public Update Update { get; set; }
			[XmlAttribute(AttributeName="identifier")]
			public string Identifier { get; set; }
			[XmlAttribute(AttributeName="releaseType")]
			public string ReleaseType { get; set; }
			[XmlAttribute(AttributeName="restart")]
			public string Restart { get; set; }
			[XmlAttribute(AttributeName="targetPartition")]
			public string TargetPartition { get; set; }
			[XmlAttribute(AttributeName="binaryPartition")]
			public string BinaryPartition { get; set; }
		}

		[XmlRoot(ElementName="assembly", Namespace="urn:schemas-microsoft-com:asm.v3")]
		public class Assembly {
			[XmlElement(ElementName="assemblyIdentity", Namespace="urn:schemas-microsoft-com:asm.v3")]
			public AssemblyIdentity AssemblyIdentity { get; set; }

            [XmlArray(ElementName="registryKeys", Namespace = "urn:schemas-microsoft-com:asm.v3")]
            [XmlArrayItem(ElementName = "registryKey")]
            public List<RegistryKey> RegistryKeys { get; set; }

            [XmlElement(ElementName="package", Namespace="urn:schemas-microsoft-com:asm.v3")]
			public Package Package { get; set; }
			[XmlAttribute(AttributeName="xmlns")]
			public string Xmlns { get; set; }
			[XmlAttribute(AttributeName="manifestVersion")]
			public string ManifestVersion { get; set; }
			[XmlAttribute(AttributeName="displayName")]
			public string DisplayName { get; set; }
			[XmlAttribute(AttributeName="company")]
			public string Company { get; set; }
			[XmlAttribute(AttributeName="copyright")]
			public string Copyright { get; set; }

            //TODO: trustInfo
		}

        [XmlRoot(ElementName="registryKey", Namespace="urn:schemas-microsoft-com:asm.v3")]
        public class RegistryKey {
            
            [XmlElement(ElementName="registryValue", Namespace="urn:schemas-microsoft-com:asm.v3")]
            public List<RegistryValue> RegistryValues { get; set; }
            [XmlAttribute(AttributeName="keyName", Namespace="urn:schemas-microsoft-com:asm.v3")]
            public string KeyName { get; set; }
            [XmlElement(ElementName="securityDescriptor", Namespace="urn:schemas-microsoft-com:asm.v3")]
            public SecurityDescriptor SecurityDescriptor { get; set; }
        }

        [XmlRoot(ElementName="securityDescriptor", Namespace="urn:schemas-microsoft-com:asm.v3")]
        public class SecurityDescriptor {
            [XmlAttribute(AttributeName="name", Namespace="urn:schemas-microsoft-com:asm.v3")]
            public string Name { get; set; }
        }

        [XmlRoot(ElementName="registryValue", Namespace="urn:schemas-microsoft-com:asm.v3")]
        public class RegistryValue {
            [XmlAttribute(AttributeName="name")]
            public string Name { get; set; }
            [XmlAttribute(AttributeName="value")]
            public string Value { get; set; }
            [XmlAttribute(AttributeName="valueType")]
            public string ValueType { get; set; }
            [XmlAttribute(AttributeName="mutable")]
            public string Mutable { get; set; }
            [XmlAttribute(AttributeName="operationHint")]
            public string OperationHint { get; set; } //e.g: replace
        }
	}
	
    public static class XmlDsm
    {
        [XmlRoot(ElementName = "Version", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Version
        {
            [XmlAttribute(AttributeName = "Major")]
            public string Major { get; set; }
            [XmlAttribute(AttributeName = "Minor")]
            public string Minor { get; set; }
            [XmlAttribute(AttributeName = "QFE")]
            public string QFE { get; set; }
            [XmlAttribute(AttributeName = "Build")]
            public string Build { get; set; }
        }

        [XmlRoot(ElementName = "Identity", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Identity
        {
            [XmlElement(ElementName = "Owner", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string Owner { get; set; }
            [XmlElement(ElementName = "Component", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string Component { get; set; }
            [XmlElement(ElementName = "SubComponent", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string SubComponent { get; set; }
            [XmlElement(ElementName = "Version", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Version Version { get; set; }
        }

        [XmlRoot(ElementName = "FileEntry", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class FileEntry
        {
            [XmlElement(ElementName = "FileType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string FileType { get; set; }
            [XmlElement(ElementName = "DevicePath", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string DevicePath { get; set; }
            [XmlElement(ElementName = "CabPath", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string CabPath { get; set; }
            [XmlElement(ElementName = "Attributes", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string Attributes { get; set; }
            [XmlElement(ElementName = "SourcePackage", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string SourcePackage { get; set; }
            [XmlElement(ElementName = "FileSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string FileSize { get; set; }
            [XmlElement(ElementName = "CompressedFileSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string CompressedFileSize { get; set; }
            [XmlElement(ElementName = "StagedFileSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string StagedFileSize { get; set; }
        }

        [XmlRoot(ElementName = "Files", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Files
        {
            [XmlElement(ElementName = "FileEntry", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<FileEntry> FileEntry { get; set; }
        }

        [XmlRoot(ElementName = "Package", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Package
        {
            [XmlElement(ElementName = "Identity", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Identity Identity { get; set; }
            [XmlElement(ElementName = "ReleaseType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string ReleaseType { get; set; }
            [XmlElement(ElementName = "OwnerType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string OwnerType { get; set; }
            [XmlElement(ElementName = "BuildType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string BuildType { get; set; }
            [XmlElement(ElementName = "CpuType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string CpuType { get; set; }
            [XmlElement(ElementName = "Partition", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string Partition { get; set; }
            [XmlElement(ElementName = "GroupingKey", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string GroupingKey { get; set; }
            [XmlElement(ElementName = "IsRemoval", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string IsRemoval { get; set; }
            [XmlElement(ElementName = "Files", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Files Files { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }
}