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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BSPExtractor
{
    public class CbsToReg
    {
        List<RegistryCollection> _registries;
        string _comment;
        public string Comment { get => _comment; set => _comment = value; }

        public CbsToReg() => _registries = new List<RegistryCollection>();

        public void Add(RegistryCollection reg) => _registries.Add(reg);

        string KeyNameReplace(string s, string software, string system) =>
            s.Replace("HKEY_LOCAL_MACHINE\\SOFTWARE", "HKEY_LOCAL_MACHINE\\" + software.ToUpper())
             .Replace("HKEY_LOCAL_MACHINE\\SYSTEM", "HKEY_LOCAL_MACHINE\\" + system.ToUpper());

        public string Build(string softwareName, string systemName)
        {
            StringBuilder str = new StringBuilder();
            str.Append("Windows Registry Editor Version 5.00\r\n");
            str.Append(Comment + "\r\n\r\n");

            foreach (var registry in _registries)
            {
                str.Append("[" + KeyNameReplace(registry.KeyName.ToUpper(), softwareName, systemName) + "]" + "\r\n");

                foreach (var registryValue in registry.RegistryValues)
                {
                    var keyName = (registryValue.Name == "") ? "@" : "\"" + registryValue.Name + "\"";

                    str.Append(keyName.Replace("\\", @"\\") + "=" + ConvertValueToString(registryValue.Value, registryValue.ValueType) + "\r\n");
                }
                str.Append("\r\n");
            }
            return str.ToString();
        }

        void FinalizeSlice(string output, string reg, string type) => File.WriteAllText(output + type + ".reg", reg, Encoding.Unicode);

        string ConvertValueToString(string value, string valueType)
        {
            if (value == null)
                value = "";

            if (valueType == "REG_DWORD")
                return "dword:" + value.Replace("0x", "");
            if (valueType == "REG_QWORD")
                return "hex(b):" + string.Join(",", SplitInParts(value, 2));
            else if (valueType == "REG_SZ")
                return "\"" + value.Replace(@"\", @"\\") + "\"";
            else if (valueType == "REG_EXPAND_SZ")
                return "hex(2):" + string.Join(",", SplitInParts(ToHex(value.Replace("\"", "") + "\0"), 2));
            else if (valueType == "REG_BINARY")
                return "hex:" + string.Join(",", SplitInParts(value, 2));
            else if (valueType == "REG_NONE")
                return "hex(0):";
            else if (valueType == "REG_MULTI_SZ")
            {
                var finalString = "";

                foreach (Match match in rg_splitComma.Matches(value))
                {
                    var tmp = match.Value.TrimStart(',');

                    if (tmp.StartsWith("\""))
                        tmp = tmp.Substring(1);
                    if (tmp.EndsWith("\""))
                        tmp = tmp.Remove(tmp.Length - 1);

                    finalString += (tmp + "\0");
                }
                finalString += "\0";

                return "hex(7):" + string.Join(",", SplitInParts(ToHex(finalString), 2)); //fix for the quots
            }
            else if (OnlyHexInString(valueType))
                return $"hex({valueType}):" + string.Join(",", SplitInParts(value, 2)); //??

            return "$([INVALID_DATA])!!"; //??
        }

        Regex rg_checkHex = new Regex(@"\A\b[0-9a-fA-F]+\b\Z", RegexOptions.Compiled);
        Regex rg_splitComma = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
        public bool OnlyHexInString(string test) => rg_checkHex.IsMatch(test);

        public static IEnumerable<String> SplitInParts(string s, int partLength)
        {
            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static string ToHex(string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                var splittedParts = SplitInParts(string.Format("{0:X4}", (int)c), 2).ToList();
                splittedParts.Reverse();
                sb.AppendFormat(String.Join("", splittedParts));
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
