using System.Text.Json;
using System.Xml;

namespace hcheck
{
    public class TestRunner
    {
        private HealthReport report;
        public ProcessRunner? pr = null;

        public TestRunner(HealthReport header)
        {
            report = header;
        }

        public HealthReport getReport()
        {
            return report;
        }

        private void AddRepeatInfo(string testPath, Dictionary<string, object> testResults, ProcessRunner pr)
        {

            if (report.testresults[testPath].ContainsKey("repeat-history"))
            {
                LinkedList<object> list= JsonSerializer.Deserialize< LinkedList<object>>( report.testresults[testPath]["repeat-history"].ToString());
                list.AddLast(testResults);
                report.testresults[testPath]["repeat-history"] = list;
                //LinkedList<object> list = (LinkedList<object>)report.testresults[testPath]["repeat-history"];
                //list.AddLast(testResults);

            }
            else
            {
                LinkedList<object> list = new LinkedList<object>();
                list.AddLast(new Dictionary<string, object>(report.testresults[testPath]));
                list.AddLast(testResults);
                report.testresults[testPath]["repeat-history"] = list;
            }
            //first test run that returned an error should be reported
            if (report.testresults[testPath]["exit-code"].ToString() == "0" && pr.exitCode != 0)
            {
                report.testresults[testPath]["exit-code"] = pr.exitCode;
                report.testresults[testPath]["extra-info"] = testResults["extra-info"];
                report.testresults[testPath][key: "message"] = testResults["message"];
            }
        }



        public void RunTest(string testPath, ArgumentProcessor args)
        {
            //consern: properly initialize the test, success or fail
            //how long took,  collect results, write the report
            //contract: if the external process is running, exit 0
            //invoke nvidia-smi 
            if (pr == null) pr = new ProcessRunner();
            //python scripts need to be run with a python installation
            if (args.ReframePath != "")
            {
                string? reportPath = Path.GetDirectoryName(args.FilePath);
                // string actualReportPath = (reportPath == null) ? "/var/log/reframe_results.json" : reportPath + "reframe_results.json";
                string actualReportPath = "/var/log/reframe_results.json";
                pr.RunProcess(args.ReframePath, new string[] { "-C",args.ReframeConfigPath,"--force-local","--report-file", actualReportPath,"-c", testPath, "-R","-r" }, 10000);
                if (!pr.isSuccess)
                {
                    Console.WriteLine("There was an error in launching the script: " + pr.stderr);
                    return;
                }
                else
                    Console.WriteLine("reframe ran with output " + pr.stdout);
                string reframeErrorMessage = ReframeWorker.ReadReframeReport(actualReportPath);
                pr.stdout = (reframeErrorMessage == "") ? "No message" : reframeErrorMessage;
            }
            else
            {
                pr.RunProcess(testPath);
            }
            var options = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true
            };
            if (!pr.isSuccess)
            {
                Console.WriteLine("There was an error in launching the script: " + pr.stderr);
                return;
            }
            Dictionary<string, object> testResults = new Dictionary<string, object>();
            try
            {
                testResults.Add("exit-code", pr.exitCode);
                testResults.Add("test-time", (pr.exitTime - pr.startTime).TotalMilliseconds);
                testResults.Add("extra-info", "None");
                Dictionary<string, object>? deserializedResult = JsonSerializer.Deserialize<Dictionary<string, object>>(pr.stdout, options);
                if (deserializedResult == null) throw new System.Text.Json.JsonException();
                Dictionary<string, object> extraInfo = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> record in deserializedResult)
                {
                    if (record.Key == "message") testResults.Add("message", record.Value);
                    else extraInfo.Add(record.Key, record.Value);
                }
                //if no "message" tag, treat the whole thing as a message
                if (!testResults.ContainsKey("message")) throw new System.Text.Json.JsonException("No message set");
                testResults["extra-info"] = extraInfo;
            }
            catch (System.Text.Json.JsonException ex) when (ex.Data != null) //if not parse-able, result was a simple message
            {
                testResults.Add("message", pr.stdout);
            }
            if (!report.testresults.ContainsKey(testPath))
                report.testresults.Add(testPath, value: testResults);
            else
            {
                AddRepeatInfo(testPath, testResults, pr);
            }
        }
    }
}