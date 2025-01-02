using Newtonsoft.Json;

using NLog;

using System.Diagnostics;

using WoWSyncNotes.Common.AssemblyInfo;
using WoWSyncNotes.Common.Logging;
using WoWSyncNotes.Common.LSONHelper;
using WoWSyncNotes.Common.Parsing;
using WoWSyncNotes.Models;

namespace WoWSyncNotes;

internal class Program
{
    // Constants - Arg

    public const string ArgPrefixShort = "-";
    public const string ArgPrefixLong = "--";
    public const string ArgDelimiter = ":";

    // Constants - Arg - Required

    public const string ArgAccount = "a";
    public const string ArgAccountHelp = $"{ArgPrefixShort}{ArgAccount}{ArgDelimiter}<path>";

    // Constants - Arg - Optional

    public const string ArgConfirm = "confirm";
    public const string ArgConfirmHelp = $"{ArgPrefixLong}{ArgConfirm}";

    public const string ArgSimulation = "simulation";
    public const string ArgSimulationHelp = $"{ArgPrefixLong}{ArgSimulation}";

    public const string ArgDebug = "debug";
    public const string ArgDebugHelp = $"{ArgPrefixLong}{ArgDebug}";

    public const string ArgNoLogo = "nologo";
    public const string ArgNoLogoHelp = $"{ArgPrefixLong}{ArgNoLogo}";

    public const string ArgHelpShort = "h";
    public const string ArgHelpShortHelp = $"{ArgPrefixShort}{ArgHelpShort}";

    public const string ArgHelpLong = "help";
    public const string ArgHelpLongHelp = $"{ArgPrefixLong}{ArgHelpLong}";

    // Constants - LSON

    const string LSONRoot = "CharacterNotesDB";
    const string LSONRealm = "realm";
    const string LSONNotes = "notes";
    const string LSONRatings = "ratings";
    const string LSONDelete = "[Delete]";

    // Variables

    static readonly AppInfo AppInfo = AppInfo.Instance;
    static readonly Logger log = LogManager.GetCurrentClassLogger();

    static readonly Dictionary<string, List<string>> deletes = [];
    static readonly Dictionary<string, Dictionary<string, List<Note>>> notes = [];
    static readonly Dictionary<string, Dictionary<string, List<Note>>> conflicts = [];

    // Methods

    static void InitLogging(Options options)
    {
        NLogUtil.Configure("NLog.config", "WoWSyncNotes-LogPath", AppDomain.CurrentDomain.BaseDirectory);
        
        if (options.Debug)
        {
            NLogUtil.SetLevel("ColoredConsole", LogLevel.Trace);
        }

        log.Debug(new string('=', 80));
        log.Debug($"{AppInfo.Title} v{AppInfo.VersionMajorMinorBuild}");
        
        if (Debugger.IsAttached || options.Debug)
        {
            string json = JsonConvert.SerializeObject(options, Formatting.Indented);
            List<string> list = [.. json.Split("\n")];
            list.ForEach(s => log.Debug(s));
            log.Debug(new string('-', 80));
        }
    }

    static void ParseCommandLine(string[] args, out Options options)
    {
        options = new();

        CommandLine arg = new(args, ArgPrefixShort, ArgDelimiter);

        bool showHeader = true;
        bool showHelp = false;
        bool invalidParameter = false;
        string? errorMessage = null;

        foreach (string argName in arg.Items.Keys)
        {
            // Help

            if (argName == ArgHelpShort || argName == ArgHelpLong)
            {
                showHelp = true;
                continue;
            }

            // Account Path

            if (argName == ArgAccount)
            {
                foreach (string? argValue in arg.Items[argName])
                {
                    if (string.IsNullOrEmpty(argValue))
                    {
                        errorMessage = "Invalid path specified.";
                        invalidParameter = true;
                        break;
                    }
                    else if (!Path.Exists(argValue))
                    {
                        errorMessage = $"Path '{argValue}' does not exist.";
                        invalidParameter = true;
                        break;
                    }
                    else
                    {
                        options.AccountPaths.Add(argValue);
                        continue;
                    }
                }
            }

            // Confirm

            if (argName == ArgConfirm)
            {
                options.Confirm = true;
                continue;
            }

            // Debug

            if (argName == ArgDebug)
            {
                options.Debug = true;
                continue;
            }

            // NoLogo

            if (argName == ArgNoLogo)
            {
                showHeader = false;
                continue;
            }

            // Simulation

            if (argName == ArgSimulation)
            {
                options.Simulation = true;
                continue;
            }
        }

        // Show Header

        if (showHeader)
        {
            ShowHeader();
        }

        // Show Help

        if (showHelp)
        {
            if (!showHeader) ShowHeader();
            ShowHelp();
            Environment.Exit(1);
        }

        // Validation

        if (options.AccountPaths.Count <= 1)
        {
            errorMessage = "You must specify at least two account/name paths.";
            invalidParameter = true;
        }

        // Invalid Parameters

        if (invalidParameter)
        {
            Console.WriteLine(errorMessage);
            Environment.Exit(1);
        }
    }

