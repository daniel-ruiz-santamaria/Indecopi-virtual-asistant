using Microsoft.VisualStudio.TestTools.UnitTesting;
using IndecopiVirtualAsistant.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace IndecopiVirtualAsistant.Services.Tests
{
    [TestClass()]
    public class ExpedientRequestServiceTests
    {
        [TestMethod()]
        public void SearchExpedientsByYearTest()
        {





            ExpedientRequestService ers = new ExpedientRequestService();
            var response = ers.SearchExpedientsByYear("Tito", "46678997", "2021");
        }
    }
}