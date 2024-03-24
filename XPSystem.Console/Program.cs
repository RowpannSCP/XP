namespace XPSystem.Console
{
    using System;
    using System.Collections.Generic;
    using Exiled.Loader.Features.Configs;
    using XPSystem.API.Config;
    using XPSystem.Config;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.NodeDeserializers;
    using CommentsObjectGraphVisitor = YamlDotNet.Serialization.ObjectGraphVisitors.CommentsObjectGraphVisitor;

    internal class Program
    {
        public static void Main(string[] args)
        {
            XPConfigManager.Serializer = new SerializerBuilder()
                .WithTypeConverter(new XPConfigCollectionYamlTypeConverter())
                .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                .WithEmissionPhaseObjectGraphVisitor(args2 => new CommentsObjectGraphVisitor(args2.InnerVisitor))
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreFields()
                .Build();

            XPConfigManager.Deserializer = new DeserializerBuilder()
                .WithTypeConverter(new XPConfigCollectionYamlTypeConverter())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), 
                    deserializer => deserializer.InsteadOf<ObjectNodeDeserializer>())
                .IgnoreFields()
                .IgnoreUnmatchedProperties()
                .Build();

            var serialized = XPConfigManager.Serializer.Serialize(XPConfigManager.XPConfigs);
            Console.WriteLine(serialized);

            var deserialized = XPConfigManager.Deserializer.Deserialize<Dictionary<string, IXPConfigCollection>>(serialized);
            Console.WriteLine(string.Join(", ", deserialized.Keys));
        }
    }
}