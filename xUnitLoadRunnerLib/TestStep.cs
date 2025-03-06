using System;
using System.Threading.Tasks;

namespace xUnitLoadRunnerLib
{
    public class TestStep
    {
        public string Name { get; set; }
        public Func<Task<bool>> Action { get; set; }
    }
}
