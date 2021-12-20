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
        public String fullName { get; set; }
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

        private DocumentType _documentType;
        public DocumentType documentType 
        {
            get
            {
                return this._documentType;
            }
            set
            {
                this.PartitionKey = value.ToString();
                _documentType = value;
            }
        }
    }
}
