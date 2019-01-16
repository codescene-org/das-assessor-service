using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SFA.DAS.AssessorService.EpaoImporter.Const;
using SFA.DAS.AssessorService.EpaoImporter.Startup;

namespace SFA.DAS.AssessorService.EpaoImporter
{
    public static class PrintFunctionFlow
    {
        //MFCMFC
        [FunctionName("PrintFunctionFlow")]
        public static void Run([TimerTrigger("0 */2 * * * *", RunOnStartup = true)] TimerInfo myTimer, TraceWriter functionLogger,     
            ExecutionContext context)
        {

            new Bootstrapper().StartUp(functionLogger, context);

            var command = Bootstrapper.Container.GetInstance<PrintProcessCommand>();
            command.Execute().GetAwaiter().GetResult();
        }
    }
}

