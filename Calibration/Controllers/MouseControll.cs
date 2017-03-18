using EyeTribe.ClientSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyeTribe.ClientSdk.Data;
using EyeTribe.Controls.Cursor;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Calibration.Controllers;

namespace Calibration
{
    public class MouseControll : CursorControl,IGazeListener
    {
        public MouseControll(){}
        
        public MouseControll(Screen screen, bool enabled, bool smooth) : base(screen,enabled,smooth){}


        public void OnGazeUpdate(GazeData gazeData)
        {
            //TODO - Logica
            if(gazeData.LeftEye.RawCoordinates.X == 0 && gazeData.LeftEye.RawCoordinates.Y == 0
                && gazeData.RightEye.RawCoordinates.X != 0 && gazeData.RightEye.RawCoordinates.Y != 0)
            {
                ClickControll.Click(Cursor.Position.X, Cursor.Position.Y);
            }
            base.OnGazeUpdate(gazeData);
        }

      
    }
}
