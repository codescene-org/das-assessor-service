using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Api.Types.Models.UserManagement;
using SFA.DAS.AssessorService.Application.Interfaces;
using SFA.DAS.AssessorService.Domain.Entities;

namespace SFA.DAS.AssessorService.Application.Handlers.UserManagement
{
    public class SetContactPrivilegesHandler : IRequestHandler<SetContactPrivilegesRequest, SetContactPrivilegesResponse>
    {
        private readonly IContactRepository _contactRepository;
        private readonly IContactQueryRepository _contactQueryRepository;
        private readonly IMediator _mediator;

        public SetContactPrivilegesHandler(IContactRepository contactRepository, IContactQueryRepository contactQueryRepository, IMediator mediator)
        {
            _contactRepository = contactRepository;
            _contactQueryRepository = contactQueryRepository;
            _mediator = mediator;
        }
        
        public async Task<SetContactPrivilegesResponse> Handle(SetContactPrivilegesRequest request, CancellationToken cancellationToken)
        {
            var currentPrivileges = (await _contactQueryRepository.GetPrivilegesFor(request.ContactId));
            var privilegesBeingRemoved = GetRemovedPrivileges(request, currentPrivileges);
            var privilegesBeingAdded = await GetAddedPrivileges(request, currentPrivileges);
            
            var privilegesBeingRemovedThatMustBelongToSomeone = privilegesBeingRemoved.Where(p => p.Privilege.MustBeAtLeastOneUserAssigned).ToList();

            foreach (var currentPrivilege in privilegesBeingRemovedThatMustBelongToSomeone)
            {
                if (await _contactRepository.IsOnlyContactWithPrivilege(request.ContactId, currentPrivilege.PrivilegeId))
                {
                    return new SetContactPrivilegesResponse()
                    {
                        Success = false, 
                        ErrorMessage = $"Before you remove '{currentPrivilege.Privilege.UserPrivilege}' you must assign '{currentPrivilege.Privilege.UserPrivilege}' to another user."
                    };
                }
            }
            
            await UpdatePrivileges(request);

            await SendEmail(privilegesBeingAdded, privilegesBeingRemoved, request);

            await LogChangesToPrivileges(privilegesBeingAdded, privilegesBeingRemoved, request);
            
            return new SetContactPrivilegesResponse() {Success = true};
        }

        private async Task LogChangesToPrivileges(List<ContactsPrivilege> privilegesBeingAdded, List<ContactsPrivilege> privilegesBeingRemoved, SetContactPrivilegesRequest request)
        {          
            await _contactRepository.CreateContactLog(request.AmendingContactId, request.ContactId, ContactLogType.PrivilegesAmended, 
                new
                {
                    PrivilegesAdded=privilegesBeingAdded.Select(p => p.Privilege.UserPrivilege),
                    PrivilegesRemoved = privilegesBeingRemoved.Select(p => p.Privilege.UserPrivilege)
                });
        }

        private async Task SendEmail(List<ContactsPrivilege> privilegesBeingAdded, List<ContactsPrivilege> privilegesBeingRemoved, SetContactPrivilegesRequest request)
        {
            if (privilegesBeingAdded.Any() || privilegesBeingRemoved.Any())
            {
                var removedText = GetRemovedPrivilegesEmailToken(privilegesBeingRemoved);
                var addedText = GetAddedPrivilegesEmailToken(privilegesBeingAdded);

                var emailTemplate = await _mediator.Send(new GetEmailTemplateRequest
                    {TemplateName= "EPAOPermissionsAmended" });

                var amendingContact = await _contactQueryRepository.GetContactById(request.AmendingContactId);
                var contactBeingAmended = await _contactQueryRepository.GetContactById(request.ContactId);
            
                await _mediator.Send(new SendEmailRequest(contactBeingAmended.Email, emailTemplate, new
                {
                    ServiceName = "Apprenticeship assessment service",
                    Contact = contactBeingAmended.DisplayName,
                    Editor = amendingContact.DisplayName,
                    ServiceTeam = "Apprenticeship assessment service team",
                    PermissionsAdded = addedText,
                    PermissionsRemoved = removedText
                }));
            }
        }

        private string GetAddedPrivilegesEmailToken(List<ContactsPrivilege> privilegesBeingAdded)
        {
            var addedText = "";
            if (privilegesBeingAdded.Any())
            {
                addedText = @"The following permissions have been added:" + Environment.NewLine;
                foreach (var added in privilegesBeingAdded)
                {
                    addedText += " • " + added.Privilege.UserPrivilege + Environment.NewLine;
                }
            }

            return addedText;
        }

        private string GetRemovedPrivilegesEmailToken(List<ContactsPrivilege> privilegesBeingRemoved)
        {
            var removedText = "";
            if (privilegesBeingRemoved.Any())
            {
                removedText = @"The following permissions have been removed:" + Environment.NewLine;
                foreach (var added in privilegesBeingRemoved)
                {
                    removedText += " • " + added.Privilege.UserPrivilege + Environment.NewLine;
                }
            }

            return removedText;
        }

        private async Task UpdatePrivileges(SetContactPrivilegesRequest request)
        {
            await _contactRepository.RemoveAllPrivileges(request.ContactId);

            foreach (var privilegeId in request.PrivilegeIds)
            {
                await _contactRepository.AddPrivilege(request.ContactId, privilegeId);
            }
        }

        private async Task<List<ContactsPrivilege>> GetAddedPrivileges(SetContactPrivilegesRequest request, IList<ContactsPrivilege> currentPrivileges)
        {
            var allPrivileges = await _contactQueryRepository.GetAllPrivileges();
            
            return request.PrivilegeIds.Where(p => !currentPrivileges.Select(cp => cp.PrivilegeId).Contains(p))
                .Select(p => new ContactsPrivilege()
                {
                    PrivilegeId = p,
                    Privilege = allPrivileges.First(ap => ap.Id == p)
                }).ToList();
        }

        private List<ContactsPrivilege> GetRemovedPrivileges(SetContactPrivilegesRequest request, IList<ContactsPrivilege> currentPrivileges)
        {
            return currentPrivileges.Where(cp => !request.PrivilegeIds.Contains(cp.PrivilegeId)).ToList();
        }
    }
}