import { Component, OnInit } from '@angular/core';
import { GameService } from './services/game.service';
import { GameMode, GameState, Scoreboard } from './models/game.models';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'ABB Tic Tac Toe';
  selectedMode: GameMode = 'TwoPlayer';
  gameState: GameState | null = null;
  scoreboard: Scoreboard = { xWins: 0, oWins: 0, draws: 0 };
  isLoading = false;
  isAiThinking = false;
  errorMessage: string | null = null;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.startNewGame(this.selectedMode);
    this.refreshScoreboard();
  }

  setMode(mode: GameMode): void {
    if (this.selectedMode === mode && this.gameState) {
      return;
    }
    this.selectedMode = mode;
    this.startNewGame(mode);
  }

  startNewGame(mode: GameMode = this.selectedMode): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.gameService.createGame(mode).subscribe({
      next: (state) => {
        this.gameState = state;
        this.scoreboard = state.scoreboard;
        this.isLoading = false;
      },
      error: (err) => {
        this.handleError(err, 'Failed to initialize a new game session.');
        this.isLoading = false;
      }
    });
  }

  onCellClick(row: number, col: number): void {
    if (!this.gameState) {
      return;
    }

    if (this.gameState.gameStatus !== 'InProgress') {
      return;
    }

    if (this.gameState.board[row][col] !== null) {
      return;
    }

    if (this.isLoading || this.isAiThinking) {
      return;
    }

    const player = this.gameState.currentPlayer;
    this.errorMessage = null;

    if (this.selectedMode === 'AgainstComputer') {
      this.isAiThinking = true;
    } else {
      this.isLoading = true;
    }

    this.gameService.makeMove(this.gameState.gameId, player, row, col).subscribe({
      next: (state) => {
        this.gameState = state;
        this.scoreboard = state.scoreboard;
        this.isLoading = false;
        this.isAiThinking = false;
      },
      error: (err) => {
        this.handleError(err, 'Failed to register move.');
        this.isLoading = false;
        this.isAiThinking = false;
      }
    });
  }

  onUndo(): void {
    if (!this.gameState || !this.gameState.canUndo || this.isLoading || this.isAiThinking) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.gameService.undoMove(this.gameState.gameId).subscribe({
      next: (state) => {
        this.gameState = state;
        this.scoreboard = state.scoreboard;
        this.isLoading = false;
      },
      error: (err) => {
        this.handleError(err, 'Unable to undo move.');
        this.isLoading = false;
      }
    });
  }

  onResetGame(): void {
    if (!this.gameState || this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.gameService.resetGame(this.gameState.gameId).subscribe({
      next: (state) => {
        this.gameState = state;
        this.scoreboard = state.scoreboard;
        this.isLoading = false;
      },
      error: (err) => {
        this.handleError(err, 'Unable to reset game.');
        this.isLoading = false;
      }
    });
  }

  onResetScoreboard(): void {
    this.errorMessage = null;

    this.gameService.resetScoreboard().subscribe({
      next: (sb) => {
        this.scoreboard = sb;
        if (this.gameState) {
          this.gameState.scoreboard = sb;
        }
      },
      error: (err) => {
        this.handleError(err, 'Unable to reset scoreboard.');
      }
    });
  }

  refreshScoreboard(): void {
    this.gameService.getScoreboard().subscribe({
      next: (sb) => {
        this.scoreboard = sb;
        if (this.gameState) {
          this.gameState.scoreboard = sb;
        }
      },
      error: (err) => {
        // Fallback silently if initial connection takes time
      }
    });
  }

  isWinningCell(row: number, col: number): boolean {
    if (!this.gameState || !this.gameState.winningCells) {
      return false;
    }
    return this.gameState.winningCells.some(cell => cell.row === row && cell.col === col);
  }

  dismissError(): void {
    this.errorMessage = null;
  }

  private handleError(err: any, fallback: string): void {
    if (err?.error?.message) {
      this.errorMessage = err.error.message;
    } else if (err?.status === 0) {
      this.errorMessage = 'Unable to connect to the .NET Backend API (http://localhost:5000). Please ensure the backend is running.';
    } else {
      this.errorMessage = fallback;
    }
  }
}
