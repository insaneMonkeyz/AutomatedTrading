﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicConcepts
{
    public interface INotifyEntityUpdated
    {
        event Action Updated;
    }
}
