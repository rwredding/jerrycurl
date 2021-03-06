﻿using System;

namespace Jerrycurl.Relations.Metadata
{
    public interface ISchema : IEquatable<ISchema>
    {
        Type Model { get; }
        IMetadataNotation Notation { get; }

        TMetadata GetMetadata<TMetadata>(string name) where TMetadata : IMetadata;
        TMetadata GetMetadata<TMetadata>() where TMetadata : IMetadata;
    }
}