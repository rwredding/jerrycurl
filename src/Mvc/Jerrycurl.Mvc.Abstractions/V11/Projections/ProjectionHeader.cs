using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.V11;

namespace Jerrycurl.Mvc.V11.Projections
{
    public class ProjectionHeader : RelationHeader
    {
        public ProjectionHeader(ProjectionAttribute2 source, IReadOnlyList<ProjectionAttribute2> attributes)
            : base(source?.Schema, attributes)
        {

        }
    }
}
