/*  
    MIX Package/File Parser
    Copyright (c) 2021 Unstoppable
    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/

using System;

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
