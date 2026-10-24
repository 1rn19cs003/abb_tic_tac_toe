using System.Text.Json.Serialization;
using TicTacToe.Api.Models.Enums;

namespace TicTacToe.Api.Models.DTOs;

public record CreateGameRequest(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    GameMode Mode = GameMode.TwoPlayer
);
