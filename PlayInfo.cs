using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCaroGame
{
    public class PlayInfo
    {
        private Point point;
        private int currentplayer;

        public int Currentplayer
        {
            get { return currentplayer; }
            set { currentplayer = value; }
        }
        public Point Point
        {
            get { return point; }
            set { point = value; }
        }

        public PlayInfo(Point point,int currentplayer)
        {
            this.Point = point;
            this.Currentplayer = currentplayer;

        }

    }
}
