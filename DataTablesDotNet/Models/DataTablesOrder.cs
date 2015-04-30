using DataTablesDotNet.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataTablesDotNet.Models
{
    public class DataTablesOrder
    {
        /// <summary>
        /// Column to which ordering should be applied. This is an index
        /// reference to the columns array of information that is also
        /// submitted to the server.
        /// </summary>
        [JsonProperty(PropertyName = "column")]
        public int Column { get; set; }

        /// <summary>
        /// Ordering direction for this column. It will be asx or desc to
        /// indicate ascending ordering descending ordering, respectively.
        /// </summary>
        [JsonProperty(PropertyName = "dir")]
        public string Dir { get; set; }
    }
}