using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11
{
    public interface IField2 : IEquatable<IField2>
    {
        FieldIdentity Identity { get; }
        object Value { get; }
        object CurrentValue { get; set; }
        IField2 Model { get; }
        FieldType2 Type { get; }
        IRelationMetadata Metadata { get; }

        void Bind();
    }
}
