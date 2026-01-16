namespace OXS.Core.AI;

public interface IAIPlayer {
    Move SelectMove(Board board, PlayerId player, int winLength);
}
