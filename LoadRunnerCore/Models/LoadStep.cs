using System;
using System.Threading.Tasks;

namespace LoadRunnerCore.Models
{
    public class LoadStep
    {
        public string Name { get; set; }
        public Func<Task<bool>> Action { get; set; }
    }
}