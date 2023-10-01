#define DEBUG_LEVEL_0
#define DEBUG_LEVEL_1
// #define DEBUG_LEVEL_2
// #define DEBUG_LEVEL_3

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ChessChallenge.API;
using Microsoft.CodeAnalysis.CSharp.Syntax;


public class MyBot : IChessBot
{
    // Encoded weights
    readonly decimal[] weights = {
        930863346448640285287185407m,7152128374755509556405999636m,7457981884606853032728794904m,7457958291076502438356262169m,
        8700790718966295744331912476m,1235536411750502471696849415m,11185119132802642577679393569m,9327042962587387629354822948m,
        13674358989427919628264025389m,9018785822739634537921716251m,9006629954002542918322624285m,9640168825311740296883481642m,
        14605250889414811816811113520m,13982658943157607535821664047m,14912322695670626967413009969m,12431597486813187492364428063m,
        20506098391417600739658448964m,22369081524613027400274625090m,21129932577737760462411744324m,20197846066677561324892079940m,
        20820471271837025318295519812m,18645571114060838282179145846m,37281399664299843924949694070m,35424513291062664297526752375m,
        38524194241061158058769021567m,37590927248988366887416263281m,33861353233430477033232428147m,626185666502997479663010051m,
        1866619337663329836432687876m,77996309845402048919176218363m,309480046996976499834422016m,76434306443831925215299828468m,
        78607974122476449680517234946m,73009348484373096795294196712m,72082083509349759390828717801m,71458282564061301189850491628m,
        70227605507788612878372758500m,77991440900647974086967224051m,69279769790865299934023769564m,68972664559320598774197639132m,
        65245522755362263467218489552m,70203341544196347966863171817m,69918111198059329162391447524m,67728680019560515860136448722m,
        64628956253876386857288388305m,65554979154353059035198968021m,64313426597231365649796747471m,65552575599591834360517220315m,
        57794784944261680124036955580m,59651713855762199395052533433m,57791162815022061463868848826m,59029102817629280836102373055m,
        59032772262553537534535973308m,59959994621260758238494952330m,42566960703195639200600525448m,41639657894405174877808265352m,
        39774304244204049118186145153m,43183470278389863815039519884m,44125275519864633634159760785m,78598288753110427596685834747m,
        78604356938122533293245659897m,608028057269537540182440697m,308261917553061898379002369m,2790224570100435022504331278m,
    };
    readonly double scaling_factor = 137.067;
    readonly double shift = 0.0;

    #if DEBUG_LEVEL_1
        public MyBot()
        {
            Console.WriteLine("MyBot loaded!");

            // test FENs
            string[] testFENs = {
                "r1b1r1k1/5q1p/p1p2p1b/3pn2N/6p1/4B1N1/PPP2PPP/R2QR1K1 b - - 11 24",
                "rn1q2k1/ppp2pb1/3p2pB/8/2P1r1b1/5N2/PP1Q1PPP/2KR1B1R w - - 1 13",
                "r4rk1/1pp2pp1/p2b1q1p/2p1P3/6b1/2NP1N2/PPP2PPP/R2QR1K1 b - - 0 13",
                "2rn1r2/pP1q2bk/3pbppn/1p2p2p/2P1N2P/3PP1P1/4NPB1/1RBQK2R b K - 0 18",
                "6k1/5pp1/8/1p1q3P/3q2K1/8/5P2/8 w - - 3 44",
                "rnbqkbnr/ppp2ppp/8/3pp3/6B1/4P3/PPPP1PPP/RNBQK1NR b KQkq - 1 3",
                "3N1k2/7R/6p1/7p/3PpP2/4P1P1/3K4/6n1 b - - 4 45",
                "3qr1k1/p4ppp/b1p1p3/3Pn3/Prp1P3/4N1P1/2Q2PBP/3RR1K1 w - - 1 24",
                "1k1r1b1r/p1p2ppp/1pqp4/4pPP1/4P3/2PPQN2/PP1N2P1/R3K2R b KQ - 1 16",
                "2kr1b1r/ppp2ppp/2n5/4P3/8/2N3P1/PPPP3P/R1B1K2R b KQ - 2 12",
                "r2q1rk1/1b2bppp/p3pn2/np4B1/3P3Q/2N2N2/PPB2PPP/R4RK1 b - - 3 14",
            };

            ChessChallenge.Chess.Board testBoard = new ();
            foreach(string testFEN in testFENs)
            {
                testBoard.LoadPosition(testFEN);
                myBoard = new Board(testBoard);
                Evaluate();
                // Console.WriteLine(
                //     ("Eval of Test FEN (" + testFEN + "):").PadRight(90) +
                //     Evaluate().ToString().PadRight(10)
                // );
            }
            // Console.WriteLine("\n");
        }
    #endif

