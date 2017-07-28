/*******************************************************************************************
 *  Copyright (C) 2004-2016 by EMGU Corporation. All rights reserved.       
 *  Adapted from Emgu CameraCapture example
 *  and Device - CDC - Basic Demo / PC Software Example for C#
 *  from Microchip Solutions V2013-06-05

 *  4-19-16: CDC serial port working very well with 4096 byte transfers & webcam very smooth
 *  5-20-16: Works beautifully with PIC32 LED COntroller CDC
 *  Strange offset error seems to be occuring. Otherwise works great.
 *  Fixed offset bug. No compression in this version
 *  For four panels.
 *  6-2-16: Incoming camera image is 423 x 270. Crop it to 64 x 64.
 *  Copy image to 128 x 128 matrix
 *  6-3-16: Added serial port # to Panel class. Added 32x32 matrix capability.
 *  Works great with sixteen 16x32 panels and one 32x32 panel.
 *  6-4-16: Rotation works. Attempted to center cam image with cropping.
 *  6-7-16: Basic video works great with three serial ports.
 *  6-12-16: Added USE_SERIAL and USE_C25 compiler options
 *           Added cropping, enlarging modified image.
 *  6-27-16: Got all panels working with six serial ports. Using both 16x32 and 32x32 panels.
 *           Transmitting one byte grayscale.
 *  6-28-16: One byte color works great!
 *  7-26-17: Retested with six serial ports - works nicely with 
 *  four horizontal panels across and six panels up, including 32x32 panels.
 *  Play video feature not working.
 ********************************************************************************************/
#define USE_SERIAL
#define USE_C920  // 1920 x 1080
// #define USE_C525  // 1280 x 720
// #define PLAY_VIDEO_FILE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using System.IO.Ports;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using PanelSpace;
using System.Runtime.InteropServices;
using Emgu.CV.VideoSurveillance;



enum colorScale
{
    RED_GREEN,
    RED_BLUE,
    GREEN_RED,
    GREEN_BLUE,
    BLUE_RED,
    BLUE_GREEN
}


[StructLayout(LayoutKind.Explicit)]
    struct convertType {
        [FieldOffset(0)]
        public byte byte0;

        [FieldOffset(1)]
        public byte byte1;

        [FieldOffset(2)]
        public byte byte2;

        [FieldOffset(3)]
        public byte byte3;

        [FieldOffset(0)]
        public UInt16 shtInteger;

        [FieldOffset(0)]
        public UInt32 lngInteger;
    };


namespace CameraCapture {
    public partial class CameraCapture : Form
    {
#if PLAY_VIDEO_FILE
        Timer My_Timer = new Timer();
        int FPS = 30;
#endif

        public const int COL_OFFSET = 16;
        public const int NUMCHANNELS = 3;
        public const int MATRIX_WIDTH = 128; 
        public const int MATRIX_HEIGHT = 96;
        public const int MATRIXSIZE = MATRIX_WIDTH * MATRIX_HEIGHT;

        public const int CAM_IMAGE_WIDTH = 96;
        public const int CAM_IMAGE_HEIGHT = CAM_IMAGE_WIDTH;    

        public const int CAM_DATASIZE = (CAM_IMAGE_WIDTH * CAM_IMAGE_HEIGHT * NUMCHANNELS);        
        public const int MAXPACKETSIZE = (LEDPanel.PANELSIZE * 4);
        public const UInt16 BRIGHTNESS = 255;

        public readonly byte[] gammaTable = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                               1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5,
                                               5, 6, 6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 9, 10, 10, 10, 11, 11, 11, 12, 12, 13, 13, 14,
                                               14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20, 21, 21, 22, 22, 23, 23, 24, 25,
                                               25, 26, 27, 27, 28, 29, 29, 30, 31, 31, 32, 33, 34, 34, 35, 36, 37, 37, 38, 39, 40,
                                               41, 42, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 52, 53, 54, 55, 56, 57, 59, 60,
                                               61, 62, 63, 64, 65, 66, 67, 68, 69, 71, 72, 73, 74, 75, 77, 78, 79, 80, 82, 83, 84,
                                               85, 87, 88, 89, 91, 92, 93, 95, 96, 98, 99, 100, 102, 103, 105, 106, 108, 109, 111,
                                               112, 114, 115, 117, 119, 120, 122, 123, 125, 127, 128, 130, 132, 133, 135, 137, 138,
                                               140, 142, 144, 145, 147, 149, 151, 153, 155, 156, 158, 160, 162, 164, 166, 168, 170,
                                               172, 174, 176, 178, 180, 182, 184, 186, 188, 190, 192, 194, 197, 199, 201, 203, 205,
                                               207, 210, 212, 214, 216, 219, 221, 223, 226, 228, 230, 233, 235, 237, 240, 242, 245,
                                               247, 250, 252, 255 };
        
