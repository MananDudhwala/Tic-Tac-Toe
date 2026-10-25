using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreboardController : ControllerBase
{
    private readonly IScoreboardService _scoreboardService;

    public ScoreboardController(IScoreboardService scoreboardService)
    {
        _scoreboardService = scoreboardService;
    }

    /// <summary>
    /// Get current scoreboard
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ScoreboardResponse>> GetScoreboard()
    {
        var response = await _scoreboardService.GetScoreboardAsync();
        return Ok(response);
    }

    /// <summary>
    /// Reset scoreboard
    /// </summary>
    [HttpPost("reset")]
    public async Task<ActionResult<ScoreboardResponse>> ResetScoreboard()
    {
        var response = await _scoreboardService.ResetScoreboardAsync();
        return Ok(response);
    }
}
