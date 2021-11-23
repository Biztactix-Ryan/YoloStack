using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloV4Stack
{
    class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(context =>
            {
                Console.WriteLine(context.Request.Path.ToString());
                
                switch (context.Request.Path.ToString())
                {
                    case "/v4/v1/vision/detection":
                        if (context.Request.Form.Files["image"] != null)
                        {
                            
                            var resp = ProcessImage.ProcessV4Image(new Bitmap(context.Request.Form.Files["image"].OpenReadStream()));
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(resp));

                        }
                        break;
                    case "/v5/v1/vision/detection":
                        if (context.Request.Form.Files["image"] != null)
                        {
                            var resp = ProcessImage.ProcessV5Image(new Bitmap(context.Request.Form.Files["image"].OpenReadStream()));
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonConvert.SerializeObject(resp));
                        }
                        break;
                }
                return context.Response.WriteAsync("Hello world");

            });

        }
    }
}
