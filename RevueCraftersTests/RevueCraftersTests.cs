using NUnit.Framework.Constraints;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace RevueCraftersTests
{
    public class Tests
    {
        private WebDriver driver;
        private readonly static string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        private Actions actions;
        private static string? lastCreatedRevueTitle;
        private static string? lastCreatedRevueDescription;
        

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            //всичко необходимо, за да се логнем в сайта
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("profile.password_manager_enabled", false);
            chromeOptions.AddArgument("--disable-search-engine-choice-screen");



            driver = new ChromeDriver(chromeOptions);
            actions = new Actions(driver);
            driver.Navigate().GoToUrl(BaseUrl);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl($"{BaseUrl}/Users/Login");
            var loginForm = driver.FindElement(By.XPath("//form[@method='post']"));
            actions.ScrollToElement(loginForm).Perform();

            driver.FindElement(By.Id("form3Example3")).SendKeys("dennyDemo@abv.bg");
            driver.FindElement(By.Id("form3Example4")).SendKeys("123456");
            driver.FindElement(By.CssSelector(".btn")).Click();

                        
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            driver.Quit();
            driver.Dispose();

        }



        [Test, Order(1)]
        public void CreateRevueWithInvalidDataTest()
        {
            var invalidTitleInput = "";

            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/Create#createRevue");
           
            driver.FindElement(By.XPath("//input[@id='form3Example1c']")).SendKeys(invalidTitleInput);
           
            var createButton = driver.FindElement(By.XPath("//button[@type='submit']"));
            actions.ScrollToElement(createButton).Perform();
            createButton.Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Revue/Create#createRevue"), "user should remain on the same page with same URL");
           
            var errorMessage = driver.FindElement(By.XPath("//li[contains(.,'Unable to create new Revue!')]"));
            
            Assert.That(errorMessage.Text.Trim(), Is.EqualTo("Unable to create new Revue!"), "The error message is not displayed as expected.");
        }

        [Test, Order(2)]

        public void CreateRandomRevueTest()
        {
            lastCreatedRevueTitle = "Revue N: " + GenerateRandomString(5);
            lastCreatedRevueDescription = "Revue Description: " + GenerateRandomString(10);

            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/Create");

            var createForm = driver.FindElement(By.CssSelector("div.card-body.p-md-5"));
            Actions actions = new Actions(driver);
            actions.MoveToElement(createForm).Perform();

            driver.FindElement(By.Id("form3Example1c")).SendKeys(lastCreatedRevueTitle);
            driver.FindElement(By.Id("form3Example4cd")).SendKeys(lastCreatedRevueDescription);
            driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-lg")).Click();

            string currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Revue/MyRevues"), "The page should redirect to My Revues.");

            var revues = driver.FindElements(By.CssSelector("div.card.mb-4.box-shadow"));
            var lastRevueTitleElement = revues.Last().FindElement(By.CssSelector("div.text-muted.text-center"));

            string actualRevueTitle = lastRevueTitleElement.Text.Trim();
            Assert.That(actualRevueTitle, Is.EqualTo(lastCreatedRevueTitle), "The last created revue title does not match the expected value.");

        }

        [Test, Order(3)]

        public void SearchRevueTitleTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/MyRevues");

            var searchForm = driver.FindElement(By.XPath("//input[@name='keyword']"));
            Actions actions = new Actions(driver);
            actions.MoveToElement(searchForm).Perform();

            var searchInput = driver.FindElement(By.Name("keyword"));
            searchInput.SendKeys(lastCreatedRevueTitle);
            var searchButton = driver.FindElement(By.Id("search-button"));
            searchButton.Click();

            var revueTitleResult = driver.FindElement(By.CssSelector(".text-muted.text-center")).Text;

            // Assert that the revue name matches the searched name
            Assert.That(revueTitleResult, Is.EqualTo(lastCreatedRevueTitle), "The revue title in the search results does not match the searched name.");
        }


        [Test, Order(4)]

        public void EditLastCreatedRevueTitleTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/MyRevues");

            var revues = driver.FindElements(By.CssSelector("div.card.mb-4.box-shadow"));
            Assert.IsTrue(revues.Count > 0, "No revues were found on the page.");

            var lastRevueElement = revues.Last();
            Actions actions = new Actions(driver);
            actions.MoveToElement(lastRevueElement).Perform();

            var editButton = lastRevueElement.FindElement(By.CssSelector("a[href*='/Revue/Edit']"));
            editButton.Click();

            var editForm = driver.FindElement(By.CssSelector("div.card-body.p-md-5"));
            actions.MoveToElement(editForm).Perform();

            var titleInput = driver.FindElement(By.Id("form3Example1c"));
            string newTitle = "Changed Title - " + lastCreatedRevueTitle;
            titleInput.Clear();
            titleInput.SendKeys(newTitle);

            var saveChangesButton = driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-lg"));
            saveChangesButton.Click();

            string currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Revue/MyRevues"), "The page should redirect to My Revues.");

            revues = driver.FindElements(By.CssSelector("div.card.mb-4.box-shadow"));
            var lastRevueTitleElement = revues.Last().FindElement(By.CssSelector("div.text-muted.text-center"));

            string actualRevueTitle = lastRevueTitleElement.Text.Trim();
            Assert.That(actualRevueTitle, Is.EqualTo(newTitle), "The last created revue title does not match the expected value.");
        }


        [Test, Order(5)]

        public void DeleteLastCreatedRevueTitleTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/MyRevues");

            var revues = driver.FindElements(By.CssSelector("div.card.mb-4.box-shadow"));
            Assert.IsTrue(revues.Count > 0, "No revues were found on the page.");

            var lastRevueElement = revues.Last();
            Actions actions = new Actions(driver);
            actions.MoveToElement(lastRevueElement).Perform();

            var deleteButton = lastRevueElement.FindElement(By.XPath("(//a[contains(.,'Delete')])[9]"));
            deleteButton.Click();

            string currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Revue/MyRevues"), "The page should redirect to My Revues.");

            revues = driver.FindElements(By.CssSelector("div.card.mb-4.box-shadow"));
            var lastRevueTitleElement = revues.Last().FindElement(By.CssSelector("div.text-muted.text-center"));

            string actualRevueTitle = lastRevueTitleElement.Text.Trim();
            Assert.That(actualRevueTitle, Is.Not.EqualTo(lastCreatedRevueTitle), "The last created revue title does not match the expected value.");
        }


        [Test, Order(6)]
        public void SearchForDeletedRevueTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Revue/MyRevues");
            var searchField = driver.FindElement(By.CssSelector(".input-group.mb-xl-5"));
            Actions actions = new Actions(driver);
            actions.MoveToElement(searchField).Perform();

            var searchInput = driver.FindElement(By.Name("keyword"));
            searchInput.SendKeys("non-existing-revue");
            var searchButton = driver.FindElement(By.Id("search-button"));
            searchButton.Click();

            // Assert that the message "No Revues yet!" is displayed
            var noRevuesMessage = driver.FindElement(By.CssSelector(".text-muted"));
            Assert.That(noRevuesMessage.Text.Trim(), Is.EqualTo("No Revues yet!"), "The 'No Revues yet!' message is not displayed as expected.");
        }


        private string GenerateRandomString(int length)
        {
            // Generate a random string of specified length
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
