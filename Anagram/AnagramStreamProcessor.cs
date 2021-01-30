namespace Anagram
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
        /// registered segments that match the anagram
        /// </summary>
        private readonly Dictionary<char, List<TextSegment>> segmentRegistration;

        /// <summary>
        /// the used characters and their count in the anagram source
        /// </summary>
        private readonly ReadOnlyDictionary<char, int> anagramCharacterCounts;

        /// <summary>
        /// The segments awaiting registration (deferred because of parallel processing)
        /// </summary>
        private readonly List<TextSegment> segmentsToRegister;

        /// <summary>
        /// subject for registration
        /// </summary>
        private readonly Subject<IEnumerable<string>> subsribeSubject;

        /// <summary>
        /// the thread synchronous output subject
        /// </summary>
        private readonly ISubject<IEnumerable<string>> synchronizedOutputSubject;

        /// <summary>
        /// The active generation, compared to <see cref="TextSegment.TestGeneration"/>
        /// </summary>
        private int activeTestGeneration;

        /// <summary>
        /// The characters of the active processed word
        /// </summary>
        private IReadOnlyDictionary<char, int> activeChars;

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
            this.anagramCharacterCounts = this.CountCharactersInAnagram(source);
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

            if (!this.CountCharacters(word, out var remainingCharacters, out this.activeChars))
            {
                return;
            }

            var segment = new TextSegment(remainingCharacters, word);
            if (segment.RemainingCharacterClassCount == 0)
            {
                this.synchronizedOutputSubject.OnNext(segment.Words);
                return;
            }

            if (segment.RemainingLength <= minWordSize)
            {
                return;
            }

            var potentialCompletions = this.activeChars.Keys.SelectMany(x => this.segmentRegistration[x]);
            this.activeTestGeneration++;
            Parallel.ForEach(potentialCompletions, x => this.HandlePossibleCompletion(word, x));

            this.AddSegmentForRegistration(segment);
            this.SingleWordSegmentCount++;
            this.DoSegmentRegistration();
        }

        /// <inheritdoc/>
        public TextSegment CreateTextSegment(string text)
        {
            if (!this.CountCharacters(text, out var remainingCharacters, out this.activeChars))
            {
                return TextSegment.InvalidSegment;
            }

            return new TextSegment(remainingCharacters, text);
        }

        /// <inheritdoc/>
        public TextSegment JoinTextAndSegment(TextSegment first, string text)
        {
            if (!this.CountCharacters(text, out var _, out this.activeChars))
            {
                return TextSegment.InvalidSegment;
            }

            return this.Join(first, text);
        }

        /// <summary>
        /// Joins two text segments and their count
        /// </summary>
        /// <param name="first">the first text segment</param>
        /// <param name="text">the text of the second segment</param>
        /// <returns>The combined text segment. May be invalid.</returns>
        private TextSegment Join(TextSegment first, string text)
        {
            var remainingCharacters = new Dictionary<char, int>(first.RemainingCharacters);
            foreach (var kvp in this.activeChars)
            {
                if (!remainingCharacters.TryGetValue(kvp.Key, out var oldCount))
                {
                    return TextSegment.InvalidSegment;
                }

                if (oldCount == kvp.Value)
                {
                    remainingCharacters.Remove(kvp.Key);
                }
                else if (oldCount < kvp.Value)
                {
                    return TextSegment.InvalidSegment;
                }
                else
                {
                    remainingCharacters[kvp.Key] -= kvp.Value;
                }
            }

            return new TextSegment(remainingCharacters, first.Words.Append(text));
        }

        /// <summary>
        /// Count the used characters and count down the remaining characters of the anagram
        /// </summary>
        /// <param name="text">the text to analyze</param>
        /// <param name="remaining">the remaining characters to complete the anagram.</param>
        /// <param name="usedCharacters">the used characters.</param>
        /// <returns>true if the remaining chars have a valid state</returns>
        private bool CountCharacters(string text, out Dictionary<char, int> remaining, out IReadOnlyDictionary<char, int> usedCharacters)
        {
            var used = new Dictionary<char, int>();
            usedCharacters = used;
            remaining = new Dictionary<char, int>(this.anagramCharacterCounts);
            foreach (var c in text.ToLowerInvariant())
            {
                if (!remaining.TryGetValue(c, out var oldCount))
                {
                    return false;
                }

                if (oldCount == 1)
                {
                    remaining.Remove(c);
                }
                else
                {
                    remaining[c] -= 1;
                }

                if (!used.TryGetValue(c, out var usedCount))
                {
                    usedCount = 0;
                }

                used[c] = usedCount + 1;
            }

            return true;
        }

        /// <summary>
        /// test combinations of registered segments (and their combinations). auto registers new segments
        /// </summary>
        /// <param name="word">the current processed word</param>
        /// <param name="completion">a possible registered completion</param>
        private void HandlePossibleCompletion(string word, TextSegment completion)
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
                if (TextSegment.IsFullCompletion(completion, this.activeChars))
                {
                    this.synchronizedOutputSubject.OnNext(completion.Words.Append(word));
                }
            }
            else if (completion.RemainingLength - word.Length > minWordSize)
            {
                var joinedWord = this.Join(completion, word);
                if (joinedWord.RemainingCharacterClassCount > 0)
                {
                    this.AddSegmentForRegistration(joinedWord);
                }
            }
            else
            {
                // remaining length is too short for valid words. do nothing.
            }
        }

        /// <summary>
        /// Count the characters in the anagram. Drop invalid characters.
        /// </summary>
        /// <param name="text">the anagram text</param>
        /// <returns>A dictionary with used characters and their count</returns>
        private ReadOnlyDictionary<char, int> CountCharactersInAnagram(string text)
        {
            var usedChars = new Dictionary<char, int>();
            foreach (var c in text.ToLowerInvariant().Where(x => Char.IsLetterOrDigit(x)))
            {
                if (usedChars.ContainsKey(c))
                {
                    usedChars[c]++;
                }
                else
                {
                    usedChars.Add(c, 1);
                    segmentRegistration.Add(c, new List<TextSegment>());
                }
            }

            return new ReadOnlyDictionary<char, int>(usedChars);
        }

        /// <summary>
        /// Register a new segment. (actually it's deferred)
        /// </summary>
        /// <param name="segment">the new segment</param>
        private void AddSegmentForRegistration(TextSegment segment)
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
                    foreach (var kvp in segment.RemainingCharacters)
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
