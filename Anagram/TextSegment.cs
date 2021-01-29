namespace Anagram
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// represent a word that is matching the anagram partially
    /// </summary>
    internal class TextSegment
    {
        /// <summary>
        /// Construct a new text segment
        /// </summary>
        /// <param name="anagramCharacterCounts">expected characters and their count</param>
        /// <param name="text">the text of the segment</param>
        public TextSegment(IReadOnlyDictionary<char, int> anagramCharacterCounts, string text)
        {
            this.RemainingChars = new Dictionary<char, int>(anagramCharacterCounts);
            this.UsedChars = new Dictionary<char, int>();

            var possible = this.InitializeUsedChars(text);
            if (possible)
            {
                UpdateRemaining();
            }
            else
            {
                Invalidate();
            }

            this.Words = new[] { text };
        }

        /// <summary>
        /// Internal constructor for join
        /// </summary>
        /// <param name="first">the first segment of the join</param>
        /// <param name="otherWords">the second word</param>
        private TextSegment(TextSegment first, IEnumerable<string> otherWords)
        {
            this.Words = first.Words.Concat(otherWords).ToArray();
            this.RemainingChars = new Dictionary<char, int>(first.RemainingChars);
            this.UsedChars = new Dictionary<char, int>(first.UsedChars);
        }


        /// <summary>
        /// Gets the remaining characters, multiple occurences of the same character is counted as one character class
        /// </summary>
        public int RemainingCharacterClasses { get; private set; }

        /// <summary>
        /// Gets the remaining lenght to match the anagram. each occurence of the same character is counted.
        /// </summary>
        public int RemainingLength { get; private set; }

        /// <summary>
        /// Gets all characters and their count used by the word combination
        /// </summary>
        public Dictionary<char, int> UsedChars { get; private set; }

        /// <summary>
        /// Gets all characters and their count remaining to complete the anagram
        /// </summary>
        public Dictionary<char, int> RemainingChars { get; init; }

        /// <summary>
        /// Gets the combination of words
        /// </summary>
        public IEnumerable<string> Words { get; }

        /// <summary>
        /// The last test generation, this item was checked
        /// </summary>
        public int TestGeneration { get; set; }

        /// <summary>
        /// Joins two text segments and their count
        /// </summary>
        /// <param name="first">the first text segment</param>
        /// <param name="other">the second text segment</param>
        /// <returns>The combined text segment. May be invalid.</returns>
        public static TextSegment Join(TextSegment first, TextSegment other)
        {
            var result = new TextSegment(first, other.Words);
            var possible = result.JoinCharacterUsage(other.UsedChars);
            if (possible)
            {
                result.UpdateRemaining();
            }
            else
            {
                result.Invalidate();
            }

            return result;
        }

        /// <summary>
        /// Tests if the <see cref="part"/> fully completes the anagram
        /// </summary>
        /// <param name="completion">the registered completions segment</param>
        /// <param name="part">the new word</param>
        /// <returns>true if completion an part are a valid anagram</returns>
        public static bool IsFullCompletion(TextSegment completion, TextSegment part)
        {
            if (completion.RemainingCharacterClasses != part.UsedChars.Count)
            {
                return false;
            }

            var joinedWord = TextSegment.Join(completion, part);
            return joinedWord.RemainingCharacterClasses == 0;
        }

        /// <summary>
        /// Add the used chars of another segment
        /// </summary>
        /// <param name="usedChars">the used chars of the other segment</param>
        /// <returns>true if all character counts are still valid</returns>
        private bool JoinCharacterUsage(IReadOnlyDictionary<char, int> usedChars)
        {
            foreach (var kvp in usedChars)
            {
                if (!this.RemainingChars.TryGetValue(kvp.Key, out var oldCount))
                {
                    return false;
                }

                if (oldCount < kvp.Value)
                {
                    return false;
                }

                this.RemainingChars[kvp.Key] -= kvp.Value;

                if (!this.UsedChars.TryGetValue(kvp.Key, out var oldUsedCount))
                {
                    oldUsedCount = 0;
                }

                this.UsedChars[kvp.Key] = oldUsedCount + kvp.Value;
            }

            return true;
        }

        /// <summary>
        /// Initialize the character usage and the remaining characters
        /// </summary>
        /// <param name="text">the text</param>
        /// <returns>true if all character counts are still valid</returns>
        private bool InitializeUsedChars(string text)
        {
            foreach (var c in text.ToLowerInvariant())
            {
                if (!this.RemainingChars.ContainsKey(c))
                {
                    return false;
                }

                this.RemainingChars[c] -= 1;
                if (this.RemainingChars[c] < 0)
                {
                    return false;
                }

                if (this.UsedChars.ContainsKey(c))
                {
                    this.UsedChars[c] += 1;
                }
                else
                {
                    this.UsedChars[c] = 1;
                }
            }

            return true;
        }

        /// <summary>
        /// Remove characters that have a 0 of expected additional count. Update the counts
        /// </summary>
        private void UpdateRemaining()
        {
            var fullyConsumed = this.RemainingChars
                .Where(kvp => kvp.Value == 0)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var c in fullyConsumed)
            {
                this.RemainingChars.Remove(c);
            }

            this.RemainingCharacterClasses = this.RemainingChars.Count;
            this.RemainingLength = this.RemainingChars.Aggregate(0, (sum, kvp) => sum + kvp.Value);
        }

        /// <summary>
        /// Indicate that the segment is not valid anymore
        /// </summary>
        private void Invalidate()
        {
            this.RemainingCharacterClasses = -1;
            this.RemainingLength = -1;
        }
    }
}
