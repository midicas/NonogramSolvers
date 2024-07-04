using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonogram
{
    class NonogramPrinter
    {
        Nonogram nonogram = null;

        public NonogramPrinter(Nonogram value)
        {
            nonogram = value;
        }
        public NonogramPrinter()
        {
            nonogram = null;
        }
        public void printRow(byte row)
        {
            string constraints = string.Join(",", nonogram.Rows[row]);
            
            StringBuilder rowString = new StringBuilder(constraints+":",constraints.Length+nonogram.Width+1);
            
            byte start = (byte) ((row-1) * nonogram.Width);
            byte end = (byte) (start + nonogram.Width);
            BitArray grid = nonogram.Grid;

            for(byte i = start; i < end; i++)
            {
                if (grid[i])
                {
                    rowString.Append("\u2588");
                }
                else
                {
                    rowString.Append("_");
                }
            }
            Console.WriteLine(rowString.ToString());
        }
        public void printRow(BitArray row)
        {
            StringBuilder rowString = new StringBuilder(row.Length);
            foreach(bool cell in row)
            {
                if (cell)
                {
                    rowString.Append("\u2588");
                }
                else
                {
                    rowString.Append("_");
                }
            }
            Console.WriteLine(rowString);
        }
        public void printRow(BitArray row, byte[] constraint)
        {
            string constraintString = string.Join(",", constraint);
            StringBuilder rowString = new StringBuilder(constraintString + ":", constraintString.Length + row.Length + 1);
            foreach (bool cell in row)
            {
                if (cell)
                {
                    rowString.Append("\u2588");
                }
                else
                {
                    rowString.Append("_");
                }
            }
            Console.WriteLine(rowString);
        }
        public void printRow(BitArray row, byte constraint)
        {
            printRow(row,new byte[] {constraint});
        }
        public void printColumn(byte column)
        {
            string constraints = string.Join(",", nonogram.Columns[column-1]);

            StringBuilder columnString = new StringBuilder(constraints + ":", constraints.Length + nonogram.Height + 1);

            BitArray grid = nonogram.Grid;

            for (byte i = (byte)(column - 1); i < grid.Length; i += nonogram.Width)
            {
                if (grid[i])
                {
                    columnString.Append("\u2588");
                }
                else
                {
                    columnString.Append("_");
                }
            }
            Console.WriteLine(columnString);
        }
        public void printColumn(BitArray column)
        {
            printRow(column);
        }
        public void printColumn(BitArray column, byte[] constraints)
        {
            printRow(column,constraints);
        }
        public void printColumn(BitArray column, byte constraint)
        {
            printRow(column, new byte[] { constraint });
        }
        public void print()
        {
            BitArray grid = nonogram.Grid;
            print(grid, nonogram.Width, nonogram.Height);
        }
        public void print(BitArray grid,byte width,byte height)
        {
            StringBuilder nonogramString = new StringBuilder((width + 1) * height);
            for (int i = 0; i < grid.Length; i++)
            {
                if (i != 0 && i % width == 0)
                {
                    nonogramString.Append("\n");

                }
                if (grid[i])
                {
                    nonogramString.Append("\u2588");
                }
                else
                {
                    nonogramString.Append("_");
                }

            }
            Console.WriteLine(nonogramString);
        }
        
    }

}
