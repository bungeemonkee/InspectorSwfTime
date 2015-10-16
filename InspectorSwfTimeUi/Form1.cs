using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using InspectorSwfTime;
using InspectorSwfTime.Swf;
using System.Linq;
using InspectorSwfTime.Swf.Tags;

namespace InspectorSwfTimeUi
{
    public partial class Form1 : Form
    {
        private string swfPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            inputDialog.ShowDialog(this);
            if (swfPath == inputDialog.FileName)
            {
                return;
            }
            swfPath = inputDialog.FileName;
            UpdateInformation(swfPath);
        }

        private void decompressButton_Click(object sender, EventArgs e)
        {
            if (swfPath == null)
            {
                return;
            }

            // get the output path
            outputDialog.ShowDialog(this);
            if (swfPath.ToLowerInvariant() == outputDialog.FileName.ToLowerInvariant())
            {
                return;
            }

            // decompress the file
            SwfFileUtility.CopyDecompressed(swfPath, outputDialog.FileName);
        }

        private void UpdateInformation(string filePath)
        {
            SwfInfo swf;
            try
            {
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    swf = new SwfInfo(file);
                }
            }
            catch (Exception ex)
            {
                results.Text = "Error:" + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace;
                return;
            }
            IEnumerable<KeyValuePair<string, string>> properties = new Dictionary<string, string>
                {
                    {"Uncompressed length", swf.Header.FileLength.ToString("n")},
                    {"CompressionType", GetCompressionName(swf.Header.Signature)},
                    {"Swf file version", swf.Header.Version.ToString()},
                    {
                        "Frame size",
                        string.Format("X: {0}, Y: {1}, W: {2}, H: {3}", swf.Header.FrameSize.Left,
                                      swf.Header.FrameSize.Top,
                                      swf.Header.FrameSize.Width, swf.Header.FrameSize.Height)
                    },
                    {
                        "Frame rate", string.Format("{0} ({1:0.00} fps)", swf.Header.FrameRate, swf.Header.FramesPerSecond)
                    },
                    {
                        "Length",
                        string.Format("{0} frames {1:0.00} seconds", swf.Header.FrameCount, swf.Header.LengthInSeconds)
                    },
                    {
                        "Total ActionScript code",
                        string.Format("{0:00000} bytes", SumCodeLength(swf.Tags))
                    },
                    {
                        "Total ActionScript percent",
                        string.Format("{0:00.0}%", 100D * SumCodeLength(swf.Tags) / swf.Header.FileLength)
                    }
                };

            properties =
                properties.Union(
                    swf.Tags.GroupBy(x => x.Code)
                       .ToDictionary(x => x.Key, x => x.Aggregate((ulong)0, (val, tag) => val + tag.Length))
                       .OrderByDescending(x => x.Value)
                       .ToDictionary(
                           x => string.Format("Total tag size ({0})", SwfTag.GetName(x.Key)),
                           x => string.Format("{0:00000} bytes {1:00.00}%", x.Value, 100.0D * x.Value / swf.Header.FileLength)));

            const int keyLength = 50;
            const int valLength = 70;
            var lineFormat = string.Format("{{0,-{0}}}| {{1,-{1}}}", keyLength, valLength);
            results.Text = string.Format(@"Swf Information for file:
{0}
{1}
{2}",
                                         filePath,
                                         MakeSeperator(keyLength, valLength),
                                         string.Join(Environment.NewLine, properties.Select(x => string.Format(lineFormat, x.Key, x.Value)))
                                         );
        }

        private static string MakeSeperator(int keyLength, int valLength)
        {
            var sb = new StringBuilder(keyLength + valLength + 1);
            for (var i = 0; i < keyLength; ++i)
            {
                sb.Append('-');
            }
            sb.Append('|');
            for (var i = 0; i < keyLength; ++i)
            {
                sb.Append('-');
            }
            return sb.ToString();
        }

        private static string GetCompressionName(SwfHeaderSignature signature)
        {
            switch (signature)
            {
                case SwfHeaderSignature.FWS:
                    return "Uncompressed";
                case SwfHeaderSignature.CWS:
                    return "ZLib";
                case SwfHeaderSignature.ZWS:
                    return "LZMA";
                default:
                    return "Unknown";
            }
        }

        private static double SumCodeLength(IEnumerable<SwfTag> tags)
        {
            var codeTags = new[] { 12, 59, 76, 82, 87 };
            return
                tags.Sum(
                    x =>
                    codeTags.Contains(x.Code)
                        ? (double)x.Length
                        : x.Code == DefineSprite.TagCode
                            ? SumCodeLength(((DefineSprite)x).ControlTags)
                            : 0D);
        }
    }
}
