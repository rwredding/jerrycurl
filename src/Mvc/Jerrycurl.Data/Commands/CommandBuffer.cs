using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Jerrycurl.Collections;
using Jerrycurl.Data.Commands.Internal;
using Jerrycurl.Data.Commands.Internal.Caching;
using Jerrycurl.Data.Commands.Internal.Compilation;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Metadata;
using Microsoft.Win32.SafeHandles;

namespace Jerrycurl.Data.Commands
{
    public sealed class CommandBuffer
    {
        private readonly Dictionary<IField, FieldBuffer> fieldBuffers = new Dictionary<IField, FieldBuffer>();
        private readonly Dictionary<string, FieldBuffer> columnHeader = new Dictionary<string, FieldBuffer>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, FieldBuffer> paramHeader = new Dictionary<string, FieldBuffer>(StringComparer.OrdinalIgnoreCase);

        public void Update(IDataReader dataReader)
        {
            List<ColumnName> names = new List<ColumnName>();
            List<FieldBuffer> buffers = new List<FieldBuffer>();

            for (int i = 0; i < this.GetFieldCount(dataReader); i++)
            {
                string columnName = dataReader.GetName(i);

                if (this.columnHeader.TryGetValue(columnName, out FieldBuffer buffer))
                {
                    MetadataIdentity metadata = buffer.Target.Identity.Metadata;
                    ColumnMetadata columnInfo = GetColumnInfo(i);

                    names.Add(new ColumnName(metadata, columnInfo));
                    buffers.Add(buffer);

                    buffer.Column.Info = columnInfo;
                }
            }

            BufferWriter writer = CommandCache.GetWriter(names);

            writer(dataReader, buffers.ToArray());

            this.Flush();

            ColumnMetadata GetColumnInfo(int i) => new ColumnMetadata(dataReader.GetName(i), dataReader.GetFieldType(i), dataReader.GetDataTypeName(i), i);
        }

        public IEnumerable<IDbDataParameter> GetParameters(IDbCommand adoCommand)
        {
            foreach (FieldBuffer buffer in this.paramHeader.Values)
            {
                IDbDataParameter adoParam = adoCommand.CreateParameter();

                adoParam.ParameterName = buffer.Parameter.Parameter.Name;
                buffer.Parameter.Parameter?.Build(adoParam);

                if (buffer.Parameter.HasSource && buffer.Parameter.HasTarget)
                    SetParameterDirection(adoParam, ParameterDirection.InputOutput);
                else if (buffer.Parameter.HasTarget)
                {
                    adoParam.Value = DBNull.Value;

                    SetParameterDirection(adoParam, ParameterDirection.InputOutput);
                }

                if (this.TryReadValue(buffer.Parameter.Parameter.Field, out object newValue))
                    adoParam.Value = newValue;

                buffer.Parameter.AdoParameter = adoParam;

                yield return adoParam;
            }

            static void SetParameterDirection(IDbDataParameter adoParameter, ParameterDirection direction)
            {
                try
                {
                    adoParameter.Direction = direction;
                }
                catch (ArgumentException) { }
            }
        }

        private bool TryReadValue(IField field, out object value)
        {
            value = null;

            if (field == null)
                return false;
            else if (this.fieldBuffers.TryGetValue(field, out FieldBuffer buffer))
                return buffer.Read(out value);

            return false;
        }

        internal FieldBuffer GetBuffer(IField target) => this.fieldBuffers.GetValueOrDefault(target);
        internal IEnumerable<IFieldSource> GetSources(IField target) => this.GetBuffer(target)?.GetSources() ?? Array.Empty<IFieldSource>();
        internal IEnumerable<IFieldSource> GetChanges(IField target) => this.GetBuffer(target)?.GetChanges() ?? Array.Empty<IFieldSource>();

        public void Flush()
        {
            this.paramHeader.Clear();
            this.columnHeader.Clear();
        }

        private int GetFieldCount(IDataReader dataReader)
        {
            try { return dataReader.FieldCount; }
            catch { return 0; }
        }

        public void Commit()
        {
            foreach (FieldBuffer buffer in this.fieldBuffers.Values)
                buffer.Bind();
        }

        public void Add(IParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            FieldBuffer buffer = this.paramHeader.GetOrAdd(parameter.Name);

            buffer.Parameter ??= new ParameterSource();
            buffer.Parameter.Parameter = parameter;

            this.paramHeader.TryAdd(parameter.Name, buffer);
        }

        public void Add(IUpdateBinding binding)
        {
            switch (binding)
            {
                case ColumnBinding columnBinding:
                    this.Add(columnBinding);
                    break;
                case ParameterBinding paramBinding:
                    this.Add(paramBinding);
                    break;
                case CascadeBinding cascadeBinding:
                    this.Add(cascadeBinding);
                    break;
                case null:
                    throw new ArgumentNullException(nameof(binding));
                default:
                    throw new CommandException("ICommandBinding must be a ColumnBinding, ParameterBinding or CascadeBinding instance.");
            }
        }

        public void Add(CascadeBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            FieldBuffer buffer = this.fieldBuffers.GetOrAdd(binding.Target);

            buffer.Cascade = new CascadeSource(binding, this);
            buffer.Target = binding.Target;
        }

        public void Add(ColumnBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            FieldBuffer buffer = this.fieldBuffers.GetOrAdd(binding.Target);

            buffer.Column ??= new ColumnSource();
            buffer.Target = binding.Target;

            this.columnHeader.TryAdd(binding.ColumnName, buffer);
        }

        public void Add(ParameterBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            FieldBuffer buffer = this.paramHeader.GetOrAdd(binding.ParameterName, () => this.fieldBuffers.GetOrAdd(binding.Target));

            buffer.Parameter ??= new ParameterSource();
            buffer.Parameter.HasTarget = true;
            buffer.Target = binding.Target;

            this.paramHeader.TryAdd(binding.ParameterName, buffer);
        }
    }
}
