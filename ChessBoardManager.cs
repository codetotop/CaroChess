using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyCaroGame
{

    public class ChessBoardManager
    {
        #region Properties;
        private Panel chessBoard;
 
        public Panel ChessBoard
        {
            get { return chessBoard; }
            set { chessBoard = value; }
        }

        private List<Player> player;

        public List<Player> Player
        {
            get { return player; }
            set { player = value; }
        }

        private TextBox playerName;

        public TextBox PlayerName
        {
            get { return playerName; }
            set { playerName = value; }
        }

        private PictureBox playerMark;

        public PictureBox PlayerMark
        {
            get { return playerMark; }
            set { playerMark = value; }
        }

        private List<List<Button>> matrix;

        public List<List<Button>> Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        public static string namePlayerWin;
        public static string namePlayerLose;

        private event EventHandler<ButtonClickEvent> playermaked;
        public event EventHandler<ButtonClickEvent> PlayerMaked
        {
            add
            {
                playermaked += value;
            }
            remove
            {
                playermaked -= value;
            }
        }

        private Stack<PlayInfo> playTimeLine;

        public Stack<PlayInfo> PlayTimeLine
        {
            get { return playTimeLine; }
            set { playTimeLine = value; }
        }

        private event EventHandler endedgame;
        public event EventHandler  EndedGame
        {
            add
            {
                endedgame += value;
            }
            remove
            {
                endedgame -= value;
            }
        }

        private int currentPlayer;
        #endregion

        #region Initialize;
        public ChessBoardManager(Panel chessBoard,TextBox playerName,PictureBox playerMark)
        {
            this.chessBoard = chessBoard;
            this.PlayerName = playerName;
            this.PlayerMark = playerMark;
            this.Player = new List<Player>() 
            { 
                new Player("Server",Image.FromFile("E://TaiLieuCacMonKy6//LapTrinhMang//MyCaroGame//MyCaroGame//Resources//o.png")),
                new Player("Client",Image.FromFile("E://TaiLieuCacMonKy6//LapTrinhMang//MyCaroGame//MyCaroGame//Resources//x.png")),
               // E:\TaiLieuCacMonKy6\LapTrinhMang\MyCaroGame\MyCaroGame\Resources
            };

           
        }
        #endregion

        #region Methods;
        #endregion

        public void DrawChessBoard()
        {
            chessBoard.Enabled = true;
            chessBoard.Controls.Clear();
            playTimeLine = new Stack<PlayInfo>();

            currentPlayer = 0;//khoi tao cho nguoi dau tien danh
            changePlayer();

            Matrix = new List<List<Button>>();
            Button oldButton = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int i = 0; i < Constant.numberSquareOnColumn; i++)
            {
                Matrix.Add(new List<Button>());
                for (int j = 0; j < Constant.numberSquareOnLine; j++)
                {
                    Button btn = new Button()
                    {
                        Width = Constant.CHESS_WIDTH,
                        Height = Constant.CHESS_HEITH,
                        Location = new Point(oldButton.Location.X + oldButton.Width, oldButton.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag=i.ToString()
                    };
                    btn.Click += btn_Click;
                    chessBoard.Controls.Add(btn);
                    Matrix[i].Add(btn);
                    oldButton = btn;
                }
                oldButton.Location = new Point(0, oldButton.Location.Y + Constant.CHESS_HEITH);
                oldButton.Width = 0;
                oldButton.Height = 0;
            }

        }

        private void btn_Click(object sender, EventArgs e)
        {
           
            Button btn = sender as Button;
            if (btn.BackgroundImage != null)
            {
                return;
            }
            Mark(btn);//thay doi dau
            playTimeLine.Push(new PlayInfo(getChessPoint(btn),currentPlayer));
            currentPlayer = currentPlayer == 1 ? 0 : 1;
            
            if (playermaked != null)
            {
                playermaked(this, new ButtonClickEvent(getChessPoint(btn)));
            }
            if (isEndGame(btn) == true)
            {
                EndGame();
                wHistoryFile("Result Game Previous: "+namePlayerWin + " thắng , " + namePlayerLose + " thua");
                return;
            }
            //Nếu game chưa kết thúc thì thay đổi tên và ảnh
            changePlayer();

        }

        public void OtherPlayerMark(Point point){
            Button btn = Matrix[point.Y][point.X];
            if (btn.BackgroundImage != null)
            {
                return;
            }
            ChessBoard.Enabled = true;
            Mark(btn);
            playTimeLine.Push(new PlayInfo(getChessPoint(btn),currentPlayer));
            currentPlayer = currentPlayer == 1 ? 0 : 1;

            if (isEndGame(btn))
            {
                EndGame();
                return;
            }
            //Nếu game chưa kết thúc thì thay đổi tên và ảnh
            changePlayer();
            
        }

        public void EndGame()
        {
            if (endedgame != null)
                endedgame(this, new EventArgs());
            string information = namePlayerWin + " thắng ,  " + namePlayerLose+" thua.";
            MessageBox.Show(information);
            wFile("Result Game Previous\n" + namePlayerWin + " thắng , " + namePlayerLose + " thua.");
        }

        private void wFile(string information)
        {
            System.IO.File.WriteAllText(@"E:\TaiLieuCacMonKy6\LapTrinhMang\MyCaroGame\MyCaroGame\ResultPlayPrevious.txt", information);
        }

        private void wHistoryFile(string information)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"E:\TaiLieuCacMonKy6\LapTrinhMang\MyCaroGame\MyCaroGame\History.txt",true))
            {
                file.WriteLine(information);
            }
        }

        private bool isEndGame(Button btn)
        {
            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimaryDiagonal(btn) || isEndForeignDiagonal(btn);
        }

        private Point getChessPoint(Button btn)
        {
            int vertical = Convert.ToInt32(btn.Tag);
            int horizontal = Matrix[vertical].IndexOf(btn);

            Point point = new Point(horizontal,vertical);

            return point;
        }
        private bool isEndHorizontal(Button btn)
        {
            Point point = getChessPoint(btn);
            int countLeft = 0;
            int countRight = 0;
            for (int i = point.X; i >=0; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                    countLeft++;
                else break;
            }
            for (int i = point.X+1; i <=Constant.numberSquareOnLine; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                    countRight++;
                else break;
            }
                return countLeft+countRight==5;
        }

        private bool isEndVertical(Button btn)
        {
            Point point = getChessPoint(btn);
            int countUp = 0;
            int countDown = 0;
            for (int i = point.Y; i >= 0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                    countUp++;
                else break;
            }
            for (int i = point.Y + 1; i <=Constant.numberSquareOnColumn; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                    countDown++;
                else break;
            }
            return countUp + countDown == 5;
        }

        private bool isEndPrimaryDiagonal(Button btn)
        {
            Point point = getChessPoint(btn);
            int countUp = 0;
            int countDown = 0;
            for (int i = 0; i <=point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0)
                    break;
                if (Matrix[point.Y-i][point.X-i].BackgroundImage == btn.BackgroundImage)
                    countUp++;
                else break;
            }
            for (int i = 1; i <= Constant.numberSquareOnColumn-point.X; i++)
            {
                if (point.Y +i>=Constant.numberSquareOnLine || point.X + i >=Constant.numberSquareOnColumn)
                    break;
                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                    countDown++;
                else break;
            }
            return countUp + countDown == 5;
        }

        private bool isEndForeignDiagonal(Button btn)
        {
            Point point = getChessPoint(btn);
            int countUp = 0;
            int countDown = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X + i < 0 || point.Y - i < 0)
                    break;
                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                    countUp++;
                else break;
            }
            for (int i = 1; i <= Constant.numberSquareOnColumn - point.X; i++)
            {
                if (point.Y + i >= Constant.numberSquareOnLine || point.X - i <0)
                    break;
                if (Matrix[point.Y + i][point.X - i].BackgroundImage == btn.BackgroundImage)
                    countDown++;
                else break;
            }
            return countUp + countDown == 5;
        }

        private void Mark(Button btn)
        {
            btn.BackgroundImage = Player[currentPlayer].Mark;
        }

        private void changePlayer()
        {
            namePlayerLose = PlayerName.Text;
            PlayerName.Text = Player[currentPlayer].Name;
            PlayerMark.Image = Player[currentPlayer].Mark;
            namePlayerWin = PlayerName.Text;
        }
    }

    public class ButtonClickEvent : EventArgs{
        private Point clickedPoint;

        public Point ClickedPoint
        {
            get { return clickedPoint; }
            set { clickedPoint = value; }
        }

        public ButtonClickEvent(Point point)
        {
            this.clickedPoint = point;
        }
        
    }
}
