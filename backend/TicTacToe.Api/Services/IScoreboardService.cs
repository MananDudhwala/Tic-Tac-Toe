using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IScoreboardService
{
    Task<ScoreboardResponse> GetScoreboardAsync();
    Task<ScoreboardResponse> ResetScoreboardAsync();
    Task IncrementScoreAsync(string? winner); // null = draw, "X" or "O" = winner
    Task DecrementScoreAsync(string? winner); // Reverse a score update (for undo)
}
