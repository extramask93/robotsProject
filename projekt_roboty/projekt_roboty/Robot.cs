using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace projekt_roboty
{
    public class Robot
    {
        public float leftEngine { get; set; }
        public float rightEngine{get; set;}
        public float Ymm { get; set; }
        public float Xmm { get; set; }
        public float angle { get; set; }
        public int id { get; set; }
        public byte detected { get; set; }
        public byte controlable { get; set; }

        public Robot(int id)
        {
            this.id = id;
            leftEngine=0;
            rightEngine = 0;
            controlable = 0;
            detected = 0;     
        }
        public List<byte> encodeData()
        {
            List<byte> bb = new List<byte>();
            byte[] en1b = BitConverter.GetBytes(leftEngine);
            byte[] en2b = BitConverter.GetBytes(rightEngine);
            bb.AddRange(en1b);
            bb.AddRange(en2b);
            return bb;
            
        }
        public void decodeData(ArraySegment<byte> segm)
        {
            if (segm.Count != 14)
                throw (new Exception("Data segment sent to decode has incorrent length of: "+segm.Count.ToString()));
            byte[] data =segm.ToArray();
            try
            {
                segm = new ArraySegment<byte>(data, 0, 1);//get id
                id = segm.Single();
                segm = new ArraySegment<byte>(data, 1, 1); //get detection flag
                detected = segm.Single();
                segm = new ArraySegment<byte>(data, 2, 4); //get X coord
                Xmm = BitConverter.ToSingle(segm.ToArray(), 0);
                segm = new ArraySegment<byte>(data, 6, 4); //get Y coord
                Ymm = BitConverter.ToSingle(segm.ToArray(), 0);
                segm = new ArraySegment<byte>(data, 10, 4); //get angle
                angle = BitConverter.ToSingle(segm.ToArray(), 0);
            }
            catch(Exception)
            {
                throw (new Exception("Error during parsing idividual robot position"));
            }
        }

    }
 
}
       
   

