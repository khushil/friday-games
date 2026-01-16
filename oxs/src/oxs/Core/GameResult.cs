namespace OXS.Core;

public abstract record GameResult {
    public sealed record Win(PlayerId Winner, ImmutableArray<Move> WinningLine) : GameResult;
    public sealed record Draw : GameResult;
    public sealed record InProgress : GameResult;
}
