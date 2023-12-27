using Bearded.TD.Shared.Annotations;

namespace Bearded.TD.Commands;

interface ICommand
{
    [IsSync]
    void Execute();
}
