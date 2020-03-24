namespace Bearded.TD.Game.Events
{
    struct ModFilesFailedLoading : IGlobalEvent
    {
        public int NumBlueprintsWithErrors { get; }

        public ModFilesFailedLoading(int numBlueprintsWithErrors)
        {
            NumBlueprintsWithErrors = numBlueprintsWithErrors;
        }
    }
}
