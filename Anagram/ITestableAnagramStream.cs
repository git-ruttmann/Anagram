namespace Anagram
{
    using System.Collections.Generic;

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
        TextSegment CreateTextSegment(string text);

        /// <summary>
        /// Gets the list of possible text segments per character
        /// </summary>
        IEnumerable<IEnumerable<TextSegment>> SegmentsPerCharacter { get; }

        /// <summary>
        /// Join an existing text segment with a new text
        /// </summary>
        /// <param name="first">the existing text segment</param>
        /// <param name="text">the new text</param>
        /// <returns>a joined text segment</returns>
        TextSegment JoinTextAndSegment(TextSegment first, string text);
    }
}
