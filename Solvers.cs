using System.Collections;
using System.Numerics;
namespace Nonogram
{
    /* 
     * Class implementing the different solving methods
     */
    class Solvers
    {

        Nonogram nonogram = null;
        public ulong iteration = 0;
        
        public Solvers(Nonogram value)
        {
            nonogram = value;
        }
        /* Method implementing a simple backtracking algorithm. 
         * This algorithm goes through the nonogram by evaluating each cell by starting left on the top row and ending right in the bottom row.
         * A cell value is rejected when the row and column where the cell is located can no longer match the constraints for that row or column.
         */
        public bool BacktrackByIndex(int verbose = 0)
        {
            NonogramPrinter printNonogram = new NonogramPrinter(nonogram);

            BitArray grid = new BitArray(nonogram.Grid.Length);

            int index = 0;
            iteration = 0;

            while (!(index == grid.Length))
            {
                iteration++;
                if (verbose > 0)
                {
                    int row = index / nonogram.Width + 1;
                    int col = index % nonogram.Width + 1;
                    if (verbose == 2)
                        printNonogram.print(grid, nonogram.Width, nonogram.Height);
                    Console.WriteLine(String.Format("Iteration:{0},Evaluating pixel:({1},{2}).", iteration, col, row));
                }

                //Change state of cell.
                grid[index] = !grid[index];

                /* Check if grid is still valid.
                 * If not than backtrack to the closest cell that has value true.
                 * Else proceed to next cell.
                 */
                if (Check())
                {
                    index++;
                }
                else if (!grid[index])
                {
                    index--;
                    while (!grid[index])
                    {
                        grid[index] = false;
                        index--;
                    }
                }
            }

            nonogram.Grid = grid;
            printNonogram.print();
            Console.WriteLine(String.Format("Nonogram solved in {0} iterations.", iteration));

            return true;

            /* Function for checking whether the grid is still valid.
             * Checks only the row and column the cell is located in.
             */
            bool Check()
            {

                int row = index / nonogram.Width;
                int col = index % nonogram.Width;

                byte[] rowConstraints = nonogram.Rows[row];
                byte[] colConstraints = nonogram.Columns[col];

                //Checks if the row is still valid only evaluates up to the current cell.

                sbyte ithConstraint = -1;
                byte constraint = 0;
                byte currentBlock = 0;
                bool inBlock = false;

                for (int ithRow = row * nonogram.Width; ithRow <= index; ithRow++)
                {
                    //The cell is colored
                    if (grid[ithRow])
                    {
                        //The cell is the first in a block
                        if (!inBlock)
                        {
                            ithConstraint++;
                            inBlock = true;

                            //There are still constraints to consider
                            if (ithConstraint < rowConstraints.Length)
                                constraint = rowConstraints[ithConstraint];
                            else
                                return false;
                        }
                        currentBlock++;

                        //The block is longer than the current constraint.
                        if (currentBlock > constraint)
                            return false;
                    }
                    //The cell is not colored
                    else
                    {
                        //The last block does not match the constraint.
                        if (currentBlock < constraint)
                            return false;
                        constraint = 0;
                        currentBlock = 0;
                        inBlock = false;
                    }
                }
                //Special check for final cell in the row. 
                if (index % nonogram.Width == nonogram.Width - 1)
                {
                    if (ithConstraint < rowConstraints.Length - 1 || currentBlock < constraint)
                        return false;
                }

                //Checks if the column is still valid only checks up to the current cell.

                ithConstraint = -1;
                constraint = 0;
                currentBlock = 0;
                inBlock = false;

                for (int ithColumn = col; ithColumn <= index; ithColumn += nonogram.Width)
                {
                    //The cell is colored
                    if (grid[ithColumn])
                    {
                        //The cell is the first in the block 
                        if (!inBlock)
                        {
                            ithConstraint++;
                            inBlock = true;

                            //There are still constraints to consider
                            if (ithConstraint < colConstraints.Length)
                                constraint = colConstraints[ithConstraint];
                            else
                                return false;
                        }
                        currentBlock++;
                        //The current block is too big
                        if (currentBlock > constraint)
                            return false;
                    }
                    //The cell is blank
                    else
                    {
                        //The last block was too small
                        if (currentBlock < constraint)
                            return false;

                        constraint = 0;
                        currentBlock = 0;
                        inBlock = false;
                    }
                }
                //Special check for last cell in column
                if (index >= grid.Length - 1 - nonogram.Width)
                {
                    if (ithConstraint < colConstraints.Length - 1 || currentBlock < constraint)
                        return false;
                }

                return true;
            }
        }
        /* Method implementing a backtracking solver.
         * It works by changing the state of an entire row and using the possibilities generated by the constraints as possible states.
         */
        public bool BacktrackByPossibilities(int verbose = 0)
        {
            NonogramPrinter printNonogram = new NonogramPrinter(nonogram);
            
            iteration = 0;

            byte width = nonogram.Width;
            byte height = nonogram.Height;

            BitArray grid = nonogram.Grid;

            byte[][] constraints = nonogram.Rows;

            //Stack to push rows onto.
            Stack<IEnumerator<byte[]>> stack = new Stack<IEnumerator<byte[]>>();

            //Creating an enumerator to iterate over all possibilities for the first row
            int nBlocks = constraints[0].Length;
            int checkSum = sum(constraints[0]);

            int freeSpaces = width - (checkSum + constraints[0].Length - 1);
            int[] indexRange = Enumerable.Range(0, nBlocks + freeSpaces).ToArray();

            IEnumerator<byte[]> enumerator = createPossibilities(indexRange, nBlocks).GetEnumerator();
            stack.Push(enumerator);

            int row = 0;
            byte length = nonogram.Width;

            //Loop to implement backtracking
            while (true)
            {
                iteration++;

                IEnumerator<byte[]> currentEnumerator = stack.Pop();
                if (verbose > 0)
                {
                    if (verbose == 2)
                        printNonogram.print(grid, nonogram.Width, nonogram.Height);
                    Console.WriteLine(String.Format("Iteration:{0},Evaluating row:{1}", iteration, row));
                }
                /* Move to next item in the enumerator.
                 * If not possible simply do not repush the current enumerator
                 * If possible build line from current item.
                 */
                if (currentEnumerator.MoveNext())
                {
                    byte[] currentPossibility = currentEnumerator.Current;

                    //Repush current enumerator
                    stack.Push(currentEnumerator);

                    //Construct line from possibility and set in grid
                    BitArray constructedLine = constructLine(constraints[row], length, currentPossibility);
                    for (int index = 0; index < length; index++)
                    {
                        int realIndex = (row * length) + index;
                        grid[realIndex] = constructedLine[index];
                    }

                    //Check if current row possibility violates any of the column constraints.
                    bool check = CheckColumns(grid, row);

                    if (check)
                    {
                        row++;
                    }
                    //Escape condition
                    if (!(row < nonogram.Height))
                        break;
                    //Construct possibility enumerator for the next line and push it onto stack.
                    if (check)
                    {
                        nBlocks = constraints[row].Length;
                        checkSum = sum(constraints[row]);

                        freeSpaces = width - (checkSum + constraints[row].Length - 1);
                        stack.Push(createPossibilities(Enumerable.Range(0, nBlocks + freeSpaces).ToArray(), nBlocks).GetEnumerator());
                    }
                }
                else
                {
                    /* Set row back to blank
                     * ---Not necessary---
                     */
                    for (int j = 0; j < length; j++)
                    {
                        int realIndex = (row * length) + j;
                        grid[realIndex] = false;
                    }
                    row--;
                }
            }
            nonogram.Grid = grid;
            printNonogram.print();
            Console.WriteLine(String.Format("Nonogram solved in {0} iterations.", iteration));
            return true;

            /* Checks all columns to see if current row possibility violates the column constraints.
             * Uses the same check logic as the backtrackByIndex.
             */
            bool CheckColumns(BitArray grid, int index)
            {
                for (byte ithColumn = 0; ithColumn < nonogram.Width; ithColumn++)
                {
                    byte[] constraints = nonogram.Columns[ithColumn];
                    sbyte ithConstraint = -1;
                    byte constraint = 0;
                    byte currentBlock = 0;
                    bool inBlock = false;

                    for (int ithRow = ithColumn; ithRow / nonogram.Width < index + 1; ithRow += nonogram.Width)
                    {
                        if (grid[ithRow])
                        {
                            if (!inBlock)
                            {
                                ithConstraint++;
                                inBlock = true;
                                if (ithConstraint >= constraints.Length)
                                    return false;
                                constraint = constraints[ithConstraint];
                            }
                            currentBlock++;
                            if (currentBlock > constraint)
                                return false;
                        }
                        else
                        {
                            if (currentBlock < constraint)
                                return false;

                            inBlock = false;
                            currentBlock = 0;
                            constraint = 0;
                        }
                    }
                    if (index == nonogram.Height - 1 && (ithConstraint < constraints.Length - 1 || currentBlock < constraint))
                        return false;
                }
                return true;
            }

            /* Constructs a line based on the possibility given.
             */
            static BitArray constructLine(byte[] constraints, byte length, byte[] possibility)
            {
                int checkSum = sum(constraints);

                //Amount of undecided spaces in the line
                int freeSpaces = length - (checkSum + constraints.Length - 1);

                /* The possibility array contains the indices where the blocks are positioned among the free spaces.
                 * From these indices the seperate array is constructed.
                 */
                BitArray possibleLine = new BitArray(length);
                byte[] separate = new byte[constraints.Length + freeSpaces];
                byte ithBlock = 0;
                for (byte i = 0; i < separate.Length; i++)
                {
                    if (ithBlock < possibility.Length && i == possibility[ithBlock])
                    {
                        separate[i] = constraints[ithBlock];
                        ithBlock++;
                    }
                    else
                        separate[i] = 0;
                }
                // separate array is then collapsed into a usable line.
                byte index = 0;
                foreach (byte element in separate)
                {
                    if (element > 0)
                    {
                        for (int i = 0; i < element; i++)
                        {
                            possibleLine[index] = true;
                            index++;
                        }
                    }
                    index++;
                }
                return possibleLine;
            }
            int GetMaxPossibilities(byte length, byte[][] constraints)
            {
                int max = 0;
                foreach (byte[] constraint in constraints)
                {
                    int sumConstraints = sum(constraint);
                    int amount = factorial(length - sumConstraints + 1) / (factorial(length - sumConstraints - constraint.Length + 1) * factorial(constraint.Length));
                    if (amount > max)
                    {
                        max = amount;
                    }
                }
                return max;
            }

            int factorial(int n)
            {
                int start = 0;
                foreach (int i in Enumerable.Range(1, n))
                    start *= i;
                return start;
            }
        }
        /* Method implementing an overlap algorithm. 
         * All possibilities for each row and column is checked against the current grid.
         * If a possibility matches the current grid than it is kept.
         * If across all those possibilities a cell has the same state it gets that state.
         */
        public bool OverlapTime(int verbose = 0)
        {
            NonogramPrinter printNonogram = new NonogramPrinter(nonogram);
            iteration = 0;

            //Two grids are necessary for having three states: Colored, Blank, Undecided.
            BitArray grid = new BitArray(nonogram.Grid);
            BitArray blankGrid = new BitArray(grid);

            Queue<(bool, byte)> queue = new Queue<(bool, byte)>();
            HashSet<(bool, byte)> inQueue = new HashSet<(bool, byte)>();
            //Fill the queue with some lines for which we know that there is at least one cell colored.
            for (byte row = 0; row < nonogram.Height; row++)
            {
                if (addToQueue(nonogram.Width, nonogram.Rows[row]))
                {
                    queue.Enqueue((true, row));
                    inQueue.Add((true, row));
                }
            }
            for (byte column = 0; column < nonogram.Width; column++)
            {

                if (addToQueue(nonogram.Height, nonogram.Columns[column]))
                {
                    queue.Enqueue((false, column));
                    inQueue.Add((false, column));
                }
            }

            byte length;

            while (queue.Count != 0)
            {
                iteration++;

                byte[] constraints;
                BitArray currentLine;
                BitArray blankCurrentLine;

                //Current line
                (bool row, byte index) line = queue.Dequeue();
                inQueue.Remove(line);
                if (verbose > 0)
                {
                    if (verbose == 2)
                        printNonogram.print(grid, nonogram.Width, nonogram.Height);
                    if (line.row)
                        Console.WriteLine(String.Format("Iteration:{0},Evaluating row:{1}", iteration, line.index));
                    else
                        Console.WriteLine(String.Format("Iteration:{0},Evaluating column:{1}", iteration, line.index));
                }
                //Fill the current lines from the grid. This makes further computation easier.
                if (line.row)
                {
                    length = nonogram.Width;
                    constraints = nonogram.Rows[line.index];
                    currentLine = new BitArray(length);
                    blankCurrentLine = new BitArray(length);
                    int realIndex = length * line.index;
                    for (int i = 0; i < length; i++)
                    {
                        currentLine[i] = grid[realIndex];
                        blankCurrentLine[i] = blankGrid[realIndex];
                        realIndex++;
                    }
                }
                else
                {
                    length = nonogram.Height;
                    constraints = nonogram.Columns[line.index];
                    currentLine = new BitArray(length);
                    blankCurrentLine = new BitArray(length);
                    int realIndex = line.index;
                    for (int i = 0; i < length; i++)
                    {
                        currentLine[i] = grid[realIndex];
                        blankCurrentLine[i] = blankGrid[realIndex];
                        realIndex += nonogram.Width;
                    }

                }

                //Create an enumerable that goes through all possibilities of the current line.
                int checkSum = sum(constraints);

                int freeSpaces = length - (checkSum + constraints.Length - 1);
                int nBlocks = constraints.Length;
                int[] indexRange = Enumerable.Range(0, nBlocks + freeSpaces).ToArray();


                BitArray accumulatorArray = new BitArray(length);
                accumulatorArray.Not();
                BitArray blankAccumulatorArray = new BitArray(accumulatorArray);

                /* For each possibility construct a line.
                 * First we compare to the currentLine to see if the possibility fits.
                 * Use the accumulator array to check against all other possibilities.
                 */
                foreach (byte[] possibility in createPossibilities(indexRange, nBlocks))
                {
                    BitArray possibleLine = new BitArray(length);
                    int[] separate = new int[nBlocks + freeSpaces];
                    int ithBlock = 0;
                    for (int i = 0; i < separate.Length; i++)
                    {
                        if (ithBlock < possibility.Length && i == possibility[ithBlock])
                        {
                            separate[i] = constraints[ithBlock];
                            ithBlock++;
                        }
                        else
                            separate[i] = 0;
                    }

                    int index = 0;
                    foreach (int element in separate)
                    {
                        if (element > 0)
                        {
                            for (int i = 0; i < element; i++)
                            {
                                possibleLine[index] = true;
                                index++;
                            }
                        }
                        index++;
                    }
                    if (keepLine(currentLine, blankCurrentLine, possibleLine))
                    {
                        accumulatorArray.And(possibleLine);
                        possibleLine.Not();
                        blankAccumulatorArray.And(possibleLine);
                        if (!hasAnySet(accumulatorArray) && !hasAnySet(blankAccumulatorArray))
                        {
                            break;
                        }
                    }
                }
                for (int i = 0; i < length; i++)
                {
                    int realIndex;
                    if (line.row)
                    {
                        realIndex = line.index * nonogram.Width + i;
                    }
                    else
                    {
                        realIndex = i * nonogram.Width + line.index;
                    }


                    if ((accumulatorArray[i] && !grid[realIndex]) || (blankAccumulatorArray[i] && !blankGrid[realIndex]))
                    {
                        grid[realIndex] = accumulatorArray[i];
                        blankGrid[realIndex] = blankAccumulatorArray[i];

                        if (!inQueue.Contains((!line.row, (byte)i)))
                        {
                            queue.Enqueue((!line.row, (byte)i));
                            inQueue.Add((!line.row, (byte)i));
                        }

                    }

                }

            }
            bool completed = true;
            for(int i = 0; i < blankGrid.Length; i++)
            {
                if (!grid[i] && !blankGrid[i])
                {
                    completed = false;
                    break;
                }
            }
            nonogram.Grid = grid;
            printNonogram.print();
            Console.WriteLine(String.Format("Nonogram resolved in {0} iterations", iteration));
            return completed;

            bool keepLine(BitArray realLine, BitArray blankRealLine, BitArray possibleLine)
            {
                for (int i = 0; i < realLine.Length; i++)
                {
                    if ((realLine[i] && !possibleLine[i]) || (blankRealLine[i] && possibleLine[i]))
                        return false;
                }
                return true;
            }
            bool hasAnySet(BitArray array)
            {
                foreach (bool element in array)
                {
                    if (element)
                        return true;
                }
                return false;
            }
            bool addToQueue(byte length, byte[] constraints)
            {
                int checkSum = 0;
                if (constraints.Length == 0)
                    return true;
                foreach (byte element in constraints)
                    checkSum += element;

                int freeSpaces = length - (checkSum + constraints.Length - 1);

                foreach (byte element in constraints)
                {
                    if (element > freeSpaces)
                        return true;
                }
                return false;
            }


        }

