using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Data;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class GameService : IGameService
{
    private readonly GameDbContext _db;
    private readonly IScoreboardService _scoreboard;

    // All possible winning combinations (indices into the 9-cell board)
    private static readonly int[][] WinPatterns = new[]
    {
        new[] { 0, 1, 2 }, // Row 0
        new[] { 3, 4, 5 }, // Row 1
        new[] { 6, 7, 8 }, // Row 2
        new[] { 0, 3, 6 }, // Col 0
        new[] { 1, 4, 7 }, // Col 1
        new[] { 2, 5, 8 }, // Col 2
        new[] { 0, 4, 8 }, // Diagonal
        new[] { 2, 4, 6 }, // Anti-diagonal
    };

    public GameService(GameDbContext db, IScoreboardService scoreboard)
    {
        _db = db;
        _scoreboard = scoreboard;
    }

    public async Task<GameResponse> CreateGameAsync(string gameMode)
    {
        if (gameMode != "TwoPlayer" && gameMode != "Computer")
            throw new ArgumentException("GameMode must be 'TwoPlayer' or 'Computer'.");

        // Clean up unplayed abandoned sessions so they don't clutter match history
        var abandoned = await _db.Games
            .Where(g => g.Status == "InProgress" && !g.Moves.Any())
            .ToListAsync();
        if (abandoned.Any())
        {
            _db.Games.RemoveRange(abandoned);
        }

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Board = "---------",
            CurrentPlayer = "X",
            GameMode = gameMode,
            Status = "InProgress",
            CreatedAt = DateTime.UtcNow
        };

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        return await BuildResponseAsync(game);
    }

    public async Task<GameResponse> GetGameAsync(Guid gameId)
    {
        var game = await LoadGameAsync(gameId);
        return await BuildResponseAsync(game);
    }

    public async Task<GameResponse> MakeMoveAsync(Guid gameId, MoveRequest request)
    {
        var game = await LoadGameAsync(gameId);

        ValidateMove(game, request);

        // Apply the move
        ApplyMove(game, request.Player, request.Row, request.Col);

        // Check for win/draw
        CheckGameStatus(game);

        // If game is still in progress and mode is Computer, make computer move
        if (game.Status == "InProgress" && game.GameMode == "Computer" && game.CurrentPlayer == "O")
        {
            var computerMove = GetComputerMove(game);
            if (computerMove != null)
            {
                ApplyMove(game, "O", computerMove.Value.row, computerMove.Value.col);
                CheckGameStatus(game);
            }
        }

        await _db.SaveChangesAsync();

        // Update scoreboard if game just completed
        if (game.Status == "Won")
            await _scoreboard.IncrementScoreAsync(game.Winner);
        else if (game.Status == "Draw")
            await _scoreboard.IncrementScoreAsync(null);

        return await BuildResponseAsync(game);
    }

    public async Task<GameResponse> UndoMoveAsync(Guid gameId)
    {
        var game = await LoadGameAsync(gameId);

        if (!game.Moves.Any())
            throw new InvalidOperationException("No moves to undo.");

        if (game.Status != "InProgress" && game.GameMode != "Computer")
            throw new InvalidOperationException("Undo is not allowed after game completion in Two Player mode.");

        // Track if we need to adjust scoreboard
        string? previousStatus = game.Status;
        string? previousWinner = game.Winner;

        if (game.GameMode == "Computer")
        {
            // In Computer mode, undo both the computer's move and the player's move
            // If the last move was by the computer, remove it + previous human move
            // If the last move was by human (game just started or only human moved), remove just that
            var lastMove = game.Moves.OrderByDescending(m => m.MoveNumber).First();

            if (lastMove.Player == "O")
            {
                // Remove computer's move
                UndoSingleMove(game, lastMove);

                // Remove the human's move before it (if exists)
                var humanMove = game.Moves.OrderByDescending(m => m.MoveNumber).FirstOrDefault();
                if (humanMove != null)
                {
                    UndoSingleMove(game, humanMove);
                }
            }
            else
            {
                // Last move was by human X (shouldn't normally happen in computer mode
                // since computer moves right after, but handle edge case)
                UndoSingleMove(game, lastMove);
            }
        }
        else
        {
            // Two Player mode: undo only the last move
            var lastMove = game.Moves.OrderByDescending(m => m.MoveNumber).First();
            UndoSingleMove(game, lastMove);
        }

        // Recalculate game status
        game.Status = "InProgress";
        game.Winner = null;
        game.WinningCells = null;
        CheckGameStatus(game);

        await _db.SaveChangesAsync();

        // Adjust scoreboard if game was previously completed but now isn't
        if (previousStatus == "Won" && game.Status != "Won")
            await _scoreboard.DecrementScoreAsync(previousWinner);
        else if (previousStatus == "Draw" && game.Status != "Draw")
            await _scoreboard.DecrementScoreAsync(null);

        return await BuildResponseAsync(game);
    }

    public async Task<GameResponse> ResetGameAsync(Guid gameId)
    {
        var game = await LoadGameAsync(gameId);

        // If game is completed, preserve the finished match in SQLite and start a fresh session
        if (game.Status != "InProgress")
        {
            var freshGame = new Game
            {
                Id = Guid.NewGuid(),
                Board = "---------",
                CurrentPlayer = "X",
                GameMode = game.GameMode,
                Status = "InProgress",
                CreatedAt = DateTime.UtcNow
            };
            _db.Games.Add(freshGame);
            await _db.SaveChangesAsync();
            return await BuildResponseAsync(freshGame);
        }

        game.Board = "---------";
        game.CurrentPlayer = "X";
        game.Status = "InProgress";
        game.Winner = null;
        game.WinningCells = null;

        // Remove all moves
        _db.Moves.RemoveRange(game.Moves);
        game.Moves.Clear();

        await _db.SaveChangesAsync();

        return await BuildResponseAsync(game);
    }

    public async Task<List<GameSummaryResponse>> GetPastGamesAsync()
    {
        return await _db.Games
            .Include(g => g.Moves)
            .Where(g => g.Moves.Any() || g.Status != "InProgress")
            .OrderByDescending(g => g.CreatedAt)
            .Take(30)
            .Select(g => new GameSummaryResponse
            {
                Id = g.Id,
                GameMode = g.GameMode,
                Status = g.Status,
                Winner = g.Winner,
                MoveCount = g.Moves.Count,
                CreatedAt = g.CreatedAt
            })
            .ToListAsync();
    }

    // --- Private helpers ---

    private async Task<Game> LoadGameAsync(Guid gameId)
    {
        var game = await _db.Games
            .Include(g => g.Moves)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
            throw new KeyNotFoundException($"Game {gameId} not found.");

        return game;
    }

    private static void ValidateMove(Game game, MoveRequest request)
    {
        if (game.Status != "InProgress")
            throw new InvalidOperationException("Game is already completed.");

        if (request.Row < 0 || request.Row > 2 || request.Col < 0 || request.Col > 2)
            throw new ArgumentException("Move is outside the board. Row and Col must be 0-2.");

        if (request.Player != "X" && request.Player != "O")
            throw new ArgumentException("Player must be 'X' or 'O'.");

        if (request.Player != game.CurrentPlayer)
            throw new InvalidOperationException($"It is {game.CurrentPlayer}'s turn, not {request.Player}'s.");

        if (game.GetCell(request.Row, request.Col) != '-')
            throw new InvalidOperationException("Cell is already occupied.");
    }

    private static void ApplyMove(Game game, string player, int row, int col)
    {
        game.SetCell(row, col, player[0]);
        game.Moves.Add(new Move
        {
            GameId = game.Id,
            MoveNumber = game.Moves.Count + 1,
            Player = player,
            Row = row,
            Col = col
        });
        game.CurrentPlayer = player == "X" ? "O" : "X";
    }

    private static void UndoSingleMove(Game game, Move move)
    {
        game.SetCell(move.Row, move.Col, '-');
        game.Moves.Remove(move);
        game.CurrentPlayer = move.Player; // Restore turn to the player who made this move
    }

    private static void CheckGameStatus(Game game)
    {
        var board = game.BoardArray;

        foreach (var pattern in WinPatterns)
        {
            char a = board[pattern[0]], b = board[pattern[1]], c = board[pattern[2]];
            if (a != '-' && a == b && b == c)
            {
                game.Status = "Won";
                game.Winner = a.ToString();
                game.WinningCells = string.Join(",", pattern);
                return;
            }
        }

        // Check for draw
        if (!board.Contains('-'))
        {
            game.Status = "Draw";
            game.Winner = null;
            game.WinningCells = null;
            return;
        }

        // Game still in progress
        game.Status = "InProgress";
        game.Winner = null;
        game.WinningCells = null;
    }

    /// <summary>
    /// Computer AI move selection with priority:
    /// 1. Win if possible
    /// 2. Block opponent win
    /// 3. Take center
    /// 4. Take corner
    /// 5. Take any available
    /// </summary>
    internal static (int row, int col)? GetComputerMove(Game game)
    {
        var board = game.BoardArray;

        // 1. Can O win?
        var winMove = FindWinningMove(board, 'O');
        if (winMove != null) return winMove;

        // 2. Block X from winning
        var blockMove = FindWinningMove(board, 'X');
        if (blockMove != null) return blockMove;

        // 3. Take center
        if (board[4] == '-') return (1, 1);

        // 4. Take a corner
        int[] corners = { 0, 2, 6, 8 };
        foreach (var idx in corners)
        {
            if (board[idx] == '-')
                return (idx / 3, idx % 3);
        }

        // 5. Take any available cell
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == '-')
                return (i / 3, i % 3);
        }

        return null;
    }

    private static (int row, int col)? FindWinningMove(char[] board, char player)
    {
        foreach (var pattern in WinPatterns)
        {
            char a = board[pattern[0]], b = board[pattern[1]], c = board[pattern[2]];
            int playerCount = (a == player ? 1 : 0) + (b == player ? 1 : 0) + (c == player ? 1 : 0);
            int emptyCount = (a == '-' ? 1 : 0) + (b == '-' ? 1 : 0) + (c == '-' ? 1 : 0);

            if (playerCount == 2 && emptyCount == 1)
            {
                // Find the empty cell
                for (int i = 0; i < 3; i++)
                {
                    if (board[pattern[i]] == '-')
                    {
                        int idx = pattern[i];
                        return (idx / 3, idx % 3);
                    }
                }
            }
        }
        return null;
    }

    private async Task<GameResponse> BuildResponseAsync(Game game)
    {
        var scoreboard = await _scoreboard.GetScoreboardAsync();

        // Convert board string to 2D array
        var board = new string[3][];
        for (int r = 0; r < 3; r++)
        {
            board[r] = new string[3];
            for (int c = 0; c < 3; c++)
            {
                var cell = game.GetCell(r, c);
                board[r][c] = cell == '-' ? "" : cell.ToString();
            }
        }

        // Convert winning cells to [row, col] pairs
        int[][]? winningCells = null;
        if (!string.IsNullOrEmpty(game.WinningCells))
        {
            var indices = game.WinningCells.Split(',').Select(int.Parse).ToArray();
            winningCells = indices.Select(i => new[] { i / 3, i % 3 }).ToArray();
        }

        return new GameResponse
        {
            Id = game.Id,
            Board = board,
            CurrentPlayer = game.CurrentPlayer,
            GameMode = game.GameMode,
            Status = game.Status,
            Winner = game.Winner,
            WinningCells = winningCells,
            MoveHistory = game.Moves
                .OrderBy(m => m.MoveNumber)
                .Select(m => new MoveResponse
                {
                    MoveNumber = m.MoveNumber,
                    Player = m.Player,
                    Row = m.Row,
                    Col = m.Col
                })
                .ToList(),
            Scoreboard = scoreboard
        };
    }
}
