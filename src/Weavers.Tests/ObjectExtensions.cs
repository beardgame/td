namespace Weavers.Tests
{
    static class ObjectExtensions
    {
        public static T GetPropertyValue<T>(this object obj, string propertyName)
            => (T)obj.GetType().GetProperty(propertyName)?.GetValue(obj, null); 
    }
}
