namespace Bearded.TD.Shared.TechEffects
{
    public interface IParametersTemplate
    {
        bool HasAttributeOfType(AttributeType type);
        bool ModifyAttribute(AttributeType type, Modification modification);
    }

    public interface IParametersTemplate<out T> : IParametersTemplate where T : IParametersTemplate<T>
    {
        T CreateModifiableInstance();
    }
}
