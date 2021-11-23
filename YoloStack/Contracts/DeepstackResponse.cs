using System;
using System.Collections.Generic;
using System.Text;

namespace YoloV4Stack.Contracts
{
    public class DeepstackResponse
    {
        public bool success { get; set; }
        public Predictions[] predictions { get; set; }

        public class Predictions
        {
            public string label { get; set; }
            public float confidence { get; set; }
            public int y_min { get; set; }
            public int x_min { get; set; }
            public int y_max { get; set; }
            public int x_max { get; set; }
        }
    }
}
