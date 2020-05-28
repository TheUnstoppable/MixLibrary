using System;
using System.Collections.Generic;
using System.Text;

namespace MixLibrary
{
    public class MixParserException : Exception
    {
        public MixParserException(string Message) : base(Message)
        {

        }

        public MixParserException(string Message, Exception Inner) : base(Message, Inner)
        {

        }
    }
}
