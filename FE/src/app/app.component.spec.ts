import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { AppComponent } from './app.component';
import { GameService } from './services/game.service';
import { GameState } from './models/game.models';

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let mockGameService: jasmine.SpyObj<GameService>;

  const initialGameState: GameState = {
    gameId: '11111111-1111-1111-1111-111111111111',
    board: [
      [null, null, null],
      [null, null, null],
      [null, null, null]
    ],
    currentPlayer: 'X',
    gameMode: 'TwoPlayer',
    gameStatus: 'InProgress',
    winner: null,
    winningCells: [],
    moveHistory: [],
    scoreboard: { xWins: 0, oWins: 0, draws: 0 },
    canUndo: false,
    message: "Player X's turn"
  };

  beforeEach(async () => {
    mockGameService = jasmine.createSpyObj('GameService', [
      'createGame',
      'getGame',
      'makeMove',
      'undoMove',
      'resetGame',
      'getScoreboard',
      'resetScoreboard'
    ]);

    mockGameService.createGame.and.returnValue(of(initialGameState));
    mockGameService.getScoreboard.and.returnValue(of({ xWins: 0, oWins: 0, draws: 0 }));

    await TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      declarations: [AppComponent],
      providers: [{ provide: GameService, useValue: mockGameService }]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the app and initialize default game', () => {
    expect(component).toBeTruthy();
    expect(mockGameService.createGame).toHaveBeenCalledWith('TwoPlayer');
    expect(component.gameState).toEqual(initialGameState);
  });

  it('should switch mode and start a new game', () => {
    const computerGameState: GameState = {
      ...initialGameState,
      gameMode: 'AgainstComputer'
    };
    mockGameService.createGame.and.returnValue(of(computerGameState));

    component.setMode('AgainstComputer');

    expect(component.selectedMode).toBe('AgainstComputer');
    expect(mockGameService.createGame).toHaveBeenCalledWith('AgainstComputer');
  });

  it('should make move when empty cell is clicked', () => {
    const updatedState: GameState = {
      ...initialGameState,
      board: [
        ['X', null, null],
        [null, null, null],
        [null, null, null]
      ],
      currentPlayer: 'O',
      canUndo: true,
      moveHistory: [
        {
          moveNumber: 1,
          player: 'X',
          row: 0,
          col: 0,
          position: 'Row 1, Column 1',
          timestamp: new Date().toISOString()
        }
      ]
    };
    mockGameService.makeMove.and.returnValue(of(updatedState));

    component.onCellClick(0, 0);

    expect(mockGameService.makeMove).toHaveBeenCalledWith(
      initialGameState.gameId,
      'X',
      0,
      0
    );
    expect(component.gameState?.board[0][0]).toBe('X');
    expect(component.gameState?.currentPlayer).toBe('O');
  });

  it('should call undoMove when onUndo is triggered and canUndo is true', () => {
    component.gameState = {
      ...initialGameState,
      canUndo: true
    };
    mockGameService.undoMove.and.returnValue(of(initialGameState));

    component.onUndo();

    expect(mockGameService.undoMove).toHaveBeenCalledWith(initialGameState.gameId);
  });

  it('should identify winning cells correctly', () => {
    component.gameState = {
      ...initialGameState,
      gameStatus: 'Won',
      winner: 'X',
      winningCells: [
        { row: 0, col: 0 },
        { row: 0, col: 1 },
        { row: 0, col: 2 }
      ]
    };

    expect(component.isWinningCell(0, 0)).toBeTrue();
    expect(component.isWinningCell(0, 1)).toBeTrue();
    expect(component.isWinningCell(0, 2)).toBeTrue();
    expect(component.isWinningCell(1, 1)).toBeFalse();
  });
});
