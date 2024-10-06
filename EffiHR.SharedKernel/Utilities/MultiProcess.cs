using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.SharedKernel.Utilities
{
    public class MultiProcess
    {
        public async Task ExecuteHandler(int threadCount, int totalItems, Func<int, int, int, Task> handler)
        {
            var pageSize = (int)Math.Ceiling((double)totalItems / threadCount);
            var tasks = new List<Task>();

            for (int i = 0; i < threadCount; i++)
            {
                var pageSkip = i * pageSize;
                tasks.Add(handler(pageSkip, pageSize, i));
            }

            await Task.WhenAll(tasks);
        }
    }

}


