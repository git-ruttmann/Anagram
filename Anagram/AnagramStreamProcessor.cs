namespace Anagram
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    /// <summary>
    /// Finds find anagrams in streamed words
    /// </summary>
    internal class AnagramStreamProcessor : ITestableAnagramStream
    {
        private readonly int minWordSize;

        /// <summary>
        /// The active generation, compared to <see cref="TextSegment.TestGeneration"/>
        /// </summary>
        private int activeTestGeneration;

        /// <summary>
        /// registered segments that match the anagram
        /// </summary>
        private readonly Dictionary<char, List<TextSegment>> segmentRegistration;

        /// <summary>
        /// the used characters and their count in the anagram source
        /// </summary>
        private readonly Dictionary<char, int> anagramCharacterCounts;

        /// <summary>
        /// The segments awaiting registration (deferred because of parallel processing)
        /// </summary>
        private readonly List<TextSegment> segmentsToRegister;

        /// <summary>
        /// subject for registration
        /// </summary>
        private Subject<IEnumerable<string>> subsribeSubject;

        /// <summary>
        /// the thread synchronous output subject
        /// </summary>
        ISubject<IEnumerable<string>> synchronizedOutputSubject;

        /// <summary>
        /// private constructor to avoid public create
        /// </summary>
        public AnagramStreamProcessor(string source, int minWordSize)
        {
            this.subsribeSubject = new Subject<IEnumerable<string>>();
            this.synchronizedOutputSubject = Subject.Synchronize(subsribeSubject);
            this.minWordSize = minWordSize;
            this.segmentsToRegister = new List<TextSegment>();
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
        public IObservable<IEnumerable<string>> Results => this.subsribeSubject;

        /// <inheritdoc/>
        public int SingleWordSegmentCount { get; private set; }

        /// <inheritdoc/>
        public int SegmentCombinationCount { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<IEnumerable<TextSegment>> SegmentsPerCharacter => this.segmentRegistration.Values;

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
                this.synchronizedOutputSubject.OnNext(segment.Words);
                return;
            }

            if (segment.RemainingLength <= minWordSize)
            {
                return;
            }

            var potentialCompletions = segment
                .UsedChars
                .Keys
                .SelectMany(x => this.segmentRegistration[x]);

            this.activeTestGeneration++;
            Parallel.ForEach(potentialCompletions, x => this.HandlePossibleCompletion(word, segment, x));

            this.RegisterSegment(segment);
            this.SingleWordSegmentCount++;
            this.DoSegmentRegistration();
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
            lock(completion)
            {
                if (completion.TestGeneration == this.activeTestGeneration)
                {
                    return;
                }

                completion.TestGeneration = this.activeTestGeneration;
            }

            if (completion.RemainingLength == word.Length)
            {
                if (TextSegment.IsFullCompletion(completion, segment))
                {
                    this.synchronizedOutputSubject.OnNext(completion.Words.Concat(segment.Words));
                }
            }
            else if (completion.UsedChars.Count > 0 && completion.RemainingLength - word.Length > minWordSize)
            {
                var joinedWord = TextSegment.Join(completion, segment);
                if (joinedWord.RemainingCharacterClasses > 0)
                {
                    this.RegisterSegment(joinedWord);
                }
            }
            else
            {
                // remaining length is too short for valid words. do nothing.
            }
        }

        /// <summary>
        /// Register a new segment. (actually it's deferred)
        /// </summary>
        /// <param name="segment">the new segment</param>
        private void RegisterSegment(TextSegment segment)
        {
            lock(this.segmentsToRegister)
            {
                this.segmentsToRegister.Add(segment);
                this.SegmentCombinationCount++;
            }
        }

        /// <summary>
        /// Register all deferred segments
        /// </summary>
        private void DoSegmentRegistration()
        {
            lock (this.segmentsToRegister)
            {
                foreach (var segment in this.segmentsToRegister)
                {
                    foreach (var kvp in segment.RemainingChars)
                    {
                        if (!segmentRegistration.TryGetValue(kvp.Key, out var listForChar))
                        {
                            listForChar = new List<TextSegment>();
                            segmentRegistration.Add(kvp.Key, listForChar);
                        }

                        segment.TestGeneration = this.activeTestGeneration;
                        listForChar.Add(segment);
                    }
                }

                this.segmentsToRegister.Clear();
            }
        }
    }
}
