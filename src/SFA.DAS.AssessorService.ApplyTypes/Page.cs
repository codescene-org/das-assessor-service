using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.AssessorService.ApplyTypes
{
    public class Page
    {
        public Guid ApplicationId { get; set; }
        private List<DisplayAnswerPage> _displayAnswerPages;
        public string PageId { get; set; }
        public string SequenceId { get; set; }
        public string SectionId { get; set; }
        public string Title { get; set; }
        public string LinkTitle { get; set; }
        public string InfoText { get; set; }
        public List<Question> Questions { get; set; }
        public List<PageOfAnswers> PageOfAnswers { get; set; }
        public List<Next> Next { get; set; }
        public bool Complete { get; set; }
        public bool AllowMultipleAnswers { get; set; }
        public int? Order { get; set; }
        public bool Active { get; set; }        
        public bool Visible { get; set; }
        public FinancialApplicationGrade FinancialApplicationGrade { get; set; }
        
        public List<Feedback> Feedback { get; set; }
        public bool HasFeedback => Feedback?.Any() ?? false;

        public bool ContainsUploadQuestions
        {
            get
            {
                return Questions.Any(q => q.Input.Type == "FileUpload");
            }
        }
        
        public bool HasNewFeedback
        {
            get
            {
                return HasFeedback && Feedback.Any(f => !f.IsCompleted);
            }
        }
        
        public bool HasReadFeedback
        {
            get
            {
                return HasFeedback && Feedback.Any(f => f.IsCompleted);
            }
        }
        
        public List<DisplayAnswerPage> DisplayAnswerPages
        {
            get
            {
                if (_displayAnswerPages != null) return _displayAnswerPages;
                
                _displayAnswerPages = new List<DisplayAnswerPage>();
                foreach (var answerPage in PageOfAnswers)
                {
                    var displayAnswerPage = new DisplayAnswerPage();
                    displayAnswerPage.DisplayAnswers = new List<DisplayAnswer>();
                        
                    foreach (var answer in answerPage.Answers)
                    {
                        var question = Questions.SingleOrDefault(q => q.QuestionId == answer.QuestionId);
                        if (question != null)
                        {
                            var displayAnswer = new DisplayAnswer
                            {
                                Answer = answer.Value,
                                Label = question.Label
                            };

                            displayAnswerPage.DisplayAnswers.Add(displayAnswer);   
                        }
                        else
                        {
                            // question is null, try embedded questions in ComplexRadio / Further Questions
                            question = Questions.Single(q => q.QuestionId == answer.QuestionId.Split(new[]{'.'},StringSplitOptions.RemoveEmptyEntries)[0]);

                            var furtherQuestion = question.Input.Options.Where(o => o.FurtherQuestions != null).SelectMany(o => o.FurtherQuestions).Single(q => q.QuestionId == answer.QuestionId);
                            
                            var displayAnswer = new DisplayAnswer
                            {
                                Answer = answer.Value,
                                Label = furtherQuestion.Label
                            };

                            displayAnswerPage.DisplayAnswers.Add(displayAnswer);   
                        }
                    }
                        
                    _displayAnswerPages.Add(displayAnswerPage);
                }
                return _displayAnswerPages;
            }
        }

        public List<Question> UploadQuestions
        {
            get
            {
                return Questions.Where(q => q.Input.Type == "FileUpload").ToList();
            }
        }
    }
}