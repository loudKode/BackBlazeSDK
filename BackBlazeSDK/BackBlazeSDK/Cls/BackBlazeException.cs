using System;

namespace BackBlazeSDK
{
    public class BackBlazeException : Exception
    {
        public BackBlazeException(string errorMesage, int errorCode) : base(errorMesage) { }
    }
}
