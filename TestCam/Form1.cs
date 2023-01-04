using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.IO.Ports;

namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture;
        private Image<Bgr, Byte> IMG;
        private Image<Gray, Byte> GrayImg;
        private Image<Gray, Byte> BwImg;
        private int N, Xpx, Ypx;
        private double Xcm, Ycm, Zcm=140.0 ; // 
        private double myScale;
        
        static SerialPort _serialPort;
		public byte []Buff = new byte[2];

//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        
        
        public Form1()
        {
            InitializeComponent();
            myScale = 170.0 / 640.0;//cm kamera 
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM3";//Set your board COM
            _serialPort.BaudRate = 9600;
            _serialPort.Open();	
        }
        
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void processFrame(object sender, EventArgs e)
        {
            if (capture == null)//very important to handel excption
            {
                try
                {
                    capture = new Capture(); 
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame();
            GrayImg = IMG.Convert<Gray,Byte>();
            BwImg = GrayImg.ThresholdBinaryInv(new Gray(40), new Gray(255));
            
            Xpx = 0;
            Ypx = 0;
            N=0;
            for(int i=0;i<BwImg.Width;i++)
            for(int j=0;j<BwImg.Height;j++)
            {
            	if(BwImg[j,i].Intensity>128)
            	{
            			N++;
            			Xpx+=i;
            			Ypx+=j;
            	}
            }
            
            if(N>0)
            {
            	Xpx = Xpx/N;
            	Ypx = Ypx/N;
            	
            	Xcm = (Xpx-320)*myScale;
            	Ycm = (240-Ypx)*myScale;
            	
            	textBox1.Text=Xcm.ToString();
            	textBox2.Text=Ycm.ToString();
            	textBox3.Text=N.ToString();
            	double Py = -Xcm;
            	double Px = -Zcm;
                double diff = 45, d1 = 3.5, // 
                Pz = Ycm +  diff;
                

            	double Th1= Math.Atan(Py/Px);
            	double Th2= Math.Atan((Math.Sin(Th1))*(Pz-d1)/Py);

                Th1 *= -(180 / Math.PI); // 
                Th2 *= +(180 / Math.PI);

            	Th1 = Th1 + 90;
            	Th2 = Th2 + 90;
            	
            	Buff[0] = (byte)Th1;; //Th1
            	Buff[1] = (byte)Th2;; //Th2
				_serialPort.Write(Buff,0,2);
            }
            else
            {
                try
                {
                    Buff[0] = (byte)90; ; //Th1
                    Buff[1] = (byte)90; ; //Th2
                    _serialPort.Write(Buff, 0, 2);
                }
                catch(Exception ex)
                {
                    return;
                } 

            	textBox1.Text="";
            	textBox2.Text="";
            	textBox3.Text=N.ToString();

            }
            	
           
      
            try
            {
            	imageBox1.Image =IMG;
            	imageBox2.Image =GrayImg;
            	imageBox3.Image =BwImg;
               
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)

        private void button1_Click(object sender, EventArgs e)
        {
            //Application.Idle += processFrame;
            timer1.Enabled=true;
            button1.Enabled = false;
            button2.Enabled = true;
        }
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button2_Click(object sender, EventArgs e)
        {
            //Application.Idle -= processFrame;
            timer1.Enabled=false;
            button1.Enabled = true;
            button2.Enabled = false;
        }    
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button3_Click(object sender, EventArgs e)
        {
            IMG.Save("Image" +  ".jpg");
        }
		void Timer1Tick(object sender, EventArgs e)
		{
			processFrame(sender, e);
		}
		
		
		      
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        
    }
}
