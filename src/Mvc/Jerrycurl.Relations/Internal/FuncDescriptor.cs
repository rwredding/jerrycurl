using System;
using System.Collections;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal
{
    internal class FuncDescriptor
    {
        public RelationIdentity Identity { get; set; }
        public MetadataIdentity[] Lists { get; set; }
        public Action<IField, IEnumerator[], IField[]>[] Factories { get; set; }
        public int VisibleDegree { get; set; }
        public int Degree { get; set; }
    }
}
