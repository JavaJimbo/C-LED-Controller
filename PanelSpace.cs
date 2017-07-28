using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraCapture;

namespace PanelSpace
{
    public class LEDPanel
    {
        public const int    NUMPANELS = 24;  
        public const int    PANELROWS = 16;
        public const int    PANELCOLS = 32;
        public const int    NUMCHANNELS = CameraCapture.CameraCapture.NUMCHANNELS;
        public const int    PANELSIZE = PANELROWS * PANELCOLS * NUMCHANNELS;
        public const int    HORIZONTAL = 0;
        public const int    VERTICAL = 1;
        public const int    HORIZONTAL_FLIPPED = 2;
        public const int    VERTICAL_FLIPPED = 3;
        public int          serialPortNumber = 1;
        public int          orientation;
        public int          ZeroCol, ZeroRow;
        public byte         subCommand;
        public Byte[]       arrPanelData = new Byte[PANELSIZE];

        public LEDPanel()
        {            
            ZeroCol = 0;
            ZeroRow = 0;
            orientation = HORIZONTAL;
            serialPortNumber = 1;

            for (int i = 0; i < PANELSIZE; i++)
                arrPanelData[i] = 0x00;
        }


        public void setOrientation(int portNumber, byte subCommand, int orientation, int ZeroRow, int ZeroCol)
        {
            this.ZeroRow = ZeroRow;
            this.ZeroCol = ZeroCol;
            this.orientation = orientation;
            this.subCommand = subCommand;
            this.serialPortNumber = portNumber;
        }

        public Byte getPanelData(int index)
        {
            return (arrPanelData[index]);
        }

        public int getPortNumber()
        {
            return (serialPortNumber);
        }

        public byte getsubCommand()
        {
            return (subCommand);
        }

        public void setPanelData(ref byte[,,] matrix)
        {
            int row, col, channel, i;

            i = 0;
            if (orientation == HORIZONTAL)
            {
                for (row = ZeroRow; row < ZeroRow + PANELROWS; row++)
                    for (col = ZeroCol; col < ZeroCol + PANELCOLS; col++)
                        for (channel = 0; channel < NUMCHANNELS; channel++)
                            arrPanelData[i++] = matrix[row, col, channel];
            }
            else if (orientation == VERTICAL)
            {
                for (col = ZeroCol + PANELROWS - 1; col >= ZeroCol; col--)
                    for (row = ZeroRow; row < row + PANELCOLS; row++)
                        for (channel = 0; channel < NUMCHANNELS; channel++)
                            arrPanelData[i++] = matrix[row, col, channel];
            }
            else if (orientation == HORIZONTAL_FLIPPED)
            {
                for (row = ZeroRow + PANELROWS - 1; row >= ZeroRow; row--)
                    for (col = ZeroCol + PANELCOLS + 1; col >= ZeroCol; col--)
                        for (channel = 0; channel < NUMCHANNELS; channel++)
                            arrPanelData[i++] = matrix[row, col, channel];
            }
            else
            {
                for (col = ZeroCol; col < ZeroCol + PANELROWS; col++)
                    for (row = ZeroRow + PANELCOLS - 1; row >= ZeroRow; row--)
                        for (channel = 0; channel < NUMCHANNELS; channel++)
                            arrPanelData[i++] = matrix[row, col, channel];
            }
        }

    }
}



