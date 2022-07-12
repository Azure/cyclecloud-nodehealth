using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Xml;
using System.Collections.ObjectModel;


namespace hcheck
{
    public class ProcessRunner
    {
        //test with /bin/test. Args = 0, no args = 1, posix executables
        //init process, pass arg, check err code. 
        public string stdout { get; set; } = "";
        public string stdin { get; set; } = "";
        public string stderr { get; set; } = ""; //
        public int exitCode { get; set; } = -1;
        public string expectedPath { get; set; } = "";
        public string expectedArgs { get; set; } = "";
        public bool isSuccess = false;
        public DateTime startTime;
        public DateTime exitTime;
        public virtual void RunProcess(string filePath, string []? args = null, int timeout = 1000)
        {
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                args = (args == null) ? new string[]{} : args;
                try
                {
                    isSuccess = false;
                    //pProcess.StartInfo.EnvironmentVariables 
                    pProcess.StartInfo.FileName = filePath;
                    foreach (string arg in args)
                    {
                        pProcess.StartInfo.ArgumentList.Add(arg); //argument
                    }
                    pProcess.StartInfo.UseShellExecute = false;
                    pProcess.StartInfo.RedirectStandardOutput = true;
                    pProcess.StartInfo.RedirectStandardError = true;
                    pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                    startTime = DateTime.Now;
                    pProcess.Start();
                    bool finished = pProcess.WaitForExit(timeout);
                    exitTime = pProcess.ExitTime; 
                    if (finished)
                    {
                        stdout = pProcess.StandardOutput.ReadToEnd(); //The output result
                        stderr = pProcess.StandardError.ReadToEnd();
                        exitCode = pProcess.ExitCode;
                        isSuccess = true;
                        stderr = "";
                    }
                    else
                    {
                        stderr = "Script " + filePath + " timed out.";
                        try
                        {
                            pProcess.Kill(true);
                            pProcess.WaitForExit(timeout);
                        }
                        catch (Exception ex)
                        {
                            stderr += " " + ex.Message;
                        }
                        exitCode = pProcess.ExitCode;
                        exitTime = startTime; 
                    }
                }
                catch (Exception ex)
                {
                    stderr = ex.Message;
                    exitCode = 1;
                    exitTime = startTime;
                }
            }
        }
    }
}
