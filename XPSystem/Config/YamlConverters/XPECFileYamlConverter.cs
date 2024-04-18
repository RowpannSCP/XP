namespace XPSystem.Config.YamlConverters
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using XPSystem.API;
    using XPSystem.Config.Events.Types;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class XPECFileYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(XPECFile);

        public object ReadYaml(IParser parser, Type type)
        {
#mark where to add stuff for additional values
            object result;

            if (!parser.TryConsume(out MappingStart _))
                throw new InvalidDataException("Invalid YAML content.");

            

            if (!parser.TryConsume(out MappingEnd _))
                throw new InvalidDataException("Invalid YAML content.");

            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            

            emitter.Emit(new MappingEnd());
        }

        private string SerializeProperty(PropertyInfo propertyInfo, object obj)
        {
            return XPAPI.Serializer.Serialize(propertyInfo.GetValue(obj));
        }

        private object DeserializeProperty(IParser parser, PropertyInfo propertyInfo)
        {
            return XPAPI.Deserializer.Deserialize(parser, propertyInfo.PropertyType);
        }
    }
}