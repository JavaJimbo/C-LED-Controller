using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CameraCapture;



namespace PanelSpace
{
    public class LEDPanel
    {
        public const int NUMPANELS = 6;
        public const int PANELROWS = 16;
        public const int PANELCOLS = 32;
        public const int PANELSIZE = PANELROWS * PANELCOLS;
        public const int HORIZONTAL = 0;
        public const int VERTICAL = 1;
        public const int HORIZONTAL_FLIPPED = 2;
        public const int VERTICAL_FLIPPED = 3;
        public int orientation;
        public int ZeroCol, ZeroRow, PanelNumber;
        public UInt16[] arrOutData = new UInt16[PANELSIZE];

        public LEDPanel(){
            int i, j, k;
            ZeroCol = 0;
            ZeroRow = 0;
            orientation = HORIZONTAL;

            for (i = 0; i < PANELSIZE; i++)
                arrOutData[i] = 0x00;
        }


        public void setOrientation(int index, int ARGorientation, int ARGrow, int ARGcol){
            ZeroRow = ARGrow;
            ZeroCol = ARGcol;
            orientation = ARGorientation;
            PanelNumber = index;
        }

        public UInt16 getOutData(int index){
            return (arrOutData[index]);
        }


        public void setOutData(ref UInt16[,] ptrInData){
            int matrixRow, matrixCol, i;

            i = 0;
            if (orientation == HORIZONTAL){
                for (matrixRow = ZeroRow; matrixRow < ZeroRow + PANELROWS; matrixRow++) 
                    for (matrixCol = ZeroCol; matrixCol < ZeroCol + PANELCOLS; matrixCol++)
                        arrOutData[i++] = ptrInData[matrixRow, matrixCol];                
            }
            else if (orientation == VERTICAL){
                for (matrixCol = ZeroCol + PANELROWS - 1; matrixCol >= ZeroCol; matrixCol--)
                    for (matrixRow = ZeroRow ; matrixRow < matrixRow + PANELCOLS; matrixRow++)
                        arrOutData[i++] = ptrInData[matrixRow, matrixCol];
            }
            else if (orientation == HORIZONTAL_FLIPPED)
            {
                for (matrixRow = ZeroRow + PANELROWS - 1; matrixRow >= ZeroRow; matrixRow--)
                    for (matrixCol = ZeroCol + PANELCOLS + 1; matrixCol >= ZeroCol; matrixCol--)
                        arrOutData[i++] = ptrInData[matrixRow, matrixCol];
            }
            else {
                for (matrixCol = ZeroCol; matrixCol < ZeroCol + PANELROWS; matrixCol++)
                    for (matrixRow = ZeroRow + PANELCOLS - 1; matrixRow >= ZeroRow; matrixRow--)
                        arrOutData[i++] = ptrInData[matrixRow, matrixCol];
            }
        } 
    }
}



