using System.ComponentModel.DataAnnotations;

namespace TicTacToe.Api.Models;

public class Scoreboard
{
    [Key]
    public int Id { get; set; } = 1; // Singleton row

    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}
