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

using System.Text;
using System.Text.RegularExpressions;

namespace BSPExtractor
{
    public class CbsToReg
    {
        private readonly List<RegistryCollection> _registries;
        private string _comment;
        public string Comment { get => _comment; set => _comment = value; }

        public CbsToReg()
        {
            _registries = new List<RegistryCollection>();
        }

        public void Add(RegistryCollection reg)
        {
            _registries.Add(reg);
        }

        private string KeyNameReplace(string s, string software, string system)
        {
            return s.Replace("HKEY_LOCAL_MACHINE\\SOFTWARE", "HKEY_LOCAL_MACHINE\\" + software.ToUpper())
.Replace("HKEY_LOCAL_MACHINE\\SYSTEM", "HKEY_LOCAL_MACHINE\\" + system.ToUpper());
        }

        public string Build(string softwareName, string systemName)
        {
            StringBuilder str = new();
            str.Append("Windows Registry Editor Version 5.00\r\n");
            str.Append(Comment + "\r\n\r\n");

            foreach (RegistryCollection registry in _registries)
            {
                str.Append("[" + KeyNameReplace(registry.KeyName.ToUpper(), softwareName, systemName) + "]" + "\r\n");

                foreach (RegistryValue registryValue in registry.RegistryValues)
                {
                    string? keyName = (registryValue.Name == "") ? "@" : "\"" + registryValue.Name + "\"";

                    str.Append(keyName.Replace("\\", @"\\") + "=" + ConvertValueToString(registryValue.Value, registryValue.ValueType) + "\r\n");
                }
                str.Append("\r\n");
            }
            return str.ToString();
        }

        private void FinalizeSlice(string output, string reg, string type)
        {
            File.WriteAllText(output + type + ".reg", reg, Encoding.Unicode);
        }

        private string ConvertValueToString(string value, string valueType)
        {
            if (value == null)
            {
                value = "";
            }

            if (valueType == "REG_DWORD")
            {
                return "dword:" + value.Replace("0x", "");
            }

            if (valueType == "REG_QWORD")
            {
                return "hex(b):" + string.Join(",", SplitInParts(value, 2));
            }
            else if (valueType == "REG_SZ")
            {
                return "\"" + value.Replace(@"\", @"\\") + "\"";
            }
            else if (valueType == "REG_EXPAND_SZ")
            {
                return "hex(2):" + string.Join(",", SplitInParts(ToHex(value.Replace("\"", "") + "\0"), 2));
            }
            else if (valueType == "REG_BINARY")
            {
                return "hex:" + string.Join(",", SplitInParts(value, 2));
            }
            else if (valueType == "REG_NONE")
            {
                return "hex(0):";
            }
            else if (valueType == "REG_MULTI_SZ")
            {
                string? finalString = "";

                foreach (Match match in rg_splitComma.Matches(value))
                {
                    string? tmp = match.Value.TrimStart(',');

                    if (tmp.StartsWith("\""))
                    {
                        tmp = tmp[1..];
                    }

                    if (tmp.EndsWith("\""))
                    {
                        tmp = tmp.Remove(tmp.Length - 1);
                    }

                    finalString += (tmp + "\0");
                }
                finalString += "\0";

                return "hex(7):" + string.Join(",", SplitInParts(ToHex(finalString), 2)); //fix for the quots
            }
            else if (OnlyHexInString(valueType))
            {
                return $"hex({valueType}):" + string.Join(",", SplitInParts(value, 2)); //??
            }

            return "$([INVALID_DATA])!!"; //??
        }

        private readonly Regex rg_checkHex = new(@"\A\b[0-9a-fA-F]+\b\Z", RegexOptions.Compiled);
        private readonly Regex rg_splitComma = new("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
        public bool OnlyHexInString(string test)
        {
            return rg_checkHex.IsMatch(test);
        }

        public static IEnumerable<string> SplitInParts(string s, int partLength)
        {
            for (int i = 0; i < s.Length; i += partLength)
            {
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
            }
        }

        public static string ToHex(string s)
        {
            StringBuilder sb = new();
            foreach (char c in s)
            {
                List<string>? splittedParts = SplitInParts(string.Format("{0:X4}", (int)c), 2).ToList();
                splittedParts.Reverse();
                sb.AppendFormat(string.Join("", splittedParts));
            }
            return sb.ToString();
        }
    }

    public struct RegistryCollection
    {
        public RegistryCollection(string keyName, List<RegistryValue> registryValues)
        {
            KeyName = keyName;
            RegistryValues = registryValues;
        }
        public string KeyName { get; set; }
        public List<RegistryValue> RegistryValues { get; set; }
    }

    public struct RegistryValue
    {
        public RegistryValue(string name, string value, string valueType, string mutable, string operationHint)
        {
            Name = name;
            Value = value;
            ValueType = valueType;
            Mutable = mutable;
            OperationHint = operationHint;
        }
        public string Name { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public string Mutable { get; set; }
        public string OperationHint { get; set; }
    }
}
