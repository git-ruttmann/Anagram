namespace Anagram
{
    /// <summary>
    /// Extent the <see cref="IAnagramStream"/> with test information. Unittesting only.
    /// </summary>
    internal interface ITestableAnagramStream : IAnagramStream
    {
        /// <summary>
        /// Gets the number of registered partial words
        /// </summary>
        int SingleWordSegmentCount { get; }

        /// <summary>
        /// Gets the number of word combinations
        /// </summary>
        int SegmentCombinationCount { get; }

        /// <summary>
        /// Create a word with character count characteristics
        /// </summary>
        TextSegment CreatePartialWord(string text);
    }
}
