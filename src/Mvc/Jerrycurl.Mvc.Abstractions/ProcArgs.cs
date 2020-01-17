using System;

namespace Jerrycurl.Mvc
{
    public sealed class ProcArgs
    {
        public Type ModelType { get; internal set; }
        public Type ResultType { get; internal set; }

        internal ProcArgs()
        {

        }

        public ProcArgs(Type modelType, Type resultType)
        {
            this.ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            this.ResultType = resultType ?? throw new ArgumentNullException(nameof(resultType));
        }
    }
}
