using TicTacToe.Api.Models.Domain;

namespace TicTacToe.Api.Services;

public class AiStrategy : IAiStrategy
{
    private static readonly List<CellPosition[]> WinningLines = new()
    {
        // Rows
        new[] { new CellPosition(0, 0), new CellPosition(0, 1), new CellPosition(0, 2) },
        new[] { new CellPosition(1, 0), new CellPosition(1, 1), new CellPosition(1, 2) },
        new[] { new CellPosition(2, 0), new CellPosition(2, 1), new CellPosition(2, 2) },
        // Columns
        new[] { new CellPosition(0, 0), new CellPosition(1, 0), new CellPosition(2, 0) },
        new[] { new CellPosition(0, 1), new CellPosition(1, 1), new CellPosition(2, 1) },
        new[] { new CellPosition(0, 2), new CellPosition(1, 2), new CellPosition(2, 2) },
        // Diagonals
        new[] { new CellPosition(0, 0), new CellPosition(1, 1), new CellPosition(2, 2) },
        new[] { new CellPosition(0, 2), new CellPosition(1, 1), new CellPosition(2, 0) }
    };

    private static readonly CellPosition Center = new(1, 1);

    private static readonly List<CellPosition> Corners = new()
    {
        new CellPosition(0, 0),
        new CellPosition(0, 2),
        new CellPosition(2, 0),
        new CellPosition(2, 2)
    };

    public CellPosition? DetermineMove(string?[,] board, string aiPlayer = "O", string humanPlayer = "X")
    {
        // Priority 1: If O can win, play the winning move
        var winningMove = FindWinningOrBlockingMove(board, aiPlayer);
        if (winningMove != null)
        {
            return winningMove;
        }

        // Priority 2: If X can win next, block X
        var blockingMove = FindWinningOrBlockingMove(board, humanPlayer);
        if (blockingMove != null)
        {
            return blockingMove;
        }

        // Priority 3: Take center if available
        if (board[Center.Row, Center.Col] == null)
        {
            return Center;
        }

        // Priority 4: Take a corner if available
        foreach (var corner in Corners)
        {
            if (board[corner.Row, corner.Col] == null)
            {
                return corner;
            }
        }

        // Priority 5: Take any available cell
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (board[r, c] == null)
                {
                    return new CellPosition(r, c);
                }
            }
        }

        return null;
    }

    private static CellPosition? FindWinningOrBlockingMove(string?[,] board, string targetPlayer)
    {
        foreach (var line in WinningLines)
        {
            int playerCount = 0;
            CellPosition? emptyCell = null;

            foreach (var pos in line)
            {
                var val = board[pos.Row, pos.Col];
                if (val == targetPlayer)
                {
                    playerCount++;
                }
                else if (val == null)
                {
                    emptyCell = pos;
                }
            }

            // Exactly 2 of targetPlayer and 1 empty cell means this line is one move away from completion
            if (playerCount == 2 && emptyCell != null)
            {
                return emptyCell;
            }
        }

        return null;
    }
}
