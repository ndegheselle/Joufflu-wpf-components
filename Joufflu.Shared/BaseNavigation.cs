using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joufflu.Shared
{
    public interface INavigationLayout : IPage
    {
        public void Show(IPage page);
    }

    public interface IPage
    {
        public INavigationLayout? Layout { get; set; }
    }
}
