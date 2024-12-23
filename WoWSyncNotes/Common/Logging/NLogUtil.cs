using Newtonsoft.Json;

using NLog;
using NLog.Config;

using System.Reflection;

using WoWSyncNotes.Common.Exceptions;

namespace WoWSyncNotes.Common.Logging;

public class NLogUtil
{
    // Variables

    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    // Properties

    public static Assembly AssemblyObject { get; set; } = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

    // Methods - Private

    private static bool ValidateRuleExists(string ruleName, bool throwException = true)
    {
        if (LogManager.Configuration.FindRuleByName(ruleName) == null)
        {
            if (throwException)
            {
                throw new CriticalException($"Rule name '{ruleName}' does not exist.");
            }

            return false;
        }

        return true;
    }

    // Methods - Public

    public static void Configure(string resourceName, string envLogPath, string? logPath = null)
    {
        // Initialize

        if (string.IsNullOrEmpty(logPath))
        {
            logPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        Environment.SetEnvironmentVariable(envLogPath, logPath);

        // Load

        string[] resourceNames = AssemblyObject.GetManifestResourceNames();
        List<string> nLogResources = resourceNames.Where(x => x.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (nLogResources.Count == 0)
        {
            throw new CriticalException("Logging configuration resource not found.");
        }
        string nLogConfig = nLogResources[0];

        using Stream? stream = AssemblyObject.GetManifestResourceStream(nLogConfig)
            ?? throw new CriticalException("Unable to retrieve the logging configuration stream.");
        using StreamReader sr = new(stream);
        string xml = sr.ReadToEnd();

        // Apply

        LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(xml);
        LogManager.ReconfigExistingLoggers();
    }

    public static void SetLevel(string ruleName, LogLevel minLevel, LogLevel maxLevel, bool reconfigure = true)
    {
        _ = ValidateRuleExists(ruleName);
        LoggingRule rule = LogManager.Configuration.FindRuleByName(ruleName);
        rule.SetLoggingLevels(minLevel, maxLevel);
        if (reconfigure)
        {
            LogManager.ReconfigExistingLoggers();
        }
    }
    public static void SetLevel(string ruleName, LogLevel minLevel, bool reconfigure = true) =>
        SetLevel(ruleName, minLevel, LogLevel.Fatal, reconfigure)
    ;

    public static void LogJSON(string? header, LogLevel level, object obj)
    {
        string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        List<string> list = [.. json.Split(Environment.NewLine)];

        if (string.IsNullOrEmpty(header))
        {
            header = $"{obj.GetType().Name}:";
        }

        log.Log(level, header);

        list.ForEach(s => log.Log(level, s));
    }
    public static void LogJSON(LogLevel level, object obj) => LogJSON(null, level, obj);

    public static void LogJSONTrace(string? header, object obj) => LogJSON(header, LogLevel.Trace, obj);
    public static void LogJSONDebug(string? header, object obj) => LogJSON(header, LogLevel.Debug, obj);
    public static void LogJSONInfo(string? header, object obj) => LogJSON(header, LogLevel.Info, obj);
    public static void LogJSONWarn(string? header, object obj) => LogJSON(header, LogLevel.Trace, obj);
    public static void LogJSONError(string? header, object obj) => LogJSON(header, LogLevel.Error, obj);
    public static void LogJSONFatal(string? header, object obj) => LogJSON(header, LogLevel.Fatal, obj);

    public static void LogJSONTrace(object obj) => LogJSON(LogLevel.Trace, obj);
    public static void LogJSONDebug(object obj) => LogJSON(LogLevel.Debug, obj);
    public static void LogJSONInfo(object obj) => LogJSON(LogLevel.Info, obj);
    public static void LogJSONWarn(object obj) => LogJSON(LogLevel.Trace, obj);
    public static void LogJSONError(object obj) => LogJSON(LogLevel.Error, obj);
    public static void LogJSONFatal(object obj) => LogJSON(LogLevel.Fatal, obj);
}
