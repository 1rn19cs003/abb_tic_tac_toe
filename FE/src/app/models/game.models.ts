export type GameMode = 'TwoPlayer' | 'AgainstComputer';

export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export type PlayerType = 'X' | 'O';

export interface CellPosition {
  row: number;
  col: number;
}

export interface MoveRecord {
  moveNumber: number;
  player: string;
  row: number;
  col: number;
  position: string;
  timestamp: string;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameState {
  gameId: string;
  board: (string | null)[][];
  currentPlayer: string;
  gameMode: GameMode;
  gameStatus: GameStatus;
  winner: string | null;
  winningCells: CellPosition[];
  moveHistory: MoveRecord[];
  scoreboard: Scoreboard;
  canUndo: boolean;
  message: string | null;
}