        private VideoCapture _capture = null;
        private VideoCapture _playVideo = null;
        //private MotionHistory _motionHistory;
        //private BackgroundSubtractor _forgroundDetector;
        private bool _captureInProgress, _playingVideo;

        public byte[,] arrPanelInit = {
                    {1, 0, LEDPanel.HORIZONTAL, 64, 0}, {1, 1, LEDPanel.HORIZONTAL, 80, 0 }, {1, 2, LEDPanel.HORIZONTAL, 64, 32 },{1, 3, LEDPanel.HORIZONTAL, 80, 32},
                    {2, 0, LEDPanel.HORIZONTAL, 64, 64}, {2, 1, LEDPanel.HORIZONTAL, 80, 64}, {2, 2, LEDPanel.HORIZONTAL, 64, 96},{2, 3, LEDPanel.HORIZONTAL, 80, 96},
                    {3, 0, LEDPanel.HORIZONTAL, 32, 0}, {3, 1, LEDPanel.HORIZONTAL, 48, 0 }, {3, 2, LEDPanel.HORIZONTAL, 32, 32 },{3, 3, LEDPanel.HORIZONTAL, 48, 32},
                    {4, 0, LEDPanel.HORIZONTAL, 32, 64}, {4, 1, LEDPanel.HORIZONTAL, 48, 64}, {4, 2, LEDPanel.HORIZONTAL, 32, 96},{4, 3, LEDPanel.HORIZONTAL, 48, 96},
                    {5, 0, LEDPanel.HORIZONTAL, 0, 0}, {5, 0, LEDPanel.HORIZONTAL, 16, 0},{5, 1, LEDPanel.HORIZONTAL, 0, 32}, {5, 1, LEDPanel.HORIZONTAL, 16, 32},
                    {6, 0, LEDPanel.HORIZONTAL, 0, 64}, {6, 0, LEDPanel.HORIZONTAL, 16, 64},{6, 1, LEDPanel.HORIZONTAL, 0, 96}, {6, 1, LEDPanel.HORIZONTAL, 16, 96},
        };

        LEDPanel[] MyPanels = new LEDPanel[LEDPanel.NUMPANELS];
        public byte[,,] matrix = new byte[MATRIX_HEIGHT, MATRIX_WIDTH, NUMCHANNELS];
        // public byte[,,] matrix = new byte[MATRIX_HEIGHT, MATRIX_WIDTH, NUMCHANNELS];

        const byte STX = (byte)'>';
        const byte DLE = (byte)'/';
        const byte ETX = (byte)'\r';

        public const int MAXDATABYTES = (LEDPanel.PANELROWS * LEDPanel.PANELCOLS * LEDPanel.NUMCHANNELS);
        public const int MAXPACKET = (MAXDATABYTES * 4);
        
        convertType convertToInteger;        
        int loopCounter = 0;
        int camWidth, camHeight;
        UInt16 colorOffset;
        
        public void initializePanels()
        {            
            for (int i = 0; i < LEDPanel.NUMPANELS; i++)
            {
                MyPanels[i] = new LEDPanel();
                MyPanels[i].setOrientation(arrPanelInit[i, 0], arrPanelInit[i, 1], arrPanelInit[i, 2], arrPanelInit[i, 3], arrPanelInit[i, 4]);
            }

        }

        public Boolean imageArrayToMatrix(ref byte[] ptrData)
        {
            int i = 0;            
            for (int row = 0; row < CAM_IMAGE_HEIGHT; row++)
                for (int col = COL_OFFSET; col < CAM_IMAGE_WIDTH + COL_OFFSET; col++)
                    for (int channel = NUMCHANNELS - 1; channel >= 0; channel--)
                        if (i < CAM_DATASIZE) matrix[row, col, channel] = ptrData[i++];
                        else return (false);
            return (true);
        }

