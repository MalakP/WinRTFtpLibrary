/// <summary>
/// Author: Jean Tuffier.
/// Company: Ippon Technologies.
/// </summary>

namespace FtpLibrary
{
    public class FtpResponse
    {
        public string Message { get; set; }

        public string Code { get; set; }

        public FtpResponse(string code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
