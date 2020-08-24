using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using Google.Cloud.Vision.V1;
using WMPLib;


namespace MusicPlayer
{
    public partial class Form1 : Form
    {
        int width = 640;
        int height = 480;

        bool music_flag = false;

        Random rand = new Random();
        Mat frame;
        VideoCapture capture;
        Bitmap bitmap;
        Graphics graphics;

        static AxWMPLib.AxWindowsMediaPlayer wplayer;
        private string[] playlist;
        private int listNum;
        private int nowNum = 0;

        int ChangekLikelihood(string s)
        {
            int i = 0;
            switch (s)
            {
                case "VeryLikely":
                    i = 4;
                    break;
                case "Likely":
                    i = 3;
                    break;
                case "Possible":
                    i = 2;
                    break;
                case "Unlikely":
                    i = 1;
                    break;
                default:
                    break;
            }
            return i;
        }

        void ChoiceMusic (int x, string s)
        {
            switch (s)
            {
                case "Joy":
                    // joyフォルダの音楽をかける
                    playlist = System.IO.Directory.GetFiles(@"C:\HackU2020\Music\Joy", "*.mp3", System.IO.SearchOption.AllDirectories);
                    listNum = playlist.Length;
                    PlayMusic();
                    break;
                case "Anger":
                    // angerフォルダの音楽をかける
                    playlist = System.IO.Directory.GetFiles(@"C:\HackU2020\Music\Anger", "*.mp3", System.IO.SearchOption.AllDirectories);
                    listNum = playlist.Length;
                    PlayMusic();
                    break;
                case "Sorrow":
                    // sorrowフォルダの音楽をかける
                    playlist = System.IO.Directory.GetFiles(@"C:\HackU2020\Music\Sorrow", "*.mp3", System.IO.SearchOption.AllDirectories);
                    listNum = playlist.Length;
                    PlayMusic();
                    break;
                case "Surprise":
                    // surpriseフォルダの音楽をかける
                    playlist = System.IO.Directory.GetFiles(@"C:\HackU2020\Music\Surprise", "*.mp3", System.IO.SearchOption.AllDirectories);
                    listNum = playlist.Length;
                    PlayMusic();
                    break;
                default:
                    // ランダムに音楽をかける
                    playlist = System.IO.Directory.GetFiles(@"C:\HackU2020\Music\Neutral", "*.mp3", System.IO.SearchOption.AllDirectories);
                    listNum = playlist.Length;
                    PlayMusic();
                    break;
            }
        }

        void PlayMusic()
        {
            // TrackBar1の値を1.0(等速)にする
            trackBar2.Value = 10;
            // TrackBar1の値を音量に反映する(0~100)
            wplayer.settings.volume = trackBar1.Value;
            // ランダムに音楽をかける
            nowNum = rand.Next(0, listNum);
            wplayer.URL = playlist[nowNum];
            music_flag = true;
            timer1.Start();
        }