    public Board myBoard;

    private class Node {
        public double prior;
        public double value_sum;
        public int visits;
        public (Move, Node)[]? children;

        public Node(double prior, double value_sum, int visits, (Move, Node)[]? children) {
            this.prior = prior;
            this.value_sum = value_sum;
            this.visits = visits;
            this.children = children;
        }

        #if DEBUG_LEVEL_0 || DEBUG_LEVEL_1
            public void Print(Board board, int parent_visits, int depth)
            {
                Console.WriteLine(
                    "Depth: " + depth.ToString().PadRight(2) +
                    "Prior: " + Math.Round(prior, 3).ToString().PadRight(8) +
                    "Avg Value: " + Math.Round(visits > 0 ? value_sum / visits : 0, 4).ToString().PadRight(9) +
                    "Visits: " + visits.ToString().PadRight(3) + 
                    " => UCB: " + Math.Round(
                            (visits > 0 ? value_sum / visits : 0) +
                            prior * Math.Sqrt(parent_visits) / (visits + 1),
                        3
                    )
                );
                if (children != null && depth < 1)
                {
                    foreach ((Move, Node) child in children)
                    {
                        Console.Write("(" + child.Item1.ToString()[6..] + ") ");
                        board.MakeMove(child.Item1);
                        child.Item2.Print(board, visits, depth + 1);
                        board.UndoMove(child.Item1);
                    }
                }
            }
        #endif
    }

