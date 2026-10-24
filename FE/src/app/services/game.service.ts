import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GameMode, GameState, Scoreboard } from '../models/game.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  createGame(mode: GameMode = 'TwoPlayer'): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games`, { mode });
  }

  getGame(id: string): Observable<GameState> {
    return this.http.get<GameState>(`${this.baseUrl}/games/${id}`);
  }

  makeMove(id: string, player: string, row: number, col: number): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${id}/moves`, {
      player,
      row,
      col
    });
  }

  undoMove(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${id}/undo`, {});
  }

  resetGame(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${id}/reset`, {});
  }

  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${this.baseUrl}/scoreboard`);
  }

  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${this.baseUrl}/scoreboard/reset`, {});
  }
}
