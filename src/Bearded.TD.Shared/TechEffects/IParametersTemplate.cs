namespace Bearded.TD.Shared.TechEffects
{
    public interface IParametersTemplate<out T> where T : IParametersTemplate<T>
    {
        T CreateModifiableInstance();
        bool ModifyAttribute(AttributeType type, Modification modification);
    }
}
