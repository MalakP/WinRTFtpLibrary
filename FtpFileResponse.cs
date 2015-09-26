using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpLibrary
{
    public class FtpFileResponse
    {
        public byte[] mBytes { get; set; }

        public string Code { get; set; }

        public FtpFileResponse(string code, byte[] pBytes)
        {
            Code = code;
            mBytes = pBytes;
        }
    }
}
