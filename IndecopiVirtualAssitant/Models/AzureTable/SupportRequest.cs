using IndecopiVirtualAssitant.Services;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IndecopiVirtualAssitant.Models.AzureTable.Enums;

namespace IndecopiVirtualAssitant.Models
{
    public class SupportRequest : TableEntity
    {

        public String _IdSupporRequest;
        public String IdSupporRequest
        {
            get
            {
                return this._IdSupporRequest;
            }
            set
            {
                this.PartitionKey = value;
                this._IdSupporRequest = value;
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

        public string userFullName { get; set; }
        public string documentType { get; set; }
        public string documentId { get; set; }
        public string IdRegistredUser { get; set; }

        public User user { get; set; }

        public string departmentId { get; set; }

        public string departmentName{ get; set; }

        public string email { get; set; }

        public string phone { get; set; }

        public string request { get; set; }

        public SupportRequest(State state)
        {
            this._IdSupporRequest = state.idRequest;
            this.PartitionKey = this._IdSupporRequest;
        }

    }
}
