namespace AnagramTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Anagram;
    using System.Collections.Generic;

    [TestClass]
    public class WordAnalysisTests
    {
        /// <summary>
        /// Analyse remaining characters with a valid subset
        /// </summary>
        [TestMethod]
        public void ValidWordAnalysis()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word = stream.CreateTextSegment("eeEtt");

            Assert.AreEqual(4, word.RemainingCharacterClassCount);
            Assert.AreEqual(5, word.RemainingLength);
            Assert.AreEqual(1, word.RemainingCharacters['b']);
            Assert.AreEqual(2, word.RemainingCharacters['s']);
            Assert.AreEqual(1, word.RemainingCharacters['c']);
            Assert.AreEqual(1, word.RemainingCharacters['r']);
        }

        /// <summary>
        /// Analyse remaining characters with a unused character
        /// </summary>
        [TestMethod]
        public void InvalidWordAnalysisBadChar()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word = stream.CreateTextSegment("eeEttx");

            Assert.AreEqual(-1, word.RemainingCharacterClassCount);
            Assert.AreEqual(-1, word.RemainingLength);
        }

        /// <summary>
        /// Analyse remaining characters with too many occurrences of 't'
        /// </summary>
        [TestMethod]
        public void InvalidWordAnalysisBadOccurences()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word = stream.CreateTextSegment("eeEttt");

            Assert.AreEqual(-1, word.RemainingCharacterClassCount);
            Assert.AreEqual(-1, word.RemainingLength);
        }

        [TestMethod]
        public void ValidWordJoin()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word1 = stream.CreateTextSegment("eet");

            // "sscr" is missing
            var joined = stream.JoinTextAndSegment(word1, "bet");

            Assert.AreEqual(3, joined.RemainingCharacterClassCount);
            Assert.AreEqual(4, joined.RemainingLength);
            Assert.AreEqual(2, joined.RemainingCharacters['s']);
            Assert.AreEqual(1, joined.RemainingCharacters['c']);
            Assert.AreEqual(1, joined.RemainingCharacters['r']);
        }

        [TestMethod]
        public void BadJoinWithCharactersAlreadyFullyUsed()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word1 = stream.CreateTextSegment("eeet");

            // "sscr" is missing
            var joined = stream.JoinTextAndSegment(word1, "bet");

            Assert.AreEqual(-1, joined.RemainingCharacterClassCount);
            Assert.AreEqual(-1, joined.RemainingLength);
        }

        [TestMethod]
        public void BadJoinWithCharactersOveruse()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word1 = stream.CreateTextSegment("eet");

            // "sscr" is missing
            var joined = stream.JoinTextAndSegment(word1, "beet");

            Assert.AreEqual(-1, joined.RemainingCharacterClassCount);
            Assert.AreEqual(-1, joined.RemainingLength);
        }
    }
}
