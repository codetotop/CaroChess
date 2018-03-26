using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyCaroGame
{
    public partial class Form1 : Form
    {
        #region Properties
        ChessBoardManager chessBoard;
        SocketManager socket;
        #endregion
        public Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;
            
            chessBoard = new ChessBoardManager(pnlChessBoard,txtPlayerName,pctMark);
            chessBoard.EndedGame += ChessBoard_EndedGame;
            chessBoard.PlayerMaked += ChessBoard_PlayerMarked;

            prcbCoolDown.Step = Constant.COOL_DOWN_STEP;
            prcbCoolDown.Maximum = Constant.COOL_DOWN_TIME;
            prcbCoolDown.Value = 0;

            socket = new SocketManager();
            tmCoolDown.Interval = Constant.COOL_DOWN_INTERVAL;
            chessBoard.DrawChessBoard();

            
        }

        #region Methods
        void EndGame()
        {
            tmCoolDown.Stop();
            pnlChessBoard.Enabled = false;//không cho tác động nên bàn cờ
            
        }

        void ChessBoard_EndedGame(object sender, EventArgs e)
        {
            EndGame();
            socket.send(new SocketData((int)SocketCommand.END_GAME, "", new Point()));
        }

        void ChessBoard_PlayerMarked(object sender, ButtonClickEvent e)
        {
            tmCoolDown.Start();
            pnlChessBoard.Enabled = false;
            prcbCoolDown.Value = 0;

            socket.send(new SocketData((int)SocketCommand.SEND_POINT,"",e.ClickedPoint));
            Listen();
        }

        private void tmCoolDown_Tick(object sender, EventArgs e)
        {
            prcbCoolDown.PerformStep();
            if (prcbCoolDown.Value >= prcbCoolDown.Maximum) {
                EndGame();
                socket.send(new SocketData((int)SocketCommand.TIME_OUT, "", new Point()));

            }
        }

        void NewGame()
        {
            string pathResultGamePrevious = "E://TaiLieuCacMonKy6//LapTrinhMang//MyCaroGame//MyCaroGame//ResultPlayPrevious.txt";
            prcbCoolDown.Value = 0;
            tmCoolDown.Stop();
            chessBoard.DrawChessBoard();
            lbResultGamePrevious.Text = rFile(pathResultGamePrevious);
        }

        private void btn_X(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thóat ván đấu không", "Thông Báo", MessageBoxButtons.OKCancel)!= DialogResult.OK)
            {
                e.Cancel = true;

            }
            else
            {
                try
                {
                    socket.send(new SocketData((int)SocketCommand.EXIT, "", new Point()));

                }catch{

                }
            }
            
            
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            socket.IP = txtIP.Text;
            if (!socket.ConnectServer())
            {
                
                socket.isServer = true;
                pnlChessBoard.Enabled = true;
                socket.CreateServer();
                txtPlayerName.Text = "Sever";
                //MessageBox.Show("Ta là server :))");
            }
            else
            {
                socket.isServer = false;
                pnlChessBoard.Enabled = false;
                Listen();
                //MessageBox.Show("Ta là client đây :))");
            }
            mnuNewGame.Enabled = true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            string pathResultGamePrevious="E://TaiLieuCacMonKy6//LapTrinhMang//MyCaroGame//MyCaroGame//ResultPlayPrevious.txt";
            txtPlayerName.Enabled = false;
            pnlChessBoard.Enabled = false;
            mnuNewGame.Enabled = false;
            txtIP.Text = "127.0.0.1";
            /*txtIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            if (string.IsNullOrEmpty(txtIP.Text))
            {
                txtIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }*/
            lbResultGamePrevious.Text = rFile(pathResultGamePrevious);
        }

        private string rFile(string path)
        {
            string information=System.IO.File.ReadAllText(@path);
            return information;
        }


        void Listen()
        {

            Thread listenThread = new Thread(() =>
            {
                try
                {   
                    SocketData data = (SocketData)socket.receive();
                    ProcessData(data);
                }
                catch(Exception e)
                {
                }
            });

            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void ProcessData(SocketData data)
        {
            switch (data.Command)
            {
               
                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                        pnlChessBoard.Enabled = false;
                    }));
                    break;
                case (int)SocketCommand.SEND_POINT:
                    this.Invoke((MethodInvoker)(()=>{
                    prcbCoolDown.Value = 0;
                    pnlChessBoard.Enabled = true;
                    tmCoolDown.Start();
                    chessBoard.OtherPlayerMark(data.Point);

                    }));                 
                    break;
                
                case (int)SocketCommand.END_GAME:
                    //MessageBox.Show("Đã đủ 5 con liền nhau");
                    break;
                case (int)SocketCommand.TIME_OUT:
                    //MessageBox.Show("Đã hết giờ");
                    break;
                case (int)SocketCommand.EXIT:
                    tmCoolDown.Stop();
                    MessageBox.Show("Đối thủ đã thoát rồi :((");
                    break;
                default:
                    break;
            }

            Listen();
        }
        #endregion

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mnuNewGame_Click(object sender, EventArgs e)
        {
            NewGame();
            socket.send(new SocketData((int)SocketCommand.NEW_GAME, "Đối thủ đã tạo game mới", new Point()));
            pnlChessBoard.Enabled = true;
        }

        private void mnuTutorial_Click(object sender, EventArgs e)
        {
            string s1= "1.Thao tác\nBan đầu vào game, bạn cần nhấn connect để có thể chơi."
                +"\nSau đó bạn sẽ phải chờ kết nối từ đối thủ.\nSau khi kết nối bạn thực hiện nhấn vào các ô vuông còn trống để chơi."
                +"\n2.Luật chơi \nTrò chơi được chơi trên bàn cờ có 576 ô, với 24 dòng và 24 cột."
                +"\nNgười chiến thắng là người tạo được đường thẳng theo chiều dọc "
                +"hoặc ngang hoặc chéo với chính xác 5 con cờ của mình."
                +"\n3.Click vào menu NewGame để tạo ván chơi mới,click vào exit để thoát game."
                +"\n\n                                ======= CHÚC BẠN MAY MẮN ========";
            MessageBox.Show(s1);
        }

        private void mnuHistory_Click(object sender, EventArgs e)
        {
            string pathHistory="E://TaiLieuCacMonKy6//LapTrinhMang//MyCaroGame//MyCaroGame//History.txt";
            string historyGame = rFile(@pathHistory);
            MessageBox.Show(historyGame,"History");
        }

        
    }
}
