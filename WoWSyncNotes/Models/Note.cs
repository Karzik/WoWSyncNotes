namespace WoWSyncNotes.Models;

public class Note(string player, string detail, Rating rating)
{
    public string Player { get; set; } = player;
    public string Detail { get; set; } = detail;
    public Rating Rating { get; set; } = rating;    
}
