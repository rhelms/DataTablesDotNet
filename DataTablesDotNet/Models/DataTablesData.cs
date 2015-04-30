using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DataTablesDotNet.Models
{
    public class DataTablesData
    {
        [JsonProperty(PropertyName = "draw")]
        public int Draw { get; set; }

        [JsonProperty(PropertyName = "recordsTotal")]
        public int RecordsTotal { get; set; }

        [JsonProperty(PropertyName = "recordsFiltered")]
        public int RecordsFiltered { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<List<string>> Data { get; set; }

        // TODO: This is supposed to be optional, so may need to do a separate object to report errors
        //[JsonProperty(PropertyName = "error")]
        //public string Error { get; set; }
    }
}