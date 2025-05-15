using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Simulation.Damage;

public sealed class DamageModifierTests
{
    public static IEnumerable<Type> GetAllDamageModifierTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.FullName?.StartsWith("DynamicProxyGenAssembly2") ?? true)
            .SelectMany(assembly => assembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IDamageModifier))))
            .Where(type => type is { IsClass: true, IsAbstract: false });
    }

    [Fact]
    public void AllDamageModifiersAreInExplicitOrder()
    {
        var orderedDamageModifiers = DamageModifiers.Order;
        var allDamageModifiers = GetAllDamageModifierTypes().ToImmutableArray();

        allDamageModifiers.Should().BeEquivalentTo(orderedDamageModifiers);
    }
}
