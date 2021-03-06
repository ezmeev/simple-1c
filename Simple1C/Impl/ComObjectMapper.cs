﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Simple1C.Impl.Com;
using Simple1C.Impl.Helpers;
using Simple1C.Interface.ObjectModel;

namespace Simple1C.Impl
{
    internal class ComObjectMapper
    {
        private readonly EnumMapper enumMapper;
        private readonly TypeMapper typeMapper;

        public ComObjectMapper(EnumMapper enumMapper, TypeMapper typeMapper)
        {
            this.enumMapper = enumMapper;
            this.typeMapper = typeMapper;
        }

        public object MapFrom1C(object source, Type declaredType)
        {
            var type = declaredType;
            if (source == null || source == DBNull.Value)
                return null;
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type == typeof (object))
                type = ResolvePropertyType(source);
            if (type.IsEnum)
                return (bool) ComHelpers.Invoke(source, "IsEmpty")
                    ? null
                    : enumMapper.MapFrom1C(type, source);
            if (typeof (Abstract1CEntity).IsAssignableFrom(type))
            {
                var isEmpty = !type.Name.StartsWith("ТабличнаяЧасть")
                              && (bool) ComHelpers.Invoke(source, "IsEmpty");
                if (isEmpty)
                    return null;
                var result = (Abstract1CEntity) FormatterServices.GetUninitializedObject(type);
                result.Controller = new ComBasedEntityController(source, this);
                return result;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (List<>))
            {
                var itemType = type.GetGenericArguments()[0];
                if (!typeof (Abstract1CEntity).IsAssignableFrom(itemType))
                    throw new InvalidOperationException("assertion failure");
                var itemsCount = Convert.ToInt32(ComHelpers.Invoke(source, "Количество"));
                var list = ListFactory.Create(itemType, null, itemsCount);
                for (var i = 0; i < itemsCount; ++i)
                    list.Add(MapFrom1C(ComHelpers.Invoke(source, "Получить", i), itemType));
                return list;
            }
            return source is IConvertible ? Convert.ChangeType(source, type) : source;
        }

        private Type ResolvePropertyType(object value)
        {
            if (value is string)
                return typeof (string);
            if (value is bool)
                return typeof (bool);
            if (value is DateTime)
                return typeof (DateTime);
            var typeName = Convert.ToString(ComHelpers.Invoke(ComHelpers.Invoke(value, "Метаданные"), "ПолноеИмя"));
            var type = typeMapper.GetTypeOrNull(typeName);
            if (type != null)
                return type;
            const string messageFormat = "can't resolve .NET type by 1c type [{0}]";
            throw new InvalidOperationException(string.Format(messageFormat, typeName));
        }
    }
}