using System.Collections;

namespace Nonogram
{
    class Nonogram
    {
        private byte width;
        private byte height;

        private BitArray grid;

        private byte[][] rows;
        private byte[][] columns;


        public byte Width
        {
            get { return width; }
        }
        public byte Height
        {
            get { return height; }
        }
        public BitArray Grid
        {
            get { return grid; }
            set { grid = value; }
        }
        public byte[][] Rows
        {
            get { return rows; }
        }
        public byte[][] Columns
        {
            get { return columns; }
        }
        public Nonogram(Nonogram nonogram)
        {
            this.width = nonogram.Width;
            this.height = nonogram.Height;
            this.grid = new BitArray(nonogram.Grid);

            this.rows = nonogram.rows;
            this.columns = nonogram.columns;
        }
        public Nonogram(string file)
        {
            try
            {
                List<string[]> lines = new List<string[]>();
                byte rowPadding = byte.MaxValue;

                using (StreamReader reader = new StreamReader(file))
                {
                    for (byte i = 0; i < 3; i++)
                    {
                        reader.ReadLine();
                    }

                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] fields = line.Split(',');
                        lines.Add(fields);

                        if (rowPadding >= fields.Length)
                        {
                            rowPadding = (byte)fields.Length;
                        }
                    }
                }
                List<string[]> columnLines = new List<string[]>();
                List<string[]> rowLines = new List<string[]>();

                for (byte i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Length == rowPadding)
                    {
                        rowLines.Add(lines[i]);
                    }
                    else
                    {
                        columnLines.Add(lines[i]);
                    }
                }
                
                height = (byte) rowLines.Count;
                width = (byte) (columnLines[0].Length - rowPadding);
                grid = new BitArray(height*width);

                rows = new byte[height][];
                columns = new byte[width][];

                for (byte i = 0; i < rows.Length; i++)
                {
                    List<byte> constraints = new List<byte>();
                    foreach(string constraintString in rowLines[i])
                    {
                        if (constraintString != "")
                        {
                            constraints.Add(byte.Parse(constraintString));
                        }
                    }
                    rows[i] = constraints.ToArray();
                }

                for (byte i = 0; i < columns.Length; i++)
                {
                    List<byte> constraints = new List<byte>();
                    foreach (string[] columnline in columnLines)
                    {
                        if (columnline[rowPadding + i] != "")
                        {
                            constraints.Add(byte.Parse(columnline[rowPadding + i]));
                        }
                    }
                    columns[i] = constraints.ToArray();
                }
            }
            
            catch (Exception ex)
            {
                Console.WriteLine("Error reading the CSV file: " + ex);
            }
        }
    }

}
