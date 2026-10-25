using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe.Api.Models;

public class Game
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Board stored as a 9-character string (e.g. "X-O------")
    /// Index mapping: 0=R0C0, 1=R0C1, 2=R0C2, 3=R1C0, ... 8=R2C2
    /// '-' = empty, 'X' = player X, 'O' = player O
    /// </summary>
    [Required]
    [StringLength(9)]
    public string Board { get; set; } = "---------";

    [Required]
    public string CurrentPlayer { get; set; } = "X";

    [Required]
    public string GameMode { get; set; } = "TwoPlayer"; // "TwoPlayer" or "Computer"

    [Required]
    public string Status { get; set; } = "InProgress"; // "InProgress", "Won", "Draw"

    public string? Winner { get; set; }

    /// <summary>
    /// Winning cells stored as comma-separated indices (e.g. "0,1,2")
    /// </summary>
    public string? WinningCells { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool CanUndo { get; set; } = false;

    public List<Move> Moves { get; set; } = new();

    // Helper methods (not mapped to DB)
    [NotMapped]
    public char[] BoardArray => Board.ToCharArray();

    public char GetCell(int row, int col) => Board[row * 3 + col];

    public void SetCell(int row, int col, char value)
    {
        var arr = Board.ToCharArray();
        arr[row * 3 + col] = value;
        Board = new string(arr);
    }

    [NotMapped]
    public int[] WinningCellsArray =>
        string.IsNullOrEmpty(WinningCells)
            ? Array.Empty<int>()
            : WinningCells.Split(',').Select(int.Parse).ToArray();
}
