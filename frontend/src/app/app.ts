import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameService } from './services/game.service';
import { GameResponse, GameSummaryResponse, ScoreboardResponse } from './models/game.model';
import { BoardComponent } from './components/board/board';
import { ScoreboardComponent } from './components/scoreboard/scoreboard';
import { MoveHistoryComponent } from './components/move-history/move-history';
import { PastGamesComponent } from './components/past-games/past-games';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    BoardComponent,
    ScoreboardComponent,
    MoveHistoryComponent,
    PastGamesComponent,
  ],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  game = signal<GameResponse | null>(null);
  selectedMode = signal<'TwoPlayer' | 'Computer'>('TwoPlayer');
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  // Past games state
  pastGames = signal<GameSummaryResponse[]>([]);
  isHistoryLoading = signal<boolean>(false);
  activeLiveGameId = signal<string | null>(null);
  isViewingArchived = signal<boolean>(false);

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.startNewGame(this.selectedMode());
  }

  loadPastGames(): void {
    this.isHistoryLoading.set(true);
    this.gameService.getPastGames().subscribe({
      next: (games) => {
        this.pastGames.set(games);
        this.isHistoryLoading.set(false);
      },
      error: () => {
        this.isHistoryLoading.set(false);
      },
    });
  }

  startNewGame(mode: 'TwoPlayer' | 'Computer'): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.selectedMode.set(mode);
    this.isViewingArchived.set(false);

    this.gameService.createGame(mode).subscribe({
      next: (response) => {
        this.game.set(response);
        this.activeLiveGameId.set(response.id);
        this.isLoading.set(false);
        this.loadPastGames();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(
          err.error?.error || 'Failed to connect to backend API. Please ensure the .NET server is running on http://localhost:5000.'
        );
      },
    });
  }

  onCellClick(event: { row: number; col: number }): void {
    const currentGame = this.game();
    if (!currentGame || this.isLoading() || currentGame.status !== 'InProgress' || this.isViewingArchived()) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.gameService
      .makeMove(currentGame.id, currentGame.currentPlayer, event.row, event.col)
      .subscribe({
        next: (updatedGame) => {
          this.game.set(updatedGame);
          this.isLoading.set(false);
          if (updatedGame.status !== 'InProgress') {
            this.loadPastGames();
          }
        },
        error: (err) => {
          this.isLoading.set(false);
          this.errorMessage.set(err.error?.error || 'Invalid move attempted.');
        },
      });
  }

  get isUndoDisabled(): boolean {
    const currentGame = this.game();
    if (!currentGame || this.isLoading() || currentGame.moveHistory.length === 0 || this.isViewingArchived()) {
      return true;
    }
    // Only allow undo after game completion in Computer mode
    if (currentGame.status !== 'InProgress' && currentGame.gameMode !== 'Computer') {
      return true;
    }
    return false;
  }

  undoMove(): void {
    if (this.isUndoDisabled) {
      return;
    }
    const currentGame = this.game()!;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.gameService.undoMove(currentGame.id).subscribe({
      next: (updatedGame) => {
        this.game.set(updatedGame);
        this.isLoading.set(false);
        this.loadPastGames();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.error || 'Failed to undo move.');
      },
    });
  }

  resetGame(): void {
    const currentGame = this.game();
    if (!currentGame || this.isLoading()) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.isViewingArchived.set(false);

    this.gameService.resetGame(this.activeLiveGameId() || currentGame.id).subscribe({
      next: (updatedGame) => {
        this.game.set(updatedGame);
        this.isLoading.set(false);
        this.loadPastGames();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.error || 'Failed to reset game.');
      },
    });
  }

  resetScoreboard(): void {
    if (this.isLoading()) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.gameService.resetScoreboard().subscribe({
      next: (updatedScoreboard) => {
        const currentGame = this.game();
        if (currentGame) {
          this.game.set({
            ...currentGame,
            scoreboard: updatedScoreboard,
          });
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.error || 'Failed to reset scoreboard.');
      },
    });
  }

  switchMode(mode: 'TwoPlayer' | 'Computer'): void {
    if (this.selectedMode() === mode && !this.isViewingArchived()) return;
    this.startNewGame(mode);
  }

  inspectPastGame(gameId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.gameService.getGame(gameId).subscribe({
      next: (archivedGame) => {
        this.game.set(archivedGame);
        this.isViewingArchived.set(true);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.error || 'Failed to load past game.');
      },
    });
  }

  returnToLiveGame(): void {
    const liveId = this.activeLiveGameId();
    if (!liveId) {
      this.startNewGame(this.selectedMode());
      return;
    }

    this.isLoading.set(true);
    this.gameService.getGame(liveId).subscribe({
      next: (liveGame) => {
        this.game.set(liveGame);
        this.isViewingArchived.set(false);
        this.isLoading.set(false);
      },
      error: () => {
        this.startNewGame(this.selectedMode());
      },
    });
  }

  get turnMessage(): string {
    const g = this.game();
    if (!g) return 'Loading game...';

    if (this.isViewingArchived()) {
      if (g.status === 'Won') {
        return `📜 Archive View: ${g.winner === 'X' ? 'Player X' : (g.gameMode === 'Computer' ? 'Computer' : 'Player O')} Won`;
      }
      if (g.status === 'Draw') {
        return '📜 Archive View: Game ended in a Draw';
      }
      return '📜 Archive View: Game In Progress';
    }

    if (g.status === 'Won') {
      if (g.gameMode === 'Computer') {
        return g.winner === 'X' ? '🎉 You Won!' : '🤖 Computer Won!';
      }
      return `🎉 Player ${g.winner} Wins!`;
    }

    if (g.status === 'Draw') {
      return "🤝 It's a Draw!";
    }

    if (g.gameMode === 'Computer') {
      return g.currentPlayer === 'X' ? "Your Turn (X)" : "Computer's Turn (O)...";
    }

    return `Player ${g.currentPlayer}'s Turn`;
  }
}
