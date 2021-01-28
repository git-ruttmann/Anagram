namespace Anagram
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Stream for potential anagram parts
    /// </summary>
    public interface IAnagramStream
    {
        /// <summary>
        /// Gets an Observable for matched anagrams
        /// </summary>
        IObservable<IEnumerable<string>> Results { get; }

        /// <summary>
        /// Process a word
        /// </summary>
        /// <param name="word">the word</param>
        void ProcessWord(string word);
    }
}
