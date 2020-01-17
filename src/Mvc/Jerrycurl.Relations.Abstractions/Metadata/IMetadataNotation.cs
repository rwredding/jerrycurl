using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jerrycurl.Relations.Metadata
{
    public interface IMetadataNotation
    {
        IEqualityComparer<string> Comparer { get; }

        string Model();
        string Combine(params string[] parts);
        string Combine(string part1, string part2);
        string Lambda(LambdaExpression expression);
        string Path(string from, string to);
        string Index(string name, int index);
        string Parent(string name);
        string Member(string name);
        bool Equals(string name1, string name2);
    }
}
