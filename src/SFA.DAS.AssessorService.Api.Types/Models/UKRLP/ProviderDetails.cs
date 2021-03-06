﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.AssessorService.Api.Types.Models.UKRLP
{

    public class ProviderDetails
    {
        public string UKPRN { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public List<ProviderContact> ContactDetails { get; set; }
        public DateTime? VerificationDate { get; set; }
        public List<ProviderAlias> ProviderAliases { get; set; }
        public List<VerificationDetails> VerificationDetails { get; set; }
    }

    public class ProviderContact
    {
        public string ContactType { get; set; }
        public ContactAddress ContactAddress { get; set; }
        public ContactPersonalDetails ContactPersonalDetails { get; set; }
        public string ContactRole { get; set; }
        public string ContactTelephone1 { get; set; }
        public string ContactTelephone2 { get; set; }
        public string ContactWebsiteAddress { get; set; }
        public string ContactEmail { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class ProviderAlias
    {
        public string Alias { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class ContactAddress
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Town { get; set; }
        public string PostCode { get; set; }
    }

    public class ContactPersonalDetails
    {
        public string PersonNameTitle { get; set; }
        public string PersonGivenName { get; set; }
        public string PersonFamilyName { get; set; }
        public string PersonNameSuffix { get; set; }
    }

    public class VerificationDetails
    {
        public string VerificationAuthority { get; set; }
        public string VerificationId { get; set; }
    }
}