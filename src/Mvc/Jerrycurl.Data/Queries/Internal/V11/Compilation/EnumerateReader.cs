using System.Data;

namespace Jerrycurl.Data.Queries.Internal.V11.Factories
{
    internal delegate TItem EnumerateReader<TItem>(IDataReader dataReader);
}
