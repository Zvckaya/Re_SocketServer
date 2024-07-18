using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class PacketManager
    {
        #region Singleton
        static PacketManager _instance;
        public static PacketManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PacketManager();
                return _instance;
            }
        }
        #endregion
    }
}
