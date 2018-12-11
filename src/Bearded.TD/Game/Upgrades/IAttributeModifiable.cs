using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Upgrades
{
    interface IAttributeModifiable
    {
        bool HasAttributeOfType(AttributeType type);
        bool ModifyAttribute(AttributeType type, Modification modification);
    }
}
