using System;

namespace hcheck
{
    public class TestScriptGenerator : IDisposable
    {
        private string _path;
        public string Path
        {
            get
            {
                return _path;
            }
        }

        public TestScriptGenerator(string script, bool isPython = false)
        {
            _path = System.IO.Path.GetTempFileName();
            if (isPython) _path += ".py"; //extension is used to detect python scripts
            else File.WriteAllText(_path, "#!/usr/bin/env bash\n");
            File.AppendAllText(_path, script);
            ProcessRunner pr = new ProcessRunner();
            pr.RunProcess("chmod", new string[]{"+x", _path});
        }

        public void Dispose()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
    }
}