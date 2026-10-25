import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MoveResponse } from '../../models/game.model';

@Component({
  selector: 'app-move-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './move-history.html',
  styleUrl: './move-history.css',
})
export class MoveHistoryComponent {
  @Input() moves: MoveResponse[] = [];
}
