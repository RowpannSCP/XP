namespace XPSystem.Config.YamlConverters
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Source: https://github.com/aaubry/YamlDotNet/blob/master/YamlDotNet.Samples/ValidatingDuringDeserialization.cs
    /// </summary>
    public class ValidatingNodeDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer nodeDeserializer;

        public ValidatingNodeDeserializer(INodeDeserializer nodeDeserializer)
        {
            this.nodeDeserializer = nodeDeserializer;
        }

        /// <inheritdoc/>
        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (nodeDeserializer.Deserialize(parser, expectedType, nestedObjectDeserializer, out value))
            {
                var context = new ValidationContext(value!, null, null);
                Validator.ValidateObject(value, context, true);
                return true;
            }

            return false;
        }
    }
}