        public Boolean matrixToimageArray (ref byte[] ptrData)
        {
            int i = 0;
            for (int row = 0; row < CAM_IMAGE_HEIGHT; row++)
                for (int col = 0; col < CAM_IMAGE_WIDTH; col++)
                    for (int channel = NUMCHANNELS - 1; channel >= 0; channel--)
                        if (i < CAM_DATASIZE)  ptrData[i++] = matrix[row, col, channel];
                        else return (false);
            return (true);
        }

        /*
        public Boolean filter()
        {
            byte red, blue, green;
            int i = 0;
            for (int row = 0; row < MATRIX_HEIGHT; row++)
                for (int col = 0; col < MATRIX_WIDTH; col++)                     
                {
                    red = (byte) (matrix[row, col, 0] & 0xFC);
                    green = (byte)(matrix[row, col, 1] & 0xFC);
                    blue = (byte)(matrix[row, col, 2] & 0xFC);
                    if (red < green && red < blue) red = 0x00;
                    else if (green < red && green < blue) green = 0x00;
                    else blue = 0x00;
                    matrix[row, col, 0] =  red;
                    matrix[row, col, 1] = green;
                    matrix[row, col, 2] = blue;
                }
            return (true);
        }
        */

        
        public byte convertRGBToByte (ref byte red, ref byte green, ref byte blue)
        {
            byte colorByte = 0;
            byte grayScale = 0;
            colorScale dominantColor;
            
            red = (byte)(red & 0xFC);
            green = (byte)(green & 0xFC);
            blue = (byte)(blue & 0xFC);

            if (red == 0 && green == 0 && blue == 0) return (0x00);

            if (red < green && red < blue)
            { 
                red = 0x00;
                if (green > blue) dominantColor = colorScale.GREEN_BLUE;
                else dominantColor = colorScale.BLUE_GREEN;
            }
            else if (green < red && green < blue)
            {
                green = 0x00;
                if (red > blue) dominantColor = colorScale.RED_BLUE;
                else dominantColor = colorScale.BLUE_RED;
            }
            else
            {
                blue = 0x00;
                if (red > green) dominantColor = colorScale.RED_GREEN;
            }
            return (grayScale);
        }
        

        public void initMatrix()
        {            
            for (int row = 0; row < MATRIX_HEIGHT; row++)
                for (int col = 0; col < MATRIX_WIDTH; col++)
                    for (int channel = 0; channel < NUMCHANNELS; channel++)
                        matrix[row, col, channel] = 0x00;
        }

        UInt32 getLongInteger(byte b0, byte b1, byte b2, byte b3){
            convertToInteger.byte0 = b0;
            convertToInteger.byte1 = b1;
            convertToInteger.byte2 = b2;
            convertToInteger.byte3 = b3;
            return (convertToInteger.lngInteger);
        }

        UInt16 getShortInteger(byte b0, byte b1)
        {
            convertToInteger.byte0 = b0;
            convertToInteger.byte1 = b1;
            return (convertToInteger.shtInteger);
        }

        bool insertByte(byte dataByte, ref byte[] ptrBuffer, ref UInt16 index)
        {
            if (index >= MAXPACKET) return (false);
            if (dataByte == STX || dataByte == DLE || dataByte == ETX) { 
                ptrBuffer[index++] = DLE; 
            }
            if (index >= MAXPACKET) return (false);
            // if (dataByte == ETX) dataByte++;  // $$$$
            ptrBuffer[index++] = dataByte;
            return (true);
        }

