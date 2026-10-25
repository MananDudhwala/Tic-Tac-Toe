import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScoreboardResponse } from '../../models/game.model';

@Component({
  selector: 'app-scoreboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './scoreboard.html',
  styleUrl: './scoreboard.css',
})
export class ScoreboardComponent {
  @Input() scoreboard: ScoreboardResponse = { xWins: 0, oWins: 0, draws: 0 };
}
