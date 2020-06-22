using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Relations.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jerrycurl.Extensions.EntityFrameworkCore.Metadata.Builders
{
    public class EntityFrameworkCoreContractResolver : IRelationContractResolver
    {
        private IEntityType[] entities;

        public EntityFrameworkCoreContractResolver(DbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            this.InitializeEntities(dbContext);
        }

        private void InitializeEntities(DbContext dbContext)
        {
            this.entities = dbContext.Model.GetEntityTypes().ToArray();
        }

        public IRelationListContract GetListContract(IRelationMetadata metadata) => null;

        public IEnumerable<Attribute> GetAnnotationContract(IRelationMetadata metadata)
        {
            IEntityType entity = this.entities.FirstOrDefault(e => e.ClrType == metadata.Type);
            IEntityType parentEntity = this.entities.FirstOrDefault(e => e.ClrType == metadata.Parent?.Type);
            IProperty property = parentEntity?.GetProperties().FirstOrDefault(p => p.Name == metadata.Member?.Name);
            IAnnotation[] propertyAnnotations = property?.GetAnnotations().ToArray() ?? new IAnnotation[0];
#if NETSTANDARD2_0
            IKey primaryKey = property?.GetContainingPrimaryKey();
#elif NETSTANDARD2_1 || NETCOREAPP3_0
            IKey primaryKey = property?.FindContainingPrimaryKey();
#endif
            IForeignKey[] foreignKeys = property?.GetContainingForeignKeys().ToArray() ?? new IForeignKey[0];


            if (entity == null && property == null)
                return null;

#if NETSTANDARD2_0
            string tableName = entity?.Relational()?.TableName;
            string schemaName = entity?.Relational()?.Schema;
            string columnName = property?.Relational()?.ColumnName;
            string keyName = primaryKey?.Relational()?.Name;
#elif NETSTANDARD2_1 || NETCOREAPP3_0
            string tableName = entity?.GetTableName() ?? entity?.GetDefaultTableName();
            string schemaName = entity?.GetSchema() ?? entity?.GetDefaultSchema();
            string columnName = property?.GetColumnName() ?? property?.GetDefaultColumnName();
            string keyName = primaryKey?.GetName();
#endif

            List<Attribute> annotations = new List<Attribute>();

            if (tableName != null && schemaName != null)
                annotations.Add(new TableAttribute(schemaName, tableName));
            else if (tableName != null)
                annotations.Add(new TableAttribute(tableName));

            if (columnName != null)
                annotations.Add(new ColumnAttribute(columnName));

            if (propertyAnnotations.Any(a => a.Name == "SqlServer:ValueGenerationStrategy" && a.Value?.ToString() == "IdentityColumn"))
                annotations.Add(new IdAttribute());

            if (keyName != null)
            {
                int index = primaryKey.Properties.ToList().IndexOf(property);

                annotations.Add(new KeyAttribute(keyName, index));
            }

            foreach (IForeignKey foreignKey in foreignKeys)
            {
#if NETSTANDARD2_0
                string principalName = foreignKey.PrincipalKey.Relational()?.Name;
                string foreignName = foreignKey.Relational()?.Name;
                int index = foreignKey.Properties.ToList().IndexOf(property);
#elif NETSTANDARD2_1 || NETCOREAPP3_0
                string principalName = foreignKey.PrincipalKey.GetName();
                string foreignName = foreignKey.GetConstraintName();
                int index = foreignKey.Properties.ToList().IndexOf(property);
#endif

                if (principalName != null)
                    annotations.Add(new RefAttribute(principalName, index, foreignName));
            }

            if (annotations.Any())
                return annotations;

            return null;
        }
    }
}
