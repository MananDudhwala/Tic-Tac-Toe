using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Data;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class ScoreboardService : IScoreboardService
{
    private readonly GameDbContext _db;

    public ScoreboardService(GameDbContext db)
    {
        _db = db;
    }

    public async Task<ScoreboardResponse> GetScoreboardAsync()
    {
        var scoreboard = await GetOrCreateScoreboardAsync();
        return new ScoreboardResponse
        {
            XWins = scoreboard.XWins,
            OWins = scoreboard.OWins,
            Draws = scoreboard.Draws
        };
    }

    public async Task<ScoreboardResponse> ResetScoreboardAsync()
    {
        var scoreboard = await GetOrCreateScoreboardAsync();
        scoreboard.XWins = 0;
        scoreboard.OWins = 0;
        scoreboard.Draws = 0;
        await _db.SaveChangesAsync();
        return new ScoreboardResponse { XWins = 0, OWins = 0, Draws = 0 };
    }

    public async Task IncrementScoreAsync(string? winner)
    {
        var scoreboard = await GetOrCreateScoreboardAsync();
        if (winner == "X") scoreboard.XWins++;
        else if (winner == "O") scoreboard.OWins++;
        else scoreboard.Draws++;
        await _db.SaveChangesAsync();
    }

    public async Task DecrementScoreAsync(string? winner)
    {
        var scoreboard = await GetOrCreateScoreboardAsync();
        if (winner == "X") scoreboard.XWins = Math.Max(0, scoreboard.XWins - 1);
        else if (winner == "O") scoreboard.OWins = Math.Max(0, scoreboard.OWins - 1);
        else scoreboard.Draws = Math.Max(0, scoreboard.Draws - 1);
        await _db.SaveChangesAsync();
    }

    private async Task<Scoreboard> GetOrCreateScoreboardAsync()
    {
        var scoreboard = await _db.Scoreboards.FindAsync(1);
        if (scoreboard == null)
        {
            scoreboard = new Scoreboard { Id = 1 };
            _db.Scoreboards.Add(scoreboard);
            await _db.SaveChangesAsync();
        }
        return scoreboard;
    }
}
