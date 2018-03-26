using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCaroGame
{
[Serializable]
    class SocketData
    {
        private int command;

        public int Command
        {
            get { return command; }
            set { command = value; }
        }

        //Khi một kiểu dữ liệu không cho phép null thì t truyền thêm dấu ? vào sau kiểu dữ liệu
        private Point point;

        public Point Point
        {
            get { return point; }
            set { point = value; }
        }

        


        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public SocketData(int command,string message, Point point)
        {
            this.Command = command;
            this.Message = message;
            this.Point = point;
        }

        
    }

    public enum SocketCommand
    {
        SEND_POINT,
        NOTIFY,
        NEW_GAME,
        END_GAME,
        TIME_OUT,
        EXIT
    }
}
