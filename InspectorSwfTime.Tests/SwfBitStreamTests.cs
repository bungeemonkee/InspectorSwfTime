using System.IO;
using InspectorSwfTime.Swf;
using NUnit.Framework;

namespace InspectorSwfTime.Tests
{
    [TestFixture]
    public class SwfBitStreamTests
    {
        [Test]
        public void Get_1_Bit()
        {
            using (var stream = MakeStream(new byte[]
                {
                    128, 0, 0, 0
                }))
            {
                var result = stream.GetUnsignedBitValue(1);

                Assert.AreEqual(1, result);
            }
        }

        [Test]
        public void GetUnsignedBitValue_get_one_bit_three_times()
        {
            using (var stream = MakeStream(new byte[]
                {
                    160, 0, 0, 0
                }))
            {
                var result1 = stream.GetUnsignedBitValue(1);
                var result2 = stream.GetUnsignedBitValue(1);
                var result3 = stream.GetUnsignedBitValue(1);

                Assert.AreEqual(1, result1);
                Assert.AreEqual(0, result2);
                Assert.AreEqual(1, result3);
            }
        }

        [Test]
        public void GetUnsignedBitValue_get_two_bits_four_times()
        {
            using (var stream = MakeStream(new byte[]
                {
                    100, 0, 0, 0
                }))
            {
                var result1 = stream.GetUnsignedBitValue(2);
                var result2 = stream.GetUnsignedBitValue(2);
                var result3 = stream.GetUnsignedBitValue(2);
                var result4 = stream.GetUnsignedBitValue(2);

                Assert.AreEqual(1, result1);
                Assert.AreEqual(2, result2);
                Assert.AreEqual(1, result3);
                Assert.AreEqual(0, result4);
            }
        }

        [Test]
        public void GetUnsignedBitValue_get_two_bits_across_two_bytes()
        {
            using (var stream = MakeStream(new byte[]
                {
                    1, 128, 0, 0
                }))
            {
                var result1 = stream.GetUnsignedBitValue(7);
                var result2 = stream.GetUnsignedBitValue(2);

                Assert.AreEqual(0, result1);
                Assert.AreEqual(3, result2);
            }
        }

        [Test]
        public void GetUnsignedBitValue_get_nine_bits()
        {
            using (var stream = MakeStream(new byte[]
                {
                    1, 1, 0, 0
                }))
            {
                var result1 = stream.GetUnsignedBitValue(7);
                var result2 = stream.GetUnsignedBitValue(9);

                Assert.AreEqual(0, result1);
                Assert.AreEqual(257, result2);
            }
        }

        [Test]
        public void GetUnsignedBitValue_get_ten_bits_across_three_bytes()
        {
            using (var stream = MakeStream(new byte[]
                {
                    1, 0, 128, 0
                }))
            {
                var result1 = stream.GetUnsignedBitValue(7);
                var result2 = stream.GetUnsignedBitValue(10);

                Assert.AreEqual(0, result1);
                Assert.AreEqual(513, result2);
            }
        }

        [Test]
        public void GetUnsignedBitValue_get_eighteen_bits_across_three_bytes()
        {
            using (var stream = MakeStream(new byte[]
                {
                    1, 0, 0, 128
                }))
            {
                var result1 = stream.GetUnsignedBitValue(7);
                var result2 = stream.GetUnsignedBitValue(18);

                Assert.AreEqual(0, result1);
                Assert.AreEqual(131073, result2);
            }
        }

        [Test]
        public void GetSignedBitValue_get_4_bits()
        {
            using (var stream = MakeStream(new byte[]
                {
                    14, 0, 0, 0
                }))
            {
                var result1 = stream.GetUnsignedBitValue(4);
                var result2 = stream.GetSignedBitValue(4);

                Assert.AreEqual(0, result1);
                Assert.AreEqual(-2, result2);
            }
        }

        private static SwfBitStream MakeStream(byte[] bytes)
        {
            return new SwfBitStream(new MemoryStream(bytes));
        }
    }
}
