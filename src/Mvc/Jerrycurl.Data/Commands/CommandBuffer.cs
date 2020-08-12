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
        private readonly Dictionary<IField, FieldPipe> targetPipes = new Dictionary<IField, FieldPipe>();
        private readonly Dictionary<string, FieldPipe> columnHeader = new Dictionary<string, FieldPipe>();
        private readonly Dictionary<string, FieldPipe> paramHeader = new Dictionary<string, FieldPipe>();

        public IOperation Read(CommandData commandData) => new Command(commandData, this);

        public void Write(IDataReader dataReader)
        {
            List<ColumnName> names = new List<ColumnName>();
            List<FieldPipe> pipes = new List<FieldPipe>();

            for (int i = 0; i < this.GetFieldCount(dataReader); i++)
            {
                string columnName = dataReader.GetName(i);

                if (this.columnHeader.TryGetValue(columnName, out FieldPipe pipe))
                {
                    MetadataIdentity metadata = pipe.Target.Identity.Metadata;
                    ColumnInfo columnInfo = this.GetColumnInfo(dataReader, i);

                    names.Add(new ColumnName(metadata, columnInfo));
                    pipes.Add(pipe);

                    pipe.Column.Info = columnInfo;
                }
            }

            BufferWriter writer = CommandCache.GetWriter(names);

            writer(dataReader, pipes.ToArray());
        }

        private ColumnInfo GetColumnInfo(IDataRecord dataRecord, int i)
            => new ColumnInfo(dataRecord.GetName(i), dataRecord.GetFieldType(i), dataRecord.GetDataTypeName(i), i);

        internal IEnumerable<IDbDataParameter> GetParameters(IDbCommand adoCommand)
        {
            foreach (FieldPipe pipe in this.paramHeader.Values)
            {
                IDbDataParameter adoParam = adoCommand.CreateParameter();

                pipe.Parameter.AdoParameter = adoParam;
                pipe.Parameter.Parameter?.Build(adoParam);

                if (pipe.Parameter.HasSource && pipe.Parameter.HasTarget)
                    this.SetParameterDirection(adoParam, ParameterDirection.InputOutput);
                else if (pipe.Parameter.HasTarget)
                {
                    adoParam.Value = DBNull.Value;

                    this.SetParameterDirection(adoParam, ParameterDirection.InputOutput);
                }

                if (pipe.Parameter.Parameter.Field != null && this.targetPipes.TryGetValue(pipe.Parameter.Parameter.Field, out FieldPipe targetPipe) && targetPipe.HasChanged)
                    adoParam.Value = targetPipe.Read();

                yield return adoParam;
            }
        }

        internal FieldPipe GetPipe(IField target) => this.targetPipes.GetValueOrDefault(target);
        internal IEnumerable<IFieldSource> GetSources(IField target) => null;

        private void SetParameterDirection(IDbDataParameter adoParameter, ParameterDirection direction)
        {
            try
            {
                adoParameter.Direction = direction;
            }
            catch (ArgumentException) { }
        }

        private void ClearState()
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
            foreach (FieldPipe pipe in this.targetPipes.Values.Where(v => v.HasChanged))
                pipe.Bind();
        }

        public void Add(IParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            FieldPipe pipe = this.paramHeader.GetOrAdd(parameter.Name, new FieldPipe(this));

            pipe.Parameter ??= new ParameterSource();
            pipe.Parameter.Parameter = parameter;

            this.paramHeader.TryAdd(parameter.Name, pipe);
        }

        public void Add(CascadeBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            FieldPipe pipe = this.targetPipes.GetOrAdd(binding.Target, new FieldPipe(this));

            pipe.Cascade ??= new CascadeSource(binding, this);
        }

        public void Add(ColumnBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            FieldPipe pipe = this.targetPipes.GetOrAdd(binding.Target, new FieldPipe(this));

            pipe.Column ??= new ColumnSource();
            pipe.Target = binding.Target;

            this.columnHeader.TryAdd(binding.ColumnName, pipe);
        }

        public void Add(ParameterBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            FieldPipe pipe = this.targetPipes.GetOrAdd(binding.Target, () => this.paramHeader.GetOrAdd(binding.ParameterName, new FieldPipe(this)));

            pipe.Parameter ??= new ParameterSource();
            pipe.Parameter.HasTarget = true;
            pipe.Target = binding.Target;
        }
    }
}
