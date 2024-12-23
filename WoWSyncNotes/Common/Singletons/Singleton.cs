using NLog;

namespace WoWSyncNotes.Common.Singletons;

public abstract class Singleton<T> where T : class, new()
{
    // Variables

    private static readonly Lazy<T> _instance = new(() => new());
    protected static readonly Logger log = LogManager.GetCurrentClassLogger();

    // Properties

    public static T Instance => _instance.Value;
}
