using System;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Testing.Components;
using Bearded.TD.Testing.Factions;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using FluentAssertions;
using Xunit;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Tests.Game.GameObjects;

public sealed class UpgradeTests
{
    private const AttributeType upgradeAttribute = AttributeType.Damage;

    private readonly GameObject building;
    private readonly GameObject weapon;
    private readonly ModifiableComponent buildingModifiable;
    private readonly ModifiableComponent weaponModifiable;

    public UpgradeTests()
    {
        buildingModifiable = createModifiableComponent();
        weaponModifiable = createModifiableComponent();

        var weaponBlueprint = GameObjectBlueprintFactory.WithPremadeComponents(weaponModifiable);
        var turret = new Turret(
            new TurretParametersTemplate(
                weaponBlueprint, Difference2.Zero, null, Direction2.Zero, null));

        var buildingBlueprint = GameObjectBlueprintFactory.WithPremadeComponents(turret, buildingModifiable);
        building = BuildingFactory.Create(
            Id<GameObject>.Invalid,
            buildingBlueprint,
            FactionTestFactory.CreateFaction(),
            new PositionedFootprint());

        weapon = turret.Weapon;
    }

    [Fact]
    public void UpgradeAppliesToBuilding()
    {
        var upgrade = createAttributeUpgrade();
        building.ApplyUpgrade(upgrade);

        buildingModifiable.AttributeValue.Should().BeApproximately(2, 0.1);
    }

    [Fact]
    public void UpgradeAppliesToWeapon()
    {
        var upgrade = createAttributeUpgrade();
        building.ApplyUpgrade(upgrade);

        weaponModifiable.AttributeValue.Should().BeApproximately(2, 0.1);
    }

    [Fact]
    public void UpgradeRollbackAppliesToBuilding()
    {
        var upgrade = createAttributeUpgrade();
        var receipt = building.ApplyUpgrade(upgrade);
        receipt.Rollback();

        buildingModifiable.AttributeValue.Should().BeApproximately(1, 0.1);
    }

    [Fact]
    public void UpgradeRollbackAppliesToWeapon()
    {
        var upgrade = createAttributeUpgrade();
        var receipt = building.ApplyUpgrade(upgrade);
        receipt.Rollback();

        weaponModifiable.AttributeValue.Should().BeApproximately(1, 0.1);
    }

    [Fact]
    public void UpgradeAppliesToBuildingComponentAddedLater()
    {
        var upgrade = createAttributeUpgrade();
        building.ApplyUpgrade(upgrade);

        var newModifiable = createModifiableComponent();
        building.AddComponent(newModifiable);

        newModifiable.AttributeValue.Should().BeApproximately(2, 0.1);
    }

    [Fact]
    public void UpgradeAppliesToWeaponComponentAddedLater()
    {
        var upgrade = createAttributeUpgrade();
        building.ApplyUpgrade(upgrade);

        var newModifiable = createModifiableComponent();
        weapon.AddComponent(newModifiable);

        newModifiable.AttributeValue.Should().BeApproximately(2, 0.1);
    }

    [Fact]
    public void UpgradeRollbackAppliesToBuildingComponentAddedLater()
    {
        var upgrade = createAttributeUpgrade();
        var receipt = building.ApplyUpgrade(upgrade);

        var newModifiable = createModifiableComponent();
        building.AddComponent(newModifiable);

        receipt.Rollback();

        newModifiable.AttributeValue.Should().BeApproximately(1, 0.1);
    }

    [Fact]
    public void UpgradeRollbackAppliesToWeaponComponentAddedLater()
    {
        var upgrade = createAttributeUpgrade();
        var receipt = building.ApplyUpgrade(upgrade);

        var newModifiable = createModifiableComponent();
        weapon.AddComponent(newModifiable);

        receipt.Rollback();

        newModifiable.AttributeValue.Should().BeApproximately(1, 0.1);
    }

    [Fact]
    public void RolledBackUpgradeDoesNotApplyToBuildingComponentAddedLater()
    {
        var upgrade = createAttributeUpgrade();
        var receipt = building.ApplyUpgrade(upgrade);

        receipt.Rollback();

        var newModifiable = createModifiableComponent();
        building.AddComponent(newModifiable);

        newModifiable.AttributeValue.Should().BeApproximately(1, 0.1);
    }

    [Fact]
    public void RolledBackUpgradeDoesNotApplyToWeaponComponentAddedLater()
    {
        var upgrade = createAttributeUpgrade();
        var receipt = building.ApplyUpgrade(upgrade);

        receipt.Rollback();

        var newModifiable = createModifiableComponent();
        weapon.AddComponent(newModifiable);

        newModifiable.AttributeValue.Should().BeApproximately(1, 0.1);
    }

    [Fact]
    public void ComponentThatIsRemovedAndAddedDoesNotReceiveSameUpgradeTwice()
    {
        var upgrade = createAttributeUpgrade();
        building.ApplyUpgrade(upgrade);

        building.RemoveComponent(buildingModifiable);
        building.AddComponent(buildingModifiable);

        buildingModifiable.AttributeValue.Should().BeApproximately(2, 0.1);
    }

    private static ModifiableComponent createModifiableComponent()
    {
        return new ModifiableComponent(new ModifiableComponent.ParametersImplementation());
    }

    private static IUpgrade createAttributeUpgrade(double factor = 2)
    {
        return Upgrade.FromEffects(
            new ModifyParameter(
                upgradeAttribute,
                Modification.MultiplyWith(factor),
                UpgradePrerequisites.Empty,
                false));
    }

    [ComponentForTesting]
    private sealed class ModifiableComponent : Component<ModifiableComponent.IParameters>
    {
        public interface IParameters : IParametersTemplate<IParameters>
        {
            double Attribute { get; }
        }

        public sealed class ParametersImplementation : ModifiableBase<ParametersImplementation>, IParameters
        {
            private readonly AttributeWithModifications<double> attribute = new(1, a => a);
            public double Attribute => attribute.Value;

            static ParametersImplementation()
            {
                InitializeAttributes(
                    new (AttributeType Type, Func<ParametersImplementation, IAttributeWithModifications> Getter)[]
                    {
                        (upgradeAttribute, instance => instance.attribute)
                    }.ToLookup(tuple => tuple.Type, tuple => tuple.Getter));
            }

            public IParameters CreateModifiableInstance() => new ParametersImplementation();
        }

        public double AttributeValue => Parameters.Attribute;

        public ModifiableComponent(IParameters parameters) : base(parameters) { }

        protected override void OnAdded() { }
        public override void Update(TimeSpan elapsedTime) { }
    }
}
