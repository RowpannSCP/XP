#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
namespace XPSystem.Config.YamlConverters
{
    using System;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class StringYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(string);

        public object ReadYaml(IParser parser, Type type)
        {
            if (!parser.TryConsume(out Scalar scalar))
                return string.Empty;

            return scalar.Value ?? string.Empty;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            emitter.Emit(new Scalar(
                AnchorName.Empty,
                TagName.Empty,
                value?.ToString() ?? string.Empty,
                ScalarStyle.DoubleQuoted,
                true,
                false));
        }
    }
}
