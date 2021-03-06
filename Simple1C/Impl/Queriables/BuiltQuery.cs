﻿using System;
using System.Collections.Generic;

namespace Simple1C.Impl.Queriables
{
    internal class BuiltQuery
    {
        public BuiltQuery(Type entityType, string queryText, Dictionary<string, object> parameters)
        {
            EntityType = entityType;
            QueryText = queryText;
            Parameters = parameters;
        }

        public Type EntityType { get; private set; }
        public string QueryText { get; private set; }
        public Dictionary<string, object> Parameters { get; private set; }
    }
}