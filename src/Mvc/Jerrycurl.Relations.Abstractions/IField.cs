using Jerrycurl.Relations.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Relations
{
    public interface IField : IEquatable<IField>
    {
        FieldIdentity Identity { get; }
        object Value { get; }
        IField Model { get; }
        FieldType Type { get; }

        void Bind(object newValue);
    }
}