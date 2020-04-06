using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ConsoleApp4
{
    public class IterableFileReader
    {
        private readonly StreamReader stream;
        private int currentLineNumber;
        private MarketDataDTO currentLine;
        private int linesCount;

        public IterableFileReader(string filePatch)
        {
            if (filePatch is null)
            {
                throw new ArgumentNullException("FilePatch can't be null");
            }

            this.linesCount = File.ReadLines(filePatch).Count();
            this.stream = new StreamReader(File.OpenRead(filePatch));
            this.currentLineNumber = 0;
        }

        public void Next()
        {
            string line = this.stream.ReadLine();

            this.currentLineNumber++;

            if (String.IsNullOrEmpty(line))
            {
                this.currentLine = null;

                return;
            }

            string[] values = line.Split(';');

            DateTimeOffset dt;
            decimal bid;

            if (!DateTimeOffset.TryParse(values[0], out dt))
            {
                return;
            }

            if (!Decimal.TryParse(values[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out bid))
            {
                return;
            }

            if (this.currentLine is null)
            {
                this.currentLine = new MarketDataDTO()
                { 
                    DateTime = dt,
                    Bid = bid,
                };
            }
            else
            {
                this.currentLine.DateTime = dt;
                this.currentLine.Bid = bid;
            }
            
        }

        public MarketDataDTO CurrentLine()
        {
            return this.currentLine;
        }

        public int GetLinesCount()
        {
            return this.linesCount;
        }

        public int GetCurrentLineNumber()
        {
            return this.currentLineNumber;
        }
    }
}
