namespace Anagram
{
    using System;
    using System.IO;
    using System.Globalization;

    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var anagramText = args[1];
            var path = args[0];
            var now = DateTime.Now;

            var stream = AanagramStreamFactory.CreateAnagramStream(anagramText);

            using (stream.Results.Subscribe(x => Console.WriteLine(string.Join(" ", x))))
            {
                foreach (var word in File.ReadAllLines(path))
                {
                    stream.ProcessWord(word);
                }
            }

            var duration = DateTime.Now - now;
            Console.WriteLine($"Execution time: { duration.TotalSeconds:0.###} seconds");
        }
    }
}
