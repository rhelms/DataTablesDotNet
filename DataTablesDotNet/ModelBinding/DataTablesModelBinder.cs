using DataTablesDotNet.Models;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataTablesDotNet.ModelBinding
{
    public class DataTablesModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            var queryString = request.QueryString;

            var model = new DataTablesRequest();

            if (queryString.Count > 0)
            {
                model.Draw = Int32.Parse(queryString["draw"]);
                model.Start = Int32.Parse(queryString["start"]);
                model.Length = Int32.Parse(queryString["length"]);
                model.Search.Value = queryString["search[value]"];
                model.Search.Regex = Boolean.Parse(queryString["search[regex]"]);

                int i = 0;
                while (true)
                {
                    string key = "columns[" + i + "]";
                    if (!ContainsKey(queryString, key + "[data]")
                    || !ContainsKey(queryString, key + "[name]")
                    || !ContainsKey(queryString, key + "[searchable]")
                    || !ContainsKey(queryString, key + "[orderable]")
                    || !ContainsKey(queryString, key + "[search][value]")
                    || !ContainsKey(queryString, key + "[search][regex]"))
                    {
                        break;
                    }
                    else
                    {
                        bool parseSuccess;

                        var column = new DataTablesColumn();
                        column.ColumnIndex = i;
                        column.Data = queryString[key + "[data]"];
                        column.Name = queryString[key + "[name]"];

                        bool searchable;
                        parseSuccess = Boolean.TryParse(queryString[key + "[searchable]"], out searchable);
                        if (parseSuccess)
                        {
                            column.IsSearchable = searchable;
                        }
                        else
                        {
                            throw new Exception("Unable to parse column data. columns[" + i +
                                "][searchable] was unable to be parsed as a valid boolean value. The value received was " +
                                queryString[key + "[searchable]"]);
                        }

                        bool orderable;
                        parseSuccess = Boolean.TryParse(queryString[key + "[orderable]"], out orderable);
                        if (parseSuccess)
                        {
                            column.IsSortable = orderable;
                        }
                        else
                        {
                            throw new Exception("Unable to parse column data. columns[" + i +
                                "][orderable] was unable to be parsed as a valid boolean value. The value received was " +
                                queryString[key + "[orderable]"]);
                        }

                        column.Search.Value = queryString[key + "[search][value]"];

                        bool searchRegex;
                        parseSuccess = Boolean.TryParse(queryString[key + "[search][regex]"], out searchRegex);
                        if (parseSuccess)
                        {
                            column.Search.Regex = searchRegex;
                        }
                        else
                        {
                            throw new Exception("Unable to parse column data. columns[" + i +
                                "][search][regex] was unable to be parsed as a valid boolean value. The value received was " +
                                queryString[key + "[search][regex]"]);
                        }
                        model.Columns.Insert(i++, column);
                    }
                }

                i = 0;
                while (true)
                {
                    string key = "order[" + i + "]";
                    if (!ContainsKey(queryString, key + "[column]")
                    || !ContainsKey(queryString, key + "[dir]"))
                    {
                        break;
                    }
                    else
                    {
                        var order = new DataTablesOrder();
                        order.Column = Int32.Parse(queryString[key + "[column]"]);
                        order.Dir = queryString[key + "[dir]"];

                        model.Order.Insert(i++, order);
                    }
                }
            }

            return model;
        }

        private bool ContainsKey(NameValueCollection collection, string key)
        {
            if (collection.Get(key) == null)
            {
                return collection.AllKeys.Contains(key);
            }

            return true;
        }
    }
}