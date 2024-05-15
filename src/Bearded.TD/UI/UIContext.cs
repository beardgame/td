using Bearded.TD.Content;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;

namespace Bearded.TD.UI;

// TODO: add global sounds
sealed record UIContext(Animations Animations, UIFactories Factories, ContentManager Content);
