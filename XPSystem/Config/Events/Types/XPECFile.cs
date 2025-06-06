﻿#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
namespace XPSystem.Config.Events.Types
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using static API.XPAPI;

    /// <summary>
    /// Represents a XP Event Config file.
    /// </summary>
    public abstract class XPECFile
    {
        /// <summary>
        /// The key of the file.
        /// Set by <see cref="XPECManager"/> upon loading.
        /// </summary>
        internal string Key = null!;

        /// <summary>
        /// Gets an item with the specified keys.
        /// Call base to log key usage.
        /// </summary>
        /// <param name="keys">The keys of the item.</param>
        /// <returns>The item.</returns>
        public virtual XPECItem? Get(params object?[]? keys)
        {
            if (Config.Debug || Config.LogXPGainedMethods)
            {
                var keysList = new List<object>();
                if (keys != null)
                    keysList.AddRange(keys.OfType<object>());

                string fullKey = $"{Key}/{string.Join("/", keysList)}";
                LogDebug("Key retrieved: " + fullKey);

                if (Config.LogXPGainedMethods)
                {
                    XPECManager.KeyUsage[fullKey] = XPECManager.KeyUsage.TryGetValue(fullKey, out int count)
                        ? count + 1
                        : 1;
                }
            }

            return null;
        }

        /// <summary>
        /// Applies the contents of the specified parser to this file.
        /// </summary>
        /// <param name="parser">The parser to read from.</param>
        public virtual void Read(IParser parser)
        {
            Type type = GetType();

            while (!parser.Accept(out MappingEnd _))
            {
                if (!parser.TryConsume(out Scalar scalar))
                    throw new InvalidDataException("Invalid YAML content: Expected scalar key.");

                PropertyInfo? property = type.GetProperty(scalar.Value);
                if (property == null)
                {
                    LogWarn("Skipping serialization of unknown property: " + scalar.Value);
                    parser.SkipThisAndNestedEvents();
                    continue;
                }

                object? value = Deserializer.Deserialize(parser, property.PropertyType);
                property.SetValue(this, value);
            }
        }

        /// <summary>
        /// Emits the contents of this file to the specified emitter.
        /// </summary>
        /// <param name="emitter">The emitter to emit to.</param>
        public virtual void Write(IEmitter emitter)
        {
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                if (property.GetMethod == null || property.SetMethod == null)
                    continue;

                emitter.Emit(new Scalar(property.Name));
                ValueSerializer.SerializeValue(emitter, property.GetValue(this), property.PropertyType);
            }
        }

        /// <summary>
        /// Gets the types of the parameters for the Get method.
        /// </summary>
        public virtual Type[][] ParametersTypes { get; } = Array.Empty<Type[]>();

        /// <summary>
        /// Checks if the specified object is of the same type as this file.
        /// Used by the loader for validation.
        /// </summary>
        /// <param name="obj">The object whose type check.</param>
        /// <returns>Whether the object is of the same type as this file.</returns>
        public virtual bool IsEqualType(object obj) => obj.GetType().IsAssignableFrom(GetType());
    }
}