// #define DEBUG

using System;
using System.Diagnostics;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    // Encoded weights
    decimal[] weights = {
        39730440962195658276260069600m,39730440962195658276260069600m,39730440962195658276260069600m,39730440962195658276260069600m,
        39730440962195658276260069600m,39730440962195658276260069600m,39730440962195658276260069600m,39730440962195658276260069600m,
        39730440962195658276260069600m,39730440962195658276260069600m,39730440962195658276260069600m,39730440962195658276260069600m,
        39730440962195658276260069600m,
    }; 

    public unsafe MyBot()
    {

        // create neurons for a (12, 12, 1) network
        int[] layers = {12, 12, 1}; // optimize later
        double[] neurons = new double[12 + 12 + 1]; // optimize later

        #if DEBUG
        Debug.Assert(weights.Length * 12 == (layers[0] * layers[1] + layers[1] * layers[2]), "Number of weights needs to match network connections!");
        #endif

        // copy board state to neurons
        for (int i = 0; i < 12; i++)
        {
            neurons[i] = 1.0;
        }

        // compute feed forward
        fixed (double * neuron_po = neurons)
        {
            fixed(decimal * weight_po = weights)
            {
                double * input_p = neuron_po, output_p = neuron_po + 12;
                sbyte * weight_p = (sbyte *) weight_po;

                // loop trough layers
                for (int l = 1; l < layers.Length; l++)
                {
                    // loop trough neurons
                    for(int n = 0; n < layers[l]; n++)
                    {
                        // loop trough inputs
                        for(int i = 0; i < layers[l - 1]; i++)
                        {
                            // skip 4 bytes at the beginning of each group of 16 weights (one decimal)
                            if ((weight_p - (sbyte*) weight_po) % 16 == 0) weight_p += 4;

                            // compute weighted sum
                            *output_p += *input_p * ((double) *weight_p) / 64; // decode sbyte [-128, 127] to double [-2.0, 2.0]

                            #if DEBUG
                            Console.WriteLine(
                                (*input_p).ToString().PadRight(4) + "("+(input_p - neuron_po)+") * " +
                                (((double) *weight_p) / 64).ToString().PadRight(4) + "(" + (weight_p - (sbyte*) weight_po) + ") -> " +
                                (*output_p).ToString().PadRight(4) + "(" + (output_p - neuron_po) + ")"
                            );
                            #endif

                            // increment pointers
                            input_p++;
                            weight_p++;
                        }

                        // apply activation function
                        *output_p = Math.Tanh(*output_p);

                        // increment/reset pointers
                        output_p++;
                        input_p -= layers[l - 1];
                    }

                    // move input pointer to next layer
                    input_p += layers[l - 1];
                }
            }
        }
        Console.WriteLine("Eval: " + neurons[^1]);
    }

    public unsafe Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        return moves[0];
    }
}
