using System;
using System.Threading.Tasks;

namespace LoadTestRunner.Models
{
    public class LoadTestStep
    {
        public string Name { get; set; }
        public Func<Task<bool>> Action { get; set; }
    }
}