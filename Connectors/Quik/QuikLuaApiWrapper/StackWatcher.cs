using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quik
{
    internal class StackWatcher
    {
        int _count;

        public void AssertIsClean()
        {
            Debug.Assert(_count > 0, "Стэк не был очищен");
        }
        public void StartWatching()
        {
            _count = 0;
        }
        public void ItemPushed()
        {
            _count++;
        }
        public void ItemPopped()
        {
            _count--;
        }
    }
}
