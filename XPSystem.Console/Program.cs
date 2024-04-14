namespace XPSystem.Console
{
    using System;
    using System.Collections.Generic;
    using Exiled.Loader.Features.Configs;
    using XPSystem.Config;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.NodeDeserializers;
    using CommentsObjectGraphVisitor = YamlDotNet.Serialization.ObjectGraphVisitors.CommentsObjectGraphVisitor;

    internal class Program
    {
        public static void Main(string[] args)
        {
            XpTranslationsManager.Serializer = new SerializerBuilder()
                .WithTypeConverter(new XPConfigCollectionYamlTypeConverter())
                .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                .WithEmissionPhaseObjectGraphVisitor(args2 => new CommentsObjectGraphVisitor(args2.InnerVisitor))
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreFields()
                .Build();

            XpTranslationsManager.Deserializer = new DeserializerBuilder()
                .WithTypeConverter(new XPConfigCollectionYamlTypeConverter())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), 
                    deserializer => deserializer.InsteadOf<ObjectNodeDeserializer>())
                .IgnoreFields()
                .IgnoreUnmatchedProperties()
                .Build();

            var serialized = XpTranslationsManager.Serializer.Serialize(XpTranslationsManager.XPConfigs);
            Console.WriteLine(serialized);

            var deserialized = XpTranslationsManager.Deserializer.Deserialize<Dictionary<string, IXPConfigCollection>>(serialized);
            Console.WriteLine(string.Join(", ", deserialized.Keys));
        }
    }
}