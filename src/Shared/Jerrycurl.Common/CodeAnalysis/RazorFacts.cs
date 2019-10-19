using System.Collections.Generic;

namespace Jerrycurl.CodeAnalysis
{
    internal static class RazorFacts
    {
        public static IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "System";
            yield return "System.Collections.Generic";
            yield return "System.Linq.Expressions";
            yield return "Jerrycurl.Mvc";
            yield return "Jerrycurl.Mvc.Projections";
            yield return "Jerrycurl.Mvc.Sql";
        }
    }
}
