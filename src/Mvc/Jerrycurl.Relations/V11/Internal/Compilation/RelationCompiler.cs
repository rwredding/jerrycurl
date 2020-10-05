using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Jerrycurl.Collections;
using Jerrycurl.Relations.V11.Internal.Enumerators;
using Jerrycurl.Relations.V11.Internal.IO;
using Jerrycurl.Relations.Metadata;
using Jerrycurl.Relations.Internal;

namespace Jerrycurl.Relations.V11.Internal.Compilation
{
    internal class RelationCompiler
    {
        private delegate void BufferInternalWriter(IField2[] fields, IRelationQueue[] queues, IField2 source, IField2 model, DotNotation2 notation, Delegate[] binders, IRelationMetadata[] metadata);

        public BufferWriter Compile(BufferTree tree)
        {
            DotNotation2 notation = tree.Notation;
            Delegate[] binders = this.GetBindersArgument(tree);
            IRelationMetadata[] metadata = this.GetMetadataArgument(tree);

            BufferInternalWriter initializer = this.Compile(tree.Source, tree.Queues);
            List<BufferInternalWriter> writers = tree.Queues.Select(this.Compile).ToList();

            return new BufferWriter()
            {
                Initializer = this.Recompile(initializer, notation, binders, metadata),
                Queues = writers.Select(w => this.Recompile(w, notation, binders, metadata)).ToArray(),
            };
        }

        private Action<RelationBuffer> Recompile(BufferInternalWriter writer, DotNotation2 notation, Delegate[] binders, IRelationMetadata[] metadata)
            => buf => writer(buf.Fields, buf.Queues, buf.Source, buf.Model, notation, binders, metadata);

        private BufferInternalWriter Compile(SourceReader reader, IEnumerable<QueueReader> queueReaders)
        {
            Expression body = this.GetInitializerExpression(reader, queueReaders);

            ParameterExpression[] innerArgs = new[] { Arguments.Fields, Arguments.Queues, Arguments.Source, Arguments.Model, Arguments.Notation, Arguments.Binders, Arguments.Metadata };

            return Expression.Lambda<BufferInternalWriter>(body, innerArgs).Compile();
        }

        private BufferInternalWriter Compile(QueueReader reader)
        {
            Expression body = this.GetReadWriteExpression(reader);

            ParameterExpression[] innerArgs = new[] { Arguments.Fields, Arguments.Queues, Arguments.Source, Arguments.Model, Arguments.Notation, Arguments.Binders, Arguments.Metadata };

            return Expression.Lambda<BufferInternalWriter>(body, innerArgs).Compile();
        }

        #region " Initializer "

        public Expression GetInitializerExpression(SourceReader reader, IEnumerable<QueueReader> queueReaders)
        {
            List<Expression> expressions = new List<Expression>();

            foreach (QueueReader queue in queueReaders)
                expressions.Add(this.GetAssignNewQueueExpression(queue.Index));

            expressions.Add(this.GetReadWriteExpression(reader));

            return this.GetBlockOrExpression(expressions);
        }



        #endregion

        #region " I/O "
        public Expression GetReadWriteExpression(SourceReader reader)
            => this.GetReadWriteExpression(reader, null);

        public Expression GetReadWriteExpression(QueueReader reader)
        {
            Expression queueIndex = this.GetQueueIndexExpression(reader.Index);
            Expression assignVariable = Expression.Assign(reader.Index.Variable, queueIndex);
            Expression parentValue = this.GetQueuePropertyExpression(reader.Index, "List");
            Expression readWrite = this.GetReadWriteExpression(reader, parentValue);

            return this.GetBlockOrExpression(new[] { assignVariable, readWrite }, new[] { reader.Index.Variable });
        }

        public Expression GetReadWriteExpression(NodeReader reader, Expression parentValue)
        {
            ParameterExpression variable = Expression.Parameter(reader.Metadata.Type);

            Expression value = this.GetReadExpression(reader, parentValue);
            Expression assignVariable = Expression.Assign(variable, value);

            List<Expression> body = new List<Expression>();

            body.AddRange(this.GetWriteExpressions(reader, parentValue, variable));
            //body.AddRange(this.GetReadExpressions(reader, variable));
            body.AddRange(this.GetReadWriteExpressions(reader, value));

            //if (!value.Type.IsValueType || Nullable.GetUnderlyingType(value.Type) != null)
            //{
            //    List<Expression> nullBody = new List<Expression>();

            //    Expression isNull = Expression.ReferenceEqual(assignVariable, Expression.Constant(null));
            //    Expression ifThen = Expression.IfThenElse(isNull, this.GetBlockOrExpression(nullBody), this.GetBlockOrExpression(body));

            //    body.Clear();
            //    body.Add(ifThen);
            //}
            //else
                body.Insert(0, assignVariable);

            return this.GetBlockOrExpression(body, new[] { variable });
        }

