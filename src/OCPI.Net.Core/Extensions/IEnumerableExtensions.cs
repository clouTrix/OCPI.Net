namespace OCPI;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> Do<T>(this IEnumerable<T> self, Action<T> sideEffect)
        => self.Select(e =>
                    {
                        sideEffect(e);
                        return e;
                    });
    
    public static void ForEach<T>(this IEnumerable<T> self, Action<T> sideEffect)
    {
        foreach (var e in self)
        {
            sideEffect(e);
        }
    }
}