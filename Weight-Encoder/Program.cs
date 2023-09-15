// #define VERBOSE

using System;
using System.Diagnostics;

namespace WeightEncoder
{
    static class Program
    {
        public unsafe static void Main()
        {
            double[] weights = {
                -.1, -1.75, -1.5, -1.25,
                0.25, 0.5, 0.75, 1.0,
                1.25, 1.5, 1.75, 1.99,
                -.1, -1.75, -1.5, -1.25,
                0.25, 0.5, 0.75, 1.0,
                1.25, 1.5, 1.75, 1.99,
                -.1, -1.75, -1.5, -1.25,
                0.25, 0.5, 0.75, 1.0,
                1.25, 1.5, 1.75, 1.99,
                -.1, -1.75, -1.5, -1.25,
                0.25, 0.5, 0.75, 1.0,
                1.25, 1.5, 1.75, 1.99,
            };

            // Check that weights are divisible by 12
            Debug.Assert(weights.Length % 12 == 0, "Weights need to be divisible by 12");

            // Decimals are 16 bytes so we need to make room for the exta 4 bytes
            sbyte[] weight_bytes = new sbyte[weights.Length * 4 / 3];

            decimal[] decimals = new decimal[weights.Length / 12];

            fixed (double * weight_po = weights)
            {
                fixed (sbyte * weight_bytes_po = weight_bytes)
                {
                    fixed (decimal * decimals_po = decimals)
                    {
                        double * weight_p = weight_po;
                        sbyte * weight_byte_p = weight_bytes_po;
                        decimal * decimal_p = decimals_po;

                        // Loop over all groups of twelve weights
                        for (int i = 0; i < weights.Length / 12; i++)
                        {
                            // Leave 4 bytes empty at the beginning of each group
                            weight_byte_p += 4;

                            // Loop over all weights in the group
                            for (int j = 0; j < 12; j++)
                            {
                                // Convert doubles [-2.0, 2.0] to sbyte [-128, 127]
                                sbyte weight_byte = (sbyte) Math.Round(*weight_p * 64, 0);

                                // Copy the weight byte into the array
                                *weight_byte_p = weight_byte;

                                #if VERBOSE
                                // Print weight conversion
                                Console.WriteLine("\n\nWeight\t (*64)\t\tsbyte\t(Binary)\n");
                                Console.WriteLine(
                                    *weight_p + "\t " + ("(" + (*weight_p * 64) + ")").PadRight(10) + "->\t" + *weight_byte_p + 
                                    "\t(" + Convert.ToString((byte)*weight_byte_p, 2).PadLeft(8, '0') + ")"
                                );
                                #endif

                                // Increment pointers
                                weight_p++;
                                weight_byte_p++;
                            }

                            // Set pointer back to the beginning of the group
                            weight_byte_p -= 16;

                            #if VERBOSE
                            // Print contents of memory
                            Console.Write("\nMemory: ");
                            for(int j = 0; j < 16; j++)
                            {
                                Console.Write(Convert.ToString((byte)*(weight_byte_p + j), 2).PadLeft(8, '0') + " ");
                                if(i % 4 == 3) Console.Write(" ");
                            }
                            Console.Write("\nDecimal: ");
                            #endif

                            // Every 16 bytes is a decimal
                            *decimal_p = *(decimal*)(weight_byte_p);
                            Console.WriteLine(*decimal_p + "m,");

                            // Increment pointers
                            weight_byte_p += 16;
                            decimal_p++;
                        }
                    }
                }
            }
        }
    }
}