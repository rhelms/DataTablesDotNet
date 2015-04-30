﻿using DataTablesDotNet.Mvc.Mvc;
using System.Web;
using System.Web.Mvc;

namespace DataTablesDotNet.Mvc
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new JsonNetActionFilter());
        }
    }
}