        UInt16 BuildPacket(byte command, byte subcommand, ref byte[] ptrData, UInt16 dataLength, ref byte[] ptrPacket)
        {

            if (dataLength <= MAXDATABYTES)
            {
                UInt16 packetIndex = 1;
                ptrPacket[0] = STX;
                insertByte(command, ref ptrPacket, ref packetIndex);
                insertByte(subcommand, ref ptrPacket, ref packetIndex);

                convertToInteger.shtInteger = dataLength;
                insertByte(convertToInteger.byte0, ref ptrPacket, ref packetIndex);
                insertByte(convertToInteger.byte1, ref ptrPacket, ref packetIndex);

                for (UInt16 dataIndex = 0; dataIndex < dataLength; dataIndex++)
                {
                    insertByte(ptrData[dataIndex], ref ptrPacket, ref packetIndex);
                }

                ptrPacket[packetIndex++] = ETX;

                return (packetIndex);
            }
            else return (0);
        }



        public CameraCapture() {
            // private VideoCapture _capture = null;
            InitializeComponent();
            initializePanels();
            initMatrix();
            _capture = new VideoCapture();

            colorOffset = 0;

        CvInvoke.UseOpenCL = false;
            try
            {

#if PLAY_VIDEO_FILE
                //Frame Rate
                My_Timer.Interval = 1000 / FPS;
                My_Timer.Tick += new EventHandler(My_Timer_Tick);
                My_Timer.Start();
                _playVideo = new VideoCapture("c:\\quick.mp4");
                // GoFullscreen(true);                
                _playVideo.ImageGrabbed += ProcessFrame;
#else
                // _capture = new Capture();
               
        _capture.FlipVertical = true;
                _capture.FlipHorizontal = true;
#endif

#if USE_C920
                _capture.SetCaptureProperty(CapProp.FrameWidth, 432);   // Scale Logitech resolution C920 from 1920 x 1080
                _capture.SetCaptureProperty(CapProp.FrameHeight, 240); 
                
                camWidth = _capture.Width;
                camHeight = _capture.Height;
                _capture.ImageGrabbed += ProcessFrame;
#endif
#if USE_C525
                _capture.SetCaptureProperty(CapProp.FrameWidth, 320);   // Scale Logitech C525 resolution from 1280 x 720
                _capture.SetCaptureProperty(CapProp.FrameHeight, 176);
                
                camWidth = _capture.Width;
                camHeight = _capture.Height;
                _capture.ImageGrabbed += ProcessFrame;
#endif
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }            
        }

#if PLAY_VIDEO_FILE

        private void My_Timer_Tick(object sender, EventArgs e)
        {
            captureImageBox.Image = _capture.QueryFrame();
            //Mat frame = new Mat();
            //_playVideo.Retrieve(frame, 0);
            //captureImageBox.Image = frame;
            // captureImageBox.Image = _playVideo.QueryFrame();            
            //Image<Bgr, Byte> img = _playVideo.QueryFrame().ToImage<Bgr, Byte>();
            //int width = Screen.PrimaryScreen.WorkingArea.Width / 2;
            //int height = Screen.PrimaryScreen.WorkingArea.Height / 2;
            //Image<Bgr, byte> cpimg = imgOriginal.Resize(width, height, Emgu.CV.CvEnum.Inter.Linear);//this is image with resize Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR
            //int width = img.Width * 4;
            //int height = img.Height * 4;
            //Image<Bgr, byte> cpimg = img.Resize(width, height, Emgu.CV.CvEnum.Inter.Linear);
            //captureImageBox.Image = cpimg;
            //captureImageBox.Width = Screen.PrimaryScreen.WorkingArea.Width;
            //captureImageBox.Height = Screen.PrimaryScreen.WorkingArea.Height;
        }

        private void GoFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
        }
        
        private void ProcessFrame(object sender, EventArgs arg)
        {
            Mat frame = new Mat();
            _playVideo.Retrieve(frame, 0);
            captureImageBox.Image = frame;
        }
        
