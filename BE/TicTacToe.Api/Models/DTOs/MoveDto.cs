namespace TicTacToe.Api.Models.DTOs;

public record MoveDto(
    int MoveNumber,
    string Player,
    int Row,
    int Col,
    string Position,
    DateTime Timestamp
);
