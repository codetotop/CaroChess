using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyCaroGame
{
    class SocketManager
    {
        #region Client
        Socket client;
        public bool ConnectServer()
        {
           // System.Windows.Forms.MessageBox.Show(IP);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"),9050);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                client.Connect(ipep);
                return true;            
            }catch{
                return false;
            }
        }
        #endregion

        #region Server
        Socket server;
        public void CreateServer()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"),9050);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipep);
            server.Listen(10);

            Thread acceptClient = new Thread(() =>
            {
                client = server.Accept();

            });
            acceptClient.IsBackground = true;
            acceptClient.Start();
        }

         
        #endregion

        #region Port
        public string IP ="127.0.0.1";
        public int PORT = 9050;
        public bool isServer=true;
        public static int BUFFER = 1024;

        public bool send(object data)
        {
            byte[] sendData = SerializeData(data);
            return SendData(client, sendData);            


        }
        public object receive()
        {
            byte[] receiveData = new Byte[BUFFER];
            bool isOk = ReceiveData(client, receiveData);
            return DeserializeData(receiveData);
        }
        private bool SendData(Socket target,byte [] data)
        {
            return target.Send(data) == 1 ? true : false;
        }

        private bool ReceiveData(Socket target, byte[] data)
        {
            return target.Receive(data) == 1 ? true : false;
        }
        #endregion
        //Nén đối tượng thành mảng byte
        private byte[] SerializeData(object data)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms, data);
            return ms.ToArray();
        }

        // chuyển mảng byte thành đối tượng
        private object DeserializeData(byte[] theByteArray)
        {
            MemoryStream ms = new MemoryStream(theByteArray);
            BinaryFormatter bf1 = new BinaryFormatter();
            ms.Seek(0, SeekOrigin.Begin);
            //ms.Position = 0;
            return bf1.Deserialize(ms);
        }

        //lay ra IP V4 của card mạng đang dùng
        public string GetLocalIPv4(NetworkInterfaceType type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output += ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }

        
    }
}
