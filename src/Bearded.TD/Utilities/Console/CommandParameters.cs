namespace Bearded.TD.Utilities.Console
{
    sealed class CommandParameters
    {
        public string[] Args { get; }

        public CommandParameters(string[] args)
        {
            Args = args;
        }
    }
}
