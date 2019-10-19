using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Data.Metadata
{
    [Flags]
    public enum ReferenceMetadataFlags
    {
        None = 0,
        CandidateKey = 1,
        ForeignKey = 2,
        Key = ForeignKey | CandidateKey,
    }
}
