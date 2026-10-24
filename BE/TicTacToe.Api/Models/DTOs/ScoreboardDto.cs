namespace TicTacToe.Api.Models.DTOs;

public record ScoreboardDto(
    int XWins,
    int OWins,
    int Draws
);