        public Form1()
        {
            InitializeComponent();

            // カメラ画像取得用のVideoCapture作成
            capture = new VideoCapture(0);
            if (!capture.IsOpened())
            {
                MessageBox.Show("camera was not found!");
                this.Close();
            }
            capture.FrameWidth = width;
            capture.FrameHeight = height;

            // 取得先のMat作成(CV_8UC3 : 3個のCV_8U(符号なし8bit整数)カラー画像の初期値)
            frame = new Mat(height, width, MatType.CV_8UC3);

            // 表示用のBitmap作成
            // (Format24bppRgb : 1ピクセルあたり24bitの形式であることを指定
            // つまり、赤、緑、および青のコンポーネントに、それぞれ8bitを使用)
            bitmap = new Bitmap(frame.Cols, frame.Rows, (int)frame.Step(),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb, frame.Data);

            // PictureBoxを出力サイズに合わせる
            // pictureBox1.Width = frame.Cols / 2;
            // pictureBox1.Height = frame.Rows / 2;

            // 描画用のGraphics作成
            graphics = pictureBox1.CreateGraphics();

            // 画像取得スレッド開始
            backgroundWorker1.RunWorkerAsync();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            int joy = 0;
            int anger = 0;
            int sorrow = 0;
            int surprise = 0;
            int most_emovalue = 0;
            string most_emotion = "";
            string s;
            frame.SaveImage(@"C:\HackU2020\cap.png");
            // 事前に環境変数GOOGLE_APPLICATION_CREDENTIALSを設定しておく必要がある
            var client = ImageAnnotatorClient.Create();
            var cv_image = Google.Cloud.Vision.V1.Image.FromFile(@"C:\HackU2020\cap.png");
            var response = client.DetectFaces(cv_image);
            int count = 1;
            foreach (var faceAnnotation in response)
            {
                // "VERY_LIKELY","LIKELY","POSSIBLE","UNLIKELY","VERY_UNLIKELY","UNKNOWN"の6段階評価
                Console.WriteLine("Face {0}:", count++);
                s = System.Convert.ToString(faceAnnotation.JoyLikelihood);
                Console.WriteLine("  Joy: {0}", s);
                joy += ChangekLikelihood(s);
                if(joy > most_emovalue)
                {
                    most_emovalue = joy;
                    most_emotion = "Joy";
                }
                s = System.Convert.ToString(faceAnnotation.AngerLikelihood);
                Console.WriteLine("  Anger: {0}", s);
                anger += ChangekLikelihood(s);
                if (anger > most_emovalue)
                {
                    most_emovalue = anger;
                    most_emotion = "Anger";
                }
                s = System.Convert.ToString(faceAnnotation.SorrowLikelihood);
                Console.WriteLine("  Sorrow: {0}", s);
                sorrow += ChangekLikelihood(s);
                if (sorrow > most_emovalue)
                {
                    most_emovalue = sorrow;
                    most_emotion = "Sorrow";
                }
                s = System.Convert.ToString(faceAnnotation.SurpriseLikelihood);
                Console.WriteLine("  Surprise: {0}", s);
                surprise += ChangekLikelihood(s);
                if (surprise > most_emovalue)
                {
                    most_emovalue = surprise;
                    most_emotion = "Surprise";
                }
            }
            Console.WriteLine("{0} {1} {2} {3}", joy, anger, sorrow, surprise);
            ChoiceMusic(most_emovalue, most_emotion);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            backgroundWorker1.CancelAsync();
            while (backgroundWorker1.IsBusy)
            {
                Application.DoEvents();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            while (!backgroundWorker1.CancellationPending)
            {
                // 画像取得
                capture.Grab();
                NativeMethods.videoio_VideoCapture_operatorRightShift_Mat(capture.CvPtr, frame.CvPtr);
                bw.ReportProgress(0);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // bitmapを左上からフレームの端まで描画
            graphics.DrawImage(bitmap, 0, 0, frame.Cols/2, frame.Rows/2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 動画プレイヤーの設定
            wplayer = axWindowsMediaPlayer1;
            wplayer.settings.autoStart = false;	// 自動再生無効
            wplayer.Ctlenabled = false;            // ダブルクリックによるフルスクリーン出力を無効化
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            wplayer.Ctlcontrols.play();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            wplayer.settings.volume = trackBar1.Value;
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            switch (e.newState)
            {
                case (int)WMPLib.WMPPlayState.wmppsMediaEnded:
                    trackBar2.Value = 10;
                    // 次の曲を選ぶ(連続して同じ曲がかかることもある)
                    nowNum = rand.Next(0, listNum);
                    wplayer.URL = playlist[nowNum];
                    music_flag = true;
                    timer1.Start();
                    break;

                case (int)WMPLib.WMPPlayState.wmppsReady:
                    //再生準備完了
                    timer1.Start();
                    break;

                default:
                    break;
            }
        }

        // 音楽のスピード調整
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            wplayer.settings.rate = Convert.ToDouble(trackBar2.Value)/10;
        }
    }
}
