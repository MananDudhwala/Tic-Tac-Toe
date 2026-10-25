using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    Task<GameResponse> CreateGameAsync(string gameMode);
    Task<GameResponse> GetGameAsync(Guid gameId);
    Task<GameResponse> MakeMoveAsync(Guid gameId, MoveRequest request);
    Task<GameResponse> UndoMoveAsync(Guid gameId);
    Task<GameResponse> ResetGameAsync(Guid gameId);
    Task<List<GameSummaryResponse>> GetPastGamesAsync();
}
