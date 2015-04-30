DataTablesDotNet
================

Based on [DataTablePager](http://activeengine.net/2011/02/09/datatablepager-now-has-multi-column-sort-capability-for-datatables-net/)  
_* Includes updated code from the comment section of the above post_

The [datatables](http://datatables.net/) plugin is one of the coolest jQuery plugins around, and I wanted to find a way to make it work easily with ASP.NET MVC sites. Initially I created some objects that included the various datatables property names and that worked well. However, when another project came along and I wanted to use datatables in _that project too_ I realized the original way I created the objects just wasn't going to cut it.

After a lot of searching I found the above blog post. I liked what I saw. Even better was the additional code contributed by Paul Inglis in the comments. The only thing I did not like was the stringify used to turn the datatables JSON into key/value pairs. I also did not like the way the project was made available - a box.net download location with no way to contribute (hence the code in the comments). I decided I would build on these great contributions and update the code to work without the serialization.

So, here it is. The code has been cleaned up, it uses Paul's Expression code for building the filters/sorts, and it doesn't stringify the datatables data which cuts the data sent pretty much in half. I also added a model binder so it all "just works".

This version uses the 1.10 version of the DataTables API.
---

How to use
----------
1. Add reference to DLL or add via nuget:  
    `PM> Install-Package DataTablesDotNet`
2. Add model binder to global.asax:  
    `ModelBinders.Binders.Add(typeof(DataTablesRequest), new DataTablesModelBinder());`
3. Add the model type parameter to the appropriate controller method:  
    `public JsonResult GetUsers(DataTablesRequest model) {
        ...
    }`
4. Pass bound model and "all" data enumerable to an new instance of DataTablesParser:  
    `var dataTableParser = new DataTablesParser<User>(model, data);`
5. Call the Process() method:  
    `var formattedList = dataTableParser.Process();`
6. Return the formatted data:  
    `return Json(formattedList, JsonRequestBehavior.AllowGet);`

For best results, implement a JsonNetActionFilter to return a result serialized with JSON.Net, so that the returned properties can remain as lower case. The implementation used in the project has been borrowed from [Using JSON.Net As Default JSON Serializer in MVC](http://labs.bjfocus.co.uk/2014/06/using-json-net-as-default-json-serializer-in-mvc/).

If you want to see it in action, you can download the code and run the included MVC app in the debugger.

Examples
--------

Example using EF `ObjectContext`:

    public DataTablesData GetData(DataTablesRequest dt) {
        if (dt == null) {
            throw new ArgumentException("dt");
        }

        using (var context = new ObjectContext(_connectionString)) {
            using (var repo = new Repository<MyObject>(context)) {
                var query = repo.Table.OrderBy(t => t.SortValue).AsQueryable();
                var parser = new DataTablesParser<MyObject>(dt, query);
                var data = parser.Process();
                return data;
            }
        }
    }