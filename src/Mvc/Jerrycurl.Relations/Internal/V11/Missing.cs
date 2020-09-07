﻿using Jerrycurl.Diagnostics;
using Jerrycurl.Relations.Metadata;
using System;
using System.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Relations.Internal.V11
{
    [DebuggerDisplay("{Identity.Name}: {ToString(),nq}")]
    internal class Missing<TValue> : IField2
    {
        public FieldIdentity Identity { get; }
        public object Value { get; }
        public IField2 Model { get; }
        public FieldType2 Type { get; } = FieldType2.Missing;
        public object CurrentValue { get; set; }
        public IRelationMetadata Metadata { get; }

        public Missing(string name, IRelationMetadata metadata, IField2 model)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.Identity = new FieldIdentity(metadata.Identity, name);
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void Bind() => throw new InvalidOperationException();

        public bool Equals(IField2 other) => Equality.Combine(this, other, m => m.Model, m => m.Identity);
        public override bool Equals(object obj) => (obj is IField2 field && this.Equals(field));
        public override int GetHashCode() => HashCode.Combine(this.Model, this.Identity);

        public override string ToString() => "<missing>";
    }
}