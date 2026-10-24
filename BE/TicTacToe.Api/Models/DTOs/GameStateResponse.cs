using TicTacToe.Api.Models.Domain;

namespace TicTacToe.Api.Models.DTOs;

public record GameStateResponse(
    Guid GameId,
    string?[][] Board,
    string CurrentPlayer,
    string GameMode,
    string GameStatus,
    string? Winner,
    List<CellPosition> WinningCells,
    List<MoveDto> MoveHistory,
    ScoreboardDto Scoreboard,
    bool CanUndo,
    string? Message = null
);
