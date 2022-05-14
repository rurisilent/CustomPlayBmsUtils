using System;
using System.Collections.Generic;
using System.Text;

namespace CustomPlayBmsUtils
{
    public class BmsFileNotFoundException : Exception
    { 
        public BmsFileNotFoundException()
        {

        }

        public BmsFileNotFoundException(string message) : base(message)
        {

        }

        public BmsFileNotFoundException(string message, Exception inner) : base(message, inner)
        {

        }
    }

    public class BmsFileNotValidException : Exception
    {
        public BmsFileNotValidException()
        {

        }

        public BmsFileNotValidException(string message) : base(message)
        {

        }

        public BmsFileNotValidException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
