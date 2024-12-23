using System.Diagnostics;

using WoWSyncNotes.Common.AssemblyInfo;
using WoWSyncNotes.Common.Parsing;
using WoWSyncNotes.Models;

namespace WoWSyncNotes;

internal class Program
{
    // Variables

    static AppInfo AppInfo = AppInfo.Instance;

    // Methods

    static bool ParseCommandLine(string[] args, out Startup startup)
    {
        startup = new();

        CommandLine arg = new(args, "-", ":");

        bool showHeader = true;
        bool showHelp = false;
        bool invalidParameter = false;
        string? errorMessage = null;

        foreach (string argName in arg.Items.Keys)
        {
            // Help

            if (argName == "h" || argName == "help")
            {
                showHelp = true;
                continue;
            }

            // NoLogo

            if (argName == "nologo")
            {
                showHeader = false;
                continue;
            }

            // Account Path

            if (argName == "p")
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
                        startup.AccountPaths.Add(argName);
                        continue;
                    }
                }
            }

            // Simulation

            if (argName == "simulation")
            {
                startup.Simulation = true;
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
            ShowHelp();
            return false;
        }

        // Validation

        if (startup.AccountPaths.Count <= 1)
        {
            errorMessage = "You must specify at least two account/name paths.";
            invalidParameter = true;
        }

        // Invalid Parameters

        if (invalidParameter)
        {
            Console.WriteLine(errorMessage);
            return false;
        }

        return true;
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
        }

        Console.WriteLine($"Usage: {AppInfo.Title} [Options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -p:<account>[,<name>]  Path to the <account> and optional <name> to sync.");
        Console.WriteLine();
        Console.WriteLine("  --simulation           Simulation mode; no destructive operations performed.");
        Console.WriteLine("  --nologo               Do not display the program/version information.");
        Console.WriteLine();
        Console.WriteLine("  -h, --help             Displays this help information and exits.");
        Console.WriteLine();      
    }

    // Main

    static void Main(string[] args)
    {
        try
        {
            if (!ParseCommandLine(args, out Startup startup))
            {
                Environment.Exit(1);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Environment.Exit(255);
        }
    }
}
