using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
            List<IterableFileReader> streams = new List<IterableFileReader>();

            streams.Add(new IterableFileReader("FILE1.csv"));
            streams.Add(new IterableFileReader("FILE2.csv"));
            streams.Add(new IterableFileReader("FILE3.csv"));

            Dictionary<IterableFileReader, MarketDataDTO> streamsData = new Dictionary<IterableFileReader, MarketDataDTO>();

            while (true)
            {
                if (streamsData.Count != streams.Count)
                {
                    foreach (IterableFileReader stream in streams)
                    {
                        var line = stream.CurrentLine();

                        if (line is null)
                        {
                            stream.Next();
                        }

                        stream.Next();

                        line = stream.CurrentLine();

                        if (!streamsData.ContainsKey(stream))
                        {
                            streamsData.Add(stream, new MarketDataDTO
                            {
                                DateTime = line.DateTime,
                                Bid = line.Bid,
                            });
                        }
                    }
                }

                if (streamsData.Count == streams.Count)
                {
                    List<KeyValuePair<IterableFileReader, MarketDataDTO>> list = streamsData.OrderByDescending(s => s.Value.DateTime).ToList();

                    KeyValuePair<IterableFileReader, MarketDataDTO> first = list.First();

                    var nextFirstLine = first.Key.CurrentLine();

                    if (nextFirstLine != null && nextFirstLine.DateTime == first.Value.DateTime)
                    {
                        first.Key.Next();
                    }

                    var dateTime = first.Value.DateTime;

                    list.RemoveAt(0);

                    foreach (var r in list)
                    {
                        var stream = r.Key;
                        MarketDataDTO currrentDataValue = r.Value;
                        DateTimeOffset currentDate = currrentDataValue.DateTime;
                        decimal currentBid = currrentDataValue.Bid;

                        if (currentDate > first.Value.DateTime)
                        {
                            continue; // если дата в стриме, больше чем самая большая дата, не обновляемся
                        }

                        var nextLine = stream.CurrentLine();

                        if (nextLine != null && currentDate == nextLine.DateTime)
                        {
                            stream.Next();

                            nextLine = stream.CurrentLine();

                            if (nextLine == null || nextLine.DateTime > first.Value.DateTime)
                            {
                                continue;
                            }

                            currrentDataValue.DateTime = nextLine.DateTime;
                            currrentDataValue.Bid = nextLine.Bid;
                        }
                        else
                        {
                            var nextFirst = first.Key.CurrentLine();

                            if (nextFirst != null && nextLine.DateTime > nextFirst.DateTime)
                            {
                                first.Value.DateTime = nextFirst.DateTime;
                                first.Value.Bid = nextFirst.Bid;
                            }
                            else
                            {
                                if (nextLine != null)
                                {
                                    currrentDataValue.DateTime = nextLine.DateTime;
                                    currrentDataValue.Bid = nextLine.Bid;
                                }
                            }
                        }
                    }

                    
                    if (isAllStreamsEnded(streamsData))
                    {
                        foreach (var s in streamsData)
                        {
                            var stream = s.Key;
                            var val = s.Value;

                            var nextFirst = stream.CurrentLine();

                            Console.WriteLine("Next: {0}, {1}; Current: {2}, {3}", nextFirst?.DateTime, nextFirst?.Bid, val.DateTime, val.Bid);
                        }

                        Console.WriteLine("ended");
                    }
                }
            }

            static bool isAllStreamsEnded(Dictionary<IterableFileReader, MarketDataDTO> streamsData)
            {
                return streamsData.Where(p => p.Key.CurrentLine() != null).FirstOrDefault().Value is null;
            }
        }
    }
}
