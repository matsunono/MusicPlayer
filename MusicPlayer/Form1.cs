using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace MusicPlayer
{
    public partial class Form1 : Form
    {
        int WIDTH = 640;
        int HEIGHT = 480;
        Mat frame;
        VideoCapture capture;
        Bitmap bitmap;
        Graphics graphics;

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
            capture.FrameWidth = WIDTH;
            capture.FrameHeight = HEIGHT;

            // 取得先のMat作成(CV_8UC3 : 3個のCV_8U(符号なし8bit整数)カラー画像の初期値)
            frame = new Mat(HEIGHT, WIDTH, MatType.CV_8UC3);

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
            frame.SaveImage(@"\img\cap.png");
            using(Mat cap = new Mat(@"\img\cap.png"))
            {
                // 保存された画像の出力
                Cv2.ImShow("test1", frame);
            }
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
    }
}
