using Microsoft.CodeAnalysis;

namespace Bearded.TD.Generators.Reports;

readonly record struct ControlToGenerateFor(INamedTypeSymbol Control, INamedTypeSymbol Report, bool RequiresGame);
