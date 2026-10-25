export interface GameResponse {
  id: string;
  board: string[][];
  currentPlayer: string;
  gameMode: string;
  status: string;
  winner: string | null;
  winningCells: number[][] | null;
  moveHistory: MoveResponse[];
  scoreboard: ScoreboardResponse;
}

export interface MoveResponse {
  moveNumber: number;
  player: string;
  row: number;
  col: number;
}

export interface ScoreboardResponse {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface CreateGameRequest {
  gameMode: string;
}

export interface MoveRequest {
  player: string;
  row: number;
  col: number;
}

export interface GameSummaryResponse {
  id: string;
  gameMode: string;
  status: string;
  winner: string | null;
  moveCount: number;
  createdAt: string;
}

export interface ErrorResponse {
  error: string;
}
