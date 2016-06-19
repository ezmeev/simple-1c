using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Simple1C.Impl.Helpers;
using Simple1C.Interface;
using Simple1C.Interface.ObjectModel;

namespace Simple1C.Impl
{
    internal struct ConfigurationName : IEquatable<ConfigurationName>
    {
        private static readonly ConcurrentDictionary<Type, ConfigurationName?> cache =
            new ConcurrentDictionary<Type, ConfigurationName?>();

        private static readonly Dictionary<string, Type> typeMapping = typeof (ConfigurationName)
            .Assembly.GetTypes()
            .Where(x => x.IsClass || x.IsEnum)
            .Select(x => new
            {
                typeName1C = GetOrNull(x),
                type = x
            })
            .Where(x => x.typeName1C.HasValue)
            .ToDictionary(x => x.typeName1C.Value.Fullname, x => x.type);

        private static Func<Type, ConfigurationName?> createAttributes;
        private readonly ConfigurationScope scope;
        private readonly string name;

        public ConfigurationName(ConfigurationScope scope, string name)
        {
            this.scope = scope;
            this.name = name;
        }

        public ConfigurationScope Scope
        {
            get { return scope; }
        }

        public string Name
        {
            get { return name; }
        }

        public static ConfigurationName? GetOrNull(Type type)
        {
            if (createAttributes == null)
                createAttributes = CreateName;
            return cache.GetOrAdd(type, createAttributes);
        }

        public Type GetTypeOrNull()
        {
            return typeMapping.GetOrDefault(Fullname);
        }

        public static ConfigurationName Get(Type type)
        {
            var result = GetOrNull(type);
            if (!result.HasValue)
            {
                const string messageFormat = "can't get [ConfigurationName] for [{0}]";
                throw new InvalidOperationException(string.Format(messageFormat, type.FormatName()));
            }
            return result.Value;
        }

        public static ConfigurationName Parse(string s)
        {
            var items = s.Split('.');
            return new ConfigurationName(ParseScopeName(items[0]), items[1]);
        }

        public string Fullname
        {
            get { return GetScopeName() + "." + name; }
        }

        public ConfigurationName ChangeName(string newName)
        {
            return new ConfigurationName(scope, newName);
        }

        private string GetScopeName()
        {
            switch (scope)
            {
                case ConfigurationScope.�����������:
                    return "����������";
                case ConfigurationScope.���������:
                    return "��������";
                case ConfigurationScope.����������������:
                    return "���������������";
                case ConfigurationScope.������������:
                    return "������������";
                case ConfigurationScope.�����������:
                    return "����������";
                default:
                    const string messageFormat = "unexpected scope [{0}]";
                    throw new InvalidOperationException(string.Format(messageFormat, scope));
            }
        }

        private static ConfigurationScope ParseScopeName(string s)
        {
            if (s == "����������")
                return ConfigurationScope.�����������;
            if (s == "��������")
                return ConfigurationScope.���������;
            if (s == "���������������")
                return ConfigurationScope.����������������;
            if (s == "������������")
                return ConfigurationScope.������������;
            if (s == "����������")
                return ConfigurationScope.�����������;
            const string messageFormat = "unexpected configuration scope name [{0}]";
            throw new InvalidOperationException(string.Format(messageFormat, s));
        }

        public bool Equals(ConfigurationName other)
        {
            return scope == other.scope && string.Equals(name, other.name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ConfigurationName && Equals((ConfigurationName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) scope*397) ^ name.GetHashCode();
            }
        }

        public static bool operator ==(ConfigurationName left, ConfigurationName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConfigurationName left, ConfigurationName right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return Fullname;
        }

        private static ConfigurationName? CreateName(Type type)
        {
            if (type.IsEnum)
                return new ConfigurationName(ConfigurationScope.������������, type.Name);
            var attributes = (ConfigurationScopeAttribute[])
                type.GetCustomAttributes<ConfigurationScopeAttribute>(true);
            if (attributes.Length == 0)
                return null;
            if (attributes.Length > 1)
            {
                const string messageFormat = "more than one [ConfigurationScopeAttribute]'s for type: [{0}]";
                throw new InvalidOperationException(string.Format(messageFormat, type.FormatName()));
            }
            return new ConfigurationName(attributes[0].Scope, type.Name);
        }
    }
}