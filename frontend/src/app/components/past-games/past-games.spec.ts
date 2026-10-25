import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { PastGamesComponent } from './past-games';
import { GameSummaryResponse } from '../../models/game.model';

describe('PastGamesComponent', () => {
  let fixture: ComponentFixture<PastGamesComponent>;
  let component: PastGamesComponent;

  const mockGame: GameSummaryResponse = {
    id: 'game-1',
    gameMode: 'TwoPlayer',
    status: 'Won',
    winner: 'X',
    moveCount: 5,
    createdAt: '2026-10-25T10:00:00Z',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PastGamesComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PastGamesComponent);
    component = fixture.componentInstance;
    component.games = [mockGame];
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should correctly format and localize ISO date strings', () => {
    const formatted = component.formatDate('2026-10-25T10:00:00Z');
    expect(formatted).toBeTruthy();
    expect(formatted).toContain('2026');
  });

  it('should handle dates missing trailing Z timezone indicator', () => {
    const formatted = component.formatDate('2026-10-25T10:00:00.000');
    expect(formatted).toBeTruthy();
    expect(formatted).toContain('2026');
  });

  it('should return empty string for null or empty input', () => {
    expect(component.formatDate('')).toBe('');
  });

  it('should toggle expanded state', () => {
    expect(component.isExpanded).toBe(true);
    component.toggleExpand();
    expect(component.isExpanded).toBe(false);
  });
});
