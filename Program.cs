using System.Collections;

namespace Nonogram
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Would you like to 1) solve a nonogram or 2) test the algorithms:");

                char c = Console.ReadKey().KeyChar;
                while (c != '1' && c != '2')
                {
                    Console.WriteLine("Invalid input. Try agian.");
                    c = Console.ReadKey().KeyChar;
                }
                bool returnValue = false;
                switch (c)
                {
                    case '1':
                        SolveNonogram();
                        break;
                    case '2':
                        testNonogram();
                        break;
                }
                Console.WriteLine();
            }
        }
        static void SolveNonogram()
        {
            Console.WriteLine("Specify the nonogram you would like to solve:");
            Nonogram nonogram;
            while(true)
            {
                string filePath = Console.ReadLine();
                try
                {
                    nonogram = new Nonogram(filePath);
                    break;
                }
                catch(Exception e)
                {
                    Console.WriteLine("Not a valid file. Please try again:");
                }
            }

            Solvers solver = new Solvers(nonogram);

            Console.WriteLine("Select which algorithm you would like to use to solve the puzzle:");
            Console.WriteLine("1) Backtracking on cell level");
            Console.WriteLine("2) Backtracking on line level");
            Console.WriteLine("3) Overlap on line level. Memory optimized");
            Console.WriteLine("4) Overlap on line level. Time optimzied");
            Console.WriteLine("5) Overlap on block level");

            bool run = true;
            while (run)
            {
                char c = Console.ReadKey().KeyChar;
                switch (c)
                {
                    case '1':
                        solver.BacktrackByIndex();
                        run = false;
                        break;
                    case '2':
                        solver.BacktrackByPossibilities();
                        run = false;
                        break;
                    case '3':
                        solver.OverlapMemory();
                        run = false;
                        break;
                    case '4':
                        solver.OverlapTime();
                        run = false;
                        break;
                    case '5':
                        solver.OverlapByConstraints();
                        run = false;
                        break;
                }
            }
               
            return;
        }
        static void testNonogram()
        {
            Console.WriteLine("Specify the folder of nonograms you would like to test:");
            string testDirectory = Console.ReadLine();
            Console.WriteLine("Specify where to output the results:");
            string outputDirectory = Console.ReadLine();
            Console.WriteLine("Specifiy how many seconds to test for:");
            int timeSeconds = int.Parse(Console.ReadLine());
            
            Tester tester = new Tester();

            tester.Test(timeSeconds, testDirectory, outputDirectory);
        }
    }
}
/*
@"..\..\..\..\NonogramTest"
@"C:\Users\casbo\OneDrive\KI 2022-2023\Thesis\output.csv"
*/