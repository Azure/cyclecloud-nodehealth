using System.Text.Json;
using System.Text.RegularExpressions;

namespace hcheck
{
    public class ReportProcessor
    {
        private HealthReport? report;
        private const string regexFilter = "(\\S+@\\S+\\.\\S+|[\\+]?[1-9]?\\(?([0-9]{3})\\)?[-.\\s]?([0-9]{3})[-.\\s]?([0-9]{4}))";
        private const string sigFilter = "(sig=[A-Za-z0-9%/]+)";

        public ReportProcessor(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var options = new JsonSerializerOptions()
                    {
                        AllowTrailingCommas = true
                    };
                    report = JsonSerializer.Deserialize<HealthReport>(File.ReadAllText(filePath), options);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Invalid report encountered: " + ex.Message);
                }
            }
        }
        public ReportProcessor(HealthReport report)
        {
            this.report = report;
        }

        public int getExitCode()
        {
            if (report == null) return 1;
            int exitCode = 0;
            foreach (var test in report.testresults)
            {
                bool isInt = Int32.TryParse(test.Value["exit-code"].ToString(), out exitCode);
                if (!isInt) return 1;
                if (exitCode != 0) return exitCode;
            }
            return 0;
        }

        private string _filterOutPersonalData(string message)
        {
            string newMessage = "";
            newMessage = Regex.Replace(message, regexFilter, "XXX");
            newMessage = Regex.Replace(newMessage, sigFilter, "sig=XXX");

            return newMessage;
        }

        private string _getMetadata(string field)
        {
            if (report == null || !report.metadata.ContainsKey(field)) return "unknown";
            //tricking the intellisense
            string? nodeInfo = report.metadata[field].ToString();
            nodeInfo = (nodeInfo == null) ? "unknown" : nodeInfo;
            return nodeInfo;
        }

        public string ReportError(ArgumentProcessor args)
        {
            if (report == null) return "";
            string message = "";
            string extraInfo = "";
            int exitCode = 0;
            AppInsightWorker? appInsW = null;
            ProcessRunner pr = new ProcessRunner();
            // if an option to send Application insights logs was chosen
            if (args.InstrumentationKey != "") appInsW = new AppInsightWorker(args.InstrumentationKey);
            foreach (var test in report.testresults)
            {
                bool isInt = Int32.TryParse(test.Value["exit-code"].ToString(), out exitCode);
                string? tryConvert = null;
                if ((!isInt || exitCode != 0))
                {
                    string nodeName = _getMetadata("name");
                    string nodeId = _getMetadata("vmId");
                    string testName = Path.GetFileName(test.Key);
                    string logMessage = "nhc: " + nodeName + " failed " + testName;
                    tryConvert = test.Value["message"].ToString();
                    message = (tryConvert == null) ? "No error message provided" : tryConvert;
                    message = _filterOutPersonalData(message);
                    tryConvert = test.Value["extra-info"].ToString();
                    extraInfo = (tryConvert == null) ? "None" : tryConvert;
                    extraInfo = _filterOutPersonalData(extraInfo);
                    tryConvert = test.Value["test-time"].ToString();
                    string testTime = (tryConvert == null) ? "0" : tryConvert;
                    if (logMessage.Last<char>() != '\n') logMessage += "\n";
                    if (args.PythonPath == "")
                    {
                        string[] arguments = { "--level", "error", "-m", logMessage, "--info", extraInfo, "--code", exitCode.ToString(),
                                            "--testname", testName, "--nodeid", nodeId, "--time", testTime, "--error", message};
                        pr.RunProcess(args.ReportScriptPath, arguments, 10000);
                    }
                    else
                    {
                        //python scripts require python to run and jetpack is a python install
                        string[] arguments = { args.ReportScriptPath, "--level", "error", "-m", logMessage, "--info", extraInfo, "--code", exitCode.ToString(),
                                            "--testname", testName, "--nodeid", nodeId, "--time", testTime, "--error", message};
                        pr.RunProcess(args.PythonPath, arguments, 10000);
                    }
                    if (appInsW != null) appInsW.Send(message).Wait();
                    return message;
                }
            }
            return "";
        }
    }
}