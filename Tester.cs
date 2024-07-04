using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

namespace Nonogram
{
    internal class Tester
    {
        Solvers solvers;
        string[] files;

        public void Test(int time,string testDirectory,string outputFile)
        {
            try
            {
                //string testDirectory = @"..\..\..\..\NonogramTest";
                if (Directory.Exists(testDirectory))
                {
                    files = Directory.GetFiles(testDirectory);
                }
                else
                {
                    Console.WriteLine("The specified folder does not exist");
                    return;
                }
                ulong[][] data = new ulong[files.Length][];

                Nonogram nonogram;
                Solvers solver;
                for (int i = 0; i < files.Length; i++)
                {
                    data[i] = new ulong[20]; // Initialize the inner array to store time, memory usage, and completeness for four methods

                    Stopwatch sw = new Stopwatch();

                    Console.WriteLine($"Solving puzzle: {files[i]}");
                    // Test BacktrackByIndex
                    nonogram = new Nonogram(files[i]);
                    solver = new Solvers(nonogram);

                    Console.WriteLine("Using BacktrackByIndex:");
                    sw.Start();
                    var (peakMemoryBacktrack,backtrackSolved ,backtrackCompleted) = MeasureMemoryUsageDuring(() => solver.BacktrackByIndex(), TimeSpan.FromSeconds(time));
                    sw.Stop();

                    if (backtrackCompleted)
                    {
                        data[i][0] = (ulong) sw.ElapsedMilliseconds;
                        data[i][1] = peakMemoryBacktrack;
                        data[i][2] = solver.iteration; // 0 for false, 1 for true
                        data[i][3] = (ulong)(backtrackSolved ? 1 : 0);
                    }
                    else
                    {
                        data[i][0] = (ulong)(time*1000); // Indicate timeout
                        data[i][1] = peakMemoryBacktrack; // Record peak memory even on timeout
                        data[i][2] = solver.iteration; // Incomplete due to timeout
                        data[i][3] = 0;
                    }

                    //Force garbage collection
                    ForceGarbageCollection();

                    // Test BacktrackByLine
                    nonogram = new Nonogram(files[i]);
                    solver = new Solvers(nonogram);

                    Console.WriteLine("Using BacktrackOverlap");
                    sw.Restart();
                    var (peakMemoryBacktrackLine, backtrackLineSolved,backtrackLineCompleted) = MeasureMemoryUsageDuring(() => solver.BacktrackByPossibilities(), TimeSpan.FromSeconds(time));
                    sw.Stop();

                    if (backtrackLineCompleted)
                    {
                        data[i][4] = (ulong) sw.ElapsedMilliseconds;
                        data[i][5] = peakMemoryBacktrackLine;
                        data[i][6] = solver.iteration; // 0 for false, 1 for true
                        data[i][7] = (ulong)(backtrackLineSolved ? 1 : 0);
                    }
                    else
                    {
                        data[i][4] = (ulong)(time * 1000); // Indicate timeout
                        data[i][5] = peakMemoryBacktrackLine; // Record peak memory even on timeout
                        data[i][6] = solver.iteration; // Incomplete due to timeout
                        data[i][7] = 0;
                    }
                    
                    //Force garbage collection
                    ForceGarbageCollection();

                    // Test OverlapTime
                    nonogram = new Nonogram(files[i]);
                    solver = new Solvers(nonogram);

                    Console.WriteLine("Using Overlap memory optimized");

                    sw.Restart();
                    var (peakMemoryOverlapTime,overlapTimeSolved,overlapTimeCompleted) = MeasureMemoryUsageDuring(() => solver.OverlapTime(), TimeSpan.FromSeconds(time));
                    sw.Stop();

                    if (overlapTimeCompleted)
                    {
                        data[i][8] = (ulong)sw.ElapsedMilliseconds;
                        data[i][9] = peakMemoryOverlapTime;
                        data[i][10] = solver.iteration; // 0 for false, 1 for true
                        data[i][11] = (ulong)(overlapTimeSolved ? 1 : 0);
                    }
                    else
                    {
                        data[i][8] = (ulong)(time * 1000); // Indicate timeout
                        data[i][9] = peakMemoryOverlapTime; // Record peak memory even on timeout
                        data[i][10] = solver.iteration; // Incomplete due to timeout
                        data[i][11] = 0;
                    }
                    //Force garbage collection
                    ForceGarbageCollection();

                    // Test OverlapMemory
                    nonogram = new Nonogram(files[i]);
                    solver = new Solvers(nonogram);

                    Console.WriteLine("Using Overlap time optimized");

                    sw.Restart();
                    var (peakMemoryOverlap,overlapSolved,overlapCompleted) = MeasureMemoryUsageDuring(() => solver.OverlapMemory(), TimeSpan.FromSeconds(time));
                    sw.Stop();

                    if (overlapCompleted)
                    {
                        data[i][12] = (ulong) sw.ElapsedMilliseconds;
                        data[i][13] = peakMemoryOverlap;
                        data[i][14] = solver.iteration; // 0 for false, 1 for true
                        data[i][15] = (ulong)(overlapSolved ? 1 : 0);
                    }
                    else
                    {
                        data[i][12] = (ulong)(time * 1000); // Indicate timeout
                        data[i][13] = peakMemoryOverlap; // Record peak memory even on timeout
                        data[i][14] = solver.iteration; // Incomplete due to timeout
                        data[i][15] = 0;
                    }
                    //Force garbage collection
                    ForceGarbageCollection();

                    // Test OverlapByConstraints
                    nonogram = new Nonogram(files[i]);
                    solver = new Solvers(nonogram);

                    Console.WriteLine("Using Overlap on Constraints");
                    sw.Restart();
                    var (peakMemoryOverlapConstraints,overlapConstraintsSolved, overlapConstraintsCompleted) = MeasureMemoryUsageDuring(() => solver.OverlapByConstraints(), TimeSpan.FromSeconds(time));
                    sw.Stop();

                    if (overlapConstraintsCompleted)
                    {
                        data[i][16] = (ulong)sw.ElapsedMilliseconds;
                        data[i][17] = peakMemoryOverlapConstraints;
                        data[i][18] = solver.iteration; // 0 for false, 1 for true
                        data[i][19] = (ulong)(overlapConstraintsSolved? 1 : 0);

                    }
                    else
                    {
                        data[i][16] = (ulong)(time*1000); // Indicate timeout
                        data[i][17] = peakMemoryOverlapConstraints; // Record peak memory even on timeout
                        data[i][18] = solver.iteration;
                        data[i][19] = (ulong)(overlapConstraintsSolved ? 1 : 0);
                    }

                    //Force garbage collection
                    ForceGarbageCollection();
                }

                WriteDataToCSV(data,outputFile);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }

        private (ulong peakMemory, bool solved, bool completed) MeasureMemoryUsageDuring(Func<bool> action, TimeSpan timeout)
        {
            ulong peakMemory = 0;
            bool completed = false;
            ulong result = 0;
            bool solved = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var memoryTask = Task.Run(() =>
                {
                    Process currentProcess = Process.GetCurrentProcess();
                    while (!cts.Token.IsCancellationRequested)
                    {
                        currentProcess.Refresh();
                        ulong memoryUsage = (ulong) currentProcess.PrivateMemorySize64;
                        if (memoryUsage > peakMemory)
                        {
                            peakMemory = memoryUsage;
                        }
                        Thread.Sleep(5); // Check memory usage every 5ms
                    }
                }, cts.Token);

                try
                {
                    Task<bool> actionTask = Task.Run(action, cts.Token);
                    completed = actionTask.Wait(timeout);
                    if (completed)
                    {
                        solved = actionTask.Result;
                    }
                    else
                    {
                        Console.WriteLine($"Timeout occurred");
                    }
                }
                catch (AggregateException ae)
                {
                    ae.Handle((x) => x is OperationCanceledException);
                }
                finally
                {
                    cts.Cancel(); // Ensure the memory measuring task is cancelled
                    try
                    {
                        memoryTask.Wait(); // Wait for the memory measuring task to complete
                    }
                    catch (AggregateException ae)
                    {
                        ae.Handle((x) => x is OperationCanceledException);
                    }
                }
            }
            return (peakMemory,solved, completed);
        }
        private void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void WriteDataToCSV(ulong[][] data, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath+ @"\output.csv"))
                {
                    // Write header
                    writer.WriteLine("Backtrack Time,Backtrack Memory,Backtrack Iterations,Backtrack Complete," +
                                     "BacktrackLine Time,BacktrackLine Memory,BacktrackLine Iterations,BacktrackLine Complete," +
                                     "Overlap mo Time,Overlap mo Memory,Overlap mo Iterations,Overlap mo Complete," +
                                     "Overlap to Time,Overlap to Memory,Overlap to Iterations,Overlap to Complete," +
                                     "OverlapConstraints Time,OverlapConstraints Memory,OverlapConstraints Iterations,OverlapConstraints Complete");

                    // Write data
                    foreach (var testCase in data)
                    {
                        writer.WriteLine($"{testCase[0]},{testCase[1]},{testCase[2]},{testCase[3]}," +
                                         $"{testCase[4]},{testCase[5]},{testCase[6]},{testCase[7]}," +
                                         $"{testCase[8]},{testCase[9]},{testCase[10]},{testCase[11]},"+
                                         $"{testCase[12]},{testCase[13]},{testCase[14]},{testCase[15]}," +
                                         $"{testCase[16]},{testCase[17]},{testCase[18]},{testCase[19]}"
                                         );
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error writing data to CSV file: {e.Message}");
            }
        }
    }
}
