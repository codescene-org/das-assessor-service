﻿using System;
using System.Threading;
using OpenQA.Selenium;
using SFA.DAS.AssessorService.Web.Specflow.Tests.Framework.Helpers;
using SFA.DAS.AssessorService.Web.Specflow.Tests.TestSupport;

namespace SFA.DAS.AssessorService.Web.Specflow.Tests.Pages
{
    public class WhereWillTheCertificateBeSentPage : BasePage
    {
        private static String PAGE_TITLE = "Search for the employer's address";

        public WhereWillTheCertificateBeSentPage(IWebDriver webDriver) : base(webDriver)
        {
            SelfVerify();
        }

        protected override bool SelfVerify()
        {
            return PageInteractionHelper.VerifyPageHeading(this.GetPageHeading(), PAGE_TITLE);
        }

        private readonly By _employerBy = By.Name("Employer");        

        private readonly By _enterAddressManuallyBy = By.Id("enterAddressManually");

        private readonly By _addressLine1By = By.Name("AddressLine1");
        private readonly By _addressLine2By = By.Name("AddressLine2");
        private readonly By _addressLine3By = By.Name("AddressLine3");
        private readonly By _cityBy = By.Name("City");
        private readonly By _addresspostCode = By.Name("Postcode");

        private readonly By _continueButton = By.XPath("//*[@id=\"content\"]/div/div/form/button");

        internal void EnterDetails()
        {          
            FormCompletionHelper.ClickElement(_enterAddressManuallyBy);
            Thread.Sleep(3000);

            FormCompletionHelper.EnterText(_employerBy, "Jongo Ltd");          
          
            FormCompletionHelper.EnterText(_addressLine1By, "1 Anchor Drive");
            FormCompletionHelper.EnterText(_addressLine2By, "Enfield");
            FormCompletionHelper.EnterText(_addressLine3By, "Westminster");
            FormCompletionHelper.EnterText(_cityBy, "London");

            FormCompletionHelper.EnterText(_addresspostCode, "B50 3DE");
        }

        internal void ClickContinue()
        {
            FormCompletionHelper.ClickElement(_continueButton);
        }
    }
}