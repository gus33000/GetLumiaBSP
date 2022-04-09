using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace borderline
{
    internal static class Helpers
    {
        internal static XElement StripNamespaces(XElement root)
        {
            XElement res = new XElement(
            root.Name.LocalName,
            root.HasElements ?
                root.Elements().Select(el => StripNamespaces(el)) :
                (object)root.Value
                );

            res.ReplaceAttributes(
                root.Attributes().Where(attr => (!attr.IsNamespaceDeclaration)));

            return res;
        }

        internal static ulong sxsHash(List<string> attribs, List<string> values)
        {
            ulong hash = 0;
            ulong hash_attr;
            ulong hash_val;
            ulong both_hashes;
            int index;

            for (index = 0; index < values.Count; index++)
            {
                if (values[index] == "none") continue;
                values[index] = values[index].ToLower();

                hash_attr = hash_string(attribs[index]);
                hash_val = hash_string(values[index]);
                both_hashes = hash_val + 0x1FFFFFFF7 * hash_attr;
                hash = both_hashes + 0x1FFFFFFF7 * hash;

            }
            return hash;
        }

        private static uint hash_char(uint hash, byte val)
        {
            return hash * 0x1003F + val;
        }

        private static ulong hash_string(string str)
        {
            ulong[] hash = new ulong[4];
            int index;
            ulong final;

            for (index = 0; index < str.Length; index++)
            {
                hash[index % 4] = hash_char((uint)hash[index % 4], (byte)str[index]);

            }

            final = hash[0] * 0x1E5FFFFFD27 + hash[1] * 0xFFFFFFDC00000051 + hash[2] * 0x1FFFFFFF7 + hash[3];
            return final;
        }
    }
}
