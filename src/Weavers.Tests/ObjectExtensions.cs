namespace Weavers.Tests
{
    static class ObjectExtensions
    {
        public static T GetPropertyValue<T>(this object obj, string propertyName)
            => (T)obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);

        public static object CallMethod(this object obj, string methodName, params object[] parameters)
            => obj.GetType().GetMethod(methodName)?.Invoke(obj, parameters);
    }
}
