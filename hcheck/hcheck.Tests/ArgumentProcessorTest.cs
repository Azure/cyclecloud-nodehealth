using System;
using hcheck;
namespace test;

public class ArgumentProcessorTest
{
    [Fact]
    public void PathTestFinAppendSetCorrectly()
    {
        string[] args = { "-k", "path", "--fin", "--append" };
        ArgumentProcessor proc = new ArgumentProcessor(args);
        Assert.True(proc.IsAppend);
        Assert.True(proc.IsFinal);
        Assert.True(proc.IsPath);
        Assert.Equal("path", proc.TestDir);
    }

    [Fact]
    public void nrRpathSetCorrectly()
    {
        string[] args = { "--nr", "3", "--rpath", "path" };
        ArgumentProcessor proc = new ArgumentProcessor(args);
        Assert.Equal(3, proc.NumRuns);
        Assert.Equal("path", proc.FilePath);
    }

     [Fact]
    public void AppInsSetCorrectly()
    {
        string[] args = {"--appin", "instrumentationKey" };
        ArgumentProcessor proc = new ArgumentProcessor(args);
        Assert.Equal("instrumentationKey", proc.InstrumentationKey);
    }
}