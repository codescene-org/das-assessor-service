﻿using System;

namespace SFA.DAS.AssessorService.Api.Types.Models.Standards
{
    public class StandardData
    {
        public int? Level { get; set; }
        public string Category { get; set; }
        public string IfaStatus { get; set; }

        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime? LastDateForNewStarts { get; set; }
        public bool IfaOnly { get; set; }
        public string EqaProviderName { get; set; }
        public string EqaProviderContactName { get; set; }
        public string EqaProviderContactAddress { get; set; }
        public string EqaProviderContactEmail { get; set; }
        public string EqaProviderWebLink { get; set; }
        public string IntegratedDegree { get; set; }
        public int? Duration { get; set; }
        public int? MaxFunding { get; set; }
        public string Trailblazer { get; set; }
        public string Ssa1 { get; set; }
        public string Ssa2 { get; set; }
        public string OverviewOfRole { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool? IsPublished { get; set; }

        public bool? IsActiveStandardInWin { get; set; }

        public string FatUri { get; set; }
        public string IfaUri { get; set; }

        public string AssessmentPlanUrl { get; set; }
        public string StandardPageUrl { get; set; }
    }
}
