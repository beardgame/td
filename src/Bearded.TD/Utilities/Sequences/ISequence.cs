namespace Bearded.TD.Utilities.Sequences
{
    interface ISequence<out T>
    {
        T GetElementAtPosition(int index);
    }
}
