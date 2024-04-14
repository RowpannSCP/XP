namespace XPSystem.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Serialization;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.TypeInspectors;

    /// <summary>
    /// Source: https://dotnetfiddle.net/8M6iIE.
    /// </summary>
    public class CommentGatheringTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;

        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            this.innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException("innerTypeDescriptor");
        }

        /// <inheritdoc/>
        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return innerTypeDescriptor
                .GetProperties(type, container)
                .Select(descriptor => new CommentsPropertyDescriptor(descriptor));
        }
    }
}