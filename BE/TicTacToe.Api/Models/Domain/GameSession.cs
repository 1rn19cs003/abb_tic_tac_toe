using TicTacToe.Api.Models.Enums;

namespace TicTacToe.Api.Models.Domain;

public class GameSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public PlayerType CurrentPlayer { get; set; } = PlayerType.X;
    public PlayerType? Winner { get; set; }
    public List<CellPosition> WinningCells { get; set; } = new();
    public string?[,] Board { get; private set; } = new string?[3, 3];
    public List<MoveRecord> MoveHistory { get; private set; } = new();
    public bool ScoreboardUpdated { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public GameSession(GameMode mode = GameMode.TwoPlayer)
    {
        Mode = mode;
        Reset();
    }

    public void Reset()
    {
        Board = new string?[3, 3];
        MoveHistory = new List<MoveRecord>();
        Status = GameStatus.InProgress;
        CurrentPlayer = PlayerType.X;
        Winner = null;
        WinningCells = new List<CellPosition>();
        ScoreboardUpdated = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RebuildBoard()
    {
        Board = new string?[3, 3];
        foreach (var move in MoveHistory)
        {
            Board[move.Row, move.Col] = move.Player;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public string?[][] GetBoardJagged()
    {
        var result = new string?[3][];
        for (int r = 0; r < 3; r++)
        {
            result[r] = new string?[3];
            for (int c = 0; c < 3; c++)
            {
                result[r][c] = Board[r, c];
            }
        }
        return result;
    }
}
