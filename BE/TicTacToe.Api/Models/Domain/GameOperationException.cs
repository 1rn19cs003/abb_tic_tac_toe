namespace TicTacToe.Api.Models.Domain;

public class GameOperationException : Exception
{
    public int StatusCode { get; }

    public GameOperationException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
