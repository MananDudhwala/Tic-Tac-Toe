namespace TicTacToe.Api.Models;

public class CreateGameRequest
{
    public string GameMode { get; set; } = "TwoPlayer"; // "TwoPlayer" or "Computer"
}

public class MoveRequest
{
    public string Player { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Col { get; set; }
}

public class GameResponse
{
    public Guid Id { get; set; }
    public string[][] Board { get; set; } = new string[3][];
    public string CurrentPlayer { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Winner { get; set; }
    public int[][]? WinningCells { get; set; }
    public List<MoveResponse> MoveHistory { get; set; } = new();
    public ScoreboardResponse Scoreboard { get; set; } = new();
}

public class MoveResponse
{
    public int MoveNumber { get; set; }
    public string Player { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Col { get; set; }
}

public class ScoreboardResponse
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}

public class GameSummaryResponse
{
    public Guid Id { get; set; }
    public string GameMode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Winner { get; set; }
    public int MoveCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}
