using Microsoft.VisualStudio.TestTools.UnitTesting;
using IndecopiVirtualAsistant.Others;
using System;
using System.Collections.Generic;
using System.Text;

namespace IndecopiVirtualAsistant.Others.Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod()]
        public void getNameTest()
        {
            string name = Others.Utils.getName("Soy Daniel Ruiz extrae mi nombre si puedes","No");
            Assert.AreEqual(name, "Tito");
        }

        [TestMethod()]
        public void getDocumentTest()
        {
            string document = Others.Utils.getDocument("hola, soy Daniel Ruiz Santamaría y mi numero de documento es 50095506B", "No");
            Assert.AreEqual(document, "50095506B");
        }

        [TestMethod()]
        public void getExpedientTest()
        {
            string document = Others.Utils.getExpedient("hola, soy Daniel Ruiz Santamaría y con numero de expediente es 1234-1234", "No");
            Assert.AreEqual(document, "1234-1234");
        }
    }
}