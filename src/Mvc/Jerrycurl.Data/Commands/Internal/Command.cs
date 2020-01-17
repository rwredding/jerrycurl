using Jerrycurl.Relations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Jerrycurl.Collections;
using Jerrycurl.Data.Sessions;

namespace Jerrycurl.Data.Commands.Internal
{
    internal class Command : IOperation
    {
        private readonly Dictionary<string, FieldData> headingMap = new Dictionary<string, FieldData>(StringComparer.OrdinalIgnoreCase);

        public CommandData Data { get; }
        public FieldMap Map { get; }
        public object Source => this.Data;

        public Command(CommandData commandData, FieldMap fieldMap)
        {
            this.Data = commandData ?? throw new ArgumentNullException(nameof(commandData));
            this.Map = fieldMap ?? throw new ArgumentNullException(nameof(fieldMap));
        }

        public FieldData[] GetHeading(TableIdentity tableInfo) => tableInfo.Columns.Select(ci => this.headingMap.TryGetValue(ci.Name)).ToArray();

        public void Build(IDbCommand adoCommand)
        {
            adoCommand.CommandText = this.Data.CommandText;

            Dictionary<string, IDbDataParameter> adoMap = new Dictionary<string, IDbDataParameter>();

            foreach (IParameter parameter in this.Data.Parameters.Where(p => !adoMap.ContainsKey(p.Name)))
            {
                IDbDataParameter adoParameter = adoCommand.CreateParameter();

                parameter.Build(adoParameter);

                if (parameter.Field != null)
                {
                    FieldData fieldData = this.Map.Get(parameter.Field);

                    if (fieldData != null)
                        adoParameter.Value = fieldData.GetValue();
                }

                adoMap.Add(parameter.Name, adoParameter);
                adoCommand.Parameters.Add(adoParameter);
            }

            foreach (var g in this.Data.Bindings.GroupBy(b => b.Field).Select(g => g.ToArray()))
            {
                ParameterBinding parameterBinding = g.OfType<ParameterBinding>().FirstOrDefault();
                ColumnBinding columnBinding = g.OfType<ColumnBinding>().FirstOrDefault();

                if (parameterBinding == null && columnBinding == null)
                    throw new CommandException("ICommandBinding must be a ColumnBinding or ParameterBinding instance.");

                IField field = columnBinding?.Field ?? parameterBinding.Field;

                FieldData fieldData = this.Map.Get(field);

                if (fieldData == null)
                    fieldData = this.Map.Add(field);

                if (columnBinding != null)
                    this.headingMap[columnBinding.ColumnName] = fieldData;
                else
                {
                    IDbDataParameter adoParameter = adoMap.TryGetValue(parameterBinding.ParameterName);

                    if (adoParameter == null)
                    {
                        adoParameter = adoCommand.CreateParameter();

                        adoParameter.ParameterName = parameterBinding.ParameterName;
                        adoParameter.Value = DBNull.Value;

                        this.SetParameterDirection(adoParameter, ParameterDirection.Output);

                        adoMap.Add(parameterBinding.ParameterName, adoParameter);
                        adoCommand.Parameters.Add(adoParameter);
                    }

                    fieldData.SetValue(adoParameter);

                    if (adoParameter.Direction == ParameterDirection.Input)
                        this.SetParameterDirection(adoParameter, ParameterDirection.InputOutput);
                }
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
    }
}
