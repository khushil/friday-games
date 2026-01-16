using OXS.Core;

namespace OXS.Networking;

public abstract record NetworkMessage {
    public sealed record MoveRequest(int Row, int Col) : NetworkMessage;
    public sealed record MoveConfirmed(int Row, int Col, PlayerId Player) : NetworkMessage;
    public sealed record GameState(byte[] BoardData, PlayerId CurrentPlayer, int Phase) : NetworkMessage;
    public sealed record PlayerAssignment(PlayerId AssignedPlayer) : NetworkMessage;
    public sealed record GameOver(int ResultType, PlayerId? Winner) : NetworkMessage;
    public sealed record StartNewRound(PlayerId StartingPlayer) : NetworkMessage;
    public sealed record Ping : NetworkMessage;
    public sealed record Pong : NetworkMessage;
}
