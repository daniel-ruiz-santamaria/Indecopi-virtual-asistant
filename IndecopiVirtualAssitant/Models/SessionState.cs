using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Models
{
    public class SessionState
    {
        public String idSesion { get; set; }
        public bool isLoged { get; set; }
        public User user { get; set; }
        public bool isCalificated { get; set; }

        public SessionState(String _idSesion)
        {
            this.idSesion = _idSesion;
        }
    }
}