    public unsafe Move Think(Board board, Timer timer) {
        int transVisits = 0;

        myBoard = board;

        var transpositionTable = new Dictionary<ulong, Node>();

        (Move, Node)[] expand()
        {
            var e_ms = myBoard.GetLegalMoves().Select(m => (Evaluate_Move(m), m)).ToArray();
            double min = e_ms.Min(e_m => e_m.Item1) - 0.01;
            double sum = e_ms.Select(e_m => e_m.Item1 - min).Sum();
            return e_ms
                .Select(e_m => {
                    myBoard.MakeMove(e_m.m);
                    var key = myBoard.ZobristKey;
                    if(transpositionTable.ContainsKey(key))
                    {
                        Node node = transpositionTable[key];
                        #if DEBUG_LEVEL_0
                            transVisits += node.visits;
                        #endif
                        myBoard.UndoMove(e_m.m);
                        return (e_m.m, node);
                    }
                    else
                    {
                        var newNode = new Node((e_m.Item1 - min) / sum, 0.0, 0, null);
                        transpositionTable.Add(key, newNode);
                        myBoard.UndoMove(e_m.m);
                        return (e_m.m, newNode);
                    }
                })
                .ToArray();
        }

        double UCB(Node node, int parent_visits) =>
            (node.visits > 0 ? node.value_sum / node.visits : 0) +     // Q(s,a)
            (
                (Math.Log((1 + parent_visits + 19652) / 19652) + 1.25) * // C(s)
                node.prior *                                             // P(s)
                Math.Sqrt(parent_visits) / (node.visits + 1)             
            );

        Node root = new(1.0, 0.0, 0, null);
        transpositionTable.Add(myBoard.ZobristKey, root);

        #if DEBUG_LEVEL_0 || DEBUG_LEVEL_1
            int rollouts = 1;
        #endif

        while(timer.MillisecondsElapsedThisTurn < timer.MillisecondsRemaining * 0.05)
        {
            Node node = root;
            List<(Move, Node)> path = new() { (new Move(), node) };

            // selection
            while (node.visits > 0 && node.children?.Where(c => !path.Contains(c)).Count() > 0)
            {
                (var move, node) = node.children.Where(c => !path.Contains(c)).MaxBy(c => UCB(c.Item2, node.visits));
                myBoard.MakeMove(move);
                path.Add((move, node));
            }

            // expansion
            if (myBoard.GetLegalMoves().Length > 0)
            {
                #if DEBUG_LEVEL_1
                    Console.WriteLine("Expanding Position:\n"+ myBoard.CreateDiagram() + "("+node.visits+")");
                #endif
                node.children = expand();
                #if DEBUG_LEVEL_1
                    Console.Write("New Children: ");
                    foreach ((Move, Node) child in node.children)
                    {
                        Console.Write(child.Item1.ToString().Substring(5) + ", ");
                    }
                    Console.WriteLine("\n");
                #endif
                (var move, node) = node.children.MaxBy(c => UCB(c.Item2, node.visits));
                myBoard.MakeMove(move);
                path.Add((move, node));
            }

            // backpropagation
            double value = myBoard.IsDraw()
                ? 0.0
                : myBoard.IsInCheckmate()
                    ? 1.0
                    : Evaluate();

            for (int i = path.Count - 1; i >= 0; --i)
            {
                #if DEBUG_LEVEL_1
                    Console.WriteLine("Backpropagating: " + path[i].Item1.ToString().Substring(5) + " " + value);
                #endif
                path[i].Item2.value_sum += value;
                path[i].Item2.visits++;
                board.UndoMove(path[i].Item1);
                value *= -1;
            }

            #if DEBUG_LEVEL_0 || DEBUG_LEVEL_1
                rollouts++;
            #endif
        }
        #if DEBUG_LEVEL_0 || DEBUG_LEVEL_1
            Console.Write("\n\nSearch Tree after "+rollouts+" rollouts:\n('Root') ");
            root.Print(myBoard, 0, 0);
            Console.WriteLine();
            Console.WriteLine("\nRollouts: " + rollouts + "\nRollouts saved by transposition table: " + transVisits + "\nTransposition table size: " + transpositionTable.Count);
        #endif

        return root.children.MaxBy(c => c.Item2.visits).Item1;
    }

    private unsafe double Evaluate_Move(Move move)
    {
        // make move
        myBoard.MakeMove(move);

        // evaluate
        double eval = Evaluate();

        // undo move
        myBoard.UndoMove(move);

        return eval;
    }

