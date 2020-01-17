using Jerrycurl.Mvc.Projections;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Mvc
{
    public interface IProcLookup
    {
        string Parameter(IProjectionIdentity identity, IField field);
        string Parameter(IProjectionIdentity identity, MetadataIdentity metadata);
        string Variable(IProjectionIdentity identity, IField field);
        string Table(IProjectionIdentity identity, MetadataIdentity metadata);

        string Custom(string prefix, IProjectionIdentity identity = null, MetadataIdentity metadata = null, IField field = null);
    }
}
