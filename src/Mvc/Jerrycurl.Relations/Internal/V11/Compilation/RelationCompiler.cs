using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Jerrycurl.Relations.Internal.V11.Enumerators;
using Jerrycurl.Relations.Internal.V11.IO;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.V11.Compilation
{
    internal class RelationCompiler
    {
        private delegate void BufferInternalWriter(IField2[] fields, IEnumerator[] enumerators, IField2 source, IField2 model, IMetadataNotation notation);

        public BufferWriter[] Compile(BufferTree tree)
        {
            List<BufferWriter> writers = new List<BufferWriter>();

            foreach (SetReader reader in tree.Sets)
                writers.Add(this.Compile(reader));

            return writers.ToArray();
        }

        private BufferWriter Compile(SetReader reader)
        {
            return null;
        }

        private Expression GetValueExpression(ValueReader reader, Expression parentValue) => reader switch
        {
            SetReader setReader => this.GetValueExpression(setReader),
            _ => Expression.MakeMemberAccess(parentValue, reader.Metadata.Member),
        };

        private bool IsVariableRequired(ValueReader reader) => reader.Writers.Count + reader.Properties.Count > 1;
        private void GetBufferWriterExpressions(ValueReader reader, Expression parentValue, List<Expression> writeExpressions)
        {
            Expression value = this.GetValueExpression(reader, parentValue);

            if (this.IsVariableRequired(reader))
            {
                ParameterExpression variable = Expression.Variable(value.Type);
                Expression assignVariable = Expression.Assign(variable, value);

                value = variable;
            }

            foreach (NodeWriter writer in reader.Writers)
            {

            }

            foreach (ValueReader propertyReader in reader.Properties)
                this.GetBufferWriterExpressions(propertyReader, value, writeExpressions);
        }

        private Expression GetSourceNameExpression()
            => Expression.Property(Expression.Property(Arguments.Source, "Identity"), "Name");

        private Expression GetSourceValueExpression(ValueReader reader)
            => Expression.Convert(Expression.Property(Arguments.Source, "Value"), reader.Metadata.Type));

        private Expression GetMetadataExpression(FieldWriter writer)
            => Expression.ArrayAccess(Arguments.Metadata, Expression.Constant(writer.BufferIndex));

        private Expression GetNotationCombineExpression(Expression part1, Expression part2)
            => Expression.Call(Arguments.Notation, nameof(IMetadataNotation.Combine), new[] { typeof(string), typeof(string) }, part1, part2);

        private Expression GetFieldBinderExpression(FieldWriter writer)
        {
            Expression binderObject = Expression.ArrayAccess(Arguments.Binders, Expression.Constant(writer.BufferIndex));

            return binderObject;
        }


        #region " Enumerators "
        private Expression GetEnumeratorCurrentExpression(int index, Type enumeratorType)
        {
            Expression enumerator = this.GetEnumeratorExpression(index, enumeratorType);

            return Expression.Property(enumerator, "Current")
        }

        private Expression GetEnumeratorIndexExpression(int index, Type enumeratorType)
        {
            Expression enumerator = this.GetEnumeratorExpression(index, enumeratorType);

            return Expression.Property(enumerator, "Index")
        }

        private Expression GetEnumeratorExpression(int index, Type enumeratorType)
        {
            Expression enumerator = Expression.ArrayAccess(Arguments.Enumerators, Expression.Constant(index));

            return Expression.Convert(enumerator, enumeratorType);
        }

        private Expression GetEnumeratorNameExpression(ListWriter writer)
        {
            Expression enumerator = this.GetEnumeratorExpression(writer);

            return Expression.Property(enumerator, "Name");
        }

        private Expression GetNewEnumerator(SetWriter writer, Expression value)
        {
            if (writer.IsSource)
            {
                Type enumeratorType = typeof(SourceEnumerator<>).MakeGenericType(writer.Item.Metadata.Type);
                ConstructorInfo enumeratorCtor = enumeratorType.GetConstructors()[0];

                return Expression.New(enumeratorCtor, value, Arguments.Source);
            }
            else
            {
                ConstructorInfo enumeratorCtor = writer.EnumeratorType.GetConstructors()[0];

                return Expression.New(enumeratorCtor, value);
            }
        }
        #endregion

        #region " Fields "
        private Expression GetNewFieldExpression(FieldWriter writer, Expression parentValue, Expression value, bool isNull)
        {
            Type fieldType = typeof(Field2<,>).MakeGenericType(parentValue.Type, value.Type);
            ConstructorInfo fieldCtor = fieldType.GetConstructors()[0];

            Expression name = Expression.Constant("");
            Expression metadata = this.GetMetadataExpression(writer);
            Expression binder = this.GetFieldBinderExpression(writer);
            Expression index = this.GetEnumeratorIndexExpression(writer.BufferIndex, null);
            Expression type = Expression.Constant(isNull ? FieldType2.Null : FieldType2.Value);

            return Expression.New(fieldCtor, name, metadata, parentValue, index, value, binder, Arguments.Model, type);
        }

        private Expression GetNewMissingExpression(FieldWriter writer)
        {
            Type fieldType = typeof(Missing<>).MakeGenericType(writer.Metadata.Type);
            ConstructorInfo fieldCtor = fieldType.GetConstructors()[0];

            Expression name = Expression.Constant("");
            Expression metadata = this.GetMetadataExpression(writer);

            return Expression.New(fieldCtor, name, metadata, Arguments.Model);
        }
        #endregion

        #region " Binder "
        private Delegate CreateBinder(FieldWriter writer)
        {
            Type binderType = typeof(ObjectBinder<,>).MakeGenericType(writer.Metadata.Type, writer.Metadata.Parent.Type);

            ParameterExpression parentValue = Expression.Parameter(writer.Metadata.Parent.Type);
            ParameterExpression index = Expression.Parameter(typeof(int));
            ParameterExpression value = Expression.Parameter(writer.Metadata.Type);

            Expression bindExpression;

            if (writer.Metadata.HasFlag(RelationMetadataFlags.Item))
            {
                if (writer.Metadata.WriteIndex != null)
                    bindExpression = Expression.Call(parentValue, writer.Metadata.WriteIndex, index, value);
                else
                    bindExpression = Expression.Throw(Expression.New(typeof(NotIndexableException)));
            }
            else if (!writer.Metadata.HasFlag(RelationMetadataFlags.Writable))
                bindExpression = Expression.Throw(Expression.New(typeof(NotWritableException)));
            else if (writer.Metadata.Member != null)
                bindExpression = Expression.Assign(Expression.MakeMemberAccess(parentValue, writer.Metadata.Member), value);
            else
                return null;

            return Expression.Lambda(binderType, bindExpression, new[] { parentValue, index, value }).Compile();
        }
        #endregion

        private static class Arguments
        {
            public static ParameterExpression Enumerators { get; } = Expression.Parameter(typeof(IEnumerator[]), "enumerators");
            public static ParameterExpression Fields { get; } = Expression.Parameter(typeof(IField2[]), "fields");
            public static ParameterExpression Model { get; } = Expression.Parameter(typeof(IField2), "model");
            public static ParameterExpression Source { get; } = Expression.Parameter(typeof(IField2), "source");
            public static ParameterExpression Metadata { get; } = Expression.Parameter(typeof(MetadataIdentity[]), "metadata");
            public static ParameterExpression Notation { get; } = Expression.Parameter(typeof(IMetadataNotation), "notation");
            public static ParameterExpression Binders { get; } = Expression.Parameter(typeof(Delegate[]), "binders");

        }
    }
}
