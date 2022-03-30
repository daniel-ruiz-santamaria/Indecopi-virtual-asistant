using OpenQA.Selenium;

namespace BotTestDBB.Pages
{
    class LoginPage
    {
        public IWebDriver WebDriver { get; }

        public LoginPage(IWebDriver webDriver)
        {
            WebDriver = webDriver;
        }

        // UI Elements
        public IWebElement lnkLogin => WebDriver.FindElement(By.LinkText("Login"));

        public IWebElement txtUserName => WebDriver.FindElement(By.Name("UserName"));

        public IWebElement txtPassword => WebDriver.FindElement(By.Name("Password"));

        public IWebElement btnLogin => WebDriver.FindElement(By.CssSelector(".btn-default"));

        public IWebElement lnkEmployeeDeatils => WebDriver.FindElement(By.LinkText("Employee Details"));

        public void ClickLogin() => lnkLogin.Click();

        public void Login(string userName, string password)
        {
            txtUserName.SendKeys(userName);
            txtPassword.SendKeys(password);
        }

        public void ClickLoginButton() => btnLogin.Submit();

        public bool IsEmployeeDetailsExist() => lnkEmployeeDeatils.Displayed;


    }
}
