using BotTestDBB.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;

namespace BotTestDBB.Steps
{
    [Binding]
    class AsistantIdentificationSteps
    {
        //Anti-Context Injection code
        VirtualAsistantPage vaPage = null;
        public string asistantURL = "https://storagepoc5.blob.core.windows.net/indecopi-bot/index_bot.html";

        // Steps definitions
        [Given(@"Cuando se inicializa el asistente")]
        public void GivenStartVirtualAsistant()
        {
            IWebDriver webDriver = new ChromeDriver();
            webDriver.Navigate().GoToUrl(asistantURL);
            vaPage = new VirtualAsistantPage(webDriver);
        }

        [Then(@"Este se identifica como un asistente virtual")]
        public void ThenIShouldSeeEmployeeDetailsLink()
        {
            var x = vaPage;
            Assert.That(vaPage.checkIfLastConversationAsText(), Is.True);
        }

    }
}
