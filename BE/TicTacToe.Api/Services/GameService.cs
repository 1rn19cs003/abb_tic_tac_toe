using TicTacToe.Api.Models.Domain;
using TicTacToe.Api.Models.DTOs;
using TicTacToe.Api.Models.Enums;
using TicTacToe.Api.Repositories;

namespace TicTacToe.Api.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _repository;
    private readonly IAiStrategy _aiStrategy;

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

    public GameService(IGameRepository repository, IAiStrategy aiStrategy)
    {
        _repository = repository;
        _aiStrategy = aiStrategy;
    }

    public GameStateResponse CreateGame(GameMode mode)
    {
        var game = _repository.CreateGame(mode);
        return MapToResponse(game);
    }

    public GameStateResponse? GetGameState(Guid id)
    {
        var game = _repository.GetGame(id);
        return game == null ? null : MapToResponse(game);
    }

    public GameStateResponse MakeMove(Guid id, string player, int row, int col)
    {
        var game = _repository.GetGame(id)
            ?? throw new GameOperationException("Game session not found.", 404);

        if (game.Status != GameStatus.InProgress)
        {
            throw new GameOperationException("Cannot make a move on an already completed game.", 400);
        }

        if (row < 0 || row > 2 || col < 0 || col > 2)
        {
            throw new GameOperationException($"Cell coordinates ({row}, {col}) are outside the 3x3 board.", 400);
        }

        if (game.Board[row, col] != null)
        {
            throw new GameOperationException($"Cell at Row {row + 1}, Column {col + 1} is already occupied.", 400);
        }

        var normalizedPlayer = player.Trim().ToUpperInvariant();
        if (normalizedPlayer != "X" && normalizedPlayer != "O")
        {
            throw new GameOperationException($"Invalid player '{player}'. Allowed players are 'X' and 'O'.", 400);
        }

        if (normalizedPlayer != game.CurrentPlayer.ToString())
        {
            throw new GameOperationException($"It is currently Player {game.CurrentPlayer}'s turn.", 400);
        }

        // Apply player move
        ApplyMove(game, normalizedPlayer, row, col);

        // Check if player won or drew
        EvaluateGameStatus(game);

        // If game is still in progress and mode is AgainstComputer, trigger computer turn
        if (game.Status == GameStatus.InProgress && game.Mode == GameMode.AgainstComputer && game.CurrentPlayer == PlayerType.O)
        {
            var computerMove = _aiStrategy.DetermineMove(game.Board, "O", "X");
            if (computerMove != null)
            {
                ApplyMove(game, "O", computerMove.Row, computerMove.Col);
                EvaluateGameStatus(game);
            }
        }

        game.UpdatedAt = DateTime.UtcNow;
        _repository.SaveGame(game);

        return MapToResponse(game);
    }

    public GameStateResponse UndoMove(Guid id)
    {
        var game = _repository.GetGame(id)
            ?? throw new GameOperationException("Game session not found.", 404);

        // Clarification Option A: Once a game is won or drawn, Undo is disabled
        if (game.Status != GameStatus.InProgress)
        {
            throw new GameOperationException("Undo is disabled for completed games.", 400);
        }

        if (game.MoveHistory.Count == 0)
        {
            throw new GameOperationException("No moves available to undo.", 400);
        }

        if (game.Mode == GameMode.TwoPlayer)
        {
            // Remove single latest move
            var lastMove = game.MoveHistory[^1];
            game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
            game.RebuildBoard();
            game.CurrentPlayer = lastMove.Player == "X" ? PlayerType.X : PlayerType.O;
        }
        else // AgainstComputer
        {
            // In Computer mode: undo computer's move and human's move together
            if (game.MoveHistory.Count >= 2)
            {
                game.MoveHistory.RemoveRange(game.MoveHistory.Count - 2, 2);
            }
            else
            {
                game.MoveHistory.Clear();
            }

            game.RebuildBoard();
            game.CurrentPlayer = PlayerType.X;
        }

        game.Status = GameStatus.InProgress;
        game.Winner = null;
        game.WinningCells.Clear();
        game.UpdatedAt = DateTime.UtcNow;
        _repository.SaveGame(game);

        return MapToResponse(game);
    }

    public GameStateResponse ResetGame(Guid id)
    {
        var game = _repository.GetGame(id)
            ?? throw new GameOperationException("Game session not found.", 404);

        game.Reset();
        _repository.SaveGame(game);

        return MapToResponse(game);
    }

    private static void ApplyMove(GameSession game, string player, int row, int col)
    {
        game.Board[row, col] = player;
        var moveNumber = game.MoveHistory.Count + 1;
        var moveRecord = new MoveRecord(
            moveNumber,
            player,
            row,
            col,
            $"Row {row + 1}, Column {col + 1}",
            DateTime.UtcNow
        );
        game.MoveHistory.Add(moveRecord);

        // Alternate turn
        game.CurrentPlayer = player == "X" ? PlayerType.O : PlayerType.X;
    }

    private void EvaluateGameStatus(GameSession game)
    {
        // Check for a winning line
        foreach (var line in WinningLines)
        {
            var first = game.Board[line[0].Row, line[0].Col];
            if (first != null &&
                first == game.Board[line[1].Row, line[1].Col] &&
                first == game.Board[line[2].Row, line[2].Col])
            {
                game.Status = GameStatus.Won;
                game.Winner = Enum.Parse<PlayerType>(first);
                game.WinningCells = line.ToList();

                if (!game.ScoreboardUpdated)
                {
                    var scoreboard = _repository.GetScoreboard();
                    if (game.Winner == PlayerType.X)
                    {
                        scoreboard.RecordXWin();
                    }
                    else
                    {
                        scoreboard.RecordOWin();
                    }
                    game.ScoreboardUpdated = true;
                }
                return;
            }
        }

        // Check for draw (all 9 cells occupied with no winner)
        if (game.MoveHistory.Count >= 9)
        {
            game.Status = GameStatus.Draw;
            game.Winner = null;
            game.WinningCells.Clear();

            if (!game.ScoreboardUpdated)
            {
                var scoreboard = _repository.GetScoreboard();
                scoreboard.RecordDraw();
                game.ScoreboardUpdated = true;
            }
        }
    }

    private GameStateResponse MapToResponse(GameSession game)
    {
        var scoreboard = _repository.GetScoreboard();
        var moveDtos = game.MoveHistory.Select(m => new MoveDto(
            m.MoveNumber,
            m.Player,
            m.Row,
            m.Col,
            m.PositionDescription,
            m.Timestamp
        )).ToList();

        var canUndo = game.Status == GameStatus.InProgress && game.MoveHistory.Count > 0;

        string message = game.Status switch
        {
            GameStatus.Won => $"Player {game.Winner} won the game!",
            GameStatus.Draw => "The game ended in a draw.",
            _ => $"Player {game.CurrentPlayer}'s turn"
        };

        return new GameStateResponse(
            game.Id,
            game.GetBoardJagged(),
            game.CurrentPlayer.ToString(),
            game.Mode.ToString(),
            game.Status.ToString(),
            game.Winner?.ToString(),
            game.WinningCells,
            moveDtos,
            new ScoreboardDto(scoreboard.XWins, scoreboard.OWins, scoreboard.Draws),
            canUndo,
            message
        );
    }
}
