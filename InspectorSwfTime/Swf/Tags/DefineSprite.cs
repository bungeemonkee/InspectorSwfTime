using System.Collections.Generic;

namespace InspectorSwfTime.Swf.Tags
{
    public class DefineSprite : SwfTag
    {
        public const ushort TagCode = 39;

        public uint SpriteID { get; protected set; }
        public uint FrameCount { get; protected set; }

        public SwfTag[] ControlTags { get; protected set; }

        internal DefineSprite(ulong length, SwfBitStream swfStream)
            : base(TagCode, length)
        {
            SpriteID = swfStream.GetUInt16();
            FrameCount = swfStream.GetUInt16();

            var tags = new List<SwfTag>();
            var startBytes = swfStream.BytesRead;
            do
            {
                tags.Add(ParseTag(swfStream));
            } while (swfStream.BytesRead - startBytes < Length);
            ControlTags = tags.ToArray();
        }
    }
}
