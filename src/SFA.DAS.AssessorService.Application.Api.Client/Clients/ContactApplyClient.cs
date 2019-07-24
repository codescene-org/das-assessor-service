﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.ApplyTypes;


namespace SFA.DAS.AssessorService.Application.Api.Client.Clients
{
   
    public class ContactApplyClient : ApiClientBase, IContactApplyClient
    {
        public ContactApplyClient(string baseUri, ITokenService applyTokenService,
            ILogger<ContactApplyClient> logger) : base(baseUri, applyTokenService, logger)
        {
        }

        public ContactApplyClient(HttpClient httpClient, ITokenService applyTokenService, ILogger<ApiClientBase> logger) : base(httpClient, applyTokenService, logger)
        {
        }

        public async Task CreateAccountInApply(NewApplyContact contact)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post,
                $"/Account"))
            {
                 await PostPutRequest(request,contact);
            }

        }

        public async Task UpdateApplySignInId(AddToApplyContactASignInId addToApplyContactASignInId)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put,
                $"/Account"))
            {
                await PostPutRequest(request, addToApplyContactASignInId);
            }
        }

        public async Task<Contact> GetApplyContactBySignInId(Guid signinId)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get,
                    $"/Account/{signinId}"))
                {
                    return await RequestAndDeserialiseAsync<Contact>(request, "Failed to determine user exists");
                }
            }catch(HttpRequestException err)
            {
                if (err.Message.Contains("204"))
                       return null;
                throw err;
            }
        }

        public async Task RemoveContactFromOrganisation(Guid contactId)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post,$"/Account/RemoveFromOrganisation"))
            {
                await PostPutRequest(request, new {contactId});
            }
        }
        
        
        public async Task CreateNewContact(SFA.DAS.AssessorService.Domain.Entities.Contact newContact)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post,$"/Account/CreateNewContact"))
            {
                await PostPutRequest(request, newContact);
            }
        }

        public async Task SignInIdCallback(SignInCallback callback)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post,$"/Account/Callback"))
            {
                await PostPutRequest(request, callback);
            }
        }
    }
}


public interface IContactApplyClient
{
    //Task CreateAccountInApply(NewApplyContact contact);
   // Task UpdateApplySignInId(AddToApplyContactASignInId addToApplyContactASignInId);
   // Task<Contact> GetApplyContactBySignInId(Guid signinId);
   // Task RemoveContactFromOrganisation(Guid contactId);
    //Task CreateNewContact(SFA.DAS.AssessorService.Domain.Entities.Contact newContact);
   // Task SignInIdCallback(SignInCallback callback);
}