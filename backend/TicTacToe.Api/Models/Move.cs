using System.ComponentModel.DataAnnotations;

namespace TicTacToe.Api.Models;

public class Move
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid GameId { get; set; }

    [Required]
    public int MoveNumber { get; set; }

    [Required]
    public string Player { get; set; } = string.Empty;

    [Required]
    public int Row { get; set; }

    [Required]
    public int Col { get; set; }

    // Navigation property
    public Game Game { get; set; } = null!;
}
