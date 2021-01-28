namespace AnagramTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Anagram;

    [TestClass]
    public class SegmentationTests
    {
        /// <summary>
        /// Match the word by two segments
        /// </summary>
        [TestMethod]
        public void SingleMatch()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");

            var collector = new AnagramCollector();
            stream.Results.Subscribe(x => collector.FoundAnagram(x));

            stream.ProcessWord("Bestsecret");

            Assert.AreEqual(1, collector.Results.Count, "One match without case must be found");
            Assert.AreEqual(1, collector.Results.First().Count, "One match without case must be found");
        }

        /// <summary>
        /// Match the word by two segments
        /// </summary>
        [TestMethod]
        public void DualMatch()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");

            var collector = new AnagramCollector();
            stream.Results.Subscribe(x => collector.FoundAnagram(x));

            stream.ProcessWord("Best");
            stream.ProcessWord("nonsense");
            stream.ProcessWord("secret");

            Assert.AreEqual(1, collector.Results.Count, "One match without case must be found");
        }

        /// <summary>
        /// Match the word by three segments
        /// </summary>
        [TestMethod]
        public void TripleMatch()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");

            var collector = new AnagramCollector();
            stream.Results.Subscribe(x => collector.FoundAnagram(x));

            stream.ProcessWord("Best");
            stream.ProcessWord("nonsense");
            stream.ProcessWord("sec");
            stream.ProcessWord("ret");

            Assert.AreEqual(1, collector.Results.Count, "One match without case must be found");
        }

        /// <summary>
        /// Match the word by three segments
        /// </summary>
        [TestMethod]
        public void MisMatchShortWord()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");

            var collector = new AnagramCollector();
            stream.Results.Subscribe(x => collector.FoundAnagram(x));

            stream.ProcessWord("Best");
            stream.ProcessWord("nonsense");
            stream.ProcessWord("secr");
            stream.ProcessWord("et");

            Assert.AreEqual(0, collector.Results.Count, "One word was too short, must not find a match");
        }

        [TestMethod]
        public void KeepValidWordsOnly()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("BestSecret");

            Assert.AreEqual(0, stream.SingleWordSegmentCount);
            stream.ProcessWord("best");
            Assert.AreEqual(1, stream.SingleWordSegmentCount);
            stream.ProcessWord("xyz");
            Assert.AreEqual(1, stream.SingleWordSegmentCount, "Must not register unusable characters");
            stream.ProcessWord("b");
            Assert.AreEqual(1, stream.SingleWordSegmentCount, "Must not register short words");
            stream.ProcessWord("be");
            Assert.AreEqual(1, stream.SingleWordSegmentCount, "Must not register short words");
            stream.ProcessWord("secret");
            Assert.AreEqual(2, stream.SingleWordSegmentCount, "also register words that produce matches");
            stream.ProcessWord("bestsecre");
            Assert.AreEqual(2, stream.SingleWordSegmentCount, "must not register words with too shoort rest length");
        }


        [TestMethod]
        public void SampleItCrowd()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("IT-Crowd");

            stream.ProcessWord("cod");
            stream.ProcessWord("writ");
            stream.ProcessWord("cord");
            stream.ProcessWord("wit");
            stream.ProcessWord("cow");
            stream.ProcessWord("dirt");
            stream.ProcessWord("doc");
            stream.ProcessWord("writ");
            stream.ProcessWord("tic");
            stream.ProcessWord("word");

            Assert.AreEqual(0, stream.SingleWordSegmentCount);
            stream.ProcessWord("best");
            Assert.AreEqual(1, stream.SingleWordSegmentCount);
            stream.ProcessWord("xyz");
            Assert.AreEqual(1, stream.SingleWordSegmentCount, "Must not register unusable characters");
            stream.ProcessWord("b");
            Assert.AreEqual(1, stream.SingleWordSegmentCount, "Must not register short words");
            stream.ProcessWord("be");
            Assert.AreEqual(1, stream.SingleWordSegmentCount, "Must not register short words");
            stream.ProcessWord("secret");
            Assert.AreEqual(2, stream.SingleWordSegmentCount, "also register words that produce matches");
            stream.ProcessWord("bestsecre");
            Assert.AreEqual(2, stream.SingleWordSegmentCount, "must not register words with too shoort rest length");
        }

        /// <summary>
        /// collect anagram data
        /// </summary>
        private class AnagramCollector
        {
            private List<IReadOnlyCollection<string>> result = new List<IReadOnlyCollection<string>>();

            public IReadOnlyCollection<IReadOnlyCollection<string>> Results => this.result;

            /// <inheritdoc/>
            public void FoundAnagram(IEnumerable<string> words)
            {
                result.Add(words.ToArray());
            }
        }
    }
}
