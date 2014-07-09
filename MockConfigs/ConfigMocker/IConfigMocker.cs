using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigMocker
{
    public interface IConfigMocker
    {
        void Mock();
        void MockConnectionStrings();
        void MockAppSettings();
    }
}
