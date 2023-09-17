#define DEBUG_LEVEL_1
// #define DEBUG_LEVEL_2
// #define DEBUG_LEVEL_3

using System;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;
using Microsoft.CodeAnalysis;

public class MyBot : IChessBot
{
    // Encoded weights
    readonly decimal[] weights = {
        2793865459972087928708466697m,7456749218187052466025404184m,7142438041765990498263503893m,8076928292633013519134496791m,
        5908134205212343792718778899m,1866619374995671734304383518m,13051766822315317737051598377m,12429174968797082485908975664m,
        13366092442393724145466289708m,11495874735691179093610144551m,11182734171924565328662828073m,9339160389540726307969577266m,
        14903874419431506729470275121m,13673178619601607058313457970m,14911118602971140949424613938m,13054180154288887579892393513m,
        19884705687017526381797981504m,20196627585589190232660526157m,19571631844471182244773250112m,19273003537254710847616402241m,
        19890764704573155618501115452m,17406407908858097000830891117m,33245990907542249943929088619m,36342035765124945714277411178m,
        32940109045262561637413841259m,35102858465101601374193870441m,30458156253982889281814620520m,3105763543147498422651718389m,
        1251332819003894985759852554m,2172520127462393957656952831m,935732269858699432575305990m,7121782040604684684783192075m,
        1555911252283150156012324104m,76433111833046569370821195764m,77365174603073955535936682743m,76740155287161985306839414515m,
        78294829077815414075797469181m,2177303863221709145154841597m,70832006950659421777280819172m,71141529610211675938267914467m,
        69595304042193289455442451678m,72381882723647993871866455518m,70233655023104773074365115116m,68972716708053029653506874596m,
        68972683651560275590342697438m,69284591124778360785453572311m,68353704021525335842175704285m,69902347607277119834555668196m,
        63386147473479037364298829005m,61826614109319874835380947658m,62448016075428880126470443212m,59361638049610858047642389444m,
        64937194466927859029743946955m,65857224772740567020916283804m,50333194949652190361525527203m,49092841873543961263843944090m,
        50336821838007673672528272802m,50953364506011686380870410655m,52191247828646681462075859871m,2171245033237072634326680069m,
        1245207890717128756978975500m,78910210469090987154491045881m,2486755523571751346581997579m,314354046439543704564336380m,
    };
    readonly double scaling_factor = 59.549;
    readonly double shift = -0.11228899999999986;

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        double[] evaluations = new double[moves.Length];

        // evaluate each move
        for (int i = 0; i < moves.Length; i++)
        {
            #if DEBUG_LEVEL_1
                Console.WriteLine("\n-----------------------\n" + moves[i].ToString());
            #endif

            // make move
            board.MakeMove(moves[i]);

            // evaluate board
            evaluations[i] = Evaluate(board);

            #if DEBUG_LEVEL_1
                Console.WriteLine("\nEval: " + evaluations[i] * 645);
            #endif

            // undo move
            board.UndoMove(moves[i]);
        }

        // find best move
        return moves[evaluations.ToList().IndexOf(evaluations.Max())];
    }

    private unsafe double Evaluate(Board board)
    {
        // load board state
        ulong[] boardState = board.GetAllPieceLists().Select(pieceList =>
            board.GetPieceBitboard(pieceList.TypeOfPieceInList, pieceList.IsWhitePieceList)
        ).ToArray();

        // create network
        int[] layers = {768, 1}; // optimize later
        double[] neurons = new double[1]; // optimize later

        #if DEBUG_LEVEL_1
            int networkConnections = 0;
            for(int i = 1; i < layers.Length; i++)
            {
                networkConnections += layers[i-1] * layers[i];
            }
            Debug.Assert(weights.Length * 12 == networkConnections, "Number of weights (" + weights.Length * 12 + ") needs to match network connections (" + networkConnections +")!");
        #endif

        #if DEBUG_LEVEL_2
            Console.WriteLine("\nBoard state:\n");
            Console.WriteLine("FEN: " + board.GetFenString() + "\n");
            Console.WriteLine("Binary: ");
            for (int s = 0; s < 768; s++)
            {
                Console.Write(boardState[s / 64] >> (s % 64) & 1);
                if ((s + 1) % 8 == 0) Console.Write(" ");
                if ((s + 1) % 64 == 0) Console.WriteLine();
            }
        #endif

        // compute feed forward
        #if DEBUG_LEVEL_3
            Console.WriteLine("\nNeurons:\n");
        #endif

        fixed (double * neuron_po = neurons)
        {
            fixed(decimal * weight_po = weights)
            {
                double * input_p = neuron_po;
                double * output_p = neuron_po;
                sbyte * weight_p = (sbyte *) weight_po;

                // loop through layers
                for (int l = 1; l < layers.Length; l++)
                {
                    // loop through neurons
                    for(int n = 0; n < layers[l]; n++)
                    {
                        // loop through inputs
                        for(int i = 0; i < layers[l - 1]; i++)
                        {
                            // skip 4 bytes at the beginning of each group of 16 weights (one decimal)
                            if ((weight_p - (sbyte*) weight_po) % 16 == 0) weight_p += 4;

                            // compute weighted sum
                            *output_p +=
                                (l == 1 ? (boardState[i / 64] >> (i % 64) & 1) : *input_p++) * // for the first layer, use the board state as input
                                (((double) *weight_p++) / scaling_factor + shift); // decode sbyte [-128, 127] to double [-2.0, 2.0]

                            #if DEBUG_LEVEL_3
                                input_p--; weight_p--; // undo increment temporarily
                                double input = l == 1 ? (boardState[i / 64] >> (i % 64) & 1) : *input_p;
                                Console.WriteLine(
                                    Math.Round(input, 2).ToString().PadRight(8) + "("+ (l == 1 ? i : input_p - neuron_po) +") * " +
                                    Math.Round(((double) *weight_p) / scaling_factor + shift, 2).ToString().PadRight(8) + "(" + (weight_p - (sbyte*) weight_po) + ") -> " +
                                    Math.Round(*output_p, 2).ToString().PadRight(8) + "(" + (output_p - neuron_po) + ")"
                                );
                                input_p++; weight_p++; // redo increment
                            #endif
                        }

                        // apply activation function
                        // *output_p = Math.Tanh(*output_p);

                        // increment/reset pointers
                        output_p++;
                        input_p -= l > 1 ? layers[l - 1] : 0;
                    }
                }
            }
        }

        // return value of output neuron
        return neurons[^1];
    }
}
