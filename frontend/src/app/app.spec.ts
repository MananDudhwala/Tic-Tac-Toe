import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { App } from './app';
import { GameService } from './services/game.service';
import { GameResponse } from './models/game.model';

describe('App', () => {
  let fixture: ComponentFixture<App>;
  let app: App;
  let mockGameService: any;

  const initialGame: GameResponse = {
    id: 'test-game-id',
    board: [
      ['', '', ''],
      ['', '', ''],
      ['', '', ''],
    ],
    currentPlayer: 'X',
    gameMode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: null,
    moveHistory: [],
    scoreboard: { xWins: 0, oWins: 0, draws: 0 },
  };

  beforeEach(async () => {
    mockGameService = {
      createGame: vi.fn().mockReturnValue(of(initialGame)),
      getGame: vi.fn().mockReturnValue(of(initialGame)),
      makeMove: vi.fn().mockReturnValue(
        of({
          ...initialGame,
          board: [['X', '', ''], ['', '', ''], ['', '', '']],
          currentPlayer: 'O',
          moveHistory: [{ moveNumber: 1, player: 'X', row: 0, col: 0 }],
        })
      ),
      undoMove: vi.fn().mockReturnValue(of(initialGame)),
      resetGame: vi.fn().mockReturnValue(of(initialGame)),
      getScoreboard: vi.fn().mockReturnValue(of({ xWins: 0, oWins: 0, draws: 0 })),
      resetScoreboard: vi.fn().mockReturnValue(of({ xWins: 0, oWins: 0, draws: 0 })),
      getPastGames: vi.fn().mockReturnValue(of([
        {
          id: 'past-game-1',
          gameMode: 'TwoPlayer',
          status: 'Won',
          winner: 'X',
          moveCount: 5,
          createdAt: new Date().toISOString(),
        },
      ])),
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [{ provide: GameService, useValue: mockGameService }],
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    app = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the application', () => {
    expect(app).toBeTruthy();
  });

  it('should render the application title', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Tic Tac Toe');
  });

  it('should load initial game in TwoPlayer mode', () => {
    expect(mockGameService.createGame).toHaveBeenCalledWith('TwoPlayer');
    expect(app.game()).toEqual(initialGame);
  });

  it('should display correct turn message in progress', () => {
    expect(app.turnMessage).toBe("Player X's Turn");
  });

  it('should trigger makeMove when cell is clicked', () => {
    app.onCellClick({ row: 0, col: 0 });
    expect(mockGameService.makeMove).toHaveBeenCalledWith('test-game-id', 'X', 0, 0);
  });

  it('should trigger undoMove when undo is called', () => {
    app.game.set({
      ...initialGame,
      moveHistory: [{ moveNumber: 1, player: 'X', row: 0, col: 0 }],
    });
    app.undoMove();
    expect(mockGameService.undoMove).toHaveBeenCalledWith('test-game-id');
  });

  it('should trigger resetGame when reset is called', () => {
    app.resetGame();
    expect(mockGameService.resetGame).toHaveBeenCalledWith('test-game-id');
  });

  it('should trigger resetScoreboard when requested', () => {
    app.resetScoreboard();
    expect(mockGameService.resetScoreboard).toHaveBeenCalled();
  });

  it('should switch game mode to Computer and start new game', () => {
    app.switchMode('Computer');
    expect(app.selectedMode()).toBe('Computer');
    expect(mockGameService.createGame).toHaveBeenCalledWith('Computer');
  });

  it('should disable undo after game completion in TwoPlayer mode', () => {
    app.game.set({
      ...initialGame,
      gameMode: 'TwoPlayer',
      status: 'Won',
      winner: 'X',
      moveHistory: [{ moveNumber: 1, player: 'X', row: 0, col: 0 }],
    });
    expect(app.isUndoDisabled).toBe(true);

    app.undoMove();
    expect(mockGameService.undoMove).not.toHaveBeenCalled();
  });

  it('should allow undo after game completion in Computer mode', () => {
    app.game.set({
      ...initialGame,
      gameMode: 'Computer',
      status: 'Won',
      winner: 'X',
      moveHistory: [
        { moveNumber: 1, player: 'X', row: 0, col: 0 },
        { moveNumber: 2, player: 'O', row: 1, col: 1 },
      ],
    });
    expect(app.isUndoDisabled).toBe(false);

    app.undoMove();
    expect(mockGameService.undoMove).toHaveBeenCalledWith('test-game-id');
  });

  it('should load past games on init', () => {
    expect(mockGameService.getPastGames).toHaveBeenCalled();
    expect(app.pastGames().length).toBe(1);
  });

  it('should inspect past game and update view state', () => {
    app.inspectPastGame('past-game-1');
    expect(mockGameService.getGame).toHaveBeenCalledWith('past-game-1');
    expect(app.isViewingArchived()).toBe(true);
  });

  it('should return to live game from archive view', () => {
    app.isViewingArchived.set(true);
    app.returnToLiveGame();
    expect(app.isViewingArchived()).toBe(false);
  });

  it('should not display "New Game" button while game is InProgress', () => {
    app.game.set({
      ...initialGame,
      status: 'InProgress',
    });
    fixture.detectChanges();
    const newGameBtn = (fixture.nativeElement as HTMLElement).querySelector('#new-game-btn');
    expect(newGameBtn).toBeNull();
  });

  it('should display "New Game" button after game has concluded with a Win and start fresh game on click', () => {
    app.game.set({
      ...initialGame,
      status: 'Won',
      winner: 'X',
    });
    fixture.detectChanges();
    const newGameBtn = (fixture.nativeElement as HTMLElement).querySelector('#new-game-btn') as HTMLButtonElement;
    expect(newGameBtn).not.toBeNull();
    expect(newGameBtn.textContent).toContain('New Game');

    newGameBtn.click();
    expect(mockGameService.createGame).toHaveBeenCalledWith('TwoPlayer');
  });

  it('should display "New Game" button after game has concluded with a Draw', () => {
    app.game.set({
      ...initialGame,
      status: 'Draw',
      winner: null,
    });
    fixture.detectChanges();
    const newGameBtn = (fixture.nativeElement as HTMLElement).querySelector('#new-game-btn') as HTMLButtonElement;
    expect(newGameBtn).not.toBeNull();
  });
});
