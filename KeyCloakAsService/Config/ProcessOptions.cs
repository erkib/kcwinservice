using System.Collections.Generic;

namespace KeyCloakAsService.Config
{
    public class ProcessOptions
    {
        public const string Process = "Process";

        public string Path { get; set; } = string.Empty;

        public List<string> Args { get; set; } = [];
    }
}
