﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
#if !EFCore
using System.Data.Entity;
#endif
using System.Linq;
using System.Linq.Expressions;
#if EFCore
using Microsoft.EntityFrameworkCore;
#endif
using Microsoft.Restier.Core;
using Microsoft.Restier.Core.Query;

#if EFCore
namespace Microsoft.Restier.EntityFrameworkCore
#else
namespace Microsoft.Restier.EntityFramework
#endif
{
    /// <summary>
    /// Represents a query expression sourcer that uses a DbContext.
    /// </summary>
    internal class EFQueryExpressionSourcer : IQueryExpressionSourcer
    {
        /// <summary>
        /// Sources an expression.
        /// </summary>
        /// <param name="context">
        /// The query expression context.
        /// </param>
        /// <param name="embedded">
        /// Indicates if the sourcing is occurring on an embedded node.
        /// </param>
        /// <returns>
        /// A data source expression that represents the visited node.
        /// </returns>
        public Expression ReplaceQueryableSource(QueryExpressionContext context, bool embedded)
        {
            Ensure.NotNull(context, nameof(context));

            if (context.ModelReference.EntitySet is null)
            {
                // EF provider can only source *ResourceSet*.
                return null;
            }


            if (!(context.QueryContext.Api is IEntityFrameworkApi frameworkApi))
            {
                // Not an EF Api.
                return null;
            }

            var dbContextType = frameworkApi.ContextType;
            var dbContext = frameworkApi.DbContext;

            var dbSetProperty = frameworkApi.ContextType.GetProperties()
                .FirstOrDefault(prop => prop.Name == context.ModelReference.EntitySet.Name);
            if (dbSetProperty is null)
            {
                // EF provider can only source EntitySet from *DbSet property*.
                return null;
            }

            if (!embedded)
            {
                // TODO GitHubIssue#37 : Add API entity manager for tracking entities
                // the underlying DbContext shouldn't track the entities
                var dbSet = dbSetProperty.GetValue(dbContext);

                ////dbSet = dbSet.GetType().GetMethod("AsNoTracking").Invoke(dbSet, null);
                return Expression.Constant(dbSet);
            }
            else
            {
                return Expression.MakeMemberAccess(
                    Expression.Constant(dbContext),
                    dbSetProperty);
            }
        }
    }
}
