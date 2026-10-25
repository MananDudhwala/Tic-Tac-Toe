using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<Move> Moves => Set<Move>();
    public DbSet<Scoreboard> Scoreboards => Set<Scoreboard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Board).HasMaxLength(9).IsRequired();
            entity.Property(g => g.CurrentPlayer).HasMaxLength(1).IsRequired();
            entity.Property(g => g.GameMode).HasMaxLength(20).IsRequired();
            entity.Property(g => g.Status).HasMaxLength(20).IsRequired();
            entity.Property(g => g.Winner).HasMaxLength(1);
            entity.Property(g => g.WinningCells).HasMaxLength(20);
            entity.HasMany(g => g.Moves).WithOne(m => m.Game).HasForeignKey(m => m.GameId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Move>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Player).HasMaxLength(1).IsRequired();
        });

        modelBuilder.Entity<Scoreboard>(entity =>
        {
            entity.HasKey(s => s.Id);
            // Seed with initial scoreboard
            entity.HasData(new Scoreboard { Id = 1, XWins = 0, OWins = 0, Draws = 0 });
        });
    }
}
