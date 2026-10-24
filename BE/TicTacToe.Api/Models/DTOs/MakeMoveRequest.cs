namespace TicTacToe.Api.Models.DTOs;

public record MakeMoveRequest(
    string Player,
    int Row,
    int Col
);
