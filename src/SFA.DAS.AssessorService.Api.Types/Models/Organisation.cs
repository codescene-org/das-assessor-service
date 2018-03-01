﻿namespace SFA.DAS.AssessorService.Api.Types.Models
{
    using System;
    using Domain.Enums;

    public class Organisation
    {
        public Guid Id { get; set; }

        public string EndPointAssessorOrganisationId { get; set; }
        public int EndPointAssessorUkprn { get; set; }
        public string EndPointAssessorName { get; set; }
        public string PrimaryContact { get; set; }

        public OrganisationStatus OrganisationStatus { get; set; }
    }
}
