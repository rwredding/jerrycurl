using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations.Test.Metadata;
using Jerrycurl.Relations.Test.Models;
using Jerrycurl.Relations.Tests.Models;
using Jerrycurl.Test;
using Shouldly;
using System;
using System.Linq;

namespace Jerrycurl.Relations.Tests
{
    public class MetadataTests
    {
        public void Test_MetadataBuilder_DisallowsRecursiveCalls()
        {
            SchemaStore store = new SchemaStore(new DotNotation(StringComparer.Ordinal)) { new RecursiveMetadataBuilder() };

            ISchema schema = store.GetSchema(typeof(TupleModel));

            schema.ShouldNotBeNull();

            Should.Throw<MetadataBuilderException>(() => schema.GetMetadata<CustomMetadata>("Item.Value"));
        }

        public void Test_MetadataNotation_StringComparison()
        {
            SchemaStore sensitive = new SchemaStore(new DotNotation(StringComparer.Ordinal)) { new RelationMetadataBuilder() };
            SchemaStore insensitive = new SchemaStore(new DotNotation()) { new RelationMetadataBuilder() };

            IRelationMetadata sensitive1 = sensitive.GetSchema(typeof(TupleModel)).GetMetadata<IRelationMetadata>("List.Item.Name");
            IRelationMetadata sensitive2 = sensitive.GetSchema(typeof(TupleModel)).GetMetadata<IRelationMetadata>("list.item.name");

            IRelationMetadata insensitive1 = insensitive.GetSchema(typeof(TupleModel)).GetMetadata<IRelationMetadata>("List.Item.Name");
            IRelationMetadata insensitive2 = insensitive.GetSchema(typeof(TupleModel)).GetMetadata<IRelationMetadata>("list.item.name");

            sensitive1.ShouldNotBeNull();
            sensitive2.ShouldBeNull();

            insensitive1.ShouldNotBeNull();
            insensitive2.ShouldNotBeNull();
        }

        public void Test_Metadata_WithCustomListContract()
        {
            RelationMetadataBuilder builder = new RelationMetadataBuilder() { new CustomListContractResolver() };
            SchemaStore customStore = new SchemaStore(new DotNotation()) { builder };

            ISchema schema1 = DatabaseHelper.Default.Schemas.GetSchema(typeof(CustomModel));
            ISchema schema2 = customStore.GetSchema(typeof(CustomModel));

            IRelationMetadata notFound = schema1.GetMetadata<IRelationMetadata>("Values.Item");
            IRelationMetadata found = schema2.GetMetadata<IRelationMetadata>("Values.Item");

            notFound.ShouldBeNull();
            found.ShouldNotBeNull();
            found.Type.ShouldBe(typeof(int));
            found.Annotations.OfType<CustomAttribute>().FirstOrDefault().ShouldNotBeNull();
        }

        public void Test_Metadata_WithInvalidListContract_Throws()
        {
            RelationMetadataBuilder builder = new RelationMetadataBuilder() { new InvalidContractResolver() };
            SchemaStore customStore = new SchemaStore(new DotNotation()) { builder };

            ISchema schema = customStore.GetSchema(typeof(CustomModel));

            Should.Throw<MetadataBuilderException>(() => schema.GetMetadata<IRelationMetadata>("List1"));
        }
    }
}
