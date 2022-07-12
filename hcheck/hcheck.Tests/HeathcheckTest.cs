using System;
using hcheck;
namespace test;

public class HealthcheckTest
{
    [Fact]
    public void AppendReportTest()
    {
        using (TestScriptGenerator tsg = new TestScriptGenerator("echo \"Hello, World\""), tsg2 = new TestScriptGenerator("echo \"Hello, World\""))
        {
            string[] args = { "-k", tsg.Path, "--rpath", "./report.json" };
            Healthcheck.Main(args);
            string[] args2 = { "-k", tsg2.Path, "--rpath", "./report.json", "--append" };
            Healthcheck.Main(args2);
            ArgumentProcessor argus = new ArgumentProcessor(args2);
            ReportBuilder builder = new ReportBuilder(argus, argus.FilePath);
            HealthReport rep = builder.BuildReport();
            Assert.True(rep.testresults.ContainsKey(tsg.Path));
            Assert.True(rep.testresults.ContainsKey(tsg2.Path));
        }
    } 

    [Fact]
    public void PythonCustomScriptRunTest()
    {
        using (TestScriptGenerator tsg = new TestScriptGenerator("print(\"Hello, python world\")\nexit(0)", true))
        {
            string[] args = { "-k", tsg.Path, "--rpath", "./report.json", "--python", "python3"};
            Healthcheck.Main(args);
            ArgumentProcessor argus = new ArgumentProcessor(args);
            ReportBuilder builder = new ReportBuilder(argus, argus.FilePath);
            HealthReport rep = builder.BuildReport();
            Assert.True(rep.testresults.ContainsKey(tsg.Path));
            Assert.True(rep.testresults[tsg.Path].ContainsKey("message"));
            Assert.Equal("Hello, python world\n", rep.testresults[tsg.Path]["message"].ToString());
        }
    }
}