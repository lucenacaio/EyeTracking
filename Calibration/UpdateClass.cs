using EyeTribe.ClientSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyeTribe.ClientSdk.Data;
using EyeTribe.Controls.Cursor;
using System.Windows.Forms;

namespace Calibration
{
    public class UpdateClass : CursorControl
    {

        public UpdateClass(){}
        
        public UpdateClass(Screen screen, bool enabled, bool smooth) : base(screen,enabled,smooth){}
        public void  OnGazeUpdate(GazeData gazeData)
        {
            throw new NotImplementedException();
        }
    }
}
