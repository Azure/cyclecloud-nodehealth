using System;
using System.Collections.ObjectModel; 

 namespace  hcheck
{
    public class FakeProcessRunner : ProcessRunner
    {

        public void setParams(int exitCode, string message, bool isExecutable = true)
        {
            this.exitCode = exitCode;
            this.stdout = message;
            this.isSuccess = isExecutable;
        }

        public override void RunProcess(string filepath, string []? args, int timeout = 1000)
        {
            return;
        }

    }
}