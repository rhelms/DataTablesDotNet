using DataTablesDotNet.ModelBinding;
using Newtonsoft.Json;
using System.Reflection;
using System.Web.Mvc;

namespace DataTablesDotNet.Models
{
    public class DataTablesColumn
    {
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonIgnore]
        public int ColumnIndex { get; set; }

        [JsonProperty(PropertyName = "searchable")]
        public bool IsSearchable { get; set; }

        [JsonProperty(PropertyName = "orderable")]
        public bool IsSortable { get; set; }

        [JsonProperty(PropertyName = "search")]
        public DataTablesSearch Search { get; set; }

        [JsonIgnore]
        public PropertyInfo Property { get; set; }

        [JsonIgnore]
        public int SortOrder { get; set; }

        [JsonIgnore]
        public bool IsCurrentlySorted { get; set; }

        [JsonIgnore]
        public string SortDirection { get; set; }

        public DataTablesColumn(string name,
                                int columnIndex,
                                bool isSearchable,
                                bool isSortable)
        {
            Name = name;
            ColumnIndex = columnIndex;
            IsSearchable = isSearchable;
            IsSortable = IsSortable;
            Search = new DataTablesSearch();
        }

        public DataTablesColumn()
            : this(string.Empty, 0, true, true)
        {
        }
    }
}