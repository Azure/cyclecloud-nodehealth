using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace hcheck
{
    public class ReframeWorker
    {
        public static string ReadReframeReport(string reportPath)
        {
            try
            {
                var options = new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true
                };
                JsonNode reframeReportDeserial = JsonSerializer.Deserialize<JsonNode>(File.ReadAllText(reportPath));
                if (!(reframeReportDeserial["runs"]==null))
                {
                    JsonArray runsArray = reframeReportDeserial["runs"].AsArray();
                    foreach (JsonNode run in runsArray)
                    {
                        if (!(run["num_failures"]==null) && run["num_failures"].GetValue<Int32>() > 0) {

                            if (!(run["testcases"] == null))
                            {
                                JsonArray testCasesArray = run["testcases"].AsArray();
                                foreach (JsonNode testCase in testCasesArray)
                                {
                                    if(!(testCase["fail_reason"] == null))
                                    {
                                        return testCase["fail_reason"].ToString();
                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return "";
        }
    }
}