using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Domain
{
    public interface IHarness
    {
        string GetSomething();
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Harness : IHarness
    {
        public string GetSomething()
        {
            return "Harness";
        }
    }
}
