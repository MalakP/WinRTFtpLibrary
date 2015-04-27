using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

/// <summary>
/// Author: Jean Tuffier.
/// Company: Ippon Technologies.
/// </summary>

namespace FtpLibrary
{
    /// <summary>
    /// Library façade to interact with your ftp server.
    /// </summary>
    public class FtpClient
    {
        private StreamSocket socketCommand;
        private StreamSocket socketDataReceiver;
        private string ip;
        private string port;
        private string username;
        private string password;
        private bool auth = false;
        private string lastPath;

        /// <summary>
        /// Generic response for not connected socket error.
        /// </summary>
        private FtpResponse responseNotConnected = 
            new FtpResponse(FtpConstants.CODE_FAIL_NOT_CONNECTED, FtpConstants.MESSAGE_FAIL_CONNECTED);

        public FtpClient(string ip, string port, string username, string password)
        {
            this.ip = ip;
            this.port = port;
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Check if client is connected AND authenticated to a server.
        /// </summary>
        /// <returns>bool value, true if connected and authenticated.</returns>
        public bool IsAuthenticated()
        {
            return auth;
        }

        /// <summary>
        /// Connect to your server.
        /// </summary>
        /// <returns>FtpResponse with code = CODE_SOCKET_CONNECTED if success.</returns>
        public async Task<FtpResponse> ConnectToServer()
        {
            if (ip == null || port == null)
                return new FtpResponse(FtpConstants.MESSAGE_NULL_CREDENTIALS_SETTINGS, FtpConstants.MESSAGE_NULL_IP_PORT);

            try
            {
                socketCommand = new StreamSocket();
                await socketCommand.ConnectAsync(new HostName(ip), port);
                return await ReadSocket();
            }
            catch (Exception e)
            {
                return new FtpResponse(Convert.ToString(e.HResult), e.Message);
            }
        }

        /// <summary>
        /// Log you against the server..
        /// </summary>
        /// <returns>FtpResponse with code = CODE_LOGGED_ON if success.</returns>
        public async Task<FtpResponse> AuthenticateOnServer()
        {
            if (username == null || password == null)
                return new FtpResponse( FtpConstants.MESSAGE_NULL_CREDENTIALS_SETTINGS, FtpConstants.MESSAGE_NULL_CREDENTIALS_SETTINGS);

            FtpResponse response = await ExecuteRequest(EnumRequest.SetUser, username);
            if (!response.Code.Equals(FtpConstants.CODE_PASSWORD_REQUIRED))
                return response;

            response = await ExecuteRequest(EnumRequest.SetPassword, password);
            if (response.Code.Equals(FtpConstants.CODE_LOGGED_ON))
                auth = true;

            return response;
        }

        /// <summary>
        /// Dispose previous socket and reconnect to the server.
        /// The method change the working directory to the last one used.
        /// </summary>
        /// <returns>FtpResponse with code = CODE_CHANGE_DIRECTORY if success.</returns>
        public async Task<FtpResponse> Reconnect()
        {
            socketCommand.Dispose();
            socketCommand = null;
            FtpResponse response = await ConnectToServer();
            if (!response.Code.Equals(FtpConstants.CODE_SOCKET_CONNECTED))
                return response;

            response = await AuthenticateOnServer();
            if (!response.Code.Equals(FtpConstants.CODE_LOGGED_ON))
                return response;

            response = await ChangeDirectory(lastPath);
            if (!response.Code.Equals(FtpConstants.CODE_CHANGE_DIRECTORY))
                return response;

            return response;
        }

        /// <summary>
        /// Change the working directory.
        /// </summary>
        /// <param name="path">Path to the desired directory.</param>
        /// <returns>FtpResponse with code = CODE_CHANGE_DIRECTORY if success.</returns>
        public async Task<FtpResponse> ChangeDirectory(string path)
        {
            if (!IsAuthenticated())
                return responseNotConnected;

            lastPath = path;
            return await ExecuteRequest(EnumRequest.ChangeDirectory, path);
        }

        /// <summary>
        /// Return the list of files from the working directory.
        /// </summary>
        /// <returns>FtpResponse with code = CODE_TRANSFER_COMPLETE if success.</returns>
        public async Task<FtpResponse> GetListDirectory()
        {
            if (!IsAuthenticated())
                return responseNotConnected;

            FtpResponse response = await SetServerToPassiveMode();
            if (!response.Code.Equals(FtpConstants.CODE_PASSIVE_MODE))
                return response;

            await ExecuteRequest(EnumRequest.GetList);
            return await ReadSocketDataReceiver();
        }

        /// <summary>
        /// Dowload a file from the working directory.
        /// </summary>
        /// <param name="fileName">FileName"Name of the file to download.</param>
        /// <returns>FtpResponse with code = CODE_TRANSFER_COMPLETE if success.</returns>
        public async Task<FtpResponse> GetFile(string fileName)
        {
            if (!IsAuthenticated())
                return responseNotConnected;

            FtpResponse response = await SetServerToPassiveMode();
            if (!response.Code.Equals(FtpConstants.CODE_PASSIVE_MODE))
                return new FtpResponse(response.Code, FtpConstants.MESSAGE_FAIL_PASSIVE_MODE + response.Message);

            response = await ExecuteRequest(EnumRequest.GetFile, fileName);
            if (response.Code.Equals(FtpConstants.CODE_TRANSFER_START) ||
                response.Code.Equals(FtpConstants.CODE_TRANSFER_COMPLETE))
                return await ReadSocketDataReceiver();

            return new FtpResponse(response.Code, FtpConstants.MESSAGE_FAIL_TRANSFER_START + response.Message);
        }

        /// <summary>
        /// Write the command on the socket output stream and read the response.
        /// </summary>
        /// <param name="request">Enumeration item representing the command to send.</param>
        /// <param name="args">Values to send with the command.</param>
        /// <returns>FtpResponse containing what has been on the socket.</returns>
        private async Task<FtpResponse> ExecuteRequest(EnumRequest request, string args = null)
        {
            FtpRequest baseRequest = RequestFactory.Create(request, args);
            string result = await WriteOnSocket(baseRequest.Command);
            if (!result.Equals(FtpConstants.OK))
                return new FtpResponse(FtpConstants.CODE_FAIL_WRITE_ON_SOCKET, "Fail to write on socket. " + result);

            if (request == EnumRequest.GetList || request == EnumRequest.GetFile)
                return await WaitForTransferComplete();

            return await ReadSocket();
        }

        /// <summary>
        /// Read the input stream of the command socket.
        /// </summary>
        /// <returns>FtpResponse containing what has been on the socket.</returns>
        private async Task<FtpResponse> ReadSocket()
        {
            try
            {
                DataReader reader = new DataReader(socketCommand.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;
                await reader.LoadAsync(2048);
                string data = reader.ReadString(reader.UnconsumedBufferLength);
                Debug.WriteLine("read on socket : {0}", data);
                return new FtpResponse(data.Substring(0, 3), data);
            }
            catch (Exception e)
            {
                Debug.WriteLine("help links : {0}", e.HelpLink);
                Debug.WriteLine("source : {0}", e.Source);
                Debug.WriteLine("stack trace : {0}", e.StackTrace);
                Debug.WriteLine("message : {0}", e.Message);
                return new FtpResponse(e.HResult.ToString(), e.Message);
            }
        }

        /// <summary>
        /// Read the input stream of the data socket.
        /// </summary>
        /// <returns>FtpResponse containing what has been on the socket.</returns>
        private async Task<FtpResponse> ReadSocketDataReceiver()
        {
            try
            {
                DataReader reader = new DataReader(socketDataReceiver.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;
                StringBuilder builder = new StringBuilder();

                uint loaded = 0;
                do
                {
                    loaded = await reader.LoadAsync(4096);
                    builder.Append(reader.ReadString(reader.UnconsumedBufferLength));
                } while (loaded > 0);

                socketDataReceiver.Dispose();
                return new FtpResponse(FtpConstants.CODE_TRANSFER_COMPLETE, 
                    builder.ToString());
            }
            catch (Exception e)
            {
                return new FtpResponse(Convert.ToString(e.HResult), e.Message);
            }
        }

        /// <summary>
        /// Wait for the data transfer complete and clean the input stream of scommand ocket.
        /// </summary>
        /// <returns>FtpResponse with code = CODE_TRANSFER_COMPLETE if success.</returns>
        private async Task<FtpResponse> WaitForTransferComplete()
        {
            FtpResponse response;
            do
            {
                response = await ReadSocket();
                if(response.Code.Equals("-2147483629"))
                {
                    break;
                }
            } while (response.Message.IndexOf(FtpConstants.CODE_TRANSFER_COMPLETE) < 0);
            return response;
        }

        /// <summary>
        /// Write a string on the command socket output stream.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task<string> WriteOnSocket(string command)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(command + "\r\n");
                data = data.Where(val => val != 0).ToArray();
                await socketCommand.OutputStream.WriteAsync(data.AsBuffer());
                Debug.WriteLine("sending command : {0}", command);
                return FtpConstants.OK;
            }
            catch (Exception e) { return e.ToString(); }
        }

        /// <summary>
        /// Set the server to passive mode and connect the data socket to the specify ip.
        /// </summary>
        /// <returns>FtpResponse with code = CODE_PASSIVE_MODE</returns>
        private async Task<FtpResponse> SetServerToPassiveMode()
        {
            FtpResponse response = await ExecuteRequest(EnumRequest.PassiveMode);
            if (!response.Code.Equals(FtpConstants.CODE_PASSIVE_MODE))
            {
                return new FtpResponse(FtpConstants.CODE_FAIL_PASSIVE_MODE, response.Message);
            }

            try
            {
                int idx = response.Message.IndexOf("(");
                string sub = response.Message.Substring(idx);
                sub = sub.Remove(0, 1);
                sub = sub.Remove(sub.IndexOf(")"));
                string[] parts = sub.Split(',');
                int p1 = Convert.ToUInt16(parts[4]);
                int p2 = Convert.ToUInt16(parts[5]);

                socketDataReceiver = new StreamSocket();
                await socketDataReceiver.ConnectAsync(new HostName(ip), Convert.ToString(p1 * 256 + p2));
                return response;
            }
            catch (Exception e)
            {
                return new FtpResponse(Convert.ToString(e.HResult), e.ToString());
            }
        }
    }
}
