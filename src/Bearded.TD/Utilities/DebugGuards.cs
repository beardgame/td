namespace Bearded.TD.Utilities;

static class DebugGuards
{
    public static bool IsInDebugMode => isInDebugMode();
    public static bool IsInReleaseMode => !isInDebugMode();
        
    private static bool isInDebugMode()
    {
#if DEBUG
        return true;
#else
            return false;
#endif
    }
}