namespace Bearded.TD.Commands
{
    interface IRequest
    {
        bool CheckPreconditions();

        ICommand ToCommand();
    }
}