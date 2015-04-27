/// <summary>
/// Author: Jean Tuffier.
/// Company: Ippon Technologies.
/// </summary>

namespace FtpLibrary
{
    public class FtpConstants
    {
        // COMMAND ------------------------------------------------------------
        internal const string COMMAND_PWD = "PWD";
        internal const string COMMAND_CWD = "CWD";
        internal const string COMMAND_RETR = "RETR";
        internal const string COMMAND_STOR = "STOR";
        internal const string COMMAND_USER = "USER";
        internal const string COMMAND_PASS = "PASS";
        internal const string COMMAND_SYST = "SYST";
        internal const string COMMAND_FEAT = "FEAT";
        internal const string COMMAND_TYPE = "TYPE";
        internal const string COMMAND_PASV = "PASV";
        internal const string COMMAND_NLST = "NLST";
        internal const string COMMAND_PORT = "PORT";

        // CODE ---------------------------------------------------------------
        public const string CODE_SOCKET_CONNECTED = "220";
        public const string CODE_TRANSFER_START = "150";
        public const string CODE_TRANSFER_COMPLETE = "226";
        public const string CODE_PASSIVE_MODE = "227";
        public const string CODE_LOGGED_ON = "230";
        public const string CODE_CHANGE_DIRECTORY = "250";
        public const string CODE_PASSWORD_REQUIRED = "331";
        public const string CODE_NO_SUCH_FILE = "550";

        public const string CODE_FAIL_WRITE_ON_SOCKET = "0000";
        public const string CODE_FAIL_NOT_CONNECTED = "0001";
        public const string CODE_FAIL_PASSIVE_MODE = "0002";

        // MESSAGES -----------------------------------------------------------
        public const string OK = "OK";
        public const string MESSAGE_FAIL_CONNECTED = "Socket is not connected or authenticated.";
        public const string MESSAGE_FAIL_LOGGED_ON = "Username or password is invalid.";
        public const string MESSAGE_FAIL_CHANGE_DIRECTORY = "Fail to change directory.";
        public const string MESSAGE_FAIL_CREATE_REQUEST = "Request not found.";
        public const string MESSAGE_FAIL_TRANSFER_START = "Fail to start transfer";
        public const string MESSAGE_FAIL_TRANSFER = "Transfer failed.";
        public const string MESSAGE_FAIL_PASSIVE_MODE = "Set to passive mode failed.";
        public const string MESSAGE_NULL_IP_PORT = "Ip address or port value is null.";
        public const string MESSAGE_NULL_CREDENTIALS_SETTINGS = "Username or password is null.";
    }
}
