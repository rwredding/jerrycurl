using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Jerrycurl.Relations.Internal;

namespace Jerrycurl.Relations.Metadata
{
    internal class Schema : ISchema
    {
        private readonly ConcurrentDictionary<MetadataKey, object> entries = new ConcurrentDictionary<MetadataKey, object>();
        private readonly SchemaStore store;
        private readonly ConcurrentDictionary<Type, ReaderWriterLockSlim> locks = new ConcurrentDictionary<Type, ReaderWriterLockSlim>();

        public Type Model { get; }
        public IMetadataNotation Notation => this.store.Notation;

        public Schema(SchemaStore store, Type model)
        {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public void AddMetadata<TMetadata>(TMetadata metadata)
            where TMetadata : IMetadata
        {
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            if (metadata.Identity == null)
                throw new ArgumentNullException(nameof(metadata.Identity));

            MetadataKey key = MetadataKey.FromIdentity<TMetadata>(metadata.Identity);

            if (!this.entries.TryAdd(key, metadata))
                throw new InvalidOperationException("Metadata already added.");
        }

        public TMetadata GetMetadata<TMetadata>()
            where TMetadata : IMetadata
            => this.GetMetadata<TMetadata>(this.Notation.Model());

        internal TMetadata GetMetadataFromCache<TMetadata>(string name)
            where TMetadata : IMetadata
        {
            MetadataKey key = new MetadataKey(typeof(TMetadata), name, this.Notation.Comparer);

            if (this.entries.TryGetValue(key, out object value))
                return (TMetadata)value;

            return default;
        }

        private ReaderWriterLockSlim GetLock<TMetadata>() => this.locks.GetOrAdd(typeof(TMetadata), _ => new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion));
        private void RemoveLock<TMetadata>() => this.locks.TryRemove(typeof(TMetadata), out _);

        public TMetadata GetMetadata<TMetadata>(string name)
            where TMetadata : IMetadata
        {
            MetadataKey key = new MetadataKey(typeof(TMetadata), name, this.Notation.Comparer);
            ReaderWriterLockSlim slim = this.GetLock<TMetadata>();

            try
            {
                slim.EnterReadLock();

                if (this.entries.TryGetValue(key, out object value))
                    return (TMetadata)value;
            }
            catch (LockRecursionException ex)
            {
                throw new MetadataBuilderException("To mitigate async deadlocks, fetching metadata recursively through ISchema is not supported.", ex);
            }
            finally
            {
                if (slim.IsReadLockHeld)
                    slim.ExitReadLock();
            }


            slim.EnterWriteLock();

            try
            {
                foreach (IMetadataBuilder<TMetadata> metadataBuilder in this.store.OfType<IMetadataBuilder<TMetadata>>())
                {
                    MetadataIdentity identity = new MetadataIdentity(this, name);
                    MetadataBuilderContext context = new MetadataBuilderContext(identity, this);

                    TMetadata metadata = metadataBuilder.GetMetadata(context);

                    if (metadata != null)
                        return metadata;
                }

                return default;
            }
            finally
            {
                if (slim.IsWriteLockHeld)
                    slim.ExitWriteLock();

                this.RemoveLock<TMetadata>();
            }
        }

        public bool Equals(ISchema other) => this.Model.Equals(other?.Model);
        public override bool Equals(object obj) => (obj is ISchema other && this.Equals(other));
        public override int GetHashCode() => this.Model.GetHashCode();
    }
}
