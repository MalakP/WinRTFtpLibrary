using System;

/// <summary>
/// Author: Jean Tuffier.
/// Company: Ippon Technologies.
/// </summary>

namespace FtpLibrary
{
    internal class RequestFactory
    {
        /// <summary>
        /// Create a BaseRequest object from an element of FtpRequest enumaration.
        /// </summary>
        /// <param name="type">FtpRequest element cast to int.</param>
        /// <param name="args">args to send with the command.</param>
        /// <returns></returns>
        public static FtpRequest Create(EnumRequest ftpRequest, string args = null)
        {
            switch (ftpRequest)
            {
                case EnumRequest.GetCurrentPath: return new FtpRequest(FtpConstants.COMMAND_PWD, null);
                case EnumRequest.GetFeature: return new FtpRequest(FtpConstants.COMMAND_FEAT, null);
                case EnumRequest.GetList: return new FtpRequest(FtpConstants.COMMAND_NLST, null);
                case EnumRequest.GetSystem: return new FtpRequest(FtpConstants.COMMAND_SYST, null);
                case EnumRequest.SetType: return new FtpRequest(FtpConstants.COMMAND_TYPE, args);
                case EnumRequest.PassiveMode: return new FtpRequest(FtpConstants.COMMAND_PASV, null);
                case EnumRequest.SetPassword: return new FtpRequest(FtpConstants.COMMAND_PASS, args);
                case EnumRequest.SetUser: return new FtpRequest(FtpConstants.COMMAND_USER, args);
                case EnumRequest.SetPort: return new FtpRequest(FtpConstants.COMMAND_PORT, args);
                case EnumRequest.ChangeDirectory: return new FtpRequest(FtpConstants.COMMAND_CWD, args);
                case EnumRequest.GetFile: return new FtpRequest(FtpConstants.COMMAND_RETR, args);
                case EnumRequest.UploadFile: return new FtpRequest(FtpConstants.COMMAND_STOR, args);
                default: throw new Exception(FtpConstants.MESSAGE_FAIL_CREATE_REQUEST);
            }
        }
    }
}
