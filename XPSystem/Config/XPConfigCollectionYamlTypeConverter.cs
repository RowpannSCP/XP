namespace XPSystem.Config
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using XPSystem.API.Config;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class XPConfigCollectionYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(XPConfigCollection<>);

        public object ReadYaml(IParser parser, Type type)
        {
            object result;

            if (!parser.TryConsume(out MappingStart _))
                throw new InvalidDataException("Invalid YAML content.");

            // Consume the type
            if (!parser.TryConsume(out Scalar typeScalar))
                throw new InvalidDataException("Invalid YAML content: missing type.");

            var typeParam = Type.GetType(typeScalar.Value);

            // Consume the value
            if (!parser.TryConsume(out Scalar valueScalar))
                throw new InvalidDataException("Invalid YAML content: missing value.");

            // Create a dictionary of the type
            var dictType = typeof(Dictionary<,>).MakeGenericType(typeParam, typeof(XPConfigElement));
            var values = XPConfigManager.Deserializer.Deserialize(valueScalar.Value, dictType);

            // Create an instance of the generic type
            var genericType = typeof(XPConfigCollection<>).MakeGenericType(typeParam);
            result = Activator.CreateInstance(genericType);

            // Set the Values property
            var valuesProperty = genericType.GetProperty("Values")!;
            valuesProperty.SetValue(result, values);

            if (!parser.TryConsume(out MappingEnd _))
                throw new InvalidDataException("Invalid YAML content: missing end of mapping.");

            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var typeParam = type.GetGenericArguments().First();
            var xpConfigElement = type.GetProperty("Values")!.GetValue(value);
            var xpConfigElementType = xpConfigElement.GetType();
            var defaultProperty = xpConfigElementType.GetProperty("DefaultValue")!;
            var valuesProperty = xpConfigElementType.GetProperty("Values")!;
            var valuesDict = (IDictionary)valuesProperty.GetValue(xpConfigElement);

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            // generic type
            emitter.Emit(new Scalar(null, "type"));
            emitter.Emit(new Scalar(null, typeParam.FullName!));

            // XPConfigElement

            // default
            emitter.Emit(new Scalar(null, "default"));
            emitter.Emit(new Scalar(null, SerializeProperty(defaultProperty, xpConfigElement)));
            
            // values
            emitter.Emit(new Scalar(null, "values")); ;
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            // ReSharper disable once NotDisposedResource
            var enumerator = valuesDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                emitter.Emit(new Scalar(null, enumerator.Key!.ToString()));
                emitter.Emit(new Scalar(null, enumerator.Value.ToString()));
            }

            emitter.Emit(new MappingEnd());

            emitter.Emit(new MappingEnd());
        }

        private string SerializeProperty(PropertyInfo propertyInfo, object obj)
        {
            return XPConfigManager.Serializer.Serialize(propertyInfo.GetValue(obj));
        }
    }
}