    static void ProcessRealm(string name, LsonDict lsonRealmData, out Realm realm)
    {
        realm = new(name);

        if (!lsonRealmData.ContainsKey(LSONNotes))
        {
            log.Warn($"        Realm does not contain a '{LSONNotes}' section.");
            return;
        }

        LsonDict lsonRealmNotes = (LsonDict)lsonRealmData[LSONNotes];
        LsonDict? lsonRealmRatings = lsonRealmData.ContainsKey(LSONRatings) ? null : (LsonDict)lsonRealmData[LSONRatings];

        foreach (string player in lsonRealmNotes.Keys.Select(v => (string)v))
        {
            log.Info($"        Player : {player}");

            string detail = (string)lsonRealmNotes[player];
            Rating rating = Rating.NotFound;

            if (lsonRealmRatings != null && lsonRealmRatings.ContainsKey(player))
            {
                rating = Enum.Parse<Rating>((string)lsonRealmRatings[player]);
            }

            Note note = new(player, detail, rating);
            realm.Notes.Add(note);
        }
    }

    static void Processing(Options options)
    {
        // Account Paths

        foreach (string accountPath in options.AccountPaths)
        {
            string fileName = string.Concat(
                accountPath,
                accountPath.EndsWith(Path.DirectorySeparatorChar) ? "" : Path.DirectorySeparatorChar,
                @"SavedVariables\CharacterNotes.lua"
            );

            log.Info($"Account : {accountPath}");

            // Load

            string fileData = File.ReadAllText(fileName);
            log.Info($"    Bytes Read : {fileData.Length}");

            // Parse

            fileData = fileData.Trim('\r', '\n');
            Dictionary<string, LsonValue> lsonParsed = LSONVars.Parse(fileData);

            if (!lsonParsed.Any(x => x.Key == LSONRoot))
            {
                log.Warn($"    File does not contain a '{LSONRoot}' section.");
                continue;
            }

            LsonDict lsonRoot = (LsonDict)lsonParsed[LSONRoot];
            LsonDict lsonRealms = (LsonDict)lsonRoot[LSONRealm];

            // Realms

            foreach (string realmName in lsonRealms.Keys.Select(v => (string)v))
            {
                log.Info($"    Realm : {realmName}");

                LsonDict lsonRealmData = (LsonDict)lsonRealms[realmName];

                ProcessRealm(realmName, lsonRealmData, out Realm realm);
            }
        }
    }

    static void ShowHeader()
    {
        Console.WriteLine($"{AppInfo.Title} Version {AppInfo.VersionMajorMinorBuild}");
        Console.WriteLine($"{AppInfo.Description}");
        Console.WriteLine($"{AppInfo.Copyright}");
        Console.WriteLine();
    }
    static void ShowHelp()
    {
        if (Debugger.IsAttached)
        {
            Console.WriteLine("---------1---------2---------3---------4---------5---------6---------7---------");
            Console.WriteLine();
        }

        Console.WriteLine($"Usage: {AppInfo.Title} [Options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine($"  {ArgAccountHelp,-20} Path to the <account> to synchronize.");
        Console.WriteLine();
        Console.WriteLine($"  {ArgConfirmHelp, -20} Bypass confirmation prompt.");
        Console.WriteLine();
        Console.WriteLine($"  {ArgDebugHelp,-20} Display debug messages.");
        Console.WriteLine($"  {ArgSimulationHelp,-20} Simulation mode; no destructive operations performed.");
        Console.WriteLine($"  {ArgNoLogoHelp,-20} Do not display the program/version information.");
        Console.WriteLine();
        Console.WriteLine($"  {(string.Concat(ArgHelpShortHelp, ", ", ArgHelpLongHelp)),-20} Display this help information and exit.");
        Console.WriteLine();
    }

    // Main

    static void Main(string[] args)
    {
        if (Debugger.IsAttached)
        {
            args = [
                @$"{ArgPrefixShort}{ArgAccount}{ArgDelimiter}C:\Program Files (x86)\World of Warcraft\_classic_era_\WTF\Account\APATTI",
                @$"{ArgPrefixShort}{ArgAccount}{ArgDelimiter}C:\Program Files (x86)\World of Warcraft\_classic_era_\WTF\Account\DPATTI",
                $"{ArgPrefixLong}{ArgConfirm}",
                $"{ArgPrefixLong}{ArgSimulation}",
                $"{ArgPrefixLong}{ArgDebug}",
                //$"{ArgPrefixLong}{ArgNoLogo}",
                //$"{ArgPrefixShort}{ArgHelpShort}",
                //$"{ArgPrefixLong}{ArgHelpLong}"
            ];
        }

        try
        {
            ParseCommandLine(args, out Options options);
            InitLogging(options);
            Processing(options);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Environment.Exit(255);
        }
    }
}
