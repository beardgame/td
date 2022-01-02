using System.Runtime.CompilerServices;

// TODO: ideally we can just mark the exported classes public, but this propagates all the way to Bearded.TD
[assembly:InternalsVisibleTo("Bearded.TD.Tests")]
