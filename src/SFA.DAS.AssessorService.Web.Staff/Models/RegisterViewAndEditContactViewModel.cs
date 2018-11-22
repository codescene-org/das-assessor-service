﻿namespace SFA.DAS.AssessorService.Web.Staff.Models
{
    public class RegisterViewAndEditContactViewModel
    {
        public string ContactId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationId { get; set; }
        public string ContactDetails { get; set; }
        public bool IsPrimaryContact { get; set; }
    }
}
