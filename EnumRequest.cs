using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpLibrary
{
    /// <summary>
    /// List of command sendable to the server.
    /// </summary>
    internal enum EnumRequest
    {
        GetCurrentPath,
        GetFeature,
        GetList,
        GetSystem,
        SetType,
        PassiveMode,
        SetPassword,
        SetUser,
        SetPort,
        ChangeDirectory,
        GetFile,
        UploadFile
    }
}
