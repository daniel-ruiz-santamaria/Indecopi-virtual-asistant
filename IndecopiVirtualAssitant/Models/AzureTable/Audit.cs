using IndecopiVirtualAssitant.Services;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace IndecopiVirtualAssitant.Models.AzureTable
{
    public class Audit : TableEntity
    {
        public String channelId { get; set; }
        public String channelName { get; set; }
        public String intent { get; set; }
        public double score { get; set; }
        public String query { get; set; }
        public String answer { get; set; }
        public String idUser { get; set; }
        public String nameUser { get; set; }
        public DateTime date { get; set; }
        private String _IdAudit;
        public String IdAudit
        {
            get
            {
                return this._IdAudit;
            }
            set
            {
                this.RowKey = value;
                _IdAudit = value;
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
                this.PartitionKey = value;
                _IdSession = value;
            }
        }

        public Audit()
        {

        }

        public Audit(String idSession)
        {
            PartitionKey = idSession;
            _IdSession = idSession;
        }

        public Audit(State state)
        {
            RowKey = state.idRequest;
            _IdAudit = state.idRequest;
            PartitionKey = state.idSession;
            _IdSession = state.idSession;
            channelId = state.idChannel;
            channelName = state.nameChannel;
            date = DateTime.Now;
            idUser = state.idUser;
            nameUser = state.nameUser;
        }
    }
}
