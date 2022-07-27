using System;

namespace hcheck
{
    public class ArgumentProcessor
    {
        private string[]? args;
        private bool _isFinal = false;
        public bool IsFinal
        {
            get
            {
                return _isFinal;
            }
        }
        private string _reportScriptPath = "";
        public string ReportScriptPath
        {
            get
            {
                return _reportScriptPath;
            }
        }

        private bool _isPath = false;

        public bool IsPath
        {
            get
            {
                return _isPath;
            }
        }

        private bool _isPattern = false;

        public bool IsPattern
        {
            get
            {
                return _isPattern;
            }
        }
        private string _pattern = "";
        public string Pattern
        {
            get
            {
                return _pattern;
            }
        }
        private string _testDir = "";

        public string TestDir
        {
            get
            {
                return _testDir;
            }
        }
        private string _instrumentationKey = "";

        public string InstrumentationKey
        {
            get
            {
                return _instrumentationKey;
            }
        }

        private int _numRuns = 1;

        public int NumRuns
        {
            get
            {
                return _numRuns;
            }
        }

        private bool _isAppend = false;

        public bool IsAppend
        {
            get
            {
                return _isAppend;
            }
        }

        private string _filePath = "";

        public string FilePath
        {
            get
            {
                return _filePath;
            }
        }
        public ArgumentProcessor(string[]? args)
        {
            this.args = args;
            processArgs();
        }

        private void processArgs()
        {
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--fin")
                    {
                        _isFinal = true;
                    }
                    // add data to the existing report
                    else if (args[i] == "--append")
                    {
                        _isAppend = true;
                        //  filePath = args[i + 1];
                    }
                    if (i < args.Length - 1) //boundary check
                    {
                        // path to the scripts
                        if (args[i] == "-k")
                        {
                            _isPath = true;
                            _testDir = args[i + 1];
                        }
                        // path to where the report is to be saved
                        else if (args[i] == "--rpath")
                        {
                            _filePath = args[i + 1];
                        }
                        // inline arguments used with the scripts
                        /*  else if (args[i] == "--args")
                          {
                              commandArgs = args[i + 1];
                          } */
                        // pattern to detect scripts in the filePath directory
                        else if (args[i] == "--pt")
                        {
                            _pattern = args[i + 1].Trim('\"');
                            _isPattern = true;
                        }
                        // number of reruns of all custom scripts
                        else if (args[i] == "--nr")
                        {
                            if (!Int32.TryParse(args[i + 1], out _numRuns) || _numRuns < 1)
                                Console.WriteLine("Invalid number of runs provided: all tests will run once");
                        }
                        // report errors to App Insights 
                        else if (args[i] == "--appin" && args[i + 1] != "null")
                        {
                            _instrumentationKey = args[i + 1];
                        }
                        // external script reporting to CycleCloud 
                        else if (args[i] == "--rscript")
                        {
                            _reportScriptPath = args[i + 1];
                        }
                    }
                }
            }
            if (_filePath == "") _filePath = "./report.json";
        }
    }
}