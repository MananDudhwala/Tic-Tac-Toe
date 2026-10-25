import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { GameSummaryResponse } from '../../models/game.model';

@Component({
  selector: 'app-past-games',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './past-games.html',
  styleUrl: './past-games.css',
})
export class PastGamesComponent {
  @Input() games: GameSummaryResponse[] = [];
  @Input() activeGameId: string | null = null;
  @Input() isLoading = false;
  @Output() viewGame = new EventEmitter<string>();
  @Output() refresh = new EventEmitter<void>();

  isExpanded = true;

  toggleExpand(): void {
    this.isExpanded = !this.isExpanded;
  }

  onViewGame(id: string): void {
    this.viewGame.emit(id);
  }

  onRefresh(event: Event): void {
    event.stopPropagation();
    this.refresh.emit();
  }

  getResultClass(game: GameSummaryResponse): string {
    if (game.status === 'Won') {
      return game.winner === 'X' ? 'result-win-x' : 'result-win-o';
    }
    if (game.status === 'Draw') return 'result-draw';
    return 'result-progress';
  }

  getResultLabel(game: GameSummaryResponse): string {
    if (game.status === 'Won') {
      if (game.gameMode === 'Computer') {
        return game.winner === 'X' ? '🎉 Human (X) Won' : '🤖 AI (O) Won';
      }
      return `🎉 Player ${game.winner} Won`;
    }
    if (game.status === 'Draw') return '🤝 Draw';
    return '⏳ In Progress';
  }
}
