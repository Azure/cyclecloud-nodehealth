using System.Text.Json;
using System.Text;

namespace hcheck
{
    public class ReportBuilder
    {
        static readonly HttpClient client = new HttpClient();
        ArgumentProcessor args;
        string path = "";
        //select useful fields to be stored in metadata
        string[] headerFieldNames = {"azEnvironment", "isHostCompatibilityLayerVm", "location", "name",
    "offer", "osType", "platformFaultDomain", "platformUpdateDomain", "provider", "publisher",
    "resourceGroupName", "resourceId", "sku", "subscriptionId", "vmId", "version", "vmScaleSetName",
    "vmSize", "zone" }; //maybe move this to a json config file later
        private const string headerURL = "http://169.254.169.254/metadata/instance?api-version=2021-02-01";

        public ReportBuilder(ArgumentProcessor args, string path)
        {
            this.args = args;
            this.path = path;
        }

        private string GetFileContent(string path)
        {
            string[] test = File.ReadAllLines(path);

            StringBuilder fileText = new StringBuilder("");
            foreach (string s in test)
            {
                fileText.Append(s);
            }
            return fileText.ToString();
        }

        private string GetHeader(string filepath)
        {
            return GetFileContent(filepath);
        }


        private async Task<string> GetHeader()
        {
            try
            {
                client.DefaultRequestHeaders.Add("Metadata", value: "true");
                HttpResponseMessage response = await client.GetAsync(headerURL);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                //just in case metadata couldn't be obtained
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return "N/A";
            }
        }

        /*
        Gets the metadata for the header, runs the tests, and returns the class containing a health report of the VM
        */
        public HealthReport BuildReport()
        {
            HealthReport? report = null;
            //string header = GetHeader("./example.json");
            if (args.IsAppend && File.Exists(path))
            {
                try
                {
                    //read the existing report to get the header and data about performed tests
                    var options = new JsonSerializerOptions()
                    {
                        AllowTrailingCommas = true
                    };
                    report = JsonSerializer.Deserialize<HealthReport>(File.ReadAllText(path), options);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Invalid report encountered: " + ex.Message);
                }
            }
            if (report == null)
            {
                // create new report from scratch
                Task<string> headGetter = GetHeader();
                headGetter.Wait();
                string header = headGetter.Result;
                report = new HealthReport();
                if (header != "N/A")
                {
                    try
                    {
                        dynamic? deserializedResult = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(header);

                        if (deserializedResult != null)
                        {
                            // only add approved fields 
                            foreach (KeyValuePair<string, object> entry in deserializedResult["compute"])
                            {
                                if (headerFieldNames.Contains(entry.Key))
                                    report.metadata.Add(entry.Key, entry.Value);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("No header data received: " + ex.Message);
                    }
                }
            }

            TestRunner tester = new TestRunner(report);

            for (int i = 0; i < args.NumRuns; i++)
            {
                if (args.IsPattern)
                {
                    try
                    {
                        var files = Directory.EnumerateFiles(args.TestDir, args.Pattern, SearchOption.TopDirectoryOnly);
                        if (files != null)
                        {
                            foreach (string testname in files)
                            {
                                tester.RunTest(testname);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Incorrect path or pattern provided: " + ex.Message);
                    }
                }
                else
                {
                    try
                    {
                        tester.RunTest(args.TestDir);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Incorrect path or pattern provided: " + ex.Message);
                    }
                }
            }
            report = tester.getReport();
            return report;
        }
    }
}