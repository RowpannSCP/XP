namespace XPSystem.Config.YamlConverters
{
    using System;
    using System.IO;
    using XPSystem.Config.Events.Types;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class XPECFileYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => typeof(XPECFile).IsAssignableFrom(type);

        public object ReadYaml(IParser parser, Type type)
        {
            object result;

            if (!parser.TryConsume(out MappingStart _))
                throw new InvalidDataException("Invalid YAML content: MappingStart not found.");

            if (!parser.TryConsume(out Scalar typeScalar)
                || typeScalar.Value != "type"
                || !parser.TryConsume(out Scalar typeValue))
                throw new InvalidDataException("Invalid YAML content: type not found.");

            type = Type.GetType(typeValue.Value)
                   ?? throw new InvalidDataException($"Invalid YAML content: type {typeValue.Value} not resolved.");

            result = Activator.CreateInstance(type);

            ((XPECFile)result).Read(parser);

            if (!parser.TryConsume(out MappingEnd _))
                throw new InvalidDataException("Invalid YAML content: MappingEnd not found.");

            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            emitter.Emit(new Scalar("type"));
            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, type.AssemblyQualifiedName!, ScalarStyle.SingleQuoted, true, true));

            emitter.Emit(new Comment("-------- Don't edit above this line unless you know what you are doing!!!!! Will break config!!!!!!! --------", false));

            ((XPECFile)value)!.Write(emitter);

            emitter.Emit(new MappingEnd());
        }
    }
}