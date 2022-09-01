using System.Text.Json;
using System.Text;


namespace hcheck
{

    public class HealthReport
    {
        public Dictionary<string, object> metadata { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, Dictionary<string, object>> testresults { get; set; } = new Dictionary<string, Dictionary<string, object>>();
    }

    public class Healthcheck
    {
        private static HealthReport makeReport(ArgumentProcessor args)
        {
            if (!args.IsAppend)
            {
                if (File.Exists(args.FilePath))
                {
                    File.Delete(args.FilePath);
                }
                FileStream fs = File.Create(args.FilePath);
                fs.Close();
            }
            ReportBuilder reportBuilder = new ReportBuilder(args, args.FilePath);
            HealthReport finalReport = reportBuilder.BuildReport();
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            string encodedReport = JsonSerializer.Serialize<HealthReport>(finalReport, options);
            File.WriteAllText(args.FilePath, encodedReport);
            return finalReport;
        }

        private static int processReport(ArgumentProcessor args, string path = "./report.json")
        {
            ReportProcessor rp = new ReportProcessor(path);
            int exitCode = rp.getExitCode();
            if (exitCode != 0) rp.ReportError(args);
            return exitCode;
        }

        private static int processReport(ArgumentProcessor args, HealthReport report)
        {
            ReportProcessor rp = new ReportProcessor(report);
            int exitCode = rp.getExitCode();
            if (exitCode != 0) rp.ReportError(args);
            return exitCode;
        }


        public static int Main(string[] args)
        {
            ArgumentProcessor proccessedArgs = new ArgumentProcessor(args);

            if (proccessedArgs.IsPath)
            {
                HealthReport report = makeReport(proccessedArgs);
                if (proccessedArgs.IsFinal)
                {
                    return processReport(proccessedArgs, report);
                }
            }
            else if (proccessedArgs.IsFinal)
                return processReport(proccessedArgs, proccessedArgs.FilePath);
            return 0;
        }
    }
}