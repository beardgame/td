using CommandLine;

namespace Bearded.TD.MasterServer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    sealed class CommandLineOptions
    {
        [Option("application_name", Default = "Bearded.TD.Master", HelpText = "The Lidgren.Network application name.")]
        public string ApplicationName { get; set; } = null!;

        [Option(Default = 24293, HelpText = "The port on which the master server runs.")]
        public int Port { get; set; }
    }
}
