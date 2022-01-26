using IndecopiVirtualAssitant.Services;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IndecopiVirtualAssitant.Models.AzureTable.Enums;

namespace IndecopiVirtualAssitant.Models
{
    public class Feedback : TableEntity
    {

        public String _IdFeedback;
        public String IdFeedback
        {
            get
            {
                return this._IdFeedback;
            }
            set
            {
                this.PartitionKey = value;
                this._IdFeedback = value;
            }
        }

        private String _IdSession;
        public String IdSession
        {
            get
            {
                return this._IdSession;
            }
            set
            {
                this.RowKey = value;
                this._IdSession = value;
            }
        }

        public string IdRegistredUser { get; set; }
        public string calification { get; set; }
        public string comments { get; set; }

    }
}
