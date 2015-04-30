using DataTablesDotNet.Helpers;
using DataTablesDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTablesDotNet
{
    public class DataTablesParser<T>
    {
        private IQueryable<T> data;
        private DataTablesRequest requestModel;
        private List<DataTablesColumn> dataTablesColumns;

        public DataTablesParser(DataTablesRequest dtm, IQueryable<T> queryable)
        {
            requestModel = dtm;
            data = queryable;
            Parse(requestModel);
        }

        public DataTablesData Process()
        {
            var model = new DataTablesData();
            //model.sColumns = requestModel.Columns;
            model.Draw = requestModel.Draw;
            model.RecordsTotal = data.Count();

            var records = Filter(data, requestModel);
            records = ApplySort(records);

            model.RecordsFiltered = (records.FirstOrDefault() == null) ? 0 : records.Count();

            // if paging is disabled, Length is always -1, so we check for that and take all records in that case.
            var takeRecords = (requestModel.Length == -1) ? model.RecordsTotal : requestModel.Length;
            var pagedRecords = records.Skip(requestModel.Start)
                     .Take(takeRecords);

            var responseData = new List<List<string>>();

            var columnsAsCsv = string.Join(",", Array.ConvertAll(requestModel.Columns.ToArray(), i => i.Name));
            pagedRecords.ToList()
                .ForEach(rec => responseData.Add(rec.PropertiesToList(columnsAsCsv)));
            model.Data = responseData;

            return model;
        }

        private void Parse(DataTablesRequest requestModel)
        {
            var sortKeys = requestModel.Order;
            /*
            var columns = requestModel.sColumns.Split(',').Select(c => {
                if (String.IsNullOrEmpty(c)) {
                    return String.Empty;
                } else {
                    return c;
                }
            }).ToList();
             */

            // set the properties from the T on the columns
            dataTablesColumns = requestModel.Columns;
            var properties = typeof(T).GetProperties();
            dataTablesColumns.ForEach(col =>
            {
                //if (string.IsNullOrEmpty(col) == false) {
                //    int i = columns.IndexOf(col);
                //    var dtColumn = new DataTablesColumn(col, i, false, false) {
                //        IsSearchable = requestModel.bSearchable[i],
                //        Property = properties.Where(x => x.Name == col).SingleOrDefault()
                //    };

                //    dataTablesColumns.Add(dtColumn);
                //}
                col.Property = properties.Where(x => x.Name == col.Name).SingleOrDefault();
            });

            // set the sortable properties
            /*
            dataTablesColumns.ForEach(sortable => {
                sortable.IsSortable = requestModel.bSortable[sortable.ColumnIndex];
                sortable.SortOrder = -1;

                //  Is this item amongst currently sorted columns?
                sortKeyPrefix.ForEach(keyPrefix => {
                    if (sortable.ColumnIndex == Convert.ToInt32(keyPrefix)) {
                        int order = sortKeyPrefix.IndexOf(keyPrefix);
                        sortable.IsCurrentlySorted = true;

                        //  Is this the primary sort column or secondary?
                        sortable.SortOrder = order;

                        //  Ascending or Descending?
                        if (requestModel.sSortDir.Count >= order) {
                            sortable.SortDirection = requestModel.sSortDir[order];
                        }
                    }
                });
            });
            */
            dataTablesColumns.ForEach(column =>
            {
                //sortable.IsSortable = requestModel.bSortable[sortable.ColumnIndex];
                column.SortOrder = -1;

                //  Is this item amongst currently sorted columns?
                int sortKeyIndex = -1;
                sortKeys.ForEach(sortKey =>
                {
                    sortKeyIndex++;
                    if (column.ColumnIndex == Convert.ToInt32(sortKey.Column))
                    {
                        column.IsCurrentlySorted = true;

                        //  Is this the primary sort column or secondary?
                        column.SortOrder = sortKeyIndex;

                        //  Ascending or Descending?
                        column.SortDirection = sortKey.Dir;
                    }
                });
            });
        }

        /// <summary>
        /// Sort the queryable items according to column selected
        /// </summary>
        private IQueryable<T> ApplySort(IQueryable<T> records)
        {
            var sorted = dataTablesColumns.Where(x => x.IsCurrentlySorted == true)
                                                .OrderBy(x => x.SortOrder)
                                                .ToList();

            //  Are we at initialization of grid with no column selected?
            if (sorted.Count == 0)
            {
                //int firstColumn = Convert.ToInt32(requestModel.iSortCol.First());
                int firstColumn = Convert.ToInt32(requestModel.Order.First().Column);
                //string sortDirection = requestModel.sSortDir.First();
                string sortDirection = requestModel.Order.First().Dir;
                if (String.IsNullOrEmpty(sortDirection))
                {
                    sortDirection = "asc";
                }

                //  Initial display will set order to first column - column 0
                //  When column 0 is not sortable, find first column that is
                var sortable = dataTablesColumns.Where(x => x.ColumnIndex == firstColumn)
                                                        .SingleOrDefault();
                if (sortable == null)
                {
                    sortable = dataTablesColumns.First(x => x.IsSortable);
                }

                return records.OrderBy(sortable.Name, sortDirection, true);
            }
            else
            {
                //  Traverse all columns selected for sort
                sorted.ForEach(sort =>
                {
                    records = records.OrderBy(sort.Name, sort.SortDirection,
                        (sort.SortOrder == 0) ? true : false);
                });

                return records;
            }
        }

        private static MethodInfo miTL = typeof(String).GetMethod("ToLower", System.Type.EmptyTypes);
        private static MethodInfo miS = typeof(String).GetMethod("StartsWith", new Type[] { typeof(String) });
        private static MethodInfo miC = typeof(String).GetMethod("Contains", new Type[] { typeof(String) });
        private static MethodInfo miE = typeof(String).GetMethod("EndsWith", new Type[] { typeof(String) });

        /// <summary>
        /// Create a Lambda Expression that is chain of Or Expressions
        /// for each column. Each column will be tested if it contains the
        /// generic search string.
        ///</summary>
        /// <remarks>
        /// Query logic = (or … or …) And (or … or …)
        /// </remarks>
        /// <returns>IQueryable of T</returns>
        private static IQueryable<T> Filter(IQueryable source, DataTablesRequest requestModel)
        {
            // If there is no search term wew don't need to bother with filtering
            /*
            if (String.IsNullOrEmpty(requestModel.Search.Value))
            {
                return (IQueryable<T>)source;
            }
            */

            var predicate = PredicateBuilder.False<T>();
            var predicateAltered = false;
            var obj = Expression.Parameter(typeof(T));

            //var columnNames = requestModel.sColumns.Split(',');
            var propNames = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .Where(e => e.PropertyType == typeof(string))
                                     .Select(x => x.Name).ToList();

            //for (int i = 0; i < columnNames.Length; i++)
            //{
            // TODO Handle searches that use Regex
            requestModel.Columns.ForEach(column =>
            {
                if (column.IsSearchable && propNames.Contains(column.Name))
                {
                    // apply global filter
                    if (!string.IsNullOrEmpty(requestModel.Search.Value))
                    {
                        var filterBy = FilterByString(obj, column.Name, requestModel.Search.Value);
                        predicate = predicate.Or(filterBy);
                        predicateAltered = true;
                    }

                    // apply column specific filter
                    if (!string.IsNullOrEmpty(column.Search.Value))
                    {
                        var filterBy = FilterByString(obj, column.Name, column.Search.Value);
                        predicate = predicate.Or(filterBy);
                        predicateAltered = true;
                    }
                }
            });
            //}

            // no predicate actually set, so don't bother with filtering
            if (!predicateAltered)
            {
                return (IQueryable<T>)source;
            }

            var rewired = RewireLambdaExpression(predicate, obj);
            var filter = source.OfType<T>().Where(rewired);
            return filter;
        }

        /// <summary>
        /// Creates a lambda expression for the given property
        ///</summary>
        ///<remarks>
        /// Anonymous class
        /// </remarks>
        /// <param name="property">property name</param>
        /// <param name="value">value of search phrase</param>
        /// <returns>Expression of T, bool</returns>
        private static Expression<Func<T, bool>> FilterByString(ParameterExpression obj, string property, string value)
        {
            var propertySelector = Expression.PropertyOrField(obj, property);
            ParameterExpression parameterExpression = null;
            Expression constExp = Expression.Constant(value.ToLower());
            var memberExpression = GetMemberExpression(propertySelector, out parameterExpression);
            var dynamicExpression = Expression.Call(memberExpression, miTL);
            dynamicExpression = Expression.Call(dynamicExpression, miC, constExp);

            var pred = Expression.Lambda<Func<T, bool>>(dynamicExpression, obj);
            return pred;
        }

        /// <summary>
        /// Gets the member expression of given property
        ///</summary>
        /// <returns>Expression</returns>
        private static Expression GetMemberExpression(Expression expression, out ParameterExpression parameterExpression)
        {
            parameterExpression = null;
            if (expression is MemberExpression)
            {
                var memberExpression = expression as MemberExpression;
                while (!(memberExpression.Expression is ParameterExpression))
                {
                    memberExpression = memberExpression.Expression as MemberExpression;
                }

                parameterExpression = memberExpression.Expression as ParameterExpression;
                return expression as MemberExpression;
            }

            if (expression is MethodCallExpression)
            {
                var methodCallExpression = expression as MethodCallExpression;
                parameterExpression = methodCallExpression.Object as ParameterExpression;
                return methodCallExpression;
            }

            return null;
        }

        private static Expression<Func<TEntity, TReturnType>> RewireLambdaExpression<TEntity, TReturnType>(Expression<Func<TEntity, TReturnType>> expression,
                                                                                                            ParameterExpression newLambdaParameter)
        {
            var newExp = new ExpressionSubstitute(expression.Parameters.Single(), newLambdaParameter).Visit(expression);
            return (Expression<Func<TEntity, TReturnType>>)newExp;
        }
    }
}