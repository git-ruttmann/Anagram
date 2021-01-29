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

        /// <summary>
        /// Run with the given sample "IT-Crowd".
        /// </summary>
        [TestMethod]
        public void SampleItCrowd()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("IT-Crowd");
            var collector = new AnagramCollector();
            stream.Results.Subscribe(x => collector.FoundAnagram(x));

            stream.ProcessWord("cod");
            stream.ProcessWord("cord");
            stream.ProcessWord("cow");
            stream.ProcessWord("dirt");
            stream.ProcessWord("doc");
            stream.ProcessWord("tic");
            stream.ProcessWord("wit");
            stream.ProcessWord("word");
            stream.ProcessWord("writ");

            Assert.IsTrue(collector.CheckWordCollection(new[] { "cow", "dirt" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "cord", "wit" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "tic", "word" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "cod", "writ" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "writ", "doc" }));
            Assert.AreEqual(5, collector.Results.Count);
        }

        /// <summary>
        /// Run with the given sample "Aschheim".
        /// </summary>
        [TestMethod]
        public void SampleAschheim()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("Aschheim");
            var collector = new AnagramCollector();
            stream.Results.Subscribe(x => collector.FoundAnagram(x));

            stream.ProcessWord("aches");
            stream.ProcessWord("ash");
            stream.ProcessWord("chase");
            stream.ProcessWord("chime");
            stream.ProcessWord("has");
            stream.ProcessWord("hash");
            stream.ProcessWord("hic");
            stream.ProcessWord("him");
            stream.ProcessWord("mice");
            stream.ProcessWord("shah");
            stream.ProcessWord("shame");

            Assert.IsTrue(collector.CheckWordCollection(new[] { "aches", "him" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "ash", "chime" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "chase", "him" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "chime", "has" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "hash", "mice" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "hic", "shame" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "mice", "shah" }));
            Assert.AreEqual(7, collector.Results.Count);
        }

        /// <summary>
        /// Run with the given sample "Best Secret".
        /// </summary>
        [TestMethod]
        public void SampleBestSecret()
        {
            var stream = AanagramStreamFactory.CreateTestableAnagramStream("Best Secret");
            var collector = new AnagramCollector();
            stream.Results.Subscribe(x => collector.FoundAnagram(x));

            stream.ProcessWord("beet");
            stream.ProcessWord("beets");
            stream.ProcessWord("beret");
            stream.ProcessWord("berets");
            stream.ProcessWord("beset");
            stream.ProcessWord("best");
            stream.ProcessWord("bests");
            stream.ProcessWord("bet");
            stream.ProcessWord("bets");
            stream.ProcessWord("better");
            stream.ProcessWord("betters");
            stream.ProcessWord("cess");
            stream.ProcessWord("crest");
            stream.ProcessWord("crests");
            stream.ProcessWord("crete");
            stream.ProcessWord("erect");
            stream.ProcessWord("erects");
            stream.ProcessWord("erst");
            stream.ProcessWord("rest");
            stream.ProcessWord("sec");
            stream.ProcessWord("secret");
            stream.ProcessWord("secrets");
            stream.ProcessWord("sect");
            stream.ProcessWord("sects");

            Assert.IsTrue(collector.CheckWordCollection(new[] { "beet", "crests" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "beets", "crest" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "beret", "sects" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "berets", "sect" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "beset", "crest" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "best", "erects" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "best", "secret" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "bests", "crete" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "bests", "erect" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "bet", "erst", "sec" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "bet", "rest", "sec" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "bet", "secrets" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "bets", "erects" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "bets", "secret" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "better", "cess" }));
            Assert.IsTrue(collector.CheckWordCollection(new[] { "betters", "sec" }));
            Assert.AreEqual(16, collector.Results.Count);
        }

        /// <summary>
        /// collect anagram data
        /// </summary>
        private class AnagramCollector
        {
            private List<IReadOnlyCollection<string>> result = new List<IReadOnlyCollection<string>>();

            public IReadOnlyCollection<IReadOnlyCollection<string>> Results => this.result;

            public bool CheckWordCollection(IEnumerable<string> words)
            {
                var wordList = words as IReadOnlyCollection<string> ?? words.ToArray();
                return this.result.Any(line => words.All(word => line.Contains(word)));
            }

            /// <inheritdoc/>
            public void FoundAnagram(IEnumerable<string> words)
            {
                result.Add(words.ToArray());
            }
        }
    }
}
