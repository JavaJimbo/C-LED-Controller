
/* ARRAY TO BITMAP CODE */


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
        public const int CAM_IMAGE_WIDTH = 160;
        public const int MATRIX_WIDTH = CAM_IMAGE_WIDTH;
        public const int CAM_IMAGE_HEIGHT = 120;
        public const int MATRIX_HEIGHT = CAM_IMAGE_HEIGHT;
        public const int CAM_DATASIZE = (CAM_IMAGE_WIDTH * CAM_IMAGE_HEIGHT * 3);
        public const int MAXPACKETSIZE = (LEDPanel.PANELSIZE * 4);
        public const int NUMCHANNELS = LEDPanel.NUMCHANNELS;        
        public const int NUMCOMPRESS_CHANNELS = 2;
        public const int COMPRESSED_SIZE = LEDPanel.PANELROWS * LEDPanel.PANELCOLS * NUMCOMPRESS_CHANNELS;
        public const int TABLE_LENGTH = 256;


        public const UInt16 BRIGHTNESS = 64;

        public byte[,,] matrix = new byte[MATRIX_HEIGHT, MATRIX_WIDTH, NUMCHANNELS];

        public readonly long[] RGBtable = {
    0xff0000, 0xfc0300, 0xf90600, 0xf60900,
    0xf30c00, 0xf00f00, 0xed1200, 0xea1500,
    0xe71800, 0xe41b00, 0xe11e00, 0xde2100,
    0xdb2400, 0xd82700, 0xd52a00, 0xd22d00,
    0xcf3000, 0xcc3300, 0xc93600, 0xc63900,
    0xc33c00, 0xc03f00, 0xbd4200, 0xba4500,
    0xb74800, 0xb44b00, 0xb14e00, 0xae5100,
    0xab5400, 0xa85700, 0xa55a00, 0xa25d00,
    0x9f6000, 0x9c6300, 0x996600, 0x966900,
    0x936c00, 0x906f00, 0x8d7200, 0x8a7500,
    0x877800, 0x847b00, 0x817e00, 0x7e8100,
    0x7b8400, 0x788700, 0x758a00, 0x728d00,
    0x6f9000, 0x6c9300, 0x699600, 0x669900,
    0x639c00, 0x609f00, 0x5da200, 0x5aa500,
    0x57a800, 0x54ab00, 0x51ae00, 0x4eb100,
    0x4bb400, 0x48b700, 0x45ba00, 0x42bd00,
    0x3fc000, 0x3cc300, 0x39c600, 0x36c900,
    0x33cc00, 0x30cf00, 0x2dd200, 0x2ad500,
    0x27d800, 0x24db00, 0x21de00, 0x1ee100,
    0x1be400, 0x18e700, 0x15ea00, 0x12ed00,
    0x0ff000, 0x0cf300, 0x09f600, 0x06f900,
    0x03fc00,
    0x00ff00, 0x00fc03, 0x00f906, 0x00f609,
    0x00f30c, 0x00f00f, 0x00ed12, 0x00ea15,
    0x00e718, 0x00e41b, 0x00e11e, 0x00de21,
    0x00db24, 0x00d827, 0x00d52a, 0x00d22d,
    0x00cf30, 0x00cc33, 0x00c936, 0x00c639,
    0x00c33c, 0x00c03f, 0x00bd42, 0x00ba45,
    0x00b748, 0x00b44b, 0x00b14e, 0x00ae51,
    0x00ab54, 0x00a857, 0x00a55a, 0x00a25d,
    0x009f60, 0x009c63, 0x009966, 0x009669,
    0x00936c, 0x00906f, 0x008d72, 0x008a75,
    0x008778, 0x00847b, 0x00817e, 0x007e81,
    0x007b84, 0x007887, 0x00758a, 0x00728d,
    0x006f90, 0x006c93, 0x006996, 0x006699,
    0x00639c, 0x00609f, 0x005da2, 0x005aa5,
    0x0057a8, 0x0054ab, 0x0051ae, 0x004eb1,
    0x004bb4, 0x0048b7, 0x0045ba, 0x0042bd,
    0x003fc0, 0x003cc3, 0x0039c6, 0x0036c9,
    0x0033cc, 0x0030cf, 0x002dd2, 0x002ad5,
    0x0027d8, 0x0024db, 0x0021de, 0x001ee1,
    0x001be4, 0x0018e7, 0x0015ea, 0x0012ed,
    0x000ff0, 0x000cf3, 0x0009f6, 0x0006f9,
    0x0003fc,
    0x0000ff, 0x0300fc, 0x0600f9, 0x0900f6,
    0x0c00f3, 0x0f00f0, 0x1200ed, 0x1500ea,
    0x1800e7, 0x1b00e4, 0x1e00e1, 0x2100de,
    0x2400db, 0x2700d8, 0x2a00d5, 0x2d00d2,
    0x3000cf, 0x3300cc, 0x3600c9, 0x3900c6,
    0x3c00c3, 0x3f00c0, 0x4200bd, 0x4500ba,
    0x4800b7, 0x4b00b4, 0x4e00b1, 0x5100ae,
    0x5400ab, 0x5700a8, 0x5a00a5, 0x5d00a2,
    0x60009f, 0x63009c, 0x660099, 0x690096,
    0x6c0093, 0x6f0090, 0x72008d, 0x75008a,
    0x780087, 0x7b0084, 0x7e0081, 0x81007e,
    0x84007b, 0x870078, 0x8a0075, 0x8d0072,
    0x90006f, 0x93006c, 0x960069, 0x990066,
    0x9c0063, 0x9f0060, 0xa2005d, 0xa5005a,
    0xa80057, 0xab0054, 0xae0051, 0xb1004e,
    0xb4004b, 0xb70048, 0xba0045, 0xbd0042,
    0xc0003f, 0xc3003c, 0xc60039, 0xc90036,
    0xcc0033, 0xcf0030, 0xd2002d, 0xd5002a,
    0xd80027, 0xdb0024, 0xde0021, 0xe1001e,
    0xe4001b, 0xe70018, 0xea0015, 0xed0012,
    0xf0000f, 0xf3000c, 0xf60009, 0xf90006,
    0xfc0003, 0xff0000
};


        public readonly long[] RGBsortedTable = {
            0x0000ff, 0x0003fc, 0x0006f9, 0x0009f6, 0x000cf3, 0x000ff0, 0x0012ed, 0x0015ea,
            0x0018e7, 0x001be4, 0x001ee1, 0x0021de, 0x0024db, 0x0027d8, 0x002ad5, 0x002dd2,
            0x0030cf, 0x0033cc, 0x0036c9, 0x0039c6, 0x003cc3, 0x003fc0, 0x0042bd, 0x0045ba,
            0x0048b7, 0x004bb4, 0x004eb1, 0x0051ae, 0x0054ab, 0x0057a8, 0x005aa5, 0x005da2,
            0x00609f, 0x00639c, 0x006699, 0x006996, 0x006c93, 0x006f90, 0x00728d, 0x00758a,
            0x007887, 0x007b84, 0x007e81, 0x00817e, 0x00847b, 0x008778, 0x008a75, 0x008d72,
            0x00906f, 0x00936c, 0x009669, 0x009966, 0x009c63, 0x009f60, 0x00a25d, 0x00a55a,
            0x00a857, 0x00ab54, 0x00ae51, 0x00b14e, 0x00b44b, 0x00b748, 0x00ba45, 0x00bd42,
            0x00c03f, 0x00c33c, 0x00c639, 0x00c936, 0x00cc33, 0x00cf30, 0x00d22d, 0x00d52a,
            0x00d827, 0x00db24, 0x00de21, 0x00e11e, 0x00e41b, 0x00e718, 0x00ea15, 0x00ed12,
            0x00f00f, 0x00f30c, 0x00f609, 0x00f906, 0x00fc03, 0x00ff00, 0x0300fc, 0x03fc00,
            0x0600f9, 0x06f900, 0x0900f6, 0x09f600, 0x0c00f3, 0x0cf300, 0x0f00f0, 0x0ff000,
            0x1200ed, 0x12ed00, 0x1500ea, 0x15ea00, 0x1800e7, 0x18e700, 0x1b00e4, 0x1be400,
            0x1e00e1, 0x1ee100, 0x2100de, 0x21de00, 0x2400db, 0x24db00, 0x2700d8, 0x27d800,
            0x2a00d5, 0x2ad500, 0x2d00d2, 0x2dd200, 0x3000cf, 0x30cf00, 0x3300cc, 0x33cc00,
            0x3600c9, 0x36c900, 0x3900c6, 0x39c600, 0x3c00c3, 0x3cc300, 0x3f00c0, 0x3fc000,
            0x4200bd, 0x42bd00, 0x4500ba, 0x45ba00, 0x4800b7, 0x48b700, 0x4b00b4, 0x4bb400,
            0x4e00b1, 0x4eb100, 0x5100ae, 0x51ae00, 0x5400ab, 0x54ab00, 0x5700a8, 0x57a800,
            0x5a00a5, 0x5aa500, 0x5d00a2, 0x5da200, 0x60009f, 0x609f00, 0x63009c, 0x639c00,
            0x660099, 0x669900, 0x690096, 0x699600, 0x6c0093, 0x6c9300, 0x6f0090, 0x6f9000,
            0x72008d, 0x728d00, 0x75008a, 0x758a00, 0x780087, 0x788700, 0x7b0084, 0x7b8400,
            0x7e0081, 0x7e8100, 0x81007e, 0x817e00, 0x84007b, 0x847b00, 0x870078, 0x877800,
            0x8a0075, 0x8a7500, 0x8d0072, 0x8d7200, 0x90006f, 0x906f00, 0x93006c, 0x936c00,
            0x960069, 0x966900, 0x990066, 0x996600, 0x9c0063, 0x9c6300, 0x9f0060, 0x9f6000,
            0xa2005d, 0xa25d00, 0xa5005a, 0xa55a00, 0xa80057, 0xa85700, 0xab0054, 0xab5400,
            0xae0051, 0xae5100, 0xb1004e, 0xb14e00, 0xb4004b, 0xb44b00, 0xb70048, 0xb74800,
            0xba0045, 0xba4500, 0xbd0042, 0xbd4200, 0xc0003f, 0xc03f00, 0xc3003c, 0xc33c00,
            0xc60039, 0xc63900, 0xc90036, 0xc93600, 0xcc0033, 0xcc3300, 0xcf0030, 0xcf3000,
            0xd2002d, 0xd22d00, 0xd5002a, 0xd52a00, 0xd80027, 0xd82700, 0xdb0024, 0xdb2400,
            0xde0021, 0xde2100, 0xe1001e, 0xe11e00, 0xe4001b, 0xe41b00, 0xe70018, 0xe71800,
            0xea0015, 0xea1500, 0xed0012, 0xed1200, 0xf0000f, 0xf00f00, 0xf3000c, 0xf30c00,
            0xf60009, 0xf60900, 0xf90006, 0xf90600, 0xfc0003, 0xfc0300, 0xff0000, 0xffffff};


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
        
        private Capture _capture = null;
        private bool _captureInProgress;
        public byte[,] arrPanelInit = {{ LEDPanel.HORIZONTAL, 0, 0}, { LEDPanel.HORIZONTAL, 16, 0 }, { LEDPanel.HORIZONTAL, 0, 32 }, { LEDPanel.HORIZONTAL, 16, 32 },
            { LEDPanel.HORIZONTAL, 0, 0}, { LEDPanel.HORIZONTAL, 16, 0 }, { LEDPanel.HORIZONTAL, 0, 32 }, { LEDPanel.HORIZONTAL, 16, 32 },
            { LEDPanel.HORIZONTAL, 0, 0}, { LEDPanel.HORIZONTAL, 16, 0 }, { LEDPanel.HORIZONTAL, 0, 32 }, { LEDPanel.HORIZONTAL, 16, 32 },
            { LEDPanel.HORIZONTAL, 0, 0}, { LEDPanel.HORIZONTAL, 16, 0 }, { LEDPanel.HORIZONTAL, 0, 32 }, { LEDPanel.HORIZONTAL, 16, 32 }};

        LEDPanel[] MyPanels = new LEDPanel[LEDPanel.NUMPANELS];

        const byte STX = (byte)'>';
        const byte DLE = (byte)'/';
        const byte ETX = (byte)'\r';

        public const int MAXDATABYTES = (LEDPanel.PANELROWS * LEDPanel.PANELCOLS * LEDPanel.NUMCHANNELS);
        public const int MAXPACKET = (MAXDATABYTES * 4);
        
        convertType convertToInteger;

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
                
        public void fillMatrix(ref byte[] ptrData)
        {
            int i = 0;
            for (int row = 0; row < MATRIX_HEIGHT; row++)
                for (int col = 0; col < MATRIX_WIDTH; col++)
                    for (int channel = 0; channel < NUMCHANNELS; channel++)
                        matrix[row,col,channel] = ptrData[i++];
        }

        public void initializePanels(){
            for (int i = 0; i < LEDPanel.NUMPANELS; i++){
                MyPanels[i] = new LEDPanel();
                MyPanels[i].setOrientation(i, arrPanelInit[i, 0], arrPanelInit[i, 1], arrPanelInit[i, 2]);
            }

        }

        public CameraCapture() {            
            InitializeComponent();
            initializePanels();

            CvInvoke.UseOpenCL = false;
            try
            {
                _capture = new Capture();
                _capture.SetCaptureProperty(CapProp.FrameWidth, 1280);
                _capture.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }

        public void compressData(ref byte[] ptrPanelData, ref byte[] ptrCompressedData)
        {
            int i, j;
            UInt16 Red, Blue, Green;

            j = 0;
            i = 0;
            do {
                Blue = (UInt16)(((UInt16)ptrPanelData[i++] & 0xF8) >> 3);
                Green = (UInt16)(((UInt16)ptrPanelData[i++] & 0xFC) << 3);
                Red = (UInt16)(((UInt16)ptrPanelData[i++] & 0xF8) << 8);                

                convertToInteger.shtInteger = (UInt16)(Red | Green | Blue);
                ptrCompressedData[j++] = convertToInteger.byte0;
                ptrCompressedData[j++] = convertToInteger.byte1;
            } while (i < LEDPanel.PANELSIZE);
        }

        public void filterData(ref byte[] ptrPanelData, ref byte[] ptrFilteredData)
        {
            int i;
            byte Red, Blue, Green;
            i = 0;
            do
            {
                Blue = ptrPanelData[i];
                Green = ptrPanelData[i+1];
                Red = ptrPanelData[i+2];
                if (Red <= Green && Red <= Blue) Red = 0x00;
                else if (Blue <= Green && Blue <= Red) Blue = 0x00;
                else Green = 0x00;
                ptrFilteredData[i] = Blue;
                ptrFilteredData[i + 1] = Green;
                ptrFilteredData[i + 2] = Red;
                i = i + 3;
            } while (i < LEDPanel.PANELSIZE);
        }

        public byte reduceVal (byte val)
        { 
            if (val < 16) return 0;
            if (val < 32) return 16;
            if (val < 64) return 32;
            return 255;
        }

    private void ProcessFrame(object sender, EventArgs arg)
        {
            int i;
            byte k;

            // _capture.SetCaptureProperty(CapProp.FrameWidth, 1080);

            byte[] outData;
            outData = new byte[LEDPanel.PANELSIZE];

            byte[] filteredData;
            filteredData = new byte[LEDPanel.PANELSIZE];

            byte[] compressedData;
            compressedData = new byte[LEDPanel.PANELSIZE];


            byte[] arrPortInput = new byte[128];

            Mat frame = new Mat();
            _capture.Retrieve(frame, 0);
            // _capture.SetCaptureProperty(CapProp.FrameWidth, 1080);
            
            // if (!_capture.FlipHorizontal) _capture.FlipHorizontal = true;
            captureImageBox.Image = frame;

            /*
            Mat matImage = frame.Clone();
            Bitmap bmpImage = matImage.Bitmap;

            System.Drawing.Image resized = ResizeImage(bmpImage, new Size(CAM_IMAGE_WIDTH, CAM_IMAGE_HEIGHT));
            Bitmap bmpResized = (Bitmap)resized;
            Bitmap imageBitMap = bmpResized.Clone(new Rectangle(0, 0, bmpResized.Width, bmpResized.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            MemoryStream ms = new MemoryStream();
            imageBitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            const int MAXBITMAP = 20000;
            const int HEADERSIZE = 54;
            byte[] bitmapData = new byte[MAXBITMAP];
            bitmapData = ms.ToArray();          
            byte[] arrDisplay = new byte[bitmapData.Length];
            for (i = 0; i < bitmapData.Length; i++) arrDisplay[i] = bitmapData[i];

            //for (i = 0; i < CAM_DATASIZE; i++)
            //    arrDisplay[i + HEADERSIZE] = imageData[i];


            byte[] imageData = new byte[CAM_DATASIZE];
            UInt16 imageInt; byte imageByte;
            for (i = 0; i < CAM_DATASIZE; i++) {
                imageByte = bitmapData[i + HEADERSIZE];
                // imageByte = gammaTable[imageByte];
                // imageInt = (UInt16)imageByte;
                // imageInt = (UInt16)(imageInt * BRIGHTNESS);
                //imageInt = (UInt16)(imageInt / 255);
                // imageByte = (byte)imageInt;
                // imageByte = reduceVal(imageByte);
                imageData[i] = imageByte;
            }

            for (i = 0; i < CAM_DATASIZE; i++) arrDisplay[i + HEADERSIZE] = imageData[i];

            // Convert bitmap data array to stream
            MemoryStream displayStream = new MemoryStream(arrDisplay);
            // Convert stream to bitmap
            Bitmap bmpDisplay = new Bitmap(System.Drawing.Image.FromStream(displayStream));
            // Convert bitmap to image
            Image<Bgr, Byte> displayImage = new Image<Bgr, Byte>(bmpDisplay);

            // Blow it up
            Image<Bgr, Byte> bigDisplayImage = displayImage.Resize(CAM_IMAGE_WIDTH*4, CAM_IMAGE_HEIGHT*4, Inter.Linear);

            // Display Image
            displayImageBox.Image = bigDisplayImage;

            fillMatrix(ref imageData);
            

            for (i = 0; i < LEDPanel.NUMPANELS; i++) MyPanels[i].setPanelData(ref matrix);

            try {
                for (k = 0; k < LEDPanel.NUMPANELS; k++) {
                    for (int j = 0; j < LEDPanel.PANELSIZE; j++) outData[j] = MyPanels[k].getPanelData(j);                    
                    compressData(ref outData, ref compressedData);
                    byte[] outPacket = new byte[MAXPACKETSIZE];
                    int packetLength = BuildPacket(222, k, ref compressedData, COMPRESSED_SIZE, ref outPacket);
                    serialPort1.Write(outPacket, 0, packetLength);
                    serialPort1.Read(arrPortInput, 0, 64);                    
                }
            }
            catch {
                try {
                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardOutBuffer();                    
                    serialPort1.Close();
                }
                catch { }
            } */
            
        }

        private void captureButtonClick(object sender, EventArgs e)
        {
            if (_capture != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                   //Dispose the In and Out buffers;
                    serialPort1.DiscardInBuffer();
                    serialPort1.DiscardOutBuffer();

                    //Close the COM port
                    serialPort1.Close();
                    captureButton.Text = "Start Capture";
                    _capture.Pause();
                }
                else
                {
                    serialPort1.PortName = "COM3";
                    serialPort1.BaudRate = 115200;
                    serialPort1.Parity = 0;
                    serialPort1.DataBits = 8;
                    serialPort1.StopBits = System.IO.Ports.StopBits.One;
                    serialPort1.Open();
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

        private void displayImageBox_Click(object sender, EventArgs e)
        {

        }

        private void btnHalt_Click(object sender, EventArgs e)
        {
            try
            {
                //Dispose the In and Out buffers;
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();

                //Close the COM port
                serialPort1.Close();
            }
            //If there was an exeception then there isn't much we can
            //  do.  The port is no longer available.
            catch { }
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

        public byte getColorTableIndex(long tableVal)
        {
            long upper, lower;
            int upperIndex, lowerIndex, diffIndex;

            if (tableVal >= (RGBsortedTable[TABLE_LENGTH - 1])) return (TABLE_LENGTH - 1);
            if (tableVal <= (RGBsortedTable[0])) return (0);

            upperIndex = TABLE_LENGTH - 1;
            lowerIndex = upperIndex / 2;

            do
            {
                upper = RGBsortedTable[upperIndex];
                lower = RGBsortedTable[lowerIndex];
                if (tableVal == upper) return ((byte)upperIndex);
                else if (tableVal == lower) return ((byte)lowerIndex);

                diffIndex = abs(upperIndex - lowerIndex);
                if (diffIndex > 1)
                {
                    if (tableVal < lower)
                    {
                        upperIndex = lowerIndex;
                        lowerIndex = upperIndex - (diffIndex / 2);
                    }
                    else if (tableVal > upper)
                    {
                        lowerIndex = upperIndex;
                        upperIndex = lowerIndex + (diffIndex / 2);
                    }
                    else upperIndex = upperIndex - (diffIndex / 2);
                    if (upperIndex >= (TABLE_LENGTH - 1)) upperIndex = TABLE_LENGTH - 1;
                    if (lowerIndex < 0) lowerIndex = 0;
                }
            } while (diffIndex > 1);

            if (absLong(tableVal - upper) < absLong(tableVal - lower)) return (byte)upperIndex;
            else return (byte)lowerIndex;
        }

        public long absLong(long longVal)
        {
            long absValue;
            if (longVal < 0) absValue = 0 - longVal;
            else absValue = longVal;
            return (absValue);
        }

        public int abs(int intVal)
        {
            int absValue;
            if (intVal < 0) absValue = 0 - intVal;
            else absValue = intVal;
            return (absValue);
        }

    } // End namespace CameraCapture

}




