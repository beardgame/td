namespace Bearded.TD.Generators
{
    sealed class NameFactory
    {
        public static NameFactory FromInterfaceName(string interfaceName)
        {
            if (interfaceName.Length <= 1)
            {
                return new NameFactory(interfaceName);
            }

            return interfaceName[0] == 'I' && isUpperCase(interfaceName[1])
                ? new NameFactory(interfaceName[1..])
                : new NameFactory(interfaceName);
        }

        private readonly string baseName;

        private NameFactory(string baseName)
        {
            this.baseName = baseName;
        }

        public string ClassName() => baseName;

        public string ClassNameWithSuffix(string suffix) => baseName + suffix;

        public string FieldName() => baseName[..1].ToLowerInvariant() + baseName[1..];

        private static bool isUpperCase(char symbol) => symbol is >= 'A' and <= 'Z';
    }
}