    private unsafe double Evaluate()
    {
        // create network
        int[] layers = {768, 1}; // optimize later
        double[] neurons = new double[200+1]; // optimize later

        #if DEBUG_LEVEL_1
            int networkConnections = 0;
            for(int i = 1; i < layers.Length; i++)
            {
                networkConnections += layers[i-1] * layers[i];
            }
            Debug.Assert(weights.Length * 12 >= networkConnections, "Number of weights (" + weights.Length * 12 + ") needs to be larger than the number of network connections (" + networkConnections +")!");
        #endif

        #if DEBUG_LEVEL_2
            Console.WriteLine("\nBoard state:\n");
            Console.WriteLine("FEN: " + myBoard.GetFenString() + "\n");
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

        int distToMid(int index) => (int)Math.Abs(index - 3.5);

        fixed (double * neuron_po = neurons)
        {
            double * input_p = neuron_po;

            // construct state vector
            // starting with the pieces 
            foreach(PieceList pieceList in myBoard.GetAllPieceLists()) {
                foreach(Piece piece in pieceList) {
                    *(input_p +
                        pieceList.TypeOfPieceInList switch {
                            PieceType.Pawn => (
                                pieceList.IsWhitePieceList
                                    ? piece.Square.Rank - 1
                                    : 6 - piece.Square.Rank
                            ) * 4 + distToMid(piece.Square.File),
                            PieceType.Knight =>  distToMid(piece.Square.Rank) + distToMid(piece.Square.File),
                            _ => (
                                pieceList.IsWhitePieceList
                                    ? piece.Square.Rank
                                    : 7 - piece.Square.Rank
                            ) * 4 + distToMid(piece.Square.File)
                        }
                    ) += pieceList.IsWhitePieceList ^ myBoard.IsWhiteToMove ? 1 : -1;
                }
                input_p += pieceList.TypeOfPieceInList switch {
                    PieceType.Pawn => 24,
                    PieceType.Knight => 7,
                    PieceType.King => -127,
                    _ => 32
                };
            }

            input_p += 159;

            // Console.WriteLine(myBoard.CreateDiagram());

            // then check whether the king is in check
            *input_p++ = myBoard.IsInCheck() ? 1 : -1;

            // check king attacks
            foreach(PieceList pieceList in myBoard.GetAllPieceLists()) {
                foreach(Piece piece in pieceList) {
                    *(input_p + (pieceList.IsWhitePieceList ^ myBoard.IsWhiteToMove ? 1 : 0)) += BitboardHelper.GetNumberOfSetBits(
                        BitboardHelper.GetPieceAttacks(
                            pieceList.TypeOfPieceInList,
                            piece.Square,
                            myBoard,
                            pieceList.IsWhitePieceList
                        ) &
                        (
                            BitboardHelper.GetKingAttacks(myBoard.GetKingSquare(!pieceList.IsWhitePieceList)) |
                            myBoard.GetPieceBitboard(PieceType.King, !pieceList.IsWhitePieceList)
                        )
                    );
                }
            }
            input_p += 2;

            for(int i = 0; i < 162; i++) {
                Console.Write(Math.Round(neurons[i], 2));
            }
            Console.WriteLine();


            fixed(decimal * weight_po = weights)
            {
                double * output_p = neuron_po;
                sbyte * weight_p = (sbyte *) weight_po;

                // loop through layers
                for (int l = 1; l < layers.Length; l++)
                {
                    // loop through neurons
                    for(int n = 0; n < layers[l]; n++)
                    {
                        // reset input pointer
                        input_p -= layers[l-1];

                        // loop through inputs
                        for(int i = 0; i <= layers[l - 1]; i++)
                        {
                            // skip 4 bytes at the beginning of each group of 16 weights (one decimal)
                            if ((weight_p - (sbyte*) weight_po) % 16 == 0) weight_p += 4;

                            // compute weighted sum
                            *output_p += *input_p++ * (((double) *weight_p++) / scaling_factor + shift); // decode sbyte [-128, 127] to double [-2.0, 2.0]

                            #if DEBUG_LEVEL_3
                                input_p--; weight_p--; // undo increment temporarily
                                double input = i == layers[l - 1] ? 1 : *input_p;
                                Console.WriteLine(
                                    Math.Round(input, 2).ToString().PadRight(8) + "("+ (input_p - neuron_po) +") * " +
                                    Math.Round(((double) *weight_p) / scaling_factor + shift, 2).ToString().PadRight(8) + "(" + (weight_p - (sbyte*) weight_po) + ") -> " +
                                    Math.Round(*output_p, 2).ToString().PadRight(8) + "(" + (output_p - neuron_po) + ")"
                                );
                                input_p++; weight_p++; // redo increment
                            #endif
                        }

                        // apply ReLU activation function except for the last layer
                        *output_p++ *= Convert.ToDouble(*output_p > 0 || l == layers.Length - 1);
                    }
                }
            }
        }

        // return value of output neuron
        return Math.Tanh(neurons[^1]);
    }
}
