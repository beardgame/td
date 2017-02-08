namespace Bearded.TD.Console
{
    internal sealed class CommandParameters
    {
        public string[] Args { get; private set; }

        public CommandParameters(string[] args)
        {
            this.Args = args;
        }
    }
}
