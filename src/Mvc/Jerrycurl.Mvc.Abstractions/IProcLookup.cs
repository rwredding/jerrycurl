using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Mvc
{
    public interface IProcLookup
    {
        string Parameter(IProjectionIdentity identity, IField field);
        string Parameter(IProjectionIdentity identity, MetadataIdentity metadata);
        string Variable(IProjectionIdentity identity, IField field);
        string Table(IProjectionIdentity identity, MetadataIdentity metadata);

        string Custom(string prefix, IProjectionIdentity identity, MetadataIdentity metadata = null, IField field = null);
    }
}
