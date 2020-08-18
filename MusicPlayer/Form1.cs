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


namespace MusicPlayer
{
    public partial class Form1 : Form
    {
        int width = 640;
        int height = 480;

        Mat frame;
        VideoCapture capture;
        Bitmap bitmap;
        Graphics graphics;

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
//                    MessageBox.Show("Joy");
                    // joyフォルダの音楽をかける
                    PlayMusic(s);
                    break;
                case "Anger":
//                    MessageBox.Show("Anger");
                    // angerフォルダの音楽をかける
                    PlayMusic(s);
                    break;
                case "Sorrow":
//                    MessageBox.Show("Sorrow");
                    // sorrowフォルダの音楽をかける
                    PlayMusic(s);
                    break;
                case "Surprise":
//                    MessageBox.Show("Surprise");
                    // surpriseフォルダの音楽をかける
                    PlayMusic(s);
                    break;
                default:
//                    MessageBox.Show("None");
                    // ランダムに音楽をかける
                    PlayMusic(s);
                    break;
            }
        }

        void PlayMusic(string s)
        {
            WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();
            // 音楽ディレクトリの指定
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(@"C:\HackU2020\Music\");

            System.IO.FileInfo[] files = dir.GetFiles();

            WMPLib.IWMPPlaylist playlist = wplayer.playlistCollection.newPlaylist("myplaylist");

            // プレイリストにディレクトリ内のMP3ファイルを追加する
            foreach (System.IO.FileInfo file in files)
            {
                WMPLib.IWMPMedia media;
                media = wplayer.newMedia(file.FullName);
                playlist.appendItem(media);
            }

            wplayer.currentPlaylist = playlist;
            // numerUpDown1の値を音量に反映する(0~100)
            wplayer.settings.volume = Convert.ToInt32(numericUpDown1.Value);
//            wplayer.settings.setMode("loop", true);
            wplayer.settings.setMode("shuffle", true);
            wplayer.controls.play();
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
            pictureBox1.Width = frame.Cols / 2;
            pictureBox1.Height = frame.Rows / 2;

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
//            using(Mat cap = new Mat(@"C:\HackU2020\cap.png"))
//            {
//                // 保存された画像の出力
//                Cv2.ImShow("test1", frame);
//            }
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

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            // 音量の調整
        }
    }
}
