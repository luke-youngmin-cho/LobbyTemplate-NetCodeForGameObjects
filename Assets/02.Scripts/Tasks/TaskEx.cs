using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace T.Tasks
{
    public static class TaskEx
    {
        public static async Task WaitUntil(Func<bool> condition, int millisecondDelay)
        {
            while (condition.Invoke() == false)
            {
                await Task.Delay(millisecondDelay);
            }
        }
    }
}
