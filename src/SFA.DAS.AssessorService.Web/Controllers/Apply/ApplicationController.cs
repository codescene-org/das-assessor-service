using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.AssessorService.Api.Types.Models;
using SFA.DAS.AssessorService.Api.Types.Models.Apply;
using SFA.DAS.AssessorService.Api.Types.Models.Validation;
using SFA.DAS.AssessorService.Application.Api.Client.Clients;
using SFA.DAS.AssessorService.Application.Exceptions;
using SFA.DAS.AssessorService.ApplyTypes;
using SFA.DAS.AssessorService.Settings;
using SFA.DAS.AssessorService.Web.Infrastructure;
using SFA.DAS.AssessorService.Web.ViewModels.Apply;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.QnA.Api.Types.Page;

namespace SFA.DAS.AssessorService.Web.Controllers.Apply
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private readonly ILogger<ApplicationController> _logger;
        private readonly IOrganisationsApiClient _orgApiClient;
        private readonly IContactsApiClient _contactsApiClient;
        private readonly IApplicationApiClient _applicationApiClient;
        private readonly IQnaApiClient _qnaApiClient;
        private readonly IWebConfiguration _config;
        private const string WorkflowType = "EPAO";

        public ApplicationController(IOrganisationsApiClient orgApiClient, IQnaApiClient qnaApiClient, IWebConfiguration config,
            IContactsApiClient contactsApiClient, IApplicationApiClient applicationApiClient, ILogger<ApplicationController> logger)
        {
            _logger = logger;
            _orgApiClient = orgApiClient;
            _contactsApiClient = contactsApiClient;
            _applicationApiClient = applicationApiClient;
            _qnaApiClient = qnaApiClient;
            _config = config;
        }

        [HttpGet("/Application")]
        public async Task<IActionResult> Applications()
        {
            _logger.LogInformation($"Got LoggedInUser from Session: { User.Identity.Name}");

            var userId = await GetUserId();
            var org = await _orgApiClient.GetOrganisationByUserId(userId);
            var applications = await _applicationApiClient.GetApplications(userId, false);
            applications = applications.Where(app => app.ApplicationStatus != ApplicationStatus.Rejected).ToList();

            if (!applications.Any())
            {
                //ON-2068 Registered org  with no application created via digital service then
                //display empty list of application screen
                if (org != null)
                    return org.RoEPAOApproved ? View(applications) : View("~/Views/Application/Declaration.cshtml");

            }
            //ON-2068 If there is an existing application for an org that is registered then display it
            //in a list of application screen
            if (applications.Count() == 1 && (org != null && org.RoEPAOApproved))
                return View(applications);

            if (applications.Count() > 1)
                return View(applications);

            //This always return one record otherwise the previous logic would have handled the response
            var application = applications.First();

            switch (application.ApplicationStatus)
            {
                case ApplicationStatus.FeedbackAdded:
                    return View("~/Views/Application/FeedbackIntro.cshtml", application.Id);
                case ApplicationStatus.Rejected:
                case ApplicationStatus.Approved:
                    return View(applications);
                default:
                    return RedirectToAction("SequenceSignPost", new { Id = application.Id });
            }

        }

        [HttpPost("/Application")]
        public async Task<IActionResult> StartApplication()
        {
            var userId = await GetUserId();
            var org = await _orgApiClient.GetOrganisationByUserId(userId);

            var applicationStartRequest = new StartApplicationRequest
            {
                UserReference = userId.ToString(),
                WorkflowType = WorkflowType,
                ApplicationData = JsonConvert.SerializeObject(new ApplicationData {
                    UseTradingName = false,
                    OrganisationName = org.EndPointAssessorName,
                    OrganisationReferenceId = org.Id.ToString()
                })
            };

            var qnaResponse = await _qnaApiClient.StartApplications(applicationStartRequest);

            var  appResponse = await _applicationApiClient.CreateApplication(new CreateApplicationRequest
            {
                ApplicationStatus = ApplicationStatus.InProgress,
                OrganisationId = org.Id,
                QnaApplicationId = qnaResponse?.ApplicationId??Guid.Empty,
                UserId = userId
            });

            return RedirectToAction("SequenceSignPost", new { Id = appResponse });
        }

        [HttpGet("/Application/{Id}")]
        public async Task<IActionResult> SequenceSignPost(Guid Id)
        {
            var userId = await GetUserId();
            var application = await _applicationApiClient.GetApplication(Id);

            if (application is null)
            {
                return RedirectToAction("Applications");
            }

            if (application.ApplicationStatus == ApplicationStatus.Approved)
            {
                return View("~/Views/Application/Approved.cshtml", application);
            }

            if (application.ApplicationStatus == ApplicationStatus.Rejected)
            {
                return View("~/Views/Application/Rejected.cshtml", application);
            }

            if (application.ApplicationStatus == ApplicationStatus.FeedbackAdded)
            {
                return View("~/Views/Application/FeedbackIntro.cshtml", application.Id);
            }

            var sequence = await _qnaApiClient.GetApplicationActiveSequence(application.ApplicationId);

            StandardApplicationData applicationData = null;

            if (application.ApplyData != null)
            {
                applicationData = new StandardApplicationData
                {
                    StandardName = application.ApplyData?.Apply?.StandardName
                };
            }
            var sequenceNo = (SequenceNo)sequence.SequenceNo;

            //// Only go to search if application hasn't got a selected standard?
            if (sequenceNo == SequenceNo.Stage1)
            {
                return RedirectToAction("Sequence", new { application.ApplicationId });
            }
            else if (sequenceNo == SequenceNo.Stage2 && string.IsNullOrWhiteSpace(applicationData?.StandardName))
            {
                var org = await _orgApiClient.GetOrganisationByUserId(userId);
                if (org.RoEPAOApproved)
                {
                   return RedirectToAction("Index", "Standard", new { application.Id });
                }

                return View("~/Views/Application/Stage2Intro.cshtml", application.Id);
            }
            else if (sequenceNo == SequenceNo.Stage2)
            {
                return RedirectToAction("Sequence", new { application.Id });
            }

            throw new BadRequestException("Section does not have a valid DisplayType");
        }

        [HttpGet("/Application/{Id}/Sequence")]
        public async Task<IActionResult> Sequence(Guid Id)
        {
            var application = await _applicationApiClient.GetApplication(Id);
            var sequence = await _qnaApiClient.GetApplicationActiveSequence(application.ApplicationId);
            var sections = await _qnaApiClient.GetSections(application.ApplicationId, sequence.Id);
            
            var sequenceVm = new SequenceViewModel(sequence, application.Id, sections, null);
            if(application.ApplyData != null && application.ApplyData.Sequences != null)
            {
                var seq= application.ApplyData.Sequences.Single(x => x.SequenceId == sequence.Id && x.SequenceNo == sequence.SequenceNo);
                if (seq != null && seq.Status == ApplicationSequenceStatus.Submitted)
                    sequenceVm.Status = ApplicationSequenceStatus.Submitted;
            }
            return View(sequenceVm);
        }

        [HttpGet("/Application/{Id}/Sequences/{sequenceNo}/Sections/{sectionId}")]
        public async Task<IActionResult> Section(Guid Id, int sequenceNo, Guid sectionId)
        {
            var application = await _applicationApiClient.GetApplication(Id);
            var activeSequence = await _qnaApiClient.GetApplicationActiveSequence(application.ApplicationId);

            var canUpdate = CanUpdateApplication(activeSequence);
            if (!canUpdate)
            {
                return RedirectToAction("Sequence", new { Id });
            }

            var section = await _qnaApiClient.GetSection(application.ApplicationId, sectionId);
            var applicationSection = new ApplicationSection { Section = section, Id = Id };
            switch (section?.DisplayType)
            {
                case null:
                case SectionDisplayType.Pages:
                    return View("~/Views/Application/Section.cshtml", applicationSection);
                case SectionDisplayType.Questions:
                    return View("~/Views/Application/Section.cshtml", applicationSection);
                case SectionDisplayType.PagesWithSections:
                    return View("~/Views/Application/PagesWithSections.cshtml", applicationSection);
                default:
                    throw new BadRequestException("Section does not have a valid DisplayType");
            }
        }

        [HttpGet("/Application/{Id}/Sequences/{sequenceNo}/Sections/{sectionId}/Pages/{pageId}"), ModelStatePersist(ModelStatePersist.RestoreEntry)]
        public async Task<IActionResult> Page(Guid Id, int sequenceNo, Guid sectionId, string pageId, string __redirectAction)
        {
            var application = await _applicationApiClient.GetApplication(Id);
            var activeSequence = await _qnaApiClient.GetApplicationActiveSequence(application.ApplicationId);

            var canUpdate = CanUpdateApplication(activeSequence);
            if (!canUpdate)
            {
                return RedirectToAction("Sequence", new { Id });
            }

            PageViewModel viewModel = null;
            var returnUrl = Request.Headers["Referer"].ToString();

            if (!ModelState.IsValid)
            {
                // when the model state has errors the page will be displayed with the values which failed validation
                var page = JsonConvert.DeserializeObject<Page>((string)TempData["InvalidPage"]);

                var errorMessages = !ModelState.IsValid
                    ? ModelState.SelectMany(k => k.Value.Errors.Select(e => new ValidationErrorDetail()
                    {
                        ErrorMessage = e.ErrorMessage,
                        Field = k.Key
                    })).ToList()
                    : null;

                viewModel = new PageViewModel(Id, sequenceNo, sectionId, pageId, page, __redirectAction,
                    returnUrl, errorMessages);
            }
            else
            {
                // when the model state has no errors the page will be displayed with the last valid values which were saved
                var page = await _qnaApiClient.GetPage(application.ApplicationId, sectionId, pageId);

                if (page != null && (!page.Active || page.NotRequired))
                {
                    var nextPage = page.Next.FirstOrDefault(p => p.Condition is null);

                    if (nextPage?.ReturnId != null && nextPage?.Action == "NextPage")
                    {
                        pageId = nextPage.ReturnId;
                        return RedirectToAction("Page",
                            new { Id, sequenceNo, sectionId, pageId, __redirectAction });
                    }
                    else
                    {
                        return RedirectToAction("Section", new { Id, sequenceNo, sectionId });
                    }
                }

                //page = await GetDataFedOptions(page);

                viewModel = new PageViewModel(Id, sequenceNo, sectionId, pageId, page, __redirectAction,
                    returnUrl, null);

                ProcessPageVmQuestionsForStandardName(viewModel.Questions, application);
            }

            if (viewModel.AllowMultipleAnswers)
            {
                return View("~/Views/Application/Pages/MultipleAnswers.cshtml", viewModel);
            }

            return View("~/Views/Application/Pages/Index.cshtml", viewModel);
        }

        [HttpPost("/Application/{Id}/Sequences/{sequenceNo}/Sections/{sectionId}/Pages/{pageId}/multiple"), ModelStatePersist(ModelStatePersist.Store)]
        public async Task<IActionResult> SaveMultiplePageAnswers(Guid Id, int sequenceNo, Guid sectionId, string pageId, string __redirectAction, string __formAction)
        {
            var application = await _applicationApiClient.GetApplication(Id);
            var page = await _qnaApiClient.GetPage(application.ApplicationId, sectionId, pageId);
           
            if (page.AllowMultipleAnswers)
            {
                var answers = GetAnswersFromForm(page);
                var pageAddResponse = await _qnaApiClient.AddAnswersToMultipleAnswerPage(application.ApplicationId, sectionId, pageId, answers);
                if (pageAddResponse?.Success != null && pageAddResponse.Success)
                {
                    if (__formAction == "Add")
                    {
                        return RedirectToAction("Page", new
                        {
                            Id,
                            sequenceNo,
                            sectionId,
                            pageId = pageAddResponse.Page.PageId,
                            __redirectAction
                        });
                    }

                    if (__redirectAction == "Feedback")
                        return RedirectToAction("Feedback", new { Id });

                    var nextAction = pageAddResponse.Page.Next.SingleOrDefault(x => x.Action == "NextPage");

                    if (!string.IsNullOrEmpty(nextAction.Action))
                        return RedirectToNextAction(Id, sequenceNo, sectionId, __redirectAction, nextAction.Action, nextAction.ReturnId);
                }
                else if(page.PageOfAnswers?.Count > 0)
                {
                    var nextAction = page.Next.SingleOrDefault(x => x.Action == "NextPage");

                    if (!string.IsNullOrEmpty(nextAction.Action))
                        return RedirectToNextAction(Id, sequenceNo, sectionId, __redirectAction, nextAction.Action, nextAction.ReturnId);
                }

                SetResponseValidationErrors(pageAddResponse?.ValidationErrors, page);
            }
            else
            {
                return BadRequest("Page is not of a type of Multiple Answers");
            }
            
            return RedirectToAction("Page", new { Id, sequenceNo, sectionId, pageId, __redirectAction });
        }

        [HttpPost("/Application/{Id}/Sequences/{sequenceNo}/Sections/{sectionId}/Pages/{pageId}"), ModelStatePersist(ModelStatePersist.Store)]
        public async Task<IActionResult> SaveAnswers(Guid Id, int sequenceNo, Guid sectionId, string pageId, string __redirectAction, string __formAction)
        {
            var application = await _applicationApiClient.GetApplication(Id);
            var activeSequence = await _qnaApiClient.GetApplicationActiveSequence(application.ApplicationId);

            var canUpdate = CanUpdateApplication(activeSequence);
            if (!canUpdate)
            {
                return RedirectToAction("Sequence", new { application.ApplicationId });
            }

            var page = await _qnaApiClient.GetPage(application.ApplicationId, sectionId, pageId);

            var answers = GetAnswersFromForm(page);
            
            SetPageAnswersResponse updatePageResult;
            var fileupload = page.Questions?.Any(q => q.Input.Type == "FileUpload");
            if(fileupload??false)
                updatePageResult = await UploadFilesToStorage(application.ApplicationId, sectionId, pageId, page);
            else
                updatePageResult = await _qnaApiClient.AddPageAnswer(application.ApplicationId, sectionId, pageId, answers);

            if (updatePageResult.ValidationPassed)
            {
                if (__redirectAction == "Feedback")
                    return RedirectToAction("Feedback", new { Id });

                if (!string.IsNullOrEmpty(updatePageResult.NextAction))
                    return RedirectToNextAction(Id, sequenceNo, sectionId, __redirectAction, updatePageResult.NextAction, updatePageResult.NextActionId);
            }

            if (!page.PageOfAnswers.Any())
            {
                page.PageOfAnswers = new List<PageOfAnswers>() {new PageOfAnswers(){Answers = new List<Answer>()}};
            }
            
            page = StoreEnteredAnswers(answers, page);

            SetResponseValidationErrors(updatePageResult?.ValidationErrors, page);
            

            return RedirectToAction("Page", new { Id, sequenceNo, sectionId, pageId, __redirectAction });
        }

        private Page StoreEnteredAnswers(List<Answer> answers, Page page)
        {
            foreach (var answer in answers)
            {
                var pageAnswer = page.PageOfAnswers.Single().Answers.SingleOrDefault(a => a.QuestionId == answer.QuestionId);
                if (pageAnswer is null)
                {
                    page.PageOfAnswers.Single().Answers.Add(answer);
                }
                else
                {
                    pageAnswer.Value = answer.Value;
                }
            }

            return page;
        }

        [HttpPost("/Application/DeleteAnswer")]
        public async Task<IActionResult> DeleteAnswer(Guid Id, int sequenceNo, Guid sectionId, string pageId, Guid answerId, string __redirectAction)
        {
            var application = await _applicationApiClient.GetApplication(Id);

            await _qnaApiClient.RemovePageAnswer(application.ApplicationId, sectionId, pageId, answerId);

            return RedirectToAction("Page", new { Id, sequenceNo, sectionId, pageId, __redirectAction });
        }

        [HttpGet("Application/{Id}/Section/{sectionId}/Page/{pageId}/Question/{questionId}/{filename}/Download")]
        public async Task<IActionResult> Download(Guid Id, Guid sectionId, string pageId, string questionId, string filename)
        {
            var application = await _applicationApiClient.GetApplication(Id);

            var response = await _qnaApiClient.DownloadFile(application.ApplicationId, sectionId, pageId, questionId, filename);

            var fileStream = await response.Content.ReadAsStreamAsync();

            return File(fileStream, response.Content.Headers.ContentType.MediaType, filename);

        }

        [HttpGet("Application/{Id}/SequenceNo/{sequenceNo}Section/{sectionId}/Page/{pageId}/Question/{questionId}/download/{filename}/RedirectAction/{redirectAction}")]
        public async Task<IActionResult> DeleteFile(Guid Id, int sequenceNo, Guid sectionId, string pageId, string questionId, string filename, string redirectAction)
        {
            var application = await _applicationApiClient.GetApplication(Id);

            await _qnaApiClient.DeleteFile(application.ApplicationId, sectionId, pageId, questionId, filename);

            return RedirectToAction("Page", new { Id, sequenceNo, sectionId, pageId,  redirectAction });
        }

        [HttpPost("/Application/Submit")]
        public async Task<IActionResult> Submit(Guid Id)
        {
            var userId = await GetUserId();
            var email = await GetUserEmail();
            var application = await _applicationApiClient.GetApplication(Id);
            var activeSequence = await _qnaApiClient.GetApplicationActiveSequence(application.ApplicationId);
            var canUpdate = CanUpdateApplication(activeSequence);
            if (!canUpdate)
            {
                return RedirectToAction("Sequence", new { Id });
            }

            var sections = await _qnaApiClient.GetSections(application.ApplicationId, activeSequence.Id);
            var errors =  ValidateSubmit(sections);
            if (errors.Any())
            {
                var sequenceVm = new SequenceViewModel(activeSequence, Id,sections, errors);

                if (activeSequence.Status == ApplicationSequenceStatus.FeedbackAdded)
                {
                    return View("~/Views/Application/Feedback.cshtml", sequenceVm);
                }
                else
                {
                    return View("~/Views/Application/Sequence.cshtml", sequenceVm);
                }
            }

            if (await _applicationApiClient.Submit(Id, userId, email, activeSequence, sections, _config.ReferenceFormat))
            {
                return RedirectToAction("Submitted", new { Id });
            }

            return RedirectToAction("NotSubmitted", new { Id });
        }

        [HttpGet("/Application/{Id}/Submitted")]
        public async Task<IActionResult> Submitted(Guid Id)
        {
            var application = await _applicationApiClient.GetApplication(Id);
            return View("~/Views/Application/Submitted.cshtml", new SubmittedViewModel
            {
                ReferenceNumber = application?.ApplyData?.Apply?.ReferenceNumber,
                FeedbackUrl = _config.FeedbackUrl,
                StandardName = application?.ApplyData?.Apply?.StandardName
            });
        }

        [HttpGet("/Application/{Id}/NotSubmitted")]
        public async Task<IActionResult> NotSubmitted(Guid Id)
        {
            var application = await _applicationApiClient.GetApplication(Id);
            return View("~/Views/Application/NotSubmitted.cshtml", new SubmittedViewModel
            {
                ReferenceNumber = application?.ApplyData?.Apply?.ReferenceNumber,
                FeedbackUrl = _config.FeedbackUrl,
                StandardName = application?.ApplyData?.Apply?.StandardName
            });
        }

        private RedirectToActionResult RedirectToNextAction(Guid Id, int sequenceNo, Guid sectionId, string redirectAction, string nextAction, string nextActionId)
        {
            if (nextAction == "NextPage")
            {
                return RedirectToAction("Page", new
                {
                    Id,
                    sequenceNo,
                    sectionId,
                    pageId = nextActionId,
                    __redirectAction = redirectAction
                });
            }

            return nextAction == "ReturnToSection"
                ? RedirectToAction("Section", "Application", new { Id, sequenceNo, sectionId })
                : RedirectToAction("Sequence", "Application", new { Id });
        }

        private void SetResponseValidationErrors(List<KeyValuePair<string, string>> validationErrors, Page page)
        {
            if (validationErrors != null)
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }

            TempData["InvalidPage"] = JsonConvert.SerializeObject(page);
        }

        private async Task<SetPageAnswersResponse> UploadFilesToStorage(Guid applicationId, Guid sectionId, string pageId, Page page)
        {
            SetPageAnswersResponse response = null;
            if (HttpContext.Request.Form.Files.Any() && page != null)
            {
                response = await _qnaApiClient.Upload(applicationId, sectionId, pageId, HttpContext.Request.Form.Files);
            }
            return response;
        }

        private List<Answer> GetAnswersFromForm(Page page)
        {
            List<Answer> answers = new List<Answer>();
            var questionId = page.Questions.Where(x => x.Input.Type == "ComplexRadio" || x.Input.Type == "Radio").Select(y => y.QuestionId).FirstOrDefault();

            foreach (var keyValuePair in HttpContext.Request.Form.Where(f => !f.Key.StartsWith("__") && !f.Key.Contains("RedirectAction")))
            {
                answers.Add(new Answer() { QuestionId = keyValuePair.Key, Value = keyValuePair.Value });
            }

            if (answers != null && !answers.Any())
            {
                answers.Add(new Answer { QuestionId = questionId, Value = "" });
            }
            else if (questionId != null && answers.Any(y => y.QuestionId.Contains(questionId)))
            {
                if (answers.All(x => x.Value == ""))
                {
                    foreach (var answer in answers.Where(y => y.QuestionId.Contains(questionId + ".") && y.Value == ""))
                    {
                        answer.QuestionId = questionId;
                        break;
                    }
                }
                else if (answers.Count(y => y.QuestionId.Contains(questionId) && y.Value == "") == 1 && answers.Count(y => y.QuestionId == questionId && y.Value != "") == 0)
                {
                    foreach (var answer in answers.Where(y => y.QuestionId.Contains(questionId + ".") && y.Value == ""))
                    {
                        answer.QuestionId = questionId;
                    }
                }
            }
            return answers;
        }

        private void ProcessPageVmQuestionsForStandardName(List<QuestionViewModel> pageVmQuestions, ApplicationResponse application)
        {
            if (pageVmQuestions == null) return;

            var placeholderString = "StandardName";
            var isPlaceholderPresent = false;

            foreach (var question in pageVmQuestions)

                if (question.Label.Contains($"[{placeholderString}]") ||
                   question.Hint.Contains($"[{placeholderString}]") ||
                    question.QuestionBodyText.Contains($"[{placeholderString}]") ||
                    question.ShortLabel.Contains($"[{placeholderString}]")
                   )
                    isPlaceholderPresent = true;

            if (!isPlaceholderPresent) return;

            var standardName = application?.ApplyData?.Apply?.StandardName;

            if (string.IsNullOrEmpty(standardName)) standardName = "the standard to be selected";

            foreach (var question in pageVmQuestions)
            {
                question.Label = question.Label?.Replace($"[{placeholderString}]", standardName);
                question.Hint = question.Hint?.Replace($"[{placeholderString}]", standardName);
                question.QuestionBodyText = question.QuestionBodyText?.Replace($"[{placeholderString}]", standardName);
                question.ShortLabel = question.Label?.Replace($"[{placeholderString}]", standardName);
            }
        }
        private List<ValidationErrorDetail> ValidateSubmit(List<Section> sections)
        {
            var validationErrors = new List<ValidationErrorDetail>();

            if (sections is null)
            {
                var validationError = new ValidationErrorDetail(string.Empty, $"Cannot submit empty sequence");
                validationErrors.Add(validationError);
            }
            else if (sections.Where(sec => sec.QnAData.Pages.Count(x => x.Complete == true) != sec.QnAData.Pages.Count(x => x.Active)).Any())
            {
                foreach (var sectionQuestionsNotYetCompleted in sections.Where(sec => sec.QnAData.Pages.Count(x => x.Complete == true) != sec.QnAData.Pages.Count(x => x.Active)))
                {
                    var validationError = new ValidationErrorDetail(sectionQuestionsNotYetCompleted.Id.ToString(), $"You need to complete the '{sectionQuestionsNotYetCompleted.LinkTitle}' section");
                    validationErrors.Add(validationError);
                }
            }
            else if (sections.Where(sec => sec.QnAData.RequestedFeedbackAnswered is false || sec.QnAData.Pages.Any(p => !p.AllFeedbackIsCompleted)).Any())
            {
                foreach (var sectionFeedbackNotYetCompleted in sections.Where(sec => sec.QnAData.RequestedFeedbackAnswered is false || sec.QnAData.Pages.Any(p => !p.AllFeedbackIsCompleted)))
                {
                    var validationError = new ValidationErrorDetail(sectionFeedbackNotYetCompleted.Id.ToString(), $"You need to complete the '{sectionFeedbackNotYetCompleted.LinkTitle}' section");
                    validationErrors.Add(validationError);
                }
            }

            return validationErrors;
        }

        private bool CanUpdateApplication(Sequence sequence)
        {
            bool canUpdate = false;

            if (sequence?.Status != null && (int)sequence.SequenceNo == sequence.SequenceNo)
            {
                canUpdate = sequence.Status == ApplicationSequenceStatus.Draft || sequence.Status == ApplicationSequenceStatus.FeedbackAdded;
            }

            return canUpdate;
        }

        private async Task<Guid> GetUserId()
        {
            var signinId = User.Claims.First(c => c.Type == "sub")?.Value;
            var contact = await GetUserContact(signinId);

            return contact?.Id ?? Guid.Empty;
        }

        private async Task<string> GetUserEmail()
        {
            var signinId = User.Claims.First(c => c.Type == "sub")?.Value;
            var contact = await GetUserContact(signinId);

            return contact.Email;
        }

        private async Task<ContactResponse> GetUserContact(string signinId)
        {
            return await _contactsApiClient.GetContactBySignInId(signinId);
        }
    }
}