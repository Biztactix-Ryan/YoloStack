using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using YoloStack.Contracts;
using YoloStack.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace YoloStack
{
    public class ProcessImage
    {
        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };
        static PredictionEngine<YoloV4BitmapData, YoloV4Prediction> V4predictionEngine;
        static PredictionEngine<YoloV5BitmapData, YoloV5Prediction> V5predictionEngine;
        public static int? GPUID = null;
        public static DeepstackResponse ProcessV4Image(Bitmap image)
        {

            MLContext mlContext = new MLContext();

            if (V4predictionEngine == null)
            {
                try
                {
                    // YoloV4 Pipeline
                    const string modelPath = @"Assets\yolov4.onnx";
                    var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                        .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                        .Append(mlContext.Transforms.ApplyOnnxModel(
                            shapeDictionary: new Dictionary<string, int[]>()
                            {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                            },
                            inputColumnNames: new[]
                            {
                        "input_1:0"
                            },
                            outputColumnNames: new[]
                            {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                            },
                            modelFile: modelPath, recursionLimit: 100,gpuDeviceId:GPUID));


                    // Fit on empty list to obtain input data schema
                    var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

                    // Create prediction engine
                    V4predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                // save model
                //mlContext.Model.Save(model, predictionEngine.OutputSchema, Path.ChangeExtension(modelPath, "zip"));
            }

            try
            {
                
                // predict                      
                var predict = V4predictionEngine.Predict(new YoloV4BitmapData() { Image = image });
                var results = predict.GetResults(classesNames, 0.3f, 0.5f);

                //    using (var g = Graphics.FromImage(image))
                //    {
                //        foreach (var res in results)
                //        {
                //            // draw predictions
                //            var x1 = res.BBox[0];
                //            var y1 = res.BBox[1];
                //            var x2 = res.BBox[2];
                //            var y2 = res.BBox[3];
                //            g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                //            using (var brushes = new SolidBrush(Color.FromArgb(50, Color.Red)))
                //            {
                //                g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                //            }

                //            g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                //                         new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));
                //        }
                //        bitmap.Save(Path.Combine(imageOutputFolder, Path.ChangeExtension(RealimageName, "_processed" + Path.GetExtension(RealimageName))));
                //    }
                //}
                var Predictions = new List<DeepstackResponse.Predictions>();
                foreach (var result in results)
                {
                    Predictions.Add(new DeepstackResponse.Predictions() { confidence = result.Confidence , label = result.Label, x_min = (int)result.BBox[0], x_max = (int)result.BBox[2], y_min = (int)result.BBox[1], y_max = (int)result.BBox[3] });
                }
                return new DeepstackResponse() { success = true, predictions = Predictions.ToArray() };
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return new DeepstackResponse() { success = false };
        }


        public static DeepstackResponse ProcessV5Image(Bitmap image)
        {
            MLContext mlContext = new MLContext();
            if (V5predictionEngine == null)
            {
                try
                {
                    // YoloV5 Pipeline
                    const string modelPath = @"Assets\yolov5.onnx";
                    var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "images", imageWidth: 640, imageHeight: 640, resizing: ResizingKind.Fill)
                        .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "images", scaleImage: 1f / 255f, interleavePixelColors: false))
                        .Append(mlContext.Transforms.ApplyOnnxModel(
                            shapeDictionary: new Dictionary<string, int[]>()
                            {
                    { "images", new[] { 1, 3, 640, 640 } },
                    { "output", new[] { 1, 25200, 85 } },
                            },
                            inputColumnNames: new[]
                            {
                    "images"
                            },
                            outputColumnNames: new[]
                            {
                    "output"
                            },
                            modelFile: modelPath,
                            recursionLimit: 100, gpuDeviceId: GPUID));

                    var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV5BitmapData>()));

                    // Create prediction engine
                    V5predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV5BitmapData, YoloV5Prediction>(model);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            // save model
            //mlContext.Model.Save(model, predictionEngine.OutputSchema, Path.ChangeExtension(modelPath, "zip"));


            try
            {

                // predict                      
                var predict = V5predictionEngine.Predict(new YoloV5BitmapData() { Image = image });
                var results = predict.GetResults(classesNames, 0.3f, 0.5f);
                var Predictions = new List<DeepstackResponse.Predictions>();
                foreach (var result in results)
                {
                    Predictions.Add(new DeepstackResponse.Predictions() { confidence = result.Confidence , label = result.Label, x_min = (int)result.BBox[0], x_max = (int)result.BBox[2], y_min = (int)result.BBox[1], y_max = (int)result.BBox[3] });
                }
                return new DeepstackResponse() { success = true, predictions = Predictions.ToArray() };
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return new DeepstackResponse() { success = false };
        }
    }
}