        public bool OverlapMemory(int verbose = 0)
        {
            /*
            float totalCell = nonogram.Height * nonogram.Width;

            float filledCell = 0;
            foreach (byte[] row in nonogram.Rows)
            {
                foreach (byte i in row)
                    filledCell += i;
            }
            float blankCell = totalCell - filledCell;
            */
            NonogramPrinter printNonogram = new NonogramPrinter(nonogram);

            Dictionary<(bool, int), Queue<byte[]>> possibilitiesPerLine = new Dictionary<(bool, int), Queue<byte[]>>();
            HashSet<(bool, int)> inQueue = new HashSet<(bool, int)>();
            Queue<(bool, int)> queue = new Queue<(bool, int)>();

            byte[] constraints;
            int freeSpaces;
            int checkSum;

            for (int row = 0; row < nonogram.Height; row++)
            {
                constraints = nonogram.Rows[row];
                checkSum = sum(constraints);
                freeSpaces = nonogram.Width - (checkSum + constraints.Length - 1);

                foreach (byte element in constraints)
                {
                    if (element > freeSpaces)
                    {
                        Queue<byte[]> possibilities = new Queue<byte[]>();
                        foreach (byte[] possibility in createPossibilities(Enumerable.Range(0, constraints.Length + freeSpaces).ToArray(), constraints.Length))
                        {
                            possibilities.Enqueue(possibility);
                        }
                        possibilities.TrimExcess();
                        possibilitiesPerLine.Add((true, row), possibilities);
                        inQueue.Add((true, row));
                        queue.Enqueue((true, row));

                        break;
                    }
                }
            }
            for (int column = 0; column < nonogram.Width; column++)
            {
                constraints = nonogram.Columns[column];
                checkSum = sum(constraints);
                freeSpaces = nonogram.Height - (checkSum + constraints.Length - 1);

                foreach (byte element in constraints)
                {
                    if (element > freeSpaces)
                    {
                        Queue<byte[]> possibilities = new Queue<byte[]>();
                        foreach (byte[] possibility in createPossibilities(Enumerable.Range(0, constraints.Length + freeSpaces).ToArray(), constraints.Length))
                        {
                            possibilities.Enqueue(possibility);
                        }
                        possibilities.TrimExcess();
                        possibilitiesPerLine.Add((false, column), possibilities);
                        inQueue.Add((false, column));
                        queue.Enqueue((false, column));
                        break;
                    }
                }
            }
            BitArray grid = nonogram.Grid;
            BitArray blankGrid = new BitArray(grid);

            bool solved = false;

            iteration = 0;

            while (!solved && queue.Count != 0)
            {
                iteration++;
                (bool row, int index) line = queue.Dequeue();

                if (verbose > 0)
                {
                    if (verbose == 2)
                    {
                        printNonogram.print(grid, nonogram.Width, nonogram.Height);
                        Console.WriteLine();
                        printNonogram.print(blankGrid, nonogram.Width, nonogram.Height);
                    }
                    if (line.row)
                        Console.WriteLine(String.Format("Iteration:{0},Evaluating row:{1}", iteration, line.index));
                    else
                        Console.WriteLine(String.Format("Iteration:{0},Evaluating column:{1}", iteration, line.index));
                }

                inQueue.Remove(line);
                byte length;
                IEnumerable<(int, int)> enumeration;
                if (line.row)
                {
                    length = nonogram.Width;
                    enumeration = realAndCurrent(length * line.index, length, 1);
                    constraints = nonogram.Rows[line.index];
                }
                else
                {
                    length = nonogram.Height;
                    enumeration = realAndCurrent(line.index, length, nonogram.Width);
                    constraints = nonogram.Columns[line.index];
                }

                BitArray currentLine = new BitArray(length);
                BitArray currentBlankLine = new BitArray(length);

                foreach ((int realIndex, int index) in enumeration)
                {
                    currentLine[index] = grid[realIndex];
                    currentBlankLine[index] = blankGrid[realIndex];
                }

                BitArray accumulatorArray = new BitArray(length);
                accumulatorArray.Not();
                BitArray blankAccumulatorArray = new BitArray(accumulatorArray);

                Queue<byte[]> possibilities;
                if (!possibilitiesPerLine.ContainsKey(line))
                {
                    possibilities = new Queue<byte[]>();


                    checkSum = sum(constraints);
                    freeSpaces = length - (checkSum + constraints.Length - 1);
                    foreach (byte[] possibility in createPossibilities(Enumerable.Range(0, constraints.Length + freeSpaces).ToArray(), constraints.Length))
                    {
                        possibilities.Enqueue(possibility);
                    }
                    possibilities.TrimExcess();
                }
                else
                    possibilities = possibilitiesPerLine[line];

                Queue<byte[]> reducedPossibilities = new Queue<byte[]>();

                while (possibilities.Count != 0)
                {
                    byte[] possibility = possibilities.Dequeue();
                    BitArray possibleLine = constructLine(constraints, length, possibility);
                    if (keepLine(currentLine, currentBlankLine, possibleLine))
                    {
                        reducedPossibilities.Enqueue(possibility);
                        accumulatorArray.And(possibleLine);
                        possibleLine.Not();
                        blankAccumulatorArray.And(possibleLine);
                    }
                }
                reducedPossibilities.TrimExcess();
                possibilitiesPerLine[line] = reducedPossibilities;

                foreach ((int realIndex, int index) in enumeration)
                {
                    bool set = false;
                    if (accumulatorArray[index] && !grid[realIndex])
                    {
                        grid[realIndex] = accumulatorArray[index];
                        set = true;
                    }
                    if (blankAccumulatorArray[index] && !blankGrid[realIndex])
                    {
                        blankGrid[realIndex] = blankAccumulatorArray[index];
                        set = true;
                    }
                    if (set && !inQueue.Contains((!line.row, index)))
                    {
                        inQueue.Add((!line.row, index));
                        queue.Enqueue((!line.row, index));
                    }
                }

                solved = true;
                for (int i = 0; i < grid.Length; i++)
                {
                    if (!grid[i] && !blankGrid[i])
                    {
                        solved = false;
                    }
                }
                /*
                float nBlankCell = 0;
                float nCell = 0;

                for (int i = 0; i < grid.Length; i++)
                {
                    nBlankCell += blankGrid[i] ? 1 : 0;
                    nCell += grid[i] ? 1 : 0;
                }
                Console.WriteLine($"{iteration};{nCell};{nCell / filledCell};{nBlankCell};{nBlankCell / blankCell}");
                */
            }

            bool completed = true;
            for(int i = 0; i < grid.Length; i++)
            {
                if (!grid[i] && !blankGrid[i])
                {
                    completed = false;
                    break;
                }
            }
            nonogram.Grid = grid;
            printNonogram.print();
            Console.WriteLine(String.Format("Nonogram resolved in {0} iterations", iteration));
            return completed;
        }
        public bool OverlapByConstraints(int verbose = 0)
        {

            NonogramPrinter printNonogram = new NonogramPrinter(nonogram);

            HashSet<(bool, int)> inQueue = new HashSet<(bool, int)>();
            Queue<(bool, int)> queue = new Queue<(bool, int)>();

            /* grid represents the cell filled-in with 1
             * blankGrid represents the cell filled-in with 0
             */
            BitArray grid = new BitArray(nonogram.Grid.Length);
            BitArray blankGrid = new BitArray(grid);

            // Defines ranges for each line.
            (byte start, byte end, byte size)[][] constraintsByPositionColumns = new (byte start, byte end, byte size)[nonogram.Width][];
            (byte start, byte end, byte size)[][] constraintsByPositionRows = new (byte start, byte end, byte size)[nonogram.Height][];

            /*Function works the same as fillQueue.
             * In addition to filling the queue it initializes the ranges.
             */
            for (int column = 0; column < nonogram.Width; column++)
            {
                byte[] constraints = nonogram.Columns[column];
                constraintsByPositionColumns[column] = new (byte start, byte end, byte size)[constraints.Length];

                byte freeSpaces = moveSpaces(constraints, nonogram.Height);

                byte start = 0;
                for (int i = 0; i < constraints.Length; i++)
                {
                    if (constraints[i] > freeSpaces && !inQueue.Contains((false, column)))
                    {
                        inQueue.Add((false, column));
                        queue.Enqueue((false, column));
                    }

                    byte size = constraints[i];
                    byte end = (byte)(start + size + freeSpaces - 1);

                    constraintsByPositionColumns[column][i] = (start, end, size);

                    start += (byte)(size + 1);
                }
            }
            for (int row = 0; row < nonogram.Height; row++)
            {
                byte[] constraints = nonogram.Rows[row];
                constraintsByPositionRows[row] = new (byte start, byte end, byte size)[constraints.Length];

                byte freeSpaces = moveSpaces(constraints, nonogram.Width);
                byte start = 0;
                for (int i = 0; i < constraints.Length; i++)
                {
                    if (constraints[i] > freeSpaces && !inQueue.Contains((true, row)))
                    {
                        inQueue.Add((true, row));
                        queue.Enqueue((true, row));
                    }

                    byte size = constraints[i];
                    byte end = (byte)(start + size + freeSpaces - 1);

                    constraintsByPositionRows[row][i] = (start, end, size);

                    start += (byte)(size + 1);
                }
            }

            iteration = 0;
            /* Main loop for handling lines.
             */
            while(queue.Count > 0)
            {
                iteration++;
                
                //Get line to evaluate
                (bool row,int index) line = queue.Dequeue();
                inQueue.Remove(line);
                
                if (verbose > 0)
                {
                    if (verbose == 2)
                    {
                        printNonogram.print(grid, nonogram.Width, nonogram.Height);
                        Console.WriteLine();
                        printNonogram.print(blankGrid, nonogram.Width, nonogram.Height);
                    }
                    if (line.row)
                        Console.WriteLine(String.Format("Iteration:{0},Evaluating row:{1}", iteration, line.index));
                    else
                        Console.WriteLine(String.Format("Iteration:{0},Evaluating column:{1}", iteration, line.index));
                }

                
                //Line specific variables
                byte length;
                (byte start, byte end, byte size)[] lineConstraints;
                IEnumerable<(int, int)> enumeration;

                //Assign line specific variables
                if (line.row)
                {
                    length = nonogram.Width;
                    lineConstraints = constraintsByPositionRows[line.index];
                    enumeration = realAndCurrent(length * line.index, length, 1);
                }
                else
                {
                    length = nonogram.Height;
                    lineConstraints = constraintsByPositionColumns[line.index];
                    enumeration = realAndCurrent(line.index, length, nonogram.Width);
                }


                //Get current situation of the line from the grid
                BitArray currentLine = new BitArray(length); 
                BitArray currentBlankLine = new BitArray(length);
                foreach ((int realIndex, int index) in enumeration)
                {
                    currentLine[index] = grid[realIndex];
                    currentBlankLine[index] = blankGrid[realIndex];
                }

                BitArray lineLevelAccumulatorArray = new BitArray(length);
                BitArray lineLevelBlankAccumulatorArray = new BitArray(lineLevelAccumulatorArray);

                //Go through the blocks of the lines and shrink the ranges
                for (int constraint = 0; constraint < lineConstraints.Length; constraint++)
                {
                    (byte start,byte end,byte size) = lineConstraints[constraint];
                    int nPossibilities = end - start - size + 2;

                    byte newStart = 255;
                    byte newEnd = 0;

                    BitArray possibleLine = new BitArray(length);
                    BitArray possibleBlankLine = new BitArray(length);

                    BitArray accumulatorArray = new BitArray(length);
                    accumulatorArray.Not();
                    BitArray blankAccumulatorArray = new BitArray(accumulatorArray);

                    //Define Left most position
                    for (int i = start; i < start+size; i++)
                        possibleLine[i] = true;
                    if (start > 0)
                        possibleBlankLine[start - 1] = true;
                    if (start + size < length)
                        possibleBlankLine[start+size] = true;

                    //Define part of line that has no overlap
                    (byte start, byte end) nonOverlapped;
                    if (lineConstraints.Length > 1) {
                        if (constraint == 0)
                            nonOverlapped = (start, (byte)(lineConstraints[constraint + 1].start - 1));
                        else if (constraint == lineConstraints.Length - 1)
                            nonOverlapped = ((byte)(lineConstraints[constraint - 1].end + 1), end);
                        else
                            nonOverlapped = ((byte)(lineConstraints[constraint - 1].end + 1), (byte)(lineConstraints[constraint + 1].start - 1));
                    }
                    else
                        nonOverlapped = (start, end);
                    //Check if there is a colored cell in the non-overlap part
                    bool inOverlap = false;
                    for(int i = nonOverlapped.start; i <= nonOverlapped.end; i++)
                    {
                        if(currentLine[i])
                        {
                            inOverlap = true; 
                            break; 
                        }
                    }
                    bool keepLine = true;
                    //Check for direct contradiction over whole range
                    for(int index = Math.Max(0,start-1); index <=Math.Min(end+1,length-1); index++)
                    {
                        if ((currentLine[index]&&possibleBlankLine[index])||(currentBlankLine[index] && possibleLine[index]))
                        {
                            keepLine = false;
                            break;
                        }
                    }
                    //Check if cell in non-overlapped part is part of block
                    if (inOverlap)
                    {
                        for(int index = nonOverlapped.start; index <= nonOverlapped.end; index++)
                        {
                            if (currentLine[index] && !possibleLine[index])
                            {
                                keepLine = false;
                                break;
                            }
                        }
                    }
                    if (keepLine)
                    {
                        //Partial solution
                        accumulatorArray.And(possibleLine);
                        blankAccumulatorArray.And(possibleBlankLine);
                        //New ranges
                        if(newStart > start)
                            newStart = start;
                        if(newEnd < start+size -1)
                            newEnd = (byte)(start + size - 1);
                    }
                    //Repeat above while shifting the possible lines to the right-most position
                    for(int i = 1; i < nPossibilities; i++)
                    {
                        possibleLine.LeftShift(1);
                        possibleBlankLine.LeftShift(1);
                        if (constraint == 0)
                        {
                            possibleBlankLine[0] = true;
                        }

                        keepLine = true;

                        for (int index = Math.Max(0, start - 1); index <= Math.Min(end + 1, length - 1); index++)
                        {
                            if ((currentLine[index] && possibleBlankLine[index]) || (currentBlankLine[index] && possibleLine[index]))
                            {
                                keepLine = false;
                                break;
                            }
                        }
                        if (inOverlap)
                        {
                            for (int index = nonOverlapped.start; index <= nonOverlapped.end; index++)
                            {
                                if (currentLine[index] && !possibleLine[index])
                                {
                                    keepLine = false;
                                    break;
                                }
                            }
                        }
                        if (keepLine)
                        {
                            accumulatorArray.And(possibleLine);
                            blankAccumulatorArray.And(possibleBlankLine);
                        
                            if(newStart > start+i)
                                newStart = (byte)(start+i);
                            if (newEnd < start + size + i)
                                newEnd = (byte)(start + size + i-1);
                        }
                    }
                    lineConstraints[constraint] = (newStart, newEnd, size);
                    //Set block partial solution into line partial solution
                    for (int index = start; index <= end; index++)
                    {
                        if (accumulatorArray[index])
                        {
                            lineLevelAccumulatorArray[index] = true;
                        }
                        if (blankAccumulatorArray[index])
                        {
                            lineLevelBlankAccumulatorArray[index] = true;
                        }
                    }
                }
                //Check if ranges no longer overlap if so color cells between ranges blank
                for(int constraint = 0; constraint < lineConstraints.Length; constraint++)
                {
                    (byte start ,byte end, byte size) current = lineConstraints[constraint];
                    if(lineConstraints.Length > 1)
                    {
                        (byte start, byte end, byte size) previous;
                        (byte start, byte end, byte size) next;
                        if (constraint == 0)
                        {
                            previous = (0, 0, 0);
                            next = lineConstraints[constraint + 1];
                        }
                        else if (constraint == lineConstraints.Length - 1)
                        {
                            previous = lineConstraints[constraint - 1];
                            next = (length, length, 0);
                        }
                        else
                        {
                            previous = lineConstraints[constraint - 1];
                            next = lineConstraints[constraint + 1];
                        }
                        for (int index = previous.end+1; index < current.start; index++)
                            lineLevelBlankAccumulatorArray[index] = true;
                        for(int index = current.end+1;index < next.start; index++)
                            lineLevelBlankAccumulatorArray[index] = true;
                    }
                    else
                    {
                        for(int index = 0; index < current.start; index++)
                            lineLevelBlankAccumulatorArray[index] = true;
                        for (int index = current.end + 1; index < length; index++)
                            lineLevelBlankAccumulatorArray[index] = true;
                    }
                }
                //Set line partial solution into the grid and enqueue new cells
                foreach((int realIndex,int index) in enumeration)
                {
                    if(lineLevelAccumulatorArray[index] && !grid[realIndex])
                    {
                        if (!inQueue.Contains((!line.row, index)))
                        {
                            queue.Enqueue((!line.row,index));
                            inQueue.Add((!line.row, index));
                        }
                        grid[realIndex] = true;
                    }
                    if(lineLevelBlankAccumulatorArray[index] && !blankGrid[realIndex])
                    {
                        if (!inQueue.Contains((!line.row, index)))
                        {
                            queue.Enqueue((!line.row, index));
                            inQueue.Add((!line.row, index));
                        }
                        blankGrid[realIndex] = true;
                    }
                }
                if (line.row)
                    constraintsByPositionRows[line.index] = lineConstraints;
                else
                    constraintsByPositionColumns[line.index] = lineConstraints;

            }
            bool completed = true;

            for (int i = 0; i < grid.Length; i++)
            {
                if (!grid[i] && !blankGrid[i])
                {
                    completed = false;
                    break;
                }
            }
            nonogram.Grid = grid;
            printNonogram.print();
            Console.WriteLine(String.Format("Nonogram resolved in {0} iterations", iteration));

            return completed;
        }
        //Helper functions
        static bool keepLine(BitArray currentLine, BitArray currentBlankLine, BitArray possibleLine)
        {
            for (int i = 0; i < currentLine.Length; i++)
            {
                if (currentLine[i] && !possibleLine[i] || currentBlankLine[i] && possibleLine[i])
                {
                    return false;
                }
            }
            return true;
        }
        static BitArray constructLine(byte[] constraints, byte length, byte[] possibility)
        {
            int checkSum = sum(constraints);

            //Amount of undecided spaces in the line
            int freeSpaces = length - (checkSum + constraints.Length - 1);

            /* The possibility array contains the indices where the blocks are positioned among the free spaces.
             * From these indices the seperate array is constructed.
             */
            BitArray possibleLine = new BitArray(length);
            byte[] separate = new byte[constraints.Length + freeSpaces];
            byte ithBlock = 0;
            for (byte i = 0; i < separate.Length; i++)
            {
                if (ithBlock < possibility.Length && i == possibility[ithBlock])
                {
                    separate[i] = constraints[ithBlock];
                    ithBlock++;
                }
                else
                    separate[i] = 0;
            }
            // separate array is then collapsed into a usable line.
            byte index = 0;
            foreach (byte element in separate)
            {
                if (element > 0)
                {
                    for (int i = 0; i < element; i++)
                    {
                        possibleLine[index] = true;
                        index++;
                    }
                }
                index++;
            }
            return possibleLine;
        }

