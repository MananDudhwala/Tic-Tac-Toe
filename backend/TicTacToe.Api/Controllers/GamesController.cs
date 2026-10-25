using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    /// <summary>
    /// Create a new game session
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GameResponse>> CreateGame([FromBody] CreateGameRequest request)
    {
        try
        {
            var response = await _gameService.CreateGameAsync(request.GameMode);
            return CreatedAtAction(nameof(GetGame), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get current game state
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GameResponse>> GetGame(Guid id)
    {
        try
        {
            var response = await _gameService.GetGameAsync(id);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ErrorResponse { Error = $"Game {id} not found." });
        }
    }

    /// <summary>
    /// Submit a player move
    /// </summary>
    [HttpPost("{id}/moves")]
    public async Task<ActionResult<GameResponse>> MakeMove(Guid id, [FromBody] MoveRequest request)
    {
        try
        {
            var response = await _gameService.MakeMoveAsync(id, request);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ErrorResponse { Error = $"Game {id} not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    /// <summary>
    /// Undo last move
    /// </summary>
    [HttpPost("{id}/undo")]
    public async Task<ActionResult<GameResponse>> UndoMove(Guid id)
    {
        try
        {
            var response = await _gameService.UndoMoveAsync(id);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ErrorResponse { Error = $"Game {id} not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    /// <summary>
    /// Reset the current game (keeps scoreboard)
    /// </summary>
    [HttpPost("{id}/reset")]
    public async Task<ActionResult<GameResponse>> ResetGame(Guid id)
    {
        try
        {
            var response = await _gameService.ResetGameAsync(id);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ErrorResponse { Error = $"Game {id} not found." });
        }
    }
}
