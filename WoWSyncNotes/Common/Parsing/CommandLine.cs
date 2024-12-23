using System.Text.RegularExpressions;

using WoWSyncNotes.Common.Exceptions;

namespace WoWSyncNotes.Common.Parsing;

public partial class CommandLine
{
    // Variables

    [GeneratedRegex("^['\"]?(.*?)['\"]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex RegexRemoveQuotes();

    // Properties

    public Dictionary<string, List<string?>> Items { get; set; } = [];

    // Constructors

    public CommandLine(string[] args, string argPrefix, string argValueDelimiter) => Parse(args, argPrefix, argValueDelimiter);
    public CommandLine(string[] args) : this(args, "-", ":") { }

    // Methods

    public void Parse(string[] args, string argPrefix = "-", string argValueDelimiter = ":")
    {
        Items = [];

        Regex splitter = new($"^{argPrefix}{{1,2}}|{argValueDelimiter}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        for (int i = 0; i < args.Length; i++)
        {
            string[] parsed = splitter.Split(args[i], 3);
            string? name = parsed[1];
            string? value = null;

            if (string.IsNullOrEmpty(name))
            {
                throw new CriticalException("Unexpected code block reached.");
            }

            if (parsed.Length == 2)
            {
                // Parameter Only
                // If there are more arguments to process, and the next one
                // is not a parameter, it is the value for this one.

                if (i < args.Length - 1)
                {
                    string next = args[i + 1];
                    if (!next.StartsWith(argPrefix))
                    {
                        value = next;
                        i++;
                    }
                }
            }
            else if (parsed.Length == 3)
            {
                // Parameter and Value
                value = parsed[2];
            }
            else
            {
                throw new CriticalException("Unexpected code block reached.");
            }

            // Remove quotes from the value.
            if (!string.IsNullOrEmpty(value))
            {
                value = RegexRemoveQuotes().Replace(value, "$1");
            }

            // Add
            if (Items.TryGetValue(name, out List<string?>? _))
            {
                Items[name].Add(value);
            }
            else
            {
                Items.Add(name, [value]);
            }
        }
    }
}
