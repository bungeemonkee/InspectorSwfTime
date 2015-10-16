using System.IO;
using InspectorSwfTime.Swf;
using InspectorSwfTime.Swf.Tags;
using NUnit.Framework;

namespace InspectorSwfTime.Tests
{
    [TestFixture]
    class SwfInfoTests
    {
        [Test]
        public void SwfInfo_parse_header()
        {
            SwfInfo swf;
            using (var stream = TestSwf_HeaderOnly())
            {
                swf = new SwfInfo(stream);
            }

            Assert.AreEqual(SwfHeaderSignature.FWS, swf.Header.Signature);
            Assert.AreEqual(9, swf.Header.Version);
            Assert.AreEqual(19, swf.Header.FileLength);
            Assert.AreEqual(0, swf.Header.FrameSize.XMin);
            Assert.AreEqual(1024, swf.Header.FrameSize.XMax);
            Assert.AreEqual(0, swf.Header.FrameSize.YMin);
            Assert.AreEqual(768, swf.Header.FrameSize.YMax);
            Assert.AreEqual(3, swf.Header.FrameRate);
            Assert.AreEqual(1, swf.Header.FrameCount);
        }

        [Test]
        public void SwfInfo_parse_no_tags()
        {
            SwfInfo swf;
            using (var stream = TestSwf_NoTags())
            {
                swf = new SwfInfo(stream);
            }

            Assert.AreEqual(0, swf.Tags.Length);
        }

        [Test]
        public void SwfInfo_parse_empty_tag()
        {
            SwfInfo swf;
            using (var stream = TestSwf_EmptyTag())
            {
                swf = new SwfInfo(stream);
            }

            Assert.AreEqual(1, swf.Tags.Length);
            Assert.AreEqual(4, swf.Tags[0].Code);
            Assert.AreEqual(0, swf.Tags[0].Length);
        }

        [Test]
        public void SwfInfo_parse_non_empty_tag()
        {
            SwfInfo swf;
            using (var stream = TestSwf_NonEmptyTag())
            {
                swf = new SwfInfo(stream);
            }

            Assert.AreEqual(1, swf.Tags.Length);
            Assert.AreEqual(4, swf.Tags[0].Code);
            Assert.AreEqual(8, swf.Tags[0].Length);
            Assert.AreEqual(8, ((GenericTag)swf.Tags[0]).Data.Length);
        }

        private static Stream TestSwf_HeaderOnly()
        {
            return new MemoryStream(new byte[]
                {
                    0x46, 0x57, 0x53, // swf header: uncompressed
                    0x09, // swf header: version 9
                    0x13, 0x00, 0x00, 0x00, // swf header: length 19
                    0x60, 0x00, 0x20, 0x00, 0x00, 0x18, 0x00, // swf rect: Xmin: 0, Xmax: 1024, Ymin: 0, Ymax: 768
                    0x03, 0x00, // swf header: frame rate 3
                    0x01, 0x00 // swf header: frames 1
                });
        }

        private static Stream TestSwf_NoTags()
        {
            return new MemoryStream(new byte[]
                {
                    0x46, 0x57, 0x53, // swf header: uncompressed
                    0x09, // swf header: version 9
                    0x15, 0x00, 0x00, 0x00, // swf header: length 21
                    0x60, 0x00, 0x20, 0x00, 0x00, 0x18, 0x00, // swf rect: Xmin: 0, Xmax: 1024, Ymin: 0, Ymax: 768
                    0x03, 0x00, // swf header: frame rate 3
                    0x01, 0x00, // swf header: frames 1

                    0x00, 0x00 // swf tag: code: 0, length: 0
                });
        }

        private static Stream TestSwf_EmptyTag()
        {
            return new MemoryStream(new byte[]
                {
                    0x46, 0x57, 0x53, // swf header: uncompressed
                    0x09, // swf header: version 9
                    0x17, 0x00, 0x00, 0x00, // swf header: length 23
                    0x60, 0x00, 0x20, 0x00, 0x00, 0x18, 0x00, // swf rect: Xmin: 0, Xmax: 1024, Ymin: 0, Ymax: 768
                    0x03, 0x00, // swf header: frame rate 3
                    0x01, 0x00, // swf header: frames 1

                    0x00, 0x01, // swf tag: code: 4, length: 0

                    0x00, 0x00 // swf tag (end tag): code: 0, length: 0
                });
        }

        private static Stream TestSwf_NonEmptyTag()
        {
            return new MemoryStream(new byte[]
                {
                    0x46, 0x57, 0x53, // swf header: uncompressed
                    0x09, // swf header: version 9
                    0x1F, 0x00, 0x00, 0x00, // swf header: length 31
                    0x60, 0x00, 0x20, 0x00, 0x00, 0x18, 0x00, // swf rect: Xmin: 0, Xmax: 1024, Ymin: 0, Ymax: 768
                    0x03, 0x00, // swf header: frame rate 3
                    0x01, 0x00, // swf header: frames 1

                    0x08, 0x01, // swf tag: code: 4, length: 8
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // tag data

                    0x00, 0x00 // swf tag (end tag): code: 0, length: 0
                });
        }
    }
}
