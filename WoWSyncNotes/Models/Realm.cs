namespace WoWSyncNotes.Models;

public class Realm(string name)
{
    // Properties

    public string Name { get; set; } = name;
    public List<Note> Notes { get; set; } = [];
}
