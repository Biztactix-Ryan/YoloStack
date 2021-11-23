using Microsoft.AspNetCore.Hosting;
using Microsoft.ML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace YoloV4Stack
{
    
    class Program
    {      


        static void Main(string[] args)
        {
            for(int i = 1; i< args.Count(); i++)
            {
                if (args[i - 1].ToLower() == "-gpu") { ProcessImage.GPUID = int.Parse(args[i]); }
            }
            
            var shouldExit = false;
            try
            {
                var host = new WebHostBuilder()
              .UseKestrel()
              .UseStartup<Startup>()
              .UseUrls("http://0.0.0.0:5000")
              .Build();
                host.Run();
                
              
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }


    }
}
