namespace Anagram
{
    using System;
    using System.IO;
    using System.Globalization;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var anagramText = args[1];
            var path = args[0];
            var now = DateTime.Now;

            var stream = AanagramStreamFactory.CreateTestableAnagramStream(anagramText);

            using (stream.Results.Subscribe(x => Console.WriteLine(string.Join(" ", x) + " " + $"{stream.SingleWordSegmentCount} {stream.SegmentCombinationCount}")))
            {
                foreach (var word in File.ReadAllLines(path))
                {
                    stream.ProcessWord(word);
                }
            }

            var duration = DateTime.Now - now;

            Console.WriteLine($"{stream.SingleWordSegmentCount} {stream.SegmentCombinationCount}");
            Console.WriteLine($"{string.Join(" ", stream.SegmentsPerCharacter.Select(x => x.Count()))} Total: {stream.SegmentsPerCharacter.Aggregate(0, (i, x) => i + x.Count())}");

            Console.WriteLine($"Execution time: { duration.TotalSeconds:0.###} seconds");
        }
    }
}
