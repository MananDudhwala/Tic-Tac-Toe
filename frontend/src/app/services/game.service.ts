import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  GameResponse,
  CreateGameRequest,
  MoveRequest,
  ScoreboardResponse,
} from '../models/game.model';

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private readonly apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  createGame(gameMode: string): Observable<GameResponse> {
    const request: CreateGameRequest = { gameMode };
    return this.http.post<GameResponse>(`${this.apiUrl}/games`, request);
  }

  getGame(gameId: string): Observable<GameResponse> {
    return this.http.get<GameResponse>(`${this.apiUrl}/games/${gameId}`);
  }

  makeMove(
    gameId: string,
    player: string,
    row: number,
    col: number
  ): Observable<GameResponse> {
    const request: MoveRequest = { player, row, col };
    return this.http.post<GameResponse>(
      `${this.apiUrl}/games/${gameId}/moves`,
      request
    );
  }

  undoMove(gameId: string): Observable<GameResponse> {
    return this.http.post<GameResponse>(
      `${this.apiUrl}/games/${gameId}/undo`,
      {}
    );
  }

  resetGame(gameId: string): Observable<GameResponse> {
    return this.http.post<GameResponse>(
      `${this.apiUrl}/games/${gameId}/reset`,
      {}
    );
  }

  getScoreboard(): Observable<ScoreboardResponse> {
    return this.http.get<ScoreboardResponse>(`${this.apiUrl}/scoreboard`);
  }

  resetScoreboard(): Observable<ScoreboardResponse> {
    return this.http.post<ScoreboardResponse>(
      `${this.apiUrl}/scoreboard/reset`,
      {}
    );
  }
}
