using TicTacToe.Api.Models.Domain;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Tests;

public class AiStrategyTests
{
    private readonly AiStrategy _aiStrategy = new();

    [Fact]
    public void DetermineMove_Priority1_TakesWinningMove_WhenAvailable()
    {
        // Arrange: O has (0, 0) and (0, 1). Winning move is (0, 2).
        // X has (1, 0) and (1, 1). X is also threatening, but O must win first!
        var board = new string?[3, 3]
        {
            { "O", "O", null },
            { "X", "X", null },
            { null, null, null }
        };

        // Act
        var move = _aiStrategy.DetermineMove(board, "O", "X");

        // Assert
        Assert.NotNull(move);
        Assert.Equal(new CellPosition(0, 2), move);
    }

    [Fact]
    public void DetermineMove_Priority2_BlocksOpponentWinningMove_WhenThreatened()
    {
        // Arrange: X threatens with (1, 0) and (1, 1). Blocking move is (1, 2).
        var board = new string?[3, 3]
        {
            { "O", null, null },
            { "X", "X", null },
            { null, null, null }
        };

        // Act
        var move = _aiStrategy.DetermineMove(board, "O", "X");

        // Assert
        Assert.NotNull(move);
        Assert.Equal(new CellPosition(1, 2), move);
    }

    [Fact]
    public void DetermineMove_Priority2_BlocksDiagonalWin()
    {
        // Arrange: X threatens anti-diagonal (0, 2) and (1, 1). Blocking move is (2, 0).
        var board = new string?[3, 3]
        {
            { null, null, "X" },
            { null, "X", null },
            { null, null, null }
        };

        // Act
        var move = _aiStrategy.DetermineMove(board, "O", "X");

        // Assert
        Assert.NotNull(move);
        Assert.Equal(new CellPosition(2, 0), move);
    }

    [Fact]
    public void DetermineMove_Priority3_TakesCenter_WhenAvailable()
    {
        // Arrange: X opened at corner (0, 0). Center (1, 1) is available.
        var board = new string?[3, 3]
        {
            { "X", null, null },
            { null, null, null },
            { null, null, null }
        };

        // Act
        var move = _aiStrategy.DetermineMove(board, "O", "X");

        // Assert
        Assert.NotNull(move);
        Assert.Equal(new CellPosition(1, 1), move);
    }

    [Fact]
    public void DetermineMove_Priority4_TakesCorner_WhenCenterOccupied()
    {
        // Arrange: Center (1, 1) is occupied by X. Corner (0, 0) is available.
        var board = new string?[3, 3]
        {
            { null, null, null },
            { null, "X", null },
            { null, null, null }
        };

        // Act
        var move = _aiStrategy.DetermineMove(board, "O", "X");

        // Assert
        Assert.NotNull(move);
        var expectedCorners = new[]
        {
            new CellPosition(0, 0),
            new CellPosition(0, 2),
            new CellPosition(2, 0),
            new CellPosition(2, 2)
        };
        Assert.Contains(move, expectedCorners);
    }

    [Fact]
    public void DetermineMove_Priority5_TakesAnyAvailableCell_WhenCenterAndCornersOccupied()
    {
        // Arrange: All 4 corners and center are occupied
        var board = new string?[3, 3]
        {
            { "X", null, "O" },
            { null, "X", null },
            { "O", null, "X" }
        };

        // Act
        var move = _aiStrategy.DetermineMove(board, "O", "X");

        // Assert
        Assert.NotNull(move);
        var availableEdges = new[]
        {
            new CellPosition(0, 1),
            new CellPosition(1, 0),
            new CellPosition(1, 2),
            new CellPosition(2, 1)
        };
        Assert.Contains(move, availableEdges);
    }

    [Fact]
    public void DetermineMove_ReturnsNull_WhenBoardIsFull()
    {
        // Arrange: Board is full
        var board = new string?[3, 3]
        {
            { "X", "O", "X" },
            { "X", "O", "O" },
            { "O", "X", "X" }
        };

        // Act
        var move = _aiStrategy.DetermineMove(board, "O", "X");

        // Assert
        Assert.Null(move);
    }
}
