using OpenQA.Selenium;
using System;

namespace BotTestDBB.Pages
{
    class VirtualAsistantPage
    {
        public IWebDriver WebDriver { get; }

        public VirtualAsistantPage(IWebDriver webDriver)
        {
            WebDriver = webDriver;
        }

        public bool checkIfLastConversationAsText() {
            var chat = WebDriver.FindElements(By.XPath("/html/body/div/div/div[2]"));
            foreach (var c in chat) {
                Console.WriteLine("Text: {0}", c.Text.Trim());
            }
            return true;
        }



    }
}
