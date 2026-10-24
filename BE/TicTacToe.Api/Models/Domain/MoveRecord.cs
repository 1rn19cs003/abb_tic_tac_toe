namespace TicTacToe.Api.Models.Domain;

public record MoveRecord(
    int MoveNumber,
    string Player,
    int Row,
    int Col,
    string PositionDescription,
    DateTime Timestamp
);
