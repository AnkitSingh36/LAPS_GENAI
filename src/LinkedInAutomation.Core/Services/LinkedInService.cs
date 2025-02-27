using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
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
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            using (IWebDriver driver = new ChromeDriver(options))
            {
                try
                {
                    driver.Navigate().GoToUrl("https://www.linkedin.com/login");
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    wait.Until(d => d.FindElement(By.Id("username"))).SendKeys(_email);

                    driver.FindElement(By.Id("password")).SendKeys(_password);
                    driver.FindElement(By.Id("password")).SendKeys(Keys.Return);

                    _logger.LogInformation("Logged into LinkedIn successfully.");
                    await Task.Delay(5000); 

                    driver.Navigate().GoToUrl("https://www.linkedin.com/feed/");
                    await Task.Delay(3000);

                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                    _logger.LogInformation("Clicking on start post button");


                    try
                    {
                        // Ensure any modal/pop-up is closed
                        try
                        {
                            var closeButton = driver.FindElement(By.XPath("//button[@aria-label='Dismiss']"));
                            if (closeButton.Displayed)
                            {
                                closeButton.Click();
                                Thread.Sleep(1000);
                            }
                        }
                        catch (NoSuchElementException)
                        {
                            Console.WriteLine("No pop-up detected.");
                        }

                        // Wait for "Start a post" button to be visible
                        IWebElement startPostButton = wait.Until(d =>
                        {
                            var btn = d.FindElement(By.XPath("//div[contains(@class, 'share-box-feed-entry__top-bar')]//button[contains(@class, 'artdeco-button')]//span[contains(@class, 'truncate')]//span[contains(@class, 't-normal')]//strong[text()='Start a post']"));
                            ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].scrollIntoView(true);", btn);
                            return (btn.Displayed && btn.Enabled) ? btn : null;
                        });



                        // Scroll into view before clicking
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", startPostButton);
                        Thread.Sleep(500);

                        // Click "Start a Post"
                        startPostButton.Click();
                        Console.WriteLine("Clicked on 'Start a post' button successfully!");

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                   

                    await Task.Delay(2000);

                    _logger.LogInformation("Entering post content");

                    // Enter post content
                    var postBox = wait.Until(d => d.FindElement(By.XPath("//div[@role='textbox']")));
                    postBox.SendKeys(content);
                    await Task.Delay(2000);

                    // Click "Post"
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
    }
}
