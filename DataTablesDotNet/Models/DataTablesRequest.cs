using DataTablesDotNet.ModelBinding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DataTablesDotNet.Models
{
    public class DataTablesRequest
    {
        [JsonProperty(PropertyName = "draw")]
        public int Draw { get; set; }

        [JsonProperty(PropertyName = "columns")]
        public List<DataTablesColumn> Columns { get; set; }

        [JsonProperty(PropertyName = "start")]
        public int Start { get; set; }

        [JsonProperty(PropertyName = "length")]
        public int Length { get; set; }

        [JsonProperty(PropertyName = "search")]
        public DataTablesSearch Search { get; set; }

        [JsonProperty(PropertyName = "order")]
        public List<DataTablesOrder> Order { get; set; }

        public DataTablesRequest()
        {
            Columns = new List<DataTablesColumn>();
            Search = new DataTablesSearch();
            Order = new List<DataTablesOrder>();
        }
    }
}