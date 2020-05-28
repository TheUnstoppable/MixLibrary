using System;
using System.Collections.Generic;
using System.Text;

namespace MixLibrary
{
    public class MixFormatException : Exception
    {
        public MixFormatException(string Message) : base(Message)
        {

        }

        public MixFormatException(string Message, Exception Inner) : base(Message, Inner)
        {

        }
    }
}
