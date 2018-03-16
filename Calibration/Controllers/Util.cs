using System;
using System.Threading.Tasks;

namespace Calibration.Controllers
{
    public class Util
    {
        public static Task<object> SetTimeout(Func<object> func, int secs)
        {
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(secs));
            return Task.Run(() => func());
        }
    }
}