        private IEnumerable<Expression> GetReadWriteExpressions(NodeReader reader, Expression value)
        {
            foreach (PropertyReader propReader in reader.Properties)
                yield return this.GetReadWriteExpression(propReader, value);
        }
        #endregion

        #region " Queues "
        private Type GetQueueGenericType(QueueIndex queue) => typeof(RelationQueue<,>).MakeGenericType(queue.List.Type, queue.Item.Type);
        private Type GetQueueItemGenericType(QueueIndex queue) => typeof(RelationQueueItem<>).MakeGenericType(queue.List.Type);

        private Expression GetAssignNewQueueExpression(QueueIndex index)
        {
            Expression arrayIndex = Expression.ArrayAccess(Arguments.Queues, Expression.Constant(index.Buffer));
            Expression newQueue = this.GetNewQueueExpression(index);

            return Expression.Assign(arrayIndex, newQueue);
        }

        private Expression GetNewQueueExpression(QueueIndex index)
        {
            Type type = this.GetQueueGenericType(index);
            ConstructorInfo ctor = type.GetConstructors()[0];

            return Expression.New(ctor, Expression.Constant(index.Type));
        }

        private Expression GetQueueAddExpression(QueueWriter writer, Expression value)
        {
            Type queueType = this.GetQueueGenericType(writer.Next);
            MethodInfo addMethod = queueType.GetMethod("Enqueue");

            Expression queue = this.GetQueueIndexExpression(writer.Next);
            Expression queueItem = this.GetNewQueueItemExpression(writer, value);

            return Expression.Call(queue, addMethod, queueItem);
        }

        private Expression GetQueueIndexExpression(QueueIndex index)
        {
            Expression arrayIndex = Expression.ArrayAccess(Arguments.Queues, Expression.Constant(index.Buffer));

            return Expression.Convert(arrayIndex, index.Variable.Type);
        }

        private Expression GetNewQueueItemExpression(QueueWriter writer, Expression value)
        {
            Type itemType = this.GetQueueItemGenericType(writer.Next);
            ConstructorInfo ctor = itemType.GetConstructors()[0];

            Expression namePart = this.GetFieldNameExpression(writer);

            return Expression.New(ctor, value, namePart, Arguments.Notation);
        }
        private Expression GetQueuePropertyExpression(QueueIndex queue, string propertyName)
            => Expression.Property(queue.Variable, propertyName);
        #endregion

