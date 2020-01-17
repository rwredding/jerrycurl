using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jerrycurl.Collections;

namespace Jerrycurl.Relations.Metadata
{
    public class SchemaStore : Collection<IMetadataBuilder>, ISchemaStore
    {
        private readonly ConcurrentDictionary<Type, ISchema> entries = new ConcurrentDictionary<Type, ISchema>();

        public IMetadataNotation Notation { get; }

        public SchemaStore(IMetadataNotation notation)
        {
            this.Notation = notation ?? throw new ArgumentNullException(nameof(notation));
        }

        public SchemaStore(IMetadataNotation notation, params IMetadataBuilder[] builders)
            : this(notation, (IEnumerable<IMetadataBuilder>)builders)
        {

        }

        public SchemaStore(IMetadataNotation notation, IEnumerable<IMetadataBuilder> builders)
            : this(notation)
        {
            foreach (IMetadataBuilder builder in builders?.NotNull() ?? Array.Empty<IMetadataBuilder>())
                this.Add(builder);
        }

        public ISchema GetSchema(Type modelType)
        {
            if (modelType == null)
                throw new ArgumentNullException(nameof(modelType));

            return this.entries.GetOrAdd(modelType, this.CreateSchema);
        }

        private Schema CreateSchema(Type modelType)
        {
            Schema newSchema = new Schema(this, modelType);

            foreach (IMetadataBuilder builder in this)
            {
                MetadataIdentity newIdentity = new MetadataIdentity(newSchema, this.Notation.Model());
                MetadataBuilderContext context = new MetadataBuilderContext(newIdentity, newSchema);

                builder.Initialize(context);
            }

            return newSchema;
        }
    }
}
