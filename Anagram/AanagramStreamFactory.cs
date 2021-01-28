namespace Anagram
{
    /// <summary>
    /// Factory to creata an anagram processing stream
    /// </summary>
    public static class AanagramStreamFactory
    {
        /// <summary>
        /// Create an anagram stream
        /// </summary>
        /// <param name="text">the anagram text</param>
        /// <returns>a stream</returns>
        public static IAnagramStream CreateAnagramStream(string text)
        {
            var stream = new AnagramStreamProcessor(text, 2);
            return stream;
        }

        /// <summary>
        /// Create an anagram stream with additional exposed test properties.
        /// </summary>
        /// <param name="text">the anagram text</param>
        /// <returns>a stream</returns>
        /// <remarks>
        /// Unittesting only
        /// </remarks>
        internal static ITestableAnagramStream CreateTestableAnagramStream(string text)
        {
            var stream = new AnagramStreamProcessor(text, 2);
            return stream;
        }
    }
}
