﻿using AutoMapper;

namespace SFA.DAS.AssessorService.Application.Api.External.AutoMapperProfiles
{
    public class EpaRecordProfile : Profile
    {
        public EpaRecordProfile()
        {
            // Request going to Int API
            CreateMap<Models.Request.Epa.EpaRecord, Domain.JsonData.EpaRecord>()
                .ForMember(dest => dest.EpaDate, opt => opt.MapFrom(source => source.EpaDate))
                .ForMember(dest => dest.EpaOutcome, opt => opt.MapFrom(source => source.EpaOutcome))
                .ForMember(dest => dest.Resit, opt => opt.MapFrom(source => source.Resit))
                .ForMember(dest => dest.Retake, opt => opt.MapFrom(source => source.Retake))
                .ForAllOtherMembers(dest => dest.Ignore());

            // Response from Int API
            CreateMap<Domain.JsonData.EpaRecord, Models.Response.Epa.EpaRecord>()
                .ForMember(dest => dest.EpaDate, opt => opt.MapFrom(source => source.EpaDate))
                .ForMember(dest => dest.EpaOutcome, opt => opt.MapFrom(source => source.EpaOutcome))
                .ForMember(dest => dest.Resit, opt => opt.MapFrom(source => source.Resit))
                .ForMember(dest => dest.Retake, opt => opt.MapFrom(source => source.Retake))
                .ForAllOtherMembers(dest => dest.Ignore());
        }
    }
}