        static BitArray constructLine2(byte[] constraint,byte length, byte[]possibility)
        {
            BitArray possibleLine = new BitArray(length);
            byte ithConstraint = 0;
            byte blockSize = constraint[ithConstraint];
            byte positionIndex = 0;

            byte currentBlockSize = 0;

            for(int i =0; i < length; i++)
            {
                if(positionIndex == possibility[ithConstraint])
                {
                    possibleLine[i] = true;
                    currentBlockSize++;

                    if(currentBlockSize == blockSize)
                    {
                        positionIndex++;
                        ithConstraint++;
                        i++;
                        if (ithConstraint == constraint.Length)
                            break;
                    }
                }
                else
                {
                    positionIndex++;
                }
            }
            return possibleLine;
        }
        IEnumerable<(int, int)> realAndCurrent(int start, int length, int step)
        {
            int realIndex = start;
            for (int index = 0; index < length; index++)
            {
                yield return (realIndex, index);
                realIndex += step;
            }
        }
        static int sum(byte[] array)
        {
            int checkSum = 0;
            foreach (byte e in array)
                checkSum += e;
            return checkSum;
        }
        IEnumerable<byte[]> createPossibilities(int[] indexRange, int nBlocks)
        {
            int nPlaces = indexRange.Length;

            if (nBlocks > nPlaces)
                yield break;

            int[] indices = Enumerable.Range(0, nBlocks).ToArray();

            yield return indices.Select(i => (byte)indexRange[i]).ToArray();

            while (true)
            {
                int i;
                for (i = nBlocks - 1; i >= 0; i--)
                {
                    if (indices[i] != i + nPlaces - nBlocks)
                        break;
                }
                if (i < 0)
                    yield break;
                indices[i]++;
                for (int j = i + 1; j < nBlocks; j++)
                {
                    indices[j] = indices[j - 1] + 1;
                }
                yield return indices.Select(j => (byte)indexRange[j]).ToArray();
            }
        }
        static byte moveSpaces(byte[] constraints, byte length)
        {
            int checkSum = sum(constraints);
            int nBlocks = constraints.Length;
            return (byte)(length - (checkSum + (nBlocks - 1)));
        }
    }
}
