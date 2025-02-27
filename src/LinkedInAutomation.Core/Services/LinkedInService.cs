using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Logging;
using SeleniumExtras.WaitHelpers;

namespace LinkedInAutomation.Core.Services
{
    public class LinkedInService : ILinkedinService
    {
        private readonly ILogger<LinkedInService> _logger;
        private readonly string _email;
        private readonly string _password;

        public LinkedInService(ILogger<LinkedInService> logger, string emailId, string secretKey)
        {
            _logger = logger;
            _email = emailId;
            _password = secretKey;
        }

        public async Task<bool> PostContentAsync(string content)
        {
            _logger.LogInformation("🚀 Starting LinkedIn post automation...");

            var options = new ChromeOptions();
            options.AddArgument("--headless=new"); 
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            using (IWebDriver driver = new ChromeDriver(options))
            {
                try
                {
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30)); 

                    _logger.LogInformation("Navigating to LinkedIn login page...");
                    driver.Navigate().GoToUrl("https://www.linkedin.com/login");

                    // Wait until username field is visible
                    var usernameField = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("username")));
                    usernameField.SendKeys(_email);

                    var passwordField = driver.FindElement(By.Id("password"));
                    passwordField.SendKeys(_password);
                    passwordField.SendKeys(Keys.Return);

                    _logger.LogInformation("Logged into LinkedIn successfully.");

                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                    _logger.LogInformation("Navigating to LinkedIn feed...");
                    driver.Navigate().GoToUrl("https://www.linkedin.com/feed/");
                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                    // Dismiss any pop-ups
                    DismissPopups(driver, wait);

                    _logger.LogInformation("Clicking 'Start a post' button...");
                    IWebElement startPostButton = wait.Until(d =>
                        {
                            var btn = d.FindElement(By.XPath("//div[contains(@class, 'share-box-feed-entry__top-bar')]//button[contains(@class, 'artdeco-button')]//span[contains(@class, 'truncate')]//span[contains(@class, 't-normal')]//strong[text()='Start a post']"));
                            ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].scrollIntoView(true);", btn);
                            return (btn.Displayed && btn.Enabled) ? btn : null;
                        });

                    _logger.LogInformation("Entering post content...");
                    IWebElement postBox = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@role='textbox']")));
                    postBox.SendKeys(content);

                    _logger.LogInformation("Clicking 'Post' button...");
                    IWebElement postButton = wait.Until(ExpectedConditions.ElementToBeClickable(
                           By.XPath("//button[contains(@class, 'share-actions__primary-action') and contains(@class, 'artdeco-button--primary')]")
                       ));
                    postButton.Click();
                    _logger.LogInformation("Successfully posted on LinkedIn!");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to post content on LinkedIn.");
                    return false;
                }
                finally
                {
                    driver.Quit();
                }
            }
        }

        private void DismissPopups(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                var popups = driver.FindElements(By.XPath("//button[@aria-label='Dismiss']"));
                foreach (var popup in popups)
                {
                    if (popup.Displayed && popup.Enabled)
                    {
                        popup.Click();
                        _logger.LogInformation("Dismissed a LinkedIn pop-up.");
                    }
                }
            }
            catch (NoSuchElementException)
            {
                _logger.LogInformation("No pop-ups found.");
            }
        }
    }
}
