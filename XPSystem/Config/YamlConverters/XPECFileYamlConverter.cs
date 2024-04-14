namespace XPSystem.Config.YamlConverters
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using XPSystem.API;
    using XPSystem.Config.Events.Models;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class XPECFileYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(XPECFile<>);

        public object ReadYaml(IParser parser, Type type)
        {
            object result;

            if (!parser.TryConsume(out MappingStart _))
                throw new InvalidDataException("Invalid YAML content.");

            // Consume the type
            if (!parser.TryConsume(out Scalar typeScalar))
                throw new InvalidDataException("Invalid YAML content: missing type.");

            var typeParam = Type.GetType(typeScalar.Value);
            if (typeParam == null)
                throw new InvalidDataException("Invalid YAML content: invalid type.");

            // Create an instance of the generic type
            var genericType = typeof(XPECFile<>).MakeGenericType(typeParam);
            result = Activator.CreateInstance(genericType);

            // Consume the properties
            while (parser.TryConsume(out Scalar key))
            {
                if (!parser.TryConsume(out MappingStart _))
                    throw new InvalidDataException("Invalid YAML content.");

                var property = genericType.GetProperty(key.Value);
                if (property == null)
                    throw new InvalidDataException($"Invalid YAML content: unknown property '{key.Value}'.");

                var value = DeserializeProperty(parser, property);
                property.SetValue(result, value);
            }

            if (!parser.TryConsume(out MappingEnd _))
                throw new InvalidDataException("Invalid YAML content.");

            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            // generic type
            var typeParam = type.GetGenericArguments().First();

            emitter.Emit(new Scalar(null, "type"));
            emitter.Emit(new Scalar(null, typeParam.FullName!));

            // default
            var defaultProperty = type.GetProperty("Default")!;

            emitter.Emit(new Scalar(null, "default"));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
            emitter.Emit(new Scalar(null, SerializeProperty(defaultProperty, value)));
            emitter.Emit(new MappingEnd());

            // items
            var itemsProperty = type.GetProperty("Items")!;

            emitter.Emit(new Scalar(null, "items"));
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
            emitter.Emit(new Scalar(null, SerializeProperty(itemsProperty, value)));
            emitter.Emit(new MappingEnd());

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