        #region " Writers "
        private Expression GetWriteExpression(NodeWriter writer, Expression parentValue, Expression value) => writer switch
        {
            FieldWriter writer2 => this.GetWriteExpression(writer2, parentValue, value),
            QueueWriter writer2 => this.GetWriteExpression(writer2, value),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetWriteExpression(FieldWriter writer, Expression parentValue, Expression value)
        {
            Expression bufferIndex = Expression.ArrayAccess(Arguments.Fields, Expression.Constant(writer.BufferIndex));
            Expression newField = writer.Queue != null ? this.GetNewFieldExpression(writer, parentValue, value, false) : Arguments.Source;

            return Expression.Assign(bufferIndex, newField);
        }

        private Expression GetWriteExpression(QueueWriter writer, Expression value) => this.GetQueueAddExpression(writer, value);

        private IEnumerable<Expression> GetWriteExpressions(NodeReader reader, Expression parentValue, Expression value)
        {
            foreach (NodeWriter writer in reader.Writers)
                yield return this.GetWriteExpression(writer, parentValue, value);
        }

        #endregion

        #region " Readers "

        private bool IsVariableRequired(NodeReader reader) => reader.Properties.Count + reader.Writers.Count > 1;

        private Expression GetReadExpression(NodeReader reader, Expression parentValue) => reader switch
        {
            QueueReader reader2 => this.GetReadExpression(reader2),
            SourceReader reader2 => this.GetReadExpression(reader2),
            PropertyReader reader2 => this.GetReadExpression(reader2, parentValue),
            _ => throw new InvalidOperationException(),
        };

        private Expression GetReadExpression(PropertyReader reader, Expression parentValue)
            => Expression.MakeMemberAccess(parentValue, reader.Metadata.Member);

        private Expression GetReadExpression(QueueReader reader)
            => this.GetQueuePropertyExpression(reader.Index, "Current");

        private Expression GetReadExpression(SourceReader reader)
            => this.GetSourceValueExpression(reader);

        private IEnumerable<Expression> GetReadExpressions(NodeReader reader, Expression value)
        {
            foreach (PropertyReader propReader in reader.Properties)
                yield return this.GetReadExpression(propReader, value);
        }
        #endregion

        #region " Inputs "
        private Expression GetSourceNameExpression()
            => Expression.Property(Expression.Property(Arguments.Source, "Identity"), "Name");

        private Expression GetSourceValueExpression(SourceReader reader)
        {
            Expression data = Expression.Property(Arguments.Source, "data");
            Expression value = Expression.Property(data, "Value");

            return Expression.Convert(value, reader.Metadata.Type);
        }

        private Expression GetMetadataExpression(FieldWriter writer)
            => Expression.ArrayAccess(Arguments.Metadata, Expression.Constant(writer.BufferIndex));

        private Expression GetBinderExpression(FieldWriter writer)
        {
            Expression binderObject = Expression.ArrayAccess(Arguments.Binders, Expression.Constant(writer.BufferIndex));

            return binderObject;
        }

        #endregion

        #region " Fields "
        private Expression GetFieldNameExpression(NodeWriter writer)
        {
            if (writer.Queue != null)
            {
                MethodInfo nameMethod = writer.Queue.Variable.Type.GetMethod("GetFieldName");

                return Expression.Call(writer.Queue.Variable, nameMethod, Expression.Constant(writer.NamePart));
            }
            else
            {
                return this.GetSourceNameExpression(); //combine too
            }
        }

        //public Field2(string name, IRelationMetadata metadata, FieldData<TValue, TParent> data, IField2 model, FieldType2 type)
        //public FieldData(object list, int index, TParent parent, TValue value, Delegate binder)
        private Expression GetNewFieldExpression(FieldWriter writer, Expression parentValue, Expression value, bool isNull)
        {
            if (parentValue == null)
                return Arguments.Source;

            Type fieldType = typeof(Field2<,>).MakeGenericType(value.Type, parentValue.Type);
            Type dataType = typeof(FieldData<,>).MakeGenericType(value.Type, parentValue.Type);

            ConstructorInfo newFieldInfo = fieldType.GetConstructors()[0];
            ConstructorInfo newDataInfo = dataType.GetConstructors()[0];

            Expression relation = Expression.Constant(null);
            Expression index = this.GetQueuePropertyExpression(writer.Queue, "Index");
            Expression binder = this.GetBinderExpression(writer);
            
            Expression name = this.GetFieldNameExpression(writer);
            Expression metadata = this.GetMetadataExpression(writer);
            Expression data = Expression.New(newDataInfo, relation, index, parentValue, value, binder);
            Expression type = Expression.Constant(isNull ? FieldType2.Null : FieldType2.Value);

            return Expression.New(newFieldInfo, name, metadata, data, Arguments.Model, type);
        }

        private Expression GetNewMissingExpression(FieldWriter writer)
        {
            Type fieldType = typeof(Missing<>).MakeGenericType(writer.Metadata.Type);
            ConstructorInfo ctor = fieldType.GetConstructors()[0];

            Expression name = this.GetFieldNameExpression(writer);
            Expression metadata = this.GetMetadataExpression(writer);

            return Expression.New(ctor, name, metadata, Arguments.Model);
        }
        #endregion

        #region " Helpers "
        private Expression GetBlockOrExpression(IList<Expression> expressions, IList<ParameterExpression> variables = null)
        {
            if (expressions.Count == 1 && (variables == null || !variables.Any()))
                return expressions[0];
            else if (variables == null)
                return Expression.Block(expressions);
            else
                return Expression.Block(variables.NotNull(), expressions);
        }
        #endregion

        #region " Arguments "

        private IRelationMetadata[] GetMetadataArgument(BufferTree tree)
        {
            IRelationMetadata[] metadata = new IRelationMetadata[tree.Fields.Count];

            foreach (FieldWriter writer in tree.Fields)
                metadata[writer.BufferIndex] = writer.Metadata;

            return metadata;
        }

        private Delegate[] GetBindersArgument(BufferTree tree)
        {
            Delegate[] binders = new Delegate[tree.Fields.Count];

            foreach (FieldWriter writer in tree.Fields)
                binders[writer.BufferIndex] = this.GetBinderArgument(writer);

            return binders;
        }

        private Delegate GetBinderArgument(FieldWriter writer)
        {
            if (writer.Metadata.Parent == null)
                return null;

            Type binderType = typeof(FieldBinder<,>).MakeGenericType(writer.Metadata.Parent.Type, writer.Metadata.Type);

            ParameterExpression parentValue = writer.Metadata.Parent != null ? Expression.Parameter(writer.Metadata.Parent.Type) : null;
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
            public static ParameterExpression Fields { get; } = Expression.Parameter(typeof(IField2[]), "fields");
            public static ParameterExpression Model { get; } = Expression.Parameter(typeof(IField2), "model");
            public static ParameterExpression Source { get; } = Expression.Parameter(typeof(IField2), "source");
            public static ParameterExpression Metadata { get; } = Expression.Parameter(typeof(IRelationMetadata[]), "metadata");
            public static ParameterExpression Notation { get; } = Expression.Parameter(typeof(DotNotation2), "notation");
            public static ParameterExpression Binders { get; } = Expression.Parameter(typeof(Delegate[]), "binders");
            public static ParameterExpression Queues { get; } = Expression.Parameter(typeof(IRelationQueue[]), "queues");

        }
    }
}
