using System;

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