namespace Anagram
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;

    /// <summary>
    /// Finds find anagrams in streamed words
    /// </summary>
    internal class AnagramStreamProcessor : ITestableAnagramStream
    {
        private readonly int minWordSize;

        /// <summary>
        /// registered segments that match the anagram
        /// </summary>
        private readonly Dictionary<char, List<TextSegment>> segmentRegistration;

        /// <summary>
        /// the used characters and their count in the anagram source
        /// </summary>
        private readonly Dictionary<char, int> anagramCharacterCounts;

        /// <summary>
        /// subject for output
        /// </summary>
        private Subject<IEnumerable<string>> outputSubject;

        /// <summary>
        /// private constructor to avoid public create
        /// </summary>
        public AnagramStreamProcessor(string source, int minWordSize)
        {
            this.outputSubject = new Subject<IEnumerable<string>>();
            this.minWordSize = minWordSize;
            this.segmentRegistration = new Dictionary<char, List<TextSegment>>();
            this.anagramCharacterCounts = new Dictionary<char, int>();
            foreach (var c in source.ToLowerInvariant().Where(x => Char.IsLetterOrDigit(x)))
            {
                if (anagramCharacterCounts.ContainsKey(c))
                {
                    anagramCharacterCounts[c]++;
                }
                else
                {
                    anagramCharacterCounts.Add(c, 1);
                    segmentRegistration.Add(c, new List<TextSegment>());
                }
            }
        }

        /// <inheritdoc/>
        public IObservable<IEnumerable<string>> Results => this.outputSubject;

        /// <inheritdoc/>
        public int SingleWordSegmentCount { get; private set; }

        /// <inheritdoc/>
        public int SegmentCombinationCount { get; private set; }

        /// <inheritdoc/>
        public void ProcessWord(string word)
        {
            if (word.Length <= minWordSize)
            {
                return;
            }

            var segment = new TextSegment(this.anagramCharacterCounts, word);
            if (segment.RemainingCharacterClasses < 0)
            {
                return;
            }

            if (segment.RemainingCharacterClasses == 0)
            {
                this.outputSubject.OnNext(segment.Words);
                return;
            }

            if (segment.RemainingLength <= minWordSize)
            {
                return;
            }

            var potentialCompletions = segment
                .RemainingChars
                .Keys
                .SelectMany(x => this.segmentRegistration[x])
                .Distinct()
                .ToList();

            this.RegisterSegment(segment);
            this.SingleWordSegmentCount++;

            foreach (var completion in potentialCompletions)
            {
                HandlePossibleCompletion(word, segment, completion);
            }
        }

        /// <inheritdoc/>
        public TextSegment CreatePartialWord(string text)
        {
            return new TextSegment(this.anagramCharacterCounts, text);
        }

        /// <summary>
        /// test combinations of registered segments (and their combinations). auto registers new segments
        /// </summary>
        /// <param name="word">the current processed word</param>
        /// <param name="segment">the segment that describes the actual word</param>
        /// <param name="completion">a possible registered completion</param>
        private void HandlePossibleCompletion(string word, TextSegment segment, TextSegment completion)
        {
            if (completion.RemainingLength == word.Length)
            {
                if (TextSegment.IsFullCompletion(completion, segment))
                {
                    this.outputSubject.OnNext(completion.Words.Concat(segment.Words));
                }
            }
            else if (completion.UsedChars.Count > 0 && completion.RemainingLength - word.Length > minWordSize)
            {
                var joinedWord = TextSegment.Join(completion, segment);
                if (joinedWord.RemainingCharacterClasses > 0)
                {
                    this.RegisterSegment(joinedWord);
                    this.SegmentCombinationCount++;
                }
            }
            else
            {
                // remaining length is too short for valid words. do nothing.
            }
        }

        /// <summary>
        /// Register a new segment
        /// </summary>
        /// <param name="segment">the new segment</param>
        private void RegisterSegment(TextSegment segment)
        {
            foreach (var kvp in segment.UsedChars.Where(x => x.Value > 0))
            {
                if (!segmentRegistration.TryGetValue(kvp.Key, out var listForChar))
                {
                    listForChar = new List<TextSegment>();
                    segmentRegistration.Add(kvp.Key, listForChar);
                }

                listForChar.Add(segment);
            }
        }
    }
}
