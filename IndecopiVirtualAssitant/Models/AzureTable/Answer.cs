/*
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Models
{
    public class Answer : TableEntity
    {
        public String answer { get; set; }
        private String _IdAnswer;
        public String IdAnswer
        { 
            get
            {
                return this._IdAnswer;
            }
            set 
            {
                this.RowKey = value;
                _IdAnswer = value;
            }
        }

        private String _AnswerType;
        public String AnswerType 
        {
            get
            {
                return this._AnswerType;
            }
            set
            {
                this.PartitionKey = value;
                _AnswerType = value;
            }
        }
    }
}
*/