using System;

namespace InspectorSwfTime.Swf
{
    public class SwfParseException : Exception
    {
        public SwfParseException(string message)
            : base(message)
        {

        }

        public SwfParseException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
