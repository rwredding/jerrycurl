using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jerrycurl.Collections;
using Jerrycurl.Data.Commands.Internal;
using Jerrycurl.Data.Commands.Internal.V11;
using Jerrycurl.Data.Commands.Internal.V11.Caching;
using Jerrycurl.Data.Commands.Internal.V11.Compilation;
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
        private readonly Dictionary<IField, FieldPipe> sourcePipes = new Dictionary<IField, FieldPipe>();
        private readonly Dictionary<string, FieldPipe> columnHeader = new Dictionary<string, FieldPipe>();
        private readonly Dictionary<string, FieldPipe> paramHeader = new Dictionary<string, FieldPipe>();

        public IOperation Read(CommandData commandData) => new Command2(commandData, this);

        public void Write(IDataReader dataReader)
        {
            List<ColumnName> names = new List<ColumnName>();
            List<FieldPipe> pipes = new List<FieldPipe>();

            for (int i = 0; i < this.GetFieldCount(dataReader); i++)
            {
                string columnName = dataReader.GetName(i);

                if (this.columnHeader.TryGetValue(columnName, out FieldPipe pipe))
                {
                    MetadataIdentity metadata = pipe.Targets.First().Identity.Metadata;
                    ColumnInfo columnInfo = this.GetColumnInfo(dataReader, i);
                    ColumnSource source = pipe.Source as ColumnSource;

                    names.Add(new ColumnName(metadata, columnInfo));

                    source.Column = columnInfo;
                }
            }

            BufferWriter writer = CommandCache.GetWriter(null, names);

            writer(dataReader, pipes.ToArray());
        }

        private IEnumerable<FieldPipe> GetChangeSet() => this.targetPipes.Values.Where(p => p.Source.HasChanged);

        private ColumnInfo GetColumnInfo(IDataRecord dataRecord, int i)
            => new ColumnInfo(dataRecord.GetName(i), dataRecord.GetFieldType(i), dataRecord.GetDataTypeName(i), i);

        public IEnumerable<IDbDataParameter> GetParameters(IDbCommand adoCommand)
        {
            foreach (FieldPipe pipe in this.paramHeader.Values)
            {
                IDbDataParameter adoParam = adoCommand.CreateParameter();
                
                if (pipe.Source is ParameterSource source)
                {
                    source.AdoParameter = adoParam;
                    source.Parameter?.Build(adoParam);

                    if (source.Parameter == null && pipe.Targets.Any())
                    {
                        source.AdoParameter.Value = DBNull.Value;

                        this.SetParameterDirection(adoParam, ParameterDirection.Output);
                    }
                    else if (pipe.Targets.Any())
                        this.SetParameterDirection(adoParam, ParameterDirection.InputOutput);
                }

                yield return adoParam;
            }
        }

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
            foreach (FieldPipe pipe in this.GetChangeSet())
                pipe.Bind();
        }

        public void Add(IParameter parameter)
        {
            FieldPipe pipe = this.sourcePipes.GetOrAdd(parameter.Field, new FieldPipe(this));
            ParameterSource source = pipe.Source as ParameterSource ?? new ParameterSource();

            source.Parameter = parameter;
            pipe.Source = source;

            this.paramHeader[parameter.Name] = pipe;
        }

        public void Add(ColumnBinding binding)
        {
            FieldPipe pipe = this.columnHeader.GetOrAdd(binding.ColumnName, new FieldPipe(this));

            pipe.Source = new ColumnSource();
            pipe.Targets.Add(binding.Target);

            this.columnHeader[binding.ColumnName] = pipe;
            this.targetPipes[binding.Target] = pipe;
        }

        public void Add(ParameterBinding binding)
        {
            FieldPipe pipe = this.paramHeader.GetOrAdd(binding.ParameterName, new FieldPipe(this));
            ParameterSource source = pipe.Source as ParameterSource ?? new ParameterSource();

            pipe.Targets.Add(binding.Target);
            pipe.Source = source;

            this.targetPipes[binding.Target] = pipe;
        }
    }
}