#else
        private void ProcessFrame(object sender, EventArgs arg){
            UInt16 i;
            byte k;
            byte command, subCommand, previousSubCommand = 0;
            byte[] outData = new byte[LEDPanel.PANELSIZE];
            byte[] arrPortInput = new byte[128];

            int camWidthCenter = (camWidth / 2) - (CAM_IMAGE_WIDTH / 2);
            int camHeightCenter = (camHeight / 2) - (CAM_IMAGE_HEIGHT / 2);
            

            Mat frame = new Mat();
            _capture.Retrieve(frame, 0);

            //Mat grayFrame = new Mat();
            //CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray, 1);     
            // captureImageBox.Image = grayFrame;
            // Image<Gray, Byte> imageToCrop = grayFrame.ToImage<Gray, Byte>();          

            captureImageBox.Image = frame;            
            Image<Bgr, Byte> imageToCrop = frame.ToImage<Bgr, Byte>();

            imageToCrop.ROI = new Rectangle(camWidthCenter, camHeightCenter, 96, 96);
            Image<Bgr, byte> ImageCropped = imageToCrop.Copy();

            int numChannels = ImageCropped.NumberOfChannels;
            int imageHeight = ImageCropped.Height;
            int imageCols = ImageCropped.Cols;
            int imageRows = ImageCropped.Rows;

            croppedImageBox.Image = ImageCropped;

            // angle = angle + 1;
            // Image<Bgr, Byte> imageRotated = ImageCropped.Rotate(angle, new Bgr(0, 0, 0), true);
            // Bitmap imageBitMap = imageRotated.ToBitmap(); // Convert Emgu image to bitmap image 

            Bitmap imageBitMap = ImageCropped.ToBitmap(); // Convert Emgu image to bitmap image             
            
            MemoryStream ms = new MemoryStream();
            imageBitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            const int MAXBITMAP = 20000;            
            byte[] bitmapData = new byte[MAXBITMAP];
            bitmapData = ms.ToArray();

            int bitmapDataLength = bitmapData.Length;
            int bitMapWidth = imageBitMap.Width;
            int bitMapHeight = imageBitMap.Height;
            int pixelArrayOffset = (int) getLongInteger(bitmapData[0x0A], bitmapData[0x0B], bitmapData[0x0C], bitmapData[0x0D]);


            byte[] arrDisplay = new byte[bitmapData.Length];
            for (i = 0; i < bitmapData.Length; i++) arrDisplay[i] = bitmapData[i];

            byte imageByte;
            // UInt16 imageInt;
            byte[] imageData = new byte[CAM_DATASIZE];
            for (i = 0; i < CAM_DATASIZE; i++){
                imageByte = bitmapData[i + pixelArrayOffset];
                imageByte = gammaTable[imageByte];
                //imageInt = (UInt16)imageByte;
                //imageInt = (UInt16)(imageInt * BRIGHTNESS);
                //imageInt = (UInt16)(imageInt / 255);
                //imageByte = (byte)imageInt;
                imageData[i] = imageByte;
            }

            imageArrayToMatrix(ref imageData);            
            
            // filter();
            matrixToimageArray(ref imageData);
            
            for (i = 0; i < CAM_DATASIZE; i++) arrDisplay[i + pixelArrayOffset] = imageData[i];

            // Convert bitmap data array to stream
            MemoryStream displayStream = new MemoryStream(arrDisplay);
            // Convert stream to bitmap
            Bitmap bmpDisplay = new Bitmap(System.Drawing.Image.FromStream(displayStream));
            // Convert bitmap to image
            Image<Bgr, Byte> displayImage = new Image<Bgr, Byte>(bmpDisplay);

            // Blow it up
            Image<Bgr, Byte> bigDisplayImage = displayImage.Resize(CAM_IMAGE_WIDTH * 4, CAM_IMAGE_HEIGHT * 4, Inter.Linear);
            //int widthActual = bigDisplayImage.Width;
            //int heightActual = bigDisplayImage.Height;

            // Display Image
            displayImageBox.Image = bigDisplayImage;
            // bigDisplayImage.Flip()
            


            for (i = 0; i < LEDPanel.NUMPANELS; i++) MyPanels[i].setPanelData(ref matrix);
            UInt16 packetDataSize = LEDPanel.PANELSIZE / 3;

#if USE_SERIAL
            try {
                for (k = 0; k < LEDPanel.NUMPANELS; k++) {
                    // for (int j = 0; j < LEDPanel.PANELSIZE; j++) outData[j] = MyPanels[k].getPanelData(j);        

                    for (UInt16 j = 0; j < packetDataSize; j++)
                    {
                        UInt16 m = (UInt16)(j * 3);
                        UInt16 Red, Green, Blue;
                        Red = (UInt16)(MyPanels[k].getPanelData(m++) & 0xE0);
                        Green = (UInt16)(MyPanels[k].getPanelData(m++) & 0xE0);
                        Blue = (UInt16)(MyPanels[k].getPanelData(m) & 0xC0);
                        UInt16 colorInt;
                        colorInt = (UInt16)(Red | Green >> 3 | Blue >> 6);
                        if (colorInt != 0) { 
                            colorInt = (UInt16)(colorInt + colorOffset);
                            if (colorInt > 255) colorInt = (UInt16)(colorInt - 255);
                        }
                        outData[j] = (Byte) colorInt;
                    }

                    byte[] outPacket = new byte[MAXPACKETSIZE];

                    subCommand = MyPanels[k].getsubCommand();
                    if ((subCommand == previousSubCommand) && k > 0) command = 1;
                    else command = 0;
                    previousSubCommand = subCommand;
                    int packetLength = BuildPacket(command, subCommand, ref outData, packetDataSize, ref outPacket);
                    
                    if (MyPanels[k].getPortNumber() == 1)
                    {
                        serialPort1.Write(outPacket, 0, packetLength);
                        serialPort1.Read(arrPortInput, 0, 64);
                    }                    
                    else if (MyPanels[k].getPortNumber() == 2)
                    {
                        serialPort2.Write(outPacket, 0, packetLength);
                        serialPort2.Read(arrPortInput, 0, 64);
                    }                    
                    else if (MyPanels[k].getPortNumber() == 3)
                    {
                        serialPort3.Write(outPacket, 0, packetLength);
                        serialPort3.Read(arrPortInput, 0, 64);
                    }
                    else if (MyPanels[k].getPortNumber() == 4)
                    {
                        serialPort4.Write(outPacket, 0, packetLength);
                        serialPort4.Read(arrPortInput, 0, 64);
                    }                    
                    else if (MyPanels[k].getPortNumber() == 5)
                    {
                        serialPort5.Write(outPacket, 0, packetLength);
                        serialPort5.Read(arrPortInput, 0, 64);
                    }
                    else if (MyPanels[k].getPortNumber() == 6)
                    {
                        serialPort6.Write(outPacket, 0, packetLength);
                        serialPort6.Read(arrPortInput, 0, 64);
                    }                    
                } // end for
                loopCounter++;
                if (loopCounter > 1000
                    )
                {
                    loopCounter = 0;
                    colorOffset++;
                    if (colorOffset > 255) colorOffset = 0;
                }

            }
            catch {
                try {
                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardOutBuffer();                    
                    serialPort1.Close();

                    serialPort2.DiscardInBuffer();
                    serialPort2.DiscardOutBuffer();
                    serialPort2.Close();

                    serialPort3.DiscardInBuffer();
                    serialPort3.DiscardOutBuffer();
                    serialPort3.Close();

                    serialPort4.DiscardInBuffer();
                    serialPort4.DiscardOutBuffer();
                    serialPort4.Close();

                    serialPort5.DiscardInBuffer();
                    serialPort5.DiscardOutBuffer();
                    serialPort5.Close();

                    serialPort6.DiscardInBuffer();
                    serialPort6.DiscardOutBuffer();
                    serialPort6.Close();

                }
                catch { }
            }
#endif            
        }
