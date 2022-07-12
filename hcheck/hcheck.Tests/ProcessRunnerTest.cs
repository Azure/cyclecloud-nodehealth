using System;
using hcheck;
namespace test;

public class ProcessRunnerTest
{
    [Fact]
    public void StdoutSetCorrectly()
    {
        ProcessRunner runner = new ProcessRunner();
        runner.RunProcess("echo", new string[] { "Hello, World" });
        Assert.Equal("Hello, World\n", runner.stdout);
    }

    [Fact]
    public void ExitCodeSetCorrectly()
    {
        ProcessRunner runner = new ProcessRunner();
        runner.RunProcess("echo", new string[] { "Hello, World" });
        Assert.Equal(0, runner.exitCode);
        runner.RunProcess("bash -c \"exit 100\"");
        Assert.Equal(1, runner.exitCode);
        runner.RunProcess("bash", new string[] { "-c", "exit 100" });
        Assert.Equal(100, runner.exitCode);
        runner.RunProcess("bash", new string[] { "ddddddd" });
        Assert.Equal(127, runner.exitCode);
    }

    [Fact]
    public void stdErrAndSuccessSetCorrectly()
    {
        ProcessRunner runner = new ProcessRunner();
        runner.RunProcess("bash", new string[] { "-c", "exit 100" });
        Assert.Equal("", runner.stderr);
        runner.RunProcess("notvalidatall", new string[] { "-c", "exit 100" });
        Assert.NotEqual("", runner.stderr);
        Assert.False(runner.isSuccess);
        runner.RunProcess(filePath: "echo", new string[] { "-t", "2", "some error" });
        Assert.NotEqual("some error", runner.stderr);
    }

    [Fact]
    public void handleIncorrectScripts()
    {
        ProcessRunner runner = new ProcessRunner();
        runner.RunProcess("../scripts/nonscript.sh");
        Assert.False(runner.isSuccess);
    }
}