﻿using AutoMapper;
using SFA.DAS.AssessorService.Api.Types.AutoMapperProfiles;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Api.Types.Models.Certificates;
using SFA.DAS.AssessorService.Api.Types.Models.Register;
using SFA.DAS.AssessorService.Application.Api.AutoMapperProfiles;
using SFA.DAS.AssessorService.Application.Mapping.CustomResolvers;
using SFA.DAS.AssessorService.Domain.Entities;
using Contact = SFA.DAS.AssessorService.Domain.Entities.Contact;
using Organisation = SFA.DAS.AssessorService.Domain.Entities.Organisation;

namespace SFA.DAS.AssessorService.Application.Api.StartupConfiguration
{
    public static class MappingStartup
    {
        public static void AddMappings()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Organisation, OrganisationResponse>();
                cfg.CreateMap<CreateOrganisationRequest, Organisation>();
                cfg.CreateMap<UpdateOrganisationRequest, Organisation>();

                cfg.CreateMap<CreateContactRequest, Contact>().ReverseMap();
                cfg.CreateMap<Contact, ContactResponse>();
                cfg.CreateMap<Ilr, SearchResult>();
                cfg.CreateMap<Ilr, StaffSearchItems>()
                    .ForMember(q => q.StandardCode, opts => {opts.MapFrom(i => i.StdCode);});
                cfg.CreateMap<CreateBatchLogRequest, BatchLog>();
                cfg.CreateMap<BatchLog, BatchLogResponse>();               

                cfg.CreateMap<Certificate, CertificateResponse>()
                    .ForMember(q => q.EndPointAssessorOrganisationId,
                        opts => { opts.MapFrom(q => q.Organisation.EndPointAssessorOrganisationId); })
                    .ForMember(q => q.EndPointAssessorOrganisationName,
                        opts => { opts.MapFrom(q => q.Organisation.EndPointAssessorName); })
                    .ForMember(q => q.BatchNumber,
                        opts => { opts.ResolveUsing<BatchNumberResolver>(); });

                cfg.CreateMap<string, CertificateDataResponse>()
                    .ConvertUsing<JsonMappingConverter<CertificateDataResponse>>();

                cfg.CreateMap<Certificate, CertificateSummaryResponse>();

                cfg.AddProfile<EpaOrganisationProfile>();
                cfg.AddProfile<OppFinderProfile>();

                cfg.CreateMap<CreateEpaOrganisationRequest, EpaOrganisationResponse>(); 
                cfg.CreateMap<UpdateEpaOrganisationRequest, EpaOrganisationResponse>(); 
                cfg.CreateMap<CreateEpaOrganisationStandardRequest, EpaoStandardResponse>();
                cfg.CreateMap<UpdateEpaOrganisationStandardRequest, EpaoStandardResponse>();

                cfg.AddProfile<AssessorServiceOrganisationProfile>();
                cfg.AddProfile<AssessorServiceOrganisationAddressProfile>();
                cfg.AddProfile<AssessorServiceOrganisationTypeProfile>();
                cfg.AddProfile<AssessorServiceOrganisationResponse>();

                cfg.AddProfile<ProviderRegisterOrganisationProfile>();
                cfg.AddProfile<ProviderRegisterOrganisationAddressProfile>();

                cfg.AddProfile<ReferenceDataOrganisationProfile>();
                cfg.AddProfile<ReferenceDataOrganisationAddressProfile>();

                cfg.AddProfile<RoatpOrganisationProfile>();
                cfg.AddProfile<UkrlpOrganisationProfile>();
                cfg.AddProfile<UkrlpOrganisationAddressProfile>();

                cfg.AddProfile<CompaniesHouseCompanyProfile>();
                cfg.AddProfile<CompaniesHouseAccountsProfile>();
                cfg.AddProfile<CompaniesHouseRegisteredOfficeAddressProfile>();
                cfg.AddProfile<CompaniesHouseOfficerAddressProfile>();
                cfg.AddProfile<CompaniesHouseOfficerProfile>();
                cfg.AddProfile<CompaniesHouseOfficerDisqualificationProfile>();
                cfg.AddProfile<CompaniesHousePersonWithSignificantControlProfile>();
                cfg.AddProfile<CompaniesHousePersonWithSignificantControlAddressProfile>();

                cfg.AddProfile<CharityCommissionProfile>();
                cfg.AddProfile<CharityCommissionAddressProfile>();
                cfg.AddProfile<CharityCommissionAccountsProfile>();
                cfg.AddProfile<CharityCommissionTrusteeProfile>();
            });
        }
    }
}