using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    // Encoded weights
    decimal[] weights = {
        54663474501337569957812117520m,
    }; 

    public unsafe MyBot()
    {
        fixed (decimal * weight_po = weights)
        {
            for (
                sbyte * weight_p = (sbyte *) weight_po;
                weight_p < weight_po + weights.Length;
                weight_p++
            )
            {
                // Decode sbyte [-128, 127] to double [-2.0, 2.0]
                double weight = ((double) *weight_p) / 64;
                Console.WriteLine(weight);
            }
        }
    }
    public unsafe Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        return moves[0];
    }
}
