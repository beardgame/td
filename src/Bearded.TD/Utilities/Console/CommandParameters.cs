namespace Bearded.TD.Utilities.Console
{
    sealed class CommandParameters
    {
        public string[] Args { get; private set; }

        public CommandParameters(string[] args)
        {
            this.Args = args;
        }
    }
}
