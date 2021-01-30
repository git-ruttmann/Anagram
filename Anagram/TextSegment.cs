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
        public TextSegment(IReadOnlyDictionary<char, int> remainingCharacters, string text)
            : this(remainingCharacters, new[] { text })
        {
        }

        /// <summary>
        /// Internal constructor for join
        /// </summary>
        /// <param name="first">the first segment of the join</param>
        /// <param name="text">the second word</param>
        public TextSegment(IReadOnlyDictionary<char, int> remainingCharacters, IEnumerable<string> words)
        {
            this.RemainingCharacters = remainingCharacters;
            this.Words = words as IReadOnlyCollection<string> ?? words.ToArray();
            this.RemainingCharacterClassCount = this.RemainingCharacters.Count;
            this.RemainingLength = this.RemainingCharacters.Aggregate(0, (sum, kvp) => sum + kvp.Value);
        }

        /// <summary>
        /// Gets the remaining characters, multiple occurences of the same character is counted as one character class
        /// </summary>
        public int RemainingCharacterClassCount { get; private init; }

        /// <summary>
        /// Gets the remaining lenght to match the anagram. each occurence of the same character is counted.
        /// </summary>
        public int RemainingLength { get; private init; }

        /// <summary>
        /// Gets all characters and their count remaining to complete the anagram
        /// </summary>
        public IReadOnlyDictionary<char, int> RemainingCharacters { get; }

        /// <summary>
        /// Gets the combination of words
        /// </summary>
        public IEnumerable<string> Words { get; }

        /// <summary>
        /// The last test generation, this item was checked
        /// </summary>
        public int TestGeneration { get; set; }

        /// <summary>
        /// A text segment that is invalid
        /// </summary>
        public static TextSegment InvalidSegment { get; } = new TextSegment(new Dictionary<char, int>(), string.Empty) 
        { 
            RemainingCharacterClassCount = -1, 
            RemainingLength = -1
        };

        /// <summary>
        /// Tests if the <see cref="part"/> fully completes the anagram
        /// </summary>
        /// <param name="completion">the registered completions segment</param>
        /// <param name="part">the new word</param>
        /// <returns>true if completion an part are a valid anagram</returns>
        public static bool IsFullCompletion(TextSegment completion, IReadOnlyDictionary<char, int> usedChars)
        {
            if (completion.RemainingCharacterClassCount != usedChars.Count)
            {
                return false;
            }

            return usedChars.Keys.All(x => completion.RemainingCharacters.ContainsKey(x) && completion.RemainingCharacters[x] == usedChars[x]);
        }
    }
}
