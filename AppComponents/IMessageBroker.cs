﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppComponents.AppStructureConcepts
{
    public interface IMessageBroker
    {
        void Subscribe();
        void Unsubscribe();

        void Send(object message);
    }
}