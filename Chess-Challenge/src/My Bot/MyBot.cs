#define DEBUG_LEVEL_1
// #define DEBUG_LEVEL_2

using System;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;
using Microsoft.CodeAnalysis;

public class MyBot : IChessBot
{
    // Encoded weights
    readonly decimal[] weights = {
        8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,
        8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,
        8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,
        8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,
        8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,
        8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,
        8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,
        8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,8200574590949850612662389230m,
    };
    readonly double scaling_factor = 73.143;
    readonly double shift = -0.25;

    public unsafe MyBot()
    {

    }

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        double[] evaluations = new double[moves.Length];

        #if DEBUG_LEVEL_1
            Console.WriteLine("\n-----------------------\nNext move\n");
        #endif

        // evaluate each move
        for (int i = 0; i < moves.Length; i++)
        {
            // make move
            board.MakeMove(moves[i]);

            // evaluate board
            evaluations[i] = Evaluate(board);

            #if DEBUG_LEVEL_1
                Console.WriteLine("Move: " + moves[i].ToString() + " Eval: " + evaluations[i]);
            #endif

            // undo move
            board.UndoMove(moves[i]);
        }

        // find best move
        return moves[0];
    }

    private unsafe double Evaluate(Board board)
    {
        // create neurons for a (12, 12, 1) network
        int[] layers = {768, 1}; // optimize later
        double[] neurons = new double[768 + 1]; // optimize later

        #if DEBUG
        int networkConnections = 0;
        for(int i = 1; i < layers.Length; i++)
        {
            networkConnections += layers[i-1] * layers[i];
        }
        Debug.Assert(weights.Length * 12 == networkConnections, "Number of weights (" + weights.Length * 12 + ") needs to match network connections (" + networkConnections +")!");
        #endif

        fixed (double * neuron_po = neurons)
        {
            double * input_p = neuron_po;

            // copy board state to neurons
            #if DEBUG_LEVEL_2
                Console.WriteLine("\nBoard:\n");
            #endif
            foreach (PieceList pieceList in board.GetAllPieceLists())
            {
                #if DEBUG_LEVEL_2
                    Console.WriteLine("\nPiece: " + pieceList.TypeOfPieceInList + " (" + (pieceList.IsWhitePieceList ? "white" : "black") +")\n");
                #endif
                for (int i = 0; i < 64; i++) {
                    *input_p++ = board.GetPieceBitboard(pieceList.TypeOfPieceInList, pieceList.IsWhitePieceList) >> i & 1;
                    #if DEBUG_LEVEL_2
                        Console.Write(*(input_p - 1) + " ");
                        if (i % 8 == 7) Console.WriteLine();
                    #endif
                }
            }

            #if DEBUG_LEVEL_2
                Console.WriteLine("\n\nNeurons:\n");
            #endif

            // compute feed forward
            fixed(decimal * weight_po = weights)
            {
                double * output_p = neuron_po + layers[0];
                sbyte * weight_p = (sbyte *) weight_po;

                // loop trough layers
                for (int l = 1; l < layers.Length; l++)
                {
                    // loop trough neurons
                    for(int n = 0; n < layers[l]; n++)
                    {
                        // reset input pointer to beginning of layer
                        input_p -= layers[l - 1];

                        // loop trough inputs
                        for(int i = 0; i < layers[l - 1]; i++)
                        {
                            // skip 4 bytes at the beginning of each group of 16 weights (one decimal)
                            if ((weight_p - (sbyte*) weight_po) % 16 == 0) weight_p += 4;

                            // compute weighted sum
                            *output_p += *input_p * (((double) *weight_p) / scaling_factor + shift); // decode sbyte [-128, 127] to double [-2.0, 2.0]

                            #if DEBUG_LEVEL_2
                                Console.WriteLine(
                                    Math.Round(*input_p, 2).ToString().PadRight(8) + "("+ (input_p - neuron_po) +") * " +
                                    Math.Round(((double) *weight_p) / scaling_factor + shift, 2).ToString().PadRight(8) + "(" + (weight_p - (sbyte*) weight_po) + ") -> " +
                                    Math.Round(*output_p, 2).ToString().PadRight(8) + "(" + (output_p - neuron_po) + ")"
                                );
                            #endif

                            // increment pointers
                            input_p++;
                            weight_p++;
                        }

                        // apply activation function
                        *output_p = Math.Tanh(*output_p);

                        // increment pointers
                        output_p++;
                    }
                }
            }
        }

        #if DEBUG_LEVEL_2
            Console.WriteLine("Eval: " + neurons[^1]);
        #endif

        // return value of output neuron
        return neurons[^1];
    }
}
