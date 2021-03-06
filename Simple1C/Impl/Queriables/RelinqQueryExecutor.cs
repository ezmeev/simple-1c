﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq;

namespace Simple1C.Impl.Queriables
{
    internal class RelinqQueryExecutor : IQueryExecutor
    {
        private readonly TypeMapper typeMapper;
        private readonly Func<BuiltQuery, IEnumerable> execute;

        public RelinqQueryExecutor(TypeMapper typeMapper, Func<BuiltQuery, IEnumerable> execute)
        {
            this.typeMapper = typeMapper;
            this.execute = execute;
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel).Single();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty
                ? ExecuteCollection<T>(queryModel).SingleOrDefault()
                : ExecuteCollection<T>(queryModel).Single();
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var builder = new QueryBuilder(typeMapper);
            queryModel.Accept(new QueryModelVisitor(builder));
            return execute(builder.Build()).Cast<T>();
        }
    }
}