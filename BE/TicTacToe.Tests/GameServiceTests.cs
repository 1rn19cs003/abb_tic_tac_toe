using TicTacToe.Api.Models.Domain;
using TicTacToe.Api.Models.Enums;
using TicTacToe.Api.Repositories;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Tests;

public class GameServiceTests
{
    private readonly InMemoryGameRepository _repository;
    private readonly AiStrategy _aiStrategy;
    private readonly GameService _service;

    public GameServiceTests()
    {
        _repository = new InMemoryGameRepository();
        _aiStrategy = new AiStrategy();
        _service = new GameService(_repository, _aiStrategy);
    }

    [Fact]
    public void CreateGame_InitializesDefaultStateCorrectly()
    {
        // Act
        var response = _service.CreateGame(GameMode.TwoPlayer);

        // Assert
        Assert.NotEqual(Guid.Empty, response.GameId);
        Assert.Equal("X", response.CurrentPlayer);
        Assert.Equal("InProgress", response.GameStatus);
        Assert.Equal("TwoPlayer", response.GameMode);
        Assert.Null(response.Winner);
        Assert.Empty(response.WinningCells);
        Assert.Empty(response.MoveHistory);
        Assert.False(response.CanUndo);

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                Assert.Null(response.Board[r][c]);
            }
        }
    }

    [Fact]
    public void MakeMove_ValidMove_UpdatesBoardAndAlternatesTurn()
    {
        // Arrange
        var game = _service.CreateGame(GameMode.TwoPlayer);

        // Act
        var response = _service.MakeMove(game.GameId, "X", 0, 0);

        // Assert
        Assert.Equal("X", response.Board[0][0]);
        Assert.Equal("O", response.CurrentPlayer);
        Assert.Single(response.MoveHistory);
        Assert.Equal(1, response.MoveHistory[0].MoveNumber);
        Assert.Equal("X", response.MoveHistory[0].Player);
        Assert.Equal("Row 1, Column 1", response.MoveHistory[0].Position);
        Assert.True(response.CanUndo);
    }

    [Fact]
    public void MakeMove_OutOfBounds_ThrowsGameOperationException()
    {
        // Arrange
        var game = _service.CreateGame(GameMode.TwoPlayer);

        // Act & Assert
        var ex = Assert.Throws<GameOperationException>(() =>
            _service.MakeMove(game.GameId, "X", 3, 0));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void MakeMove_OccupiedCell_ThrowsGameOperationException()
    {
        // Arrange
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 1, 1);

        // Act & Assert
        var ex = Assert.Throws<GameOperationException>(() =>
            _service.MakeMove(game.GameId, "O", 1, 1));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void MakeMove_WrongPlayerTurn_ThrowsGameOperationException()
    {
        // Arrange: Initial player is X, so O moving first is an error
        var game = _service.CreateGame(GameMode.TwoPlayer);

        // Act & Assert
        var ex = Assert.Throws<GameOperationException>(() =>
            _service.MakeMove(game.GameId, "O", 0, 0));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void WinDetection_RowWin_SetsStatusAndWinningCells()
    {
        // Arrange:
        // X plays (0,0), O plays (1,0)
        // X plays (0,1), O plays (1,1)
        // X plays (0,2) -> Row 0 win
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 0, 0);
        _service.MakeMove(game.GameId, "O", 1, 0);
        _service.MakeMove(game.GameId, "X", 0, 1);
        _service.MakeMove(game.GameId, "O", 1, 1);
        var finalResponse = _service.MakeMove(game.GameId, "X", 0, 2);

        // Assert
        Assert.Equal("Won", finalResponse.GameStatus);
        Assert.Equal("X", finalResponse.Winner);
        Assert.Equal(3, finalResponse.WinningCells.Count);
        Assert.Contains(new CellPosition(0, 0), finalResponse.WinningCells);
        Assert.Contains(new CellPosition(0, 1), finalResponse.WinningCells);
        Assert.Contains(new CellPosition(0, 2), finalResponse.WinningCells);
        Assert.Equal(1, finalResponse.Scoreboard.XWins);
        Assert.Equal(0, finalResponse.Scoreboard.OWins);
    }

    [Fact]
    public void WinDetection_ColumnWin_SetsStatusAndWinningCells()
    {
        // Arrange:
        // X plays (0,0), O plays (0,1)
        // X plays (1,0), O plays (1,1)
        // X plays (2,0) -> Column 0 win
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 0, 0);
        _service.MakeMove(game.GameId, "O", 0, 1);
        _service.MakeMove(game.GameId, "X", 1, 0);
        _service.MakeMove(game.GameId, "O", 1, 1);
        var finalResponse = _service.MakeMove(game.GameId, "X", 2, 0);

        // Assert
        Assert.Equal("Won", finalResponse.GameStatus);
        Assert.Equal("X", finalResponse.Winner);
        Assert.Contains(new CellPosition(0, 0), finalResponse.WinningCells);
        Assert.Contains(new CellPosition(1, 0), finalResponse.WinningCells);
        Assert.Contains(new CellPosition(2, 0), finalResponse.WinningCells);
    }

    [Fact]
    public void WinDetection_DiagonalWin_SetsStatusAndWinningCells()
    {
        // Arrange:
        // X plays (0,0), O plays (0,1)
        // X plays (1,1), O plays (0,2)
        // X plays (2,2) -> Main Diagonal win
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 0, 0);
        _service.MakeMove(game.GameId, "O", 0, 1);
        _service.MakeMove(game.GameId, "X", 1, 1);
        _service.MakeMove(game.GameId, "O", 0, 2);
        var finalResponse = _service.MakeMove(game.GameId, "X", 2, 2);

        // Assert
        Assert.Equal("Won", finalResponse.GameStatus);
        Assert.Equal("X", finalResponse.Winner);
        Assert.Contains(new CellPosition(0, 0), finalResponse.WinningCells);
        Assert.Contains(new CellPosition(1, 1), finalResponse.WinningCells);
        Assert.Contains(new CellPosition(2, 2), finalResponse.WinningCells);
    }

    [Fact]
    public void MakeMove_AfterGameWon_ThrowsGameOperationException()
    {
        // Arrange: Complete game with X win
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 0, 0);
        _service.MakeMove(game.GameId, "O", 1, 0);
        _service.MakeMove(game.GameId, "X", 0, 1);
        _service.MakeMove(game.GameId, "O", 1, 1);
        _service.MakeMove(game.GameId, "X", 0, 2); // Game Won

        // Act & Assert
        var ex = Assert.Throws<GameOperationException>(() =>
            _service.MakeMove(game.GameId, "O", 2, 2));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void DrawDetection_FullBoardWithoutWinner_SetsDrawStatusAndScoreboard()
    {
        // Arrange:
        // X O X
        // X X O
        // O X O
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 0, 0);
        _service.MakeMove(game.GameId, "O", 0, 1);
        _service.MakeMove(game.GameId, "X", 0, 2);
        _service.MakeMove(game.GameId, "O", 1, 2);
        _service.MakeMove(game.GameId, "X", 1, 0);
        _service.MakeMove(game.GameId, "O", 2, 0);
        _service.MakeMove(game.GameId, "X", 1, 1);
        _service.MakeMove(game.GameId, "O", 2, 2);
        var finalResponse = _service.MakeMove(game.GameId, "X", 2, 1);

        // Assert
        Assert.Equal("Draw", finalResponse.GameStatus);
        Assert.Null(finalResponse.Winner);
        Assert.Empty(finalResponse.WinningCells);
        Assert.Equal(1, finalResponse.Scoreboard.Draws);
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyLatestMoveAndRestoresTurn()
    {
        // Arrange:
        // Move 1: X plays (0, 0)
        // Move 2: O plays (1, 1)
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 0, 0);
        _service.MakeMove(game.GameId, "O", 1, 1);

        // Act: Undo O's move
        var response = _service.UndoMove(game.GameId);

        // Assert:
        // (1, 1) should be null, turn should be O again, 1 move remaining
        Assert.Null(response.Board[1][1]);
        Assert.Equal("X", response.Board[0][0]);
        Assert.Equal("O", response.CurrentPlayer);
        Assert.Single(response.MoveHistory);
    }

    [Fact]
    public void Undo_ComputerMode_RemovesBothComputerAndHumanMovesTogether()
    {
        // Arrange:
        // In Computer mode, when X moves, computer O moves automatically right after.
        var game = _service.CreateGame(GameMode.AgainstComputer);
        var responseAfterMove = _service.MakeMove(game.GameId, "X", 0, 0);

        // Should have 2 moves: X at (0,0) and O at center (1,1)
        Assert.Equal(2, responseAfterMove.MoveHistory.Count);

        // Act: Undo
        var responseAfterUndo = _service.UndoMove(game.GameId);

        // Assert: Both moves are undone, it's X's turn again, board empty
        Assert.Empty(responseAfterUndo.MoveHistory);
        Assert.Equal("X", responseAfterUndo.CurrentPlayer);
        Assert.Null(responseAfterUndo.Board[0][0]);
        Assert.Null(responseAfterUndo.Board[1][1]);
    }

    [Fact]
    public void Undo_WhenNoMoves_ThrowsGameOperationException()
    {
        // Arrange
        var game = _service.CreateGame(GameMode.TwoPlayer);

        // Act & Assert
        var ex = Assert.Throws<GameOperationException>(() =>
            _service.UndoMove(game.GameId));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void Undo_WhenGameCompleted_ThrowsGameOperationException()
    {
        // Arrange: Complete game with X win
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 0, 0);
        _service.MakeMove(game.GameId, "O", 1, 0);
        _service.MakeMove(game.GameId, "X", 0, 1);
        _service.MakeMove(game.GameId, "O", 1, 1);
        _service.MakeMove(game.GameId, "X", 0, 2);

        // Act & Assert (Option A: Disabled after completion)
        var ex = Assert.Throws<GameOperationException>(() =>
            _service.UndoMove(game.GameId));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void ResetGame_ClearsBoardAndMoves_PreservesScoreboard()
    {
        // Arrange:
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 0, 0);
        _service.MakeMove(game.GameId, "O", 1, 0);
        _service.MakeMove(game.GameId, "X", 0, 1);
        _service.MakeMove(game.GameId, "O", 1, 1);
        _service.MakeMove(game.GameId, "X", 0, 2); // X Wins, XWins = 1

        // Act: Reset game
        var resetResponse = _service.ResetGame(game.GameId);

        // Assert
        Assert.Equal("InProgress", resetResponse.GameStatus);
        Assert.Equal("X", resetResponse.CurrentPlayer);
        Assert.Null(resetResponse.Winner);
        Assert.Empty(resetResponse.WinningCells);
        Assert.Empty(resetResponse.MoveHistory);
        Assert.Equal(1, resetResponse.Scoreboard.XWins); // Scoreboard preserved!
    }

    [Fact]
    public void ScoreboardReset_ResetsAllCountsToZero()
    {
        // Arrange: Generate some wins
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.MakeMove(game.GameId, "X", 0, 0);
        _service.MakeMove(game.GameId, "O", 1, 0);
        _service.MakeMove(game.GameId, "X", 0, 1);
        _service.MakeMove(game.GameId, "O", 1, 1);
        _service.MakeMove(game.GameId, "X", 0, 2); // X wins

        var scoreboardService = new ScoreboardService(_repository);
        Assert.Equal(1, scoreboardService.GetScoreboard().XWins);

        // Act
        var resetScoreboard = scoreboardService.ResetScoreboard();

        // Assert
        Assert.Equal(0, resetScoreboard.XWins);
        Assert.Equal(0, resetScoreboard.OWins);
        Assert.Equal(0, resetScoreboard.Draws);
    }
}
