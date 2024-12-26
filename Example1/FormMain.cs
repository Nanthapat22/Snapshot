using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace Example1
{
    public partial class FormMain : Form
    {
        private VideoCapture _capture = null;
        private Mat _frame = new Mat(); // Mat สำหรับเก็บเฟรมจากกล้อง
        private bool IsConnect = true;
        private bool isCapturing = true; // Track connection state
        private LBPHFaceRecognizer recognizer;
        private bool isRecognizerActive = false;
        private bool isSnapshotActive = false;
        private int snapshotCount = 0;
        private Image<Bgr, byte> capturedFaceImage = null;
        private string saveFolderPath = @"D:\New folder 3\Example1_6510301020";

        CascadeClassifier _cascadeClassifier = new CascadeClassifier(@"D:\New folder 3\Example1_6510301020\haarcascade_frontalface_default.xml");

        private void ProcessFrame(object sender, EventArgs e)
        {
            if (_capture == null || _capture.Ptr == IntPtr.Zero) return;

            // ดึงเฟรมจากกล้อง
            _capture.Retrieve(_frame);
            if (!_frame.IsEmpty)
            {
                using (var imageFrame = _frame.ToImage<Bgr, Byte>())
                {
                    if (imageFrame != null)
                    {
                        using (var grayFrame = imageFrame.Convert<Gray, byte>())
                        {
                            grayFrame._EqualizeHist();

                            // ตรวจจับใบหน้า
                            var faces = _cascadeClassifier.DetectMultiScale(grayFrame, 1.1, 5, new Size(30, 30));

                            if (faces.Length > 0)
                            {
                                // วาดกรอบรอบใบหน้าที่ตรวจพบ
                                foreach (var face in faces)
                                {
                                    imageFrame.Draw(face, new Bgr(Color.Red), 2);

                                    // ตัดภาพเฉพาะใบหน้า (ROI)
                                    var faceImage = imageFrame.Copy(face); // ตัดใบหน้าออกมาเป็นภาพใหม่

                                    // เก็บภาพใบหน้าที่แคปไว้ในตัวแปร
                                    capturedFaceImage = faceImage;

                                    // แสดงใบหน้าใน imageBox2
                                    Invoke(new Action(() =>
                                    {
                                        imageBox2.Image = faceImage; // แสดงเฉพาะใบหน้าที่แคป
                                    }));
                                }
                            }
                        }
                    }
                }
            }
        }



        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_capture != null)
            {
                _capture.Pause();
                _capture.Dispose();
                _capture = null;
            }
            // แสดงข้อความแจ้งเตือน (ถ้าต้องการ)
            DialogResult result = MessageBox.Show("คุณต้องการปิดโปรแกรมใช่หรือไม่?",
                                                  "ยืนยันการปิด",
                                                  MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {
                e.Cancel = true; // ยกเลิกการปิดฟอร์ม
            }
        }



        public FormMain()
        {
            InitializeComponent();
            buttonStsrt.Enabled = false; // ปิดปุ่ม Start ไว้ก่อนจนกว่าจะ Connect
            timerClock.Enabled = true;   // เปิดตัวโชว์นาฬิกา
        }


        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }



        private void buttonFlipHor_Click(object sender, EventArgs e)
        {
            if (_capture != null)
                _capture.FlipHorizontal = !_capture.FlipHorizontal;
        }

        private void buttonFlipVer_Click(object sender, EventArgs e)
        {
            if (_capture != null)
                _capture.FlipVertical = !_capture.FlipVertical;
        }

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                _capture = new VideoCapture(0); // เปิดกล้องตัวแรก
                if (!_capture.IsOpened)
                {
                    MessageBox.Show("ไม่สามารถเปิดกล้องได้");
                    return;
                }
                _capture.ImageGrabbed += ProcessFrame;
                _frame = new Mat();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}");
            }

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

            if (IsConnect)
            {

                buttonConnect.Text = "Disconnect";
                tbCarmera.BackColor = Color.Green;
                tbCarmera.Text = "Connected";
                buttonStsrt.Enabled = true;

                try
                {
                    _capture = new VideoCapture();
                    _capture.ImageGrabbed += ProcessFrame;
                    _frame = new Mat();

                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }

            }
            else
            {

                buttonConnect.Text = "Connect";
                tbCarmera.BackColor = Color.Red;
                tbCarmera.Text = "DisConnected";
                buttonStsrt.Enabled = false;

                try
                {
                    if (_capture != null)
                    {
                        _capture.Pause();
                        _capture.Dispose();

                    }


                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }


            }
            IsConnect = !IsConnect;

        }



        private void button5_Click(object sender, EventArgs e)
        {
            if (isCapturing)
            {

                buttonStsrt.Text = "Pause";
                textBox2.Text = "Record";
                textBox2.BackColor = Color.Green;
                buttonConnect.Enabled = false;
                if (_capture != null)
                {
                    _capture.Start();

                }

            }
            else
            {
                buttonStsrt.Text = "Start";
                textBox2.BackColor = Color.Red;
                textBox2.Text = "No record";
                buttonConnect.Enabled = true;
                if (_capture != null)
                {
                    _capture.Pause();
                }


            }
            isCapturing = !isCapturing;

        }



        private void textBox1_TextChanged_2(object sender, EventArgs e)
        {
            try
            {
                _capture = new VideoCapture();
                _capture.ImageGrabbed += ProcessFrame;
                _frame = new Mat();

            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            string formatStringClock = "HH:mm:ss";
            string formatStringDate = "yyyy-MMM-dd";

            DateTime dtNow = DateTime.Now;
            statusLabelClock.Text = dtNow.ToString(formatStringClock);
            statusLabalDate.Text = dtNow.ToString(formatStringDate);
        }

        private void imageBox2_Click(object sender, EventArgs e)
        {

        }

        private void imageBox2_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (_frame != null)
                {
                    string fileName = $"Captured_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                    _frame.Save(fileName);

                    // เพิ่มข้อความ Log ใน TextBox2
                    textBox2.Text = $"Image saved as {fileName}";
                }
                else
                {
                    // แจ้งเตือนถ้าไม่มี Frame
                    textBox2.Text = "No frame available to save.";
                }
            }
            catch (Exception ex)
            {
                // แสดงข้อความ Error ใน TextBox2
                textBox2.Text = $"Error saving image: {ex.Message}";
            }


        }


        private void logListBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isRecognizerActive)
            {
                // ปิดการจดจำใบหน้า
                recognizer = null;
                isRecognizerActive = false;
                button1.Text = "Start Recognition";
            }
            else
            {
                // เปิดการจดจำใบหน้า
                recognizer = new LBPHFaceRecognizer();
                isRecognizerActive = true;
                button1.Text = "Stop Recognition";


            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // เมื่อกดปุ่ม Capture จะบันทึกใบหน้าที่แคป
            if (capturedFaceImage != null)
            {
                // สร้างชื่อไฟล์พร้อมที่อยู่
                string fileName = Path.Combine(saveFolderPath, $"Snapshot_{snapshotCount + 1}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

                // บันทึกใบหน้าที่แคป
                capturedFaceImage.Save(fileName);
                snapshotCount++;  // เพิ่มจำนวนแคป

                // อัปเดตข้อความใน textBox1 และ textBox3
                Invoke(new Action(() =>
                {
                    textBox1.Text = $"Image saved as {fileName}";
                    textBox3.Text = $"{snapshotCount}";  // แสดงจำนวนแคป
                }));

                // แสดงข้อความใน logListBox เกี่ยวกับที่อยู่ของไฟล์ที่บันทึก
                Invoke(new Action(() =>
                {
                    logListBox.Items.Add($"Snapshot {snapshotCount}: Image saved at {fileName}");
                }));
            }
            else
            {
                MessageBox.Show("No face detected to capture.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