#endif

        private void captureButtonClick(object sender, EventArgs e)
        {
            if (_capture != null)
            {
                if (_captureInProgress)
                {
#if USE_SERIAL
                    //Dispose the In and Out buffers;
                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardOutBuffer();                    
                    serialPort2.DiscardInBuffer();
                    serialPort2.DiscardOutBuffer();                    
                    serialPort3.DiscardInBuffer();
                    serialPort3.DiscardOutBuffer();
                    serialPort4.DiscardInBuffer();
                    serialPort4.DiscardOutBuffer();
                    serialPort5.DiscardInBuffer();
                    serialPort5.DiscardOutBuffer();
                    serialPort6.DiscardInBuffer();
                    serialPort6.DiscardOutBuffer();

                    //Close the COM port
                    serialPort1.Close();
                    serialPort2.Close();
                    serialPort3.Close();
                    serialPort4.Close();
                    serialPort5.Close();
                    serialPort6.Close();
#endif

                    captureButton.Text = "Start Capture";
                    _capture.Pause(); //stop the capture
                }
                else
                {
#if USE_SERIAL
                    serialPort1.PortName = "COM5";
                    serialPort1.BaudRate = 115200;
                    serialPort1.Parity = 0;
                    serialPort1.DataBits = 8;
                    serialPort1.StopBits = System.IO.Ports.StopBits.One;
                    serialPort1.Open();
                    
                    serialPort2.PortName = "COM6";
                    serialPort2.BaudRate = 115200;
                    serialPort2.Parity = 0;
                    serialPort2.DataBits = 8;
                    serialPort2.StopBits = System.IO.Ports.StopBits.One;
                    serialPort2.Open();
                    
                    serialPort3.PortName = "COM9";
                    serialPort3.BaudRate = 115200;
                    serialPort3.Parity = 0;
                    serialPort3.DataBits = 8;
                    serialPort3.StopBits = System.IO.Ports.StopBits.One;
                    serialPort3.Open();

                    serialPort4.PortName = "COM11";
                    serialPort4.BaudRate = 115200;
                    serialPort4.Parity = 0;
                    serialPort4.DataBits = 8;
                    serialPort4.StopBits = System.IO.Ports.StopBits.One;
                    serialPort4.Open();

                    serialPort5.PortName = "COM3";
                    serialPort5.BaudRate = 115200;
                    serialPort5.Parity = 0;
                    serialPort5.DataBits = 8;
                    serialPort5.StopBits = System.IO.Ports.StopBits.One;
                    serialPort5.Open();

                    serialPort6.PortName = "COM18";
                    serialPort6.BaudRate = 115200;
                    serialPort6.Parity = 0;
                    serialPort6.DataBits = 8;
                    serialPort6.StopBits = System.IO.Ports.StopBits.One;
                    serialPort6.Open();

#endif
                    //start the capture
                    captureButton.Text = "Stop";
                    _capture.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
        }

        public static System.Drawing.Image ResizeImage(System.Drawing.Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            System.Drawing.Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        private void ReleaseData()
        {
            if (_capture != null)
                _capture.Dispose();
        }

        private void FlipHorizontalButtonClick(object sender, EventArgs e)
        {
            if (_capture != null) _capture.FlipHorizontal = !_capture.FlipHorizontal;
        }

        private void FlipVerticalButtonClick(object sender, EventArgs e)
        {
            if (_capture != null) _capture.FlipVertical = !_capture.FlipVertical;
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void grayscaleImageBox_Click(object sender, EventArgs e)
        {

        }

        private void btnHalt_Click(object sender, EventArgs e)
        {
#if USE_SERIAL
            try
            {
                //Dispose the In and Out buffers;
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();                
                serialPort2.DiscardInBuffer();
                serialPort2.DiscardOutBuffer();                
                serialPort3.DiscardInBuffer();
                serialPort3.DiscardOutBuffer();
                serialPort4.DiscardInBuffer();
                serialPort4.DiscardOutBuffer();
                serialPort5.DiscardInBuffer();
                serialPort5.DiscardOutBuffer();
                serialPort6.DiscardInBuffer();
                serialPort6.DiscardOutBuffer();

                //Close the COM port
                serialPort1.Close();
                serialPort2.Close();
                serialPort3.Close();
                serialPort4.Close();
                serialPort5.Close();
                serialPort6.Close();
            }
            //If there was an exeception then there isn't much we can
            //  do.  The port is no longer available.
            catch { }
#endif
        }

        private void captureImageBox_Click(object sender, EventArgs e)
        {

        }

        public static Bitmap ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }

        private void CameraCapture_Load(object sender, EventArgs e)
        {

        }

        private void displayImageBox_Click(object sender, EventArgs e)
        {

        }

        private void btnPlayVideo_Click(object sender, EventArgs e)
        {
            if (_playVideo != null)
            {
                if (_playingVideo)
                {
                    btnPlayVideo.Text = "Play Video";
                    _playVideo.Pause();
                }
                else
                {                    
                    btnPlayVideo.Text = "Pause Video";
                    _playVideo.Start();
                }
                _playingVideo = !_playingVideo;
            }

        }

        private void cannyImageBox_Click(object sender, EventArgs e)
        {

        }
    }



} // End namespace CameraCapture


