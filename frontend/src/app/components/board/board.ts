import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './board.html',
  styleUrl: './board.css',
})
export class BoardComponent {
  @Input() board: string[][] = [];
  @Input() winningCells: number[][] | null = null;
  @Input() disabled = false;
  @Output() cellClick = new EventEmitter<{ row: number; col: number }>();

  onCellClick(row: number, col: number): void {
    if (!this.disabled && this.board[row][col] === '') {
      this.cellClick.emit({ row, col });
    }
  }

  isWinningCell(row: number, col: number): boolean {
    if (!this.winningCells) return false;
    return this.winningCells.some(
      (cell) => cell[0] === row && cell[1] === col
    );
  }

  getCellClass(row: number, col: number): string {
    const classes: string[] = ['cell'];
    const value = this.board[row]?.[col];

    if (value === 'X') classes.push('cell-x');
    else if (value === 'O') classes.push('cell-o');
    else if (!this.disabled) classes.push('cell-empty');

    if (this.isWinningCell(row, col)) classes.push('cell-winning');
    if (this.disabled && !value) classes.push('cell-disabled');

    return classes.join(' ');
  }
}
