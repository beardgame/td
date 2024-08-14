using System;

namespace Bearded.TD.Content.Models;

interface IMeshesImplementation : IDisposable
{
    IMesh GetMesh(string key);
}
