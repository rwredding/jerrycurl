using System;
using System.Collections.Generic;
using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc
{
    public class ProcLookup : IProcLookup
    {
        private readonly Dictionary<ProcLookupKey, string> nameMap = new Dictionary<ProcLookupKey, string>();
        private readonly Dictionary<string, int> prefixCount = new Dictionary<string, int>();

        private string FromKey(ProcLookupKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (this.nameMap.TryGetValue(key, out string alias))
                return alias;

            this.prefixCount.TryGetValue(key.Prefix, out int prefixes);
            this.prefixCount[key.Prefix] = prefixes + 1;

            return this.nameMap[key] = key.Prefix + prefixes;
        }

        public string Custom(string prefix, IProjectionIdentity identity = null, MetadataIdentity metadata = null, IField field = null) => this.FromKey(new ProcLookupKey(prefix, identity, metadata, field));

        public string Parameter(IProjectionIdentity identity, IField field) => this.Custom("P", identity, field: field);
        public string Parameter(IProjectionIdentity identity, MetadataIdentity metadata) => this.Custom("P", identity, metadata: metadata);
        public string Table(IProjectionIdentity identity, MetadataIdentity metadata) => this.Custom("T", identity, metadata);
        public string Variable(IProjectionIdentity identity, IField field) => this.Custom("V", identity, field: field);
    }
}
