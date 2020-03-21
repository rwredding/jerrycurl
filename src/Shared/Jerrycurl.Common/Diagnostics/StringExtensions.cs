using System;

namespace Jerrycurl.Diagnostics
{
    internal static class StringExtensions
    {
        public static int GetStableHashCode(this string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            // courtesy of @andrewlock, thanks! https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < s.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ s[i];
                    if (i == s.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ s[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}