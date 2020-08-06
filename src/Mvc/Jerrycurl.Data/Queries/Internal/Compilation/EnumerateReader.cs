using System.Data;

namespace Jerrycurl.Data.Queries.Internal.Compilation
{
    internal delegate TItem EnumerateReader<TItem>(IDataReader dataReader);
}
