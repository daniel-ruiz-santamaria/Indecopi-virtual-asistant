using IndecopiVirtualAssitant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndecopiVirtualAssitant.Services
{
    public class SessionsData
    {
        private Dictionary<String, SessionState> sesionState;

        public SessionsData()
        {
            this.sesionState = new Dictionary<string, SessionState>();
        }

        public void addSesionState(SessionState sesionState) {

            this.sesionState[sesionState.idSesion] = sesionState;
        }

        public SessionState getSesionState(String idSesion)
        {
            if (this.sesionState.ContainsKey(idSesion)) {
                return this.sesionState[idSesion];
            } else
            {
                return null;
            }
        }

        public void addUserBySesionState(String idSesion, User user)
        {
            if (this.sesionState.ContainsKey(idSesion))
            {
                this.sesionState[idSesion].user = user;
            }
        }
    }
}
