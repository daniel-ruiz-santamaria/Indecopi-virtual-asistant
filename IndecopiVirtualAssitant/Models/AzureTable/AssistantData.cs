using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Models
{
    public class AssistantData : TableEntity
    {
        public String key { get; set; }
        public String value { get; set; }
        public String link { get; set; }
        public String icon { get; set; }
        private String _IdConstant;
        public String IdConstant
        { 
            get
            {
                return this.IdConstant;
            }
            set 
            {
                this.RowKey = value;
                IdConstant = value;
            }
        }

        private String _Topic;
        public String Topic 
        {
            get
            {
                return this._Topic;
            }
            set
            {
                this.PartitionKey = value;
                _Topic = value;
            }
        }
    }
}
