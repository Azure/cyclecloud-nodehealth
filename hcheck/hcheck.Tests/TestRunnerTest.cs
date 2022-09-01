using System;
using hcheck;
namespace test;

public class TestRunnerTest
{
    
    
     [Fact]
    public void HeaderUnchangedTest()
    {
        HealthReport report = new HealthReport();
        report.metadata.Add("header", "header-data");
        TestRunner tester = new TestRunner(report);
        FakeProcessRunner fakePr = new FakeProcessRunner();
        fakePr.setParams(0, "Hi\n"); 
        tester.pr = fakePr;
        tester.RunTest("echo", new ArgumentProcessor(new string []{""}));
        Assert.Equal(1, report.metadata.Count);
        Assert.True(report.metadata.ContainsKey("header"));
        Assert.Equal("header-data", report.metadata["header"]);
    }

    [Fact]
    public void MessageSetCorrectly()
    {
        HealthReport report = new HealthReport();
        report.metadata.Add("header", "header-data");
        TestRunner tester = new TestRunner(report);
        FakeProcessRunner fakePr = new FakeProcessRunner();
        fakePr.setParams(0, "Hello, World\n");
        tester.pr = fakePr;
        tester.RunTest("echo", new ArgumentProcessor(new string []{""}));
        report = tester.getReport();
        Assert.True(report.testresults.ContainsKey("echo"));
        Assert.True(report.testresults["echo"].ContainsKey("message"));
        Assert.Equal("Hello, World\n", tester.getReport().testresults["echo"]["message"].ToString());
        //ask if I need to support multiple runs of the same test script and how the results should be stored
    }

    [Fact]
    public void ExitCodeSetCorrectly()
    {
        HealthReport report = new HealthReport();
        report.metadata.Add("header", "header-data");
        TestRunner tester = new TestRunner(report);
        FakeProcessRunner fakePr = new FakeProcessRunner();
        fakePr.setParams(0, "Hello, World\n");
        tester.pr = fakePr;
        tester.RunTest("echo", new ArgumentProcessor(new string []{""}));
        report = tester.getReport();
        Assert.True(report.testresults.ContainsKey("echo"));
        Assert.True(report.testresults["echo"].ContainsKey("exit-code"));
        Assert.Equal("0", tester.getReport().testresults["echo"]["exit-code"].ToString());
    }

    [Fact]
    public void ExtraInfoSetCorrectly()
    {
        HealthReport report = new HealthReport();
        report.metadata.Add("header", "header-data");
        TestRunner tester = new TestRunner(report);
        FakeProcessRunner fakePr = new FakeProcessRunner();
        tester.pr = fakePr;
        fakePr.setParams(0, "Hello, World\n");
        tester.RunTest("echo",new ArgumentProcessor(new string []{""}));
        report = tester.getReport();
        Assert.True(report.testresults.ContainsKey("echo"));
        Assert.True(report.testresults["echo"].ContainsKey("extra-info"));
        Assert.Equal("None", tester.getReport().testresults["echo"]["extra-info"].ToString());
        fakePr.setParams(1, "{ \"some-info\": \"Information\", \"message\": \"This is a message\" }");
        tester.RunTest(testPath: "complex-test",new ArgumentProcessor(new string []{""}));
        report = tester.getReport();
        Assert.True(report.testresults.ContainsKey("complex-test"));
        Assert.True(report.testresults["complex-test"].ContainsKey("extra-info"));
        Assert.NotEqual("None", report.testresults["complex-test"]["extra-info"]);
        Dictionary<string, object> extraInfo = (Dictionary<string, object>)report.testresults["complex-test"]["extra-info"];
        Assert.True(extraInfo.ContainsKey("some-info"));
        Assert.Equal("Information", extraInfo[key: "some-info"].ToString());
        Assert.Equal("This is a message", report.testresults["complex-test"]["message"].ToString());
        Assert.False(report.testresults["complex-test"].ContainsKey("repeat-history"));
        Assert.True(report.testresults.ContainsKey("echo"));
        Assert.False(report.testresults["echo"].ContainsKey("repeat-history"));
        //Assert.Equal(report.testresults["complex-test"]["message"].ToString(), extraInfo[key: "message"].ToString());
    }


    [Fact]
    public void AllTestResultsRecorded()
    {
        HealthReport report = new HealthReport();
        report.metadata.Add("header", "header-data");
        TestRunner tester = new TestRunner(report);
        FakeProcessRunner fakePr = new FakeProcessRunner();
        tester.pr = fakePr;
        fakePr.setParams(0, "Hello, World\n");
        tester.RunTest("echo",new ArgumentProcessor(new string []{""}));
        fakePr.setParams(100, "");
        tester.RunTest("bash",new ArgumentProcessor(new string []{""}));
        report = tester.getReport();
        Assert.Equal(2, report.testresults.Count);
        Assert.True(report.testresults.ContainsKey("echo"));
        Assert.True(report.testresults.ContainsKey("bash"));
        Assert.Equal("Hello, World\n", report.testresults["echo"]["message"]);
        Assert.NotEqual("Hello, World\n", report.testresults["bash"]["message"]);
        Assert.Equal("0", report.testresults["echo"]["exit-code"].ToString());
        Assert.Equal("100", report.testresults["bash"]["exit-code"].ToString());
    }


    [Fact]
    public void MultipleRunOfSameScript()
    {
        ProcessRunner runner = new ProcessRunner();
        HealthReport report = new HealthReport();
        report.metadata.Add("header", "header-data");
        TestRunner tester = new TestRunner(report);
        FakeProcessRunner fakePr = new FakeProcessRunner();
        tester.pr = fakePr;
        for (int i = 0; i < 10; i++)
        {
            fakePr.setParams(i, "");
            if (i == 1) fakePr.setParams(i, "Special message");
            tester.RunTest("bash",new ArgumentProcessor(new string []{""}));
        }
        report = tester.getReport();
        Assert.Equal(1, report.testresults.Count);
        Assert.True(report.testresults.ContainsKey("bash"));
        Assert.True(report.testresults["bash"].ContainsKey("exit-code"));
        Assert.Equal(1, report.testresults["bash"]["exit-code"]); //0 got rewritten
        Assert.Equal("Special message", report.testresults["bash"]["message"]); //0 got rewritten
        Assert.True(report.testresults["bash"].ContainsKey("extra-info"));
        Assert.True(report.testresults["bash"].ContainsKey("repeat-history"));
        LinkedList<object> extraInfo = (LinkedList<object>)report.testresults["bash"]["repeat-history"];
        Assert.Equal(10, extraInfo.Count);
    }

}