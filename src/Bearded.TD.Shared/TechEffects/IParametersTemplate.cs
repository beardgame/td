namespace Bearded.TD.Shared.TechEffects
{
    public interface IParametersTemplate<out T> where T : IParametersTemplate<T>
    {
        bool HasAttributeOfType(AttributeType type);
        bool ModifyAttribute(AttributeType type, Modification modification);
        T CreateModifiableInstance();
    }
}
