using System;
using System.Collections.Generic;
using System.Text;

namespace Wexflow.Server
{
    public interface IAppConfiguration
    {
        Logging Logging { get; }
        Smtp Smtp { get; }
    }
}
