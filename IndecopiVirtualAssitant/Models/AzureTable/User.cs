using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IndecopiVirtualAssitant.Models.AzureTable.Enums;

namespace IndecopiVirtualAssitant.Models
{
    public class User : TableEntity
    {
        private String _IdRegistredUser;
        public String IdRegistredUser {
            get
            {
                return this.documentType.ToString() +"-"+ this._IdDocument;
            }
            set
            {
                _IdRegistredUser = value;
            }
        }
        public String fullName { get; set; }
        public bool isLoged { get; set; } = false;

        public DocumentType documentType { get; set; }

        private String _IdDocument;
        public String IdDocument
        { 
            get
            {
                return this._IdDocument;
            }
            set 
            {
                this.RowKey = value;
                _IdDocument = value;
            }
        }

        private string _idSession;
        public string idSession
        {
            get
            {
                return this._idSession;
            }
            set
            {
                this.PartitionKey = value.ToString();
                _idSession = value;
            }
        }
    }
}
