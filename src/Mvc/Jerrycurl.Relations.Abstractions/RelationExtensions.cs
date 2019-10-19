using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jerrycurl.Relations
{
    public static class RelationExtensions
    {
        public static ITuple Row(this IRelation relation) => relation.FirstOrDefault();
        public static IField Scalar(this IRelation relation) => relation.FirstOrDefault()?.FirstOrDefault();
        public static IEnumerable<IField> Column(this IRelation relation) => relation.Select(t => t.FirstOrDefault());
    }
}
