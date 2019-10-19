using Jerrycurl.Data.Metadata;
using Jerrycurl.Mvc.Metadata;
using Jerrycurl.Relations.Metadata;
using System;
using System.Linq;

namespace Jerrycurl.Mvc
{
    public static class DomainExtensions
    {
        public static void AddContract(this ISchemaStore schemas, IBindingContractResolver contract)
        {
            if (schemas == null)
                throw new ArgumentNullException(nameof(schemas));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            BindingMetadataBuilder builder = schemas.OfType<BindingMetadataBuilder>().FirstOrDefault();

            if (builder == null)
                throw new InvalidOperationException("No binding metadata builder found.");

            builder.Add(contract);
        }

        public static void AddContract(this ISchemaStore schemas, IJsonContractResolver contract)
        {
            if (schemas == null)
                throw new ArgumentNullException(nameof(schemas));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            JsonMetadataBuilder builder = schemas.OfType<JsonMetadataBuilder>().FirstOrDefault();

            if (builder == null)
                throw new InvalidOperationException("No JSON metadata builder found.");

            builder.Add(contract);
        }

        public static void AddContract(this ISchemaStore schemas, IRelationContractResolver contract)
        {
            if (schemas == null)
                throw new ArgumentNullException(nameof(schemas));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            RelationMetadataBuilder builder = schemas.OfType<RelationMetadataBuilder>().FirstOrDefault();

            if (builder == null)
                throw new InvalidOperationException("No relation metadata builder found.");

            builder.Add(contract);
        }
    }
}
