namespace TicTacToe.Api.Models.Domain;

public class Scoreboard
{
    private int _xWins;
    private int _oWins;
    private int _draws;

    public int XWins => _xWins;
    public int OWins => _oWins;
    public int Draws => _draws;

    public Scoreboard(int xWins = 0, int oWins = 0, int draws = 0)
    {
        _xWins = xWins;
        _oWins = oWins;
        _draws = draws;
    }

    public void RecordXWin() => Interlocked.Increment(ref _xWins);
    public void RecordOWin() => Interlocked.Increment(ref _oWins);
    public void RecordDraw() => Interlocked.Increment(ref _draws);

    public void Reset()
    {
        Interlocked.Exchange(ref _xWins, 0);
        Interlocked.Exchange(ref _oWins, 0);
        Interlocked.Exchange(ref _draws, 0);
    }
}
