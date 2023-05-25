namespace Olsson.GET.Common.DataContracts.Models
{
    public class ModelExecutable
    {
        public int ModelID { get; set; }
        public string ExecutableName { get; set; }
        public string Arguments { get; set; }
        public int RunOrder { get; set; }
        public string WorkingDirectory { get; set; }
        public bool WrapWithBatchFile { get; set; }
        public bool UseShellExecute { get; set; }
        public bool RedirectStandardOutput { get; set; }
        public bool CreateNoWindow { get; set; }
    }
}