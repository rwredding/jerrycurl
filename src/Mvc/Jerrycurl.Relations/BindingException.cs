﻿using Jerrycurl.Diagnostics;
using Jerrycurl.Reflection;
using Jerrycurl.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Jerrycurl.Relations
{
    [Serializable]
    public class BindingException : Exception
    {
        public BindingException()
        {

        }

        public BindingException(string message)
            : base(message)
        {

        }

        public BindingException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected BindingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #region " Exception helpers "

        public static BindingException FromField(IField field, string message = null, Exception innerException = null)
        {
            message = message ?? innerException?.Message;

            if (message != null)
                message = $"Binding to field '{field.Identity.Name}' in schema '{field.Identity.Schema.Model.GetSanitizedName()}' failed. {message}";
            else
                message = $"Binding to field '{field.Identity.Name}' in schema '{field.Identity.Schema.Model.GetSanitizedName()}' failed.";

            return new BindingException(message, innerException);
        }

        #endregion
    }
}
