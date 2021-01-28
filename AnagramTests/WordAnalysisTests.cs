namespace AnagramTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Anagram;

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
            var word = stream.CreatePartialWord("eeEtt");

            Assert.AreEqual(4, word.RemainingCharacterClasses);
            Assert.AreEqual(5, word.RemainingLength);
            Assert.AreEqual(1, word.RemainingChars['b']);
            Assert.AreEqual(2, word.RemainingChars['s']);
            Assert.AreEqual(1, word.RemainingChars['c']);
            Assert.AreEqual(1, word.RemainingChars['r']);
        }

        /// <summary>
        /// Analyse remaining characters with a unused character
        /// </summary>
        [TestMethod]
        public void InvalidWordAnalysisBadChar()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word = stream.CreatePartialWord("eeEttx");

            Assert.AreEqual(-1, word.RemainingCharacterClasses);
            Assert.AreEqual(-1, word.RemainingLength);
        }

        /// <summary>
        /// Analyse remaining characters with too many occurrences of 't'
        /// </summary>
        [TestMethod]
        public void InvalidWordAnalysisBadOccurences()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word = stream.CreatePartialWord("eeEttt");

            Assert.AreEqual(-1, word.RemainingCharacterClasses);
            Assert.AreEqual(-1, word.RemainingLength);
        }

        [TestMethod]
        public void ValidWordJoin()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word1 = stream.CreatePartialWord("eet");
            var word2 = stream.CreatePartialWord("bet");

            // "sscr" is missing
            var joined = TextSegment.Join(word1, word2);

            Assert.AreEqual(3, joined.RemainingCharacterClasses);
            Assert.AreEqual(4, joined.RemainingLength);
            Assert.AreEqual(2, joined.RemainingChars['s']);
            Assert.AreEqual(1, joined.RemainingChars['c']);
            Assert.AreEqual(1, joined.RemainingChars['r']);
        }

        [TestMethod]
        public void BadJoinWithCharactersAlreadyFullyUsed()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word1 = stream.CreatePartialWord("eeet");
            var word2 = stream.CreatePartialWord("bet");

            // "sscr" is missing
            var joined = TextSegment.Join(word1, word2);

            Assert.AreEqual(-1, joined.RemainingCharacterClasses);
            Assert.AreEqual(-1, joined.RemainingLength);
        }

        [TestMethod]
        public void BadJoinWithCharactersOveruse()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");
            var word1 = stream.CreatePartialWord("eet");
            var word2 = stream.CreatePartialWord("beet");

            // "sscr" is missing
            var joined = TextSegment.Join(word1, word2);

            Assert.AreEqual(-1, joined.RemainingCharacterClasses);
            Assert.AreEqual(-1, joined.RemainingLength);
        }
    }
}
