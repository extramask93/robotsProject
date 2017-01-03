using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projekt_roboty
{
    class RobotSearch
    {
        int id;
        public RobotSearch(int id)
        { this.id = id; }
        public bool equals(Robot r)
        {
            return r.id == id;
        }
    }
}
