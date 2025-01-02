namespace WoWSyncNotes.Models;

public class Options
{
    public List<string> AccountPaths { get; set; } = [];

    public bool Confirm { get; set; } = false;
    public bool Debug { get; set; } = false;
    public bool NoLogo { get; set; } = false;
    public bool Simulation { get; set; } = false;
}
