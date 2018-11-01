﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity.UI.Pages.Internal.Account;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Api.Types.Models.Register;
using SFA.DAS.AssessorService.Api.Types.Models.Validation;
using SFA.DAS.AssessorService.Application.Api.Consts;
using SFA.DAS.AssessorService.Application.Exceptions;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.ExternalApis.AssessmentOrgs;
using StructureMap.Diagnostics;
using Swashbuckle.AspNetCore.Swagger;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SFA.DAS.AssessorService.Application.Api.Validators
{
    public class EpaOrganisationValidator: IEpaOrganisationValidator
    {
        private readonly IRegisterValidationRepository _registerRepository;
        private readonly IRegisterQueryRepository _registerQueryRepository;
        private readonly IStringLocalizer<EpaOrganisationValidator> _localizer;
        private readonly ISpecialCharacterCleanserService _cleanserService;
       
        public EpaOrganisationValidator( IRegisterValidationRepository registerRepository,  IRegisterQueryRepository registerQueryRepository, 
                                         ISpecialCharacterCleanserService cleanserService, IStringLocalizer<EpaOrganisationValidator> localizer) 
        {
            _registerRepository = registerRepository;
            _registerQueryRepository = registerQueryRepository;
            _cleanserService = cleanserService;
            _localizer = localizer;
        }
        
        public string CheckOrganisationIdIsPresentAndValid(string organisationId)
        {
            if (string.IsNullOrEmpty(organisationId) || organisationId.Trim().Length==0)
            {
               return FormatErrorMessage(EpaOrganisationValidatorMessageName.NoOrganisationId);
            }

            return organisationId.Trim().Length > 12 ? 
                FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationIdTooLong) 
                : string.Empty;
        }

        public string CheckOrganisationName(string name)
        {
            var organisationName = _cleanserService.CleanseStringForSpecialCharacters(name);
            if (string.IsNullOrEmpty(organisationName) || organisationName.Trim().Length==0)
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationNameEmpty);
            
            return organisationName.Trim().Length < 2 
                ? FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationNameTooShort) 
                : string.Empty;
        }

        public string CheckIfOrganisationAlreadyExists(string organisationId)
        {
            if (organisationId == null ||
                !_registerRepository.EpaOrganisationExistsWithOrganisationId(organisationId).Result) return string.Empty;
            return FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationIdAlreadyUsed);
        }

        public string CheckIfOrganisationUkprnExists(long? ukprn)
        {
            if (ukprn == null || !_registerRepository.EpaOrganisationExistsWithUkprn(ukprn.Value).Result) return string.Empty;
            return FormatErrorMessage(EpaOrganisationValidatorMessageName.UkprnAlreadyUsed);
        }

        public string CheckOrganisationTypeIsNullOrExists(int? organisationTypeId)
        {
            if (organisationTypeId == null || _registerRepository.OrganisationTypeExists(organisationTypeId.Value).Result) return string.Empty;
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationTypeIsInvalid);
        }

        public string CheckUkprnIsValid(long? ukprn)
        {
            if (ukprn == null) return string.Empty;
            var isValid = ukprn >= 10000000 && ukprn <= 99999999;
            return isValid ? string.Empty : FormatErrorMessage(EpaOrganisationValidatorMessageName.UkprnIsInvalid);
        }

        public string CheckOrganisationNameNotUsed(string name)
        {
            return _registerRepository.EpaOrganisationAlreadyUsingName(name, string.Empty).Result 
                ? FormatErrorMessage(EpaOrganisationValidatorMessageName.ErrorMessageOrganisationNameAlreadyPresent) : 
                string.Empty;
        }

        public string CheckOrganisationNameNotUsedForOtherOrganisations(string name, string organisationIdToIgnore)
        {
            return _registerRepository.EpaOrganisationAlreadyUsingName(name, organisationIdToIgnore).Result 
                ? FormatErrorMessage(EpaOrganisationValidatorMessageName.ErrorMessageOrganisationNameAlreadyPresent) : 
                string.Empty;
        }

        public string CheckIfDeliveryAreasAreValid(List<int> deliveryAreas)
        {
            if (deliveryAreas == null || deliveryAreas.Count == 0)
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.NoDeliveryAreasPresent);

            var validDeliveryAreas = _registerQueryRepository.GetDeliveryAreas().Result;

            foreach (var deliveryArea in deliveryAreas)
            {
                if (!validDeliveryAreas.Any(x => x.Id == deliveryArea))
                    return FormatErrorMessage(EpaOrganisationValidatorMessageName.DeliveryAreaNotValid);               
            }

            return string.Empty;
        }

        public string CheckSearchStringForStandardsIsValid(string searchstring)
        {
            if (searchstring == null) searchstring = string.Empty;       
            var isAnInt = int.TryParse(searchstring, out _);
            if (!isAnInt && searchstring.Length < 2)
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.SearchStandardsTooShort);

            return string.Empty;
        }

        public string CheckIfOrganisationUkprnExistsForOtherOrganisations(long? ukprn, string organisationIdToIgnore)
        {
        if (ukprn == null || !_registerRepository.EpaOrganisationAlreadyUsingUkprn(ukprn.Value, organisationIdToIgnore).Result) return string.Empty;
            return FormatErrorMessage(EpaOrganisationValidatorMessageName.UkprnAlreadyUsed);
        }

        public string CheckIfOrganisationNotFound(string organisationId)
        {
            return organisationId != null && _registerRepository.EpaOrganisationExistsWithOrganisationId(organisationId).Result 
                ? string.Empty :
                FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationNotFound);
        }

        public Standard GetStandard(int standardCode)
        {
            var apiClient = new AssessmentOrgsApiClient();

            try
            {
                var res = apiClient.GetStandard(standardCode).Result;
                return res;
            }
            catch
            {
                return (Standard)null;
            }
        }

        public string CheckIfOrganisationStandardAlreadyExists(string organisationId, int standardCode)
        {
            return _registerRepository.EpaOrganisationStandardExists(organisationId, standardCode).Result
                ? FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationStandardAlreadyExists)
                : string.Empty;
        }

        public string CheckIfOrganisationStandardDoesNotExist(string organisationId, int standardCode)
        {
            return _registerRepository.EpaOrganisationStandardExists(organisationId, standardCode).Result
                ? string.Empty
                : FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationStandardDoesNotExist);
        }
        
        public string CheckIfContactIdIsValid(string contactId, string organisationId)
        {
            if (!Guid.TryParse(contactId, out Guid newContactId))
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.ContactIdIsRequired);

            return _registerRepository.ContactIdIsValidForOrganisationId(newContactId, organisationId).Result
                ? string.Empty 
                : FormatErrorMessage(EpaOrganisationValidatorMessageName.ContactIdInvalidForOrganisationId);
        }

        public string CheckDisplayName(string name)
        {
            var newName = _cleanserService.CleanseStringForSpecialCharacters(name);
            if (string.IsNullOrEmpty(newName) || newName.Trim().Length == 0)
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.DisplayNameIsMissing);

            return newName.Trim().Length < 2
                ? FormatErrorMessage(EpaOrganisationValidatorMessageName.DisplayNameTooShort)
                : string.Empty;
        }

        public string CheckIfEmailIsMissing(string emailName)
        {
            return string.IsNullOrEmpty(emailName?.Trim())
                ? FormatErrorMessage(EpaOrganisationValidatorMessageName.EmailIsMissing)
                : string.Empty;
        }


        public string CheckContactIdExists(string contactId)
        {

            if (!Guid.TryParse(contactId, out Guid newContactId))
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.ContactIdDoesntExist);
            
            return _registerRepository.ContactExists(newContactId).Result
                ? string.Empty
                : FormatErrorMessage(EpaOrganisationValidatorMessageName.ContactIdDoesntExist);
        }

        public string CheckIfEmailAlreadyPresentInAnotherOrganisation(string email, string organisationId)
        {
            return _registerRepository.EmailAlreadyPresentInAnotherOrganisation(email, organisationId).Result
                ? FormatErrorMessage(EpaOrganisationValidatorMessageName.EmailAlreadyPresentInAnotherOrganisation)
                : string.Empty;
        }


        public string CheckIfEmailAlreadyPresentInOrganisationNotAssociatedWithContact(string email, string contactId)
        {
            if (!Guid.TryParse(contactId, out Guid newContactId))
                return string.Empty;

            return _registerRepository
                .EmailAlreadyPresentInAnOrganisationNotAssociatedWithContact(email, newContactId).Result
                ? FormatErrorMessage(EpaOrganisationValidatorMessageName.EmailAlreadyPresentInAnotherOrganisation)
                : string.Empty;
        }

        public string CheckIfEmailIsPresentAndInSuitableFormat(string email)
        {
            var validationResults = new EmailValidator().Validate(new EmailChecker {EmailToCheck = email});
            return validationResults.IsValid ? string.Empty : FormatErrorMessage(validationResults.Errors.First().ErrorMessage);
        }


        public string CheckOrganisationStandardFromDateIsWithinStandardDateRanges(DateTime? effectiveFrom, DateTime? standardEffectiveFrom,
            DateTime? standardEffectiveTo, DateTime? lastDateForNewStarts)
        {
            if (effectiveFrom == null || standardEffectiveFrom == null)
                return string.Empty;

            if (effectiveFrom < standardEffectiveFrom)
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationStandardEffectiveFromBeforeStandardEffectiveFrom);


            if (standardEffectiveTo.HasValue && effectiveFrom > standardEffectiveTo)
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationStandardEffectiveFromAfterStandardEffectiveTo);


            if (lastDateForNewStarts.HasValue && effectiveFrom > lastDateForNewStarts)
                return
                    FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationStandardEffectiveFromAfterStandardLastDayForNewStarts);

            return string.Empty;
        }


        public string CheckOrganisationStandardToDateIsWithinStandardDateRanges(DateTime? effectiveTo, DateTime? standardEffectiveFrom, DateTime? standardEffectiveTo)
        {
            if (effectiveTo == null)
                return string.Empty;

            if (effectiveTo < standardEffectiveFrom)
                return FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationStandardEffectiveToBeforeStandardEffectiveFrom);

            if (standardEffectiveTo.HasValue && effectiveTo > standardEffectiveTo)
               return FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationStandardEffectiveToAfterStandardEffectiveTo);

            return string.Empty;
        }


        public string CheckEffectiveFromIsOnOrBeforeEffectiveTo(DateTime? effectiveFrom, DateTime? effectiveTo)
        {
            if (!effectiveFrom.HasValue || !effectiveTo.HasValue || effectiveFrom.Value <= effectiveTo.Value) return string.Empty;

            return FormatErrorMessage(EpaOrganisationValidatorMessageName.OrganisationStandardEffectiveFromAfterEffectiveTo);

        }
        public ValidationResponse ValidatorCreateEpaOrganisationRequest(CreateEpaOrganisationRequest request)
        {
            var validationResult = new ValidationResponse();

            RunValidationCheckAndAppendAnyError("Name", CheckOrganisationName(request.Name), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("OrganisationTypeId", CheckOrganisationTypeIsNullOrExists(request.OrganisationTypeId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("Ukprn", CheckUkprnIsValid(request.Ukprn), validationResult, ValidationStatusCode.BadRequest);    
            RunValidationCheckAndAppendAnyError("Name", CheckOrganisationNameNotUsed(request.Name), validationResult, ValidationStatusCode.AlreadyExists);
            RunValidationCheckAndAppendAnyError("Ukprn", CheckIfOrganisationUkprnExists(request.Ukprn), validationResult, ValidationStatusCode.AlreadyExists);

            return validationResult;
        }

        public ValidationResponse ValidatorCreateEpaOrganisationContactRequest(CreateEpaOrganisationContactRequest request)
        {
            var validationResult = new ValidationResponse();
            RunValidationCheckAndAppendAnyError("EndPointAssessorOrganisationId", CheckIfOrganisationNotFound(request.EndPointAssessorOrganisationId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("DisplayName", CheckDisplayName(request.DisplayName), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("Email", CheckIfEmailIsPresentAndInSuitableFormat(request.Email), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("Email", CheckIfEmailAlreadyPresentInAnotherOrganisation(request.Email, request.EndPointAssessorOrganisationId), validationResult, ValidationStatusCode.AlreadyExists);
            return validationResult;
        }

        public ValidationResponse ValidatorUpdateEpaOrganisationContactRequest(UpdateEpaOrganisationContactRequest request)
        {
            var validationResult = new ValidationResponse();

            RunValidationCheckAndAppendAnyError("ContactId", CheckContactIdExists(request.ContactId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("DisplayName", CheckDisplayName(request.DisplayName), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("Email", CheckIfEmailIsPresentAndInSuitableFormat(request.Email), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("Email", CheckIfEmailAlreadyPresentInOrganisationNotAssociatedWithContact(request.Email, request.ContactId), validationResult, ValidationStatusCode.AlreadyExists);
            return validationResult;
        }

        public ValidationResponse ValidatorCreateEpaOrganisationStandardRequest(
            CreateEpaOrganisationStandardRequest request)
        {
            var validationResult = new ValidationResponse();

            RunValidationCheckAndAppendAnyError("OrganisationId", CheckIfOrganisationNotFound(request.OrganisationId), validationResult, ValidationStatusCode.NotFound);
            if (!validationResult.IsValid) return validationResult;



            RunValidationCheckAndAppendAnyError("OrganisationId", CheckIfOrganisationStandardAlreadyExists(request.OrganisationId, request.StandardCode), validationResult, ValidationStatusCode.AlreadyExists);
            if (!validationResult.IsValid) return validationResult;

            var standard = GetStandard(request.StandardCode);
            if (standard is null)
            {
                var standardErrorMessage = FormatErrorMessage(EpaOrganisationValidatorMessageName.StandardNotFound);
                RunValidationCheckAndAppendAnyError("StandardCode", standardErrorMessage, validationResult, ValidationStatusCode.NotFound);
                return validationResult;
            }

            RunValidationCheckAndAppendAnyError("OrganisationId", CheckOrganisationIdIsPresentAndValid(request.OrganisationId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("ContactId", CheckIfContactIdIsValid(request.ContactId, request.OrganisationId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("DeliveryAreas", CheckIfDeliveryAreasAreValid(request.DeliveryAreas), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("EffectiveFrom", CheckOrganisationStandardFromDateIsWithinStandardDateRanges(request.EffectiveFrom, standard.EffectiveFrom, standard.EffectiveTo, standard.LastDateForNewStarts), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("EffectiveFrom", CheckEffectiveFromIsOnOrBeforeEffectiveTo(request.EffectiveFrom, request.EffectiveTo), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("EffectiveTo", CheckOrganisationStandardToDateIsWithinStandardDateRanges(request.EffectiveTo, standard.EffectiveFrom, standard.EffectiveTo), validationResult, ValidationStatusCode.BadRequest);

            return validationResult;
        }

        public ValidationResponse ValidatorUpdateEpaOrganisationStandardRequest(UpdateEpaOrganisationStandardRequest request)
        {
            var validationResult = new ValidationResponse();
            var standard = GetStandard(request.StandardCode);
            if (standard is null)
            {
                var standardErrorMessage = FormatErrorMessage(EpaOrganisationValidatorMessageName.StandardNotFound);
                RunValidationCheckAndAppendAnyError("StandardCode", standardErrorMessage, validationResult, ValidationStatusCode.NotFound);
                return validationResult;
            }

            RunValidationCheckAndAppendAnyError("OrganisationId", CheckIfOrganisationStandardDoesNotExist(request.OrganisationId, request.StandardCode), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("ContactId", CheckIfContactIdIsValid(request.ContactId,request.OrganisationId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("DeliveryAreas", CheckIfDeliveryAreasAreValid(request.DeliveryAreas), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("EffectiveFrom", CheckOrganisationStandardFromDateIsWithinStandardDateRanges(request.EffectiveFrom, standard.EffectiveFrom, standard.EffectiveTo, standard.LastDateForNewStarts), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("EffectiveFrom", CheckEffectiveFromIsOnOrBeforeEffectiveTo(request.EffectiveFrom, request.EffectiveTo), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("EffectiveTo", CheckOrganisationStandardToDateIsWithinStandardDateRanges(request.EffectiveTo, standard.EffectiveFrom, standard.EffectiveTo), validationResult, ValidationStatusCode.BadRequest);


            return validationResult;
        }

        private string FormatErrorMessage(string messageName)
        {
            return $"{_localizer[messageName].Value}; ";
        }

        private void RunValidationCheckAndAppendAnyError(string fieldName, string errorMessage, ValidationResponse validationResult, ValidationStatusCode statusCode)
        {
            if (errorMessage != string.Empty)
                validationResult.Errors.Add(new ValidationErrorDetail(fieldName, errorMessage.Replace("; ", ""), statusCode));
        }

        public ValidationResponse ValidatorUpdateEpaOrganisationRequest(UpdateEpaOrganisationRequest request)
        {
            var validationResult = new ValidationResponse();
            RunValidationCheckAndAppendAnyError("OrganisationId", CheckIfOrganisationNotFound(request.OrganisationId), validationResult, ValidationStatusCode.NotFound);
            if (!validationResult.IsValid)
                return validationResult;

            RunValidationCheckAndAppendAnyError("OrganisationId", CheckOrganisationIdIsPresentAndValid(request.OrganisationId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("Name", CheckOrganisationName(request.Name), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("OrganisationTypeId", CheckOrganisationTypeIsNullOrExists(request.OrganisationTypeId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("Ukprn", CheckIfOrganisationUkprnExistsForOtherOrganisations(request.Ukprn, request.OrganisationId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("Name", CheckOrganisationNameNotUsedForOtherOrganisations(request.Name, request.OrganisationId), validationResult, ValidationStatusCode.BadRequest);
            RunValidationCheckAndAppendAnyError("Ukprn", CheckUkprnIsValid(request.Ukprn), validationResult, ValidationStatusCode.BadRequest);

            return validationResult;
        }

        public ValidationResponse ValidatorSearchStandardsRequest(SearchStandardsValidationRequest request)
        {
            var validationResult = new ValidationResponse();
            RunValidationCheckAndAppendAnyError("StandardSearchString", CheckSearchStringForStandardsIsValid(request.Searchstring), validationResult, ValidationStatusCode.BadRequest);
            return validationResult;

        }  
    }
}
