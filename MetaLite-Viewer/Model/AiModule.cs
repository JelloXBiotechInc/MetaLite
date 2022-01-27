using MetaLite_Viewer.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.CodeDom;
using System.ComponentModel;
using MetaLite_Viewer.Helper;

namespace MetaLite_Viewer
{
    abstract class AIModule
    {
        protected int PixelWidth;
        protected int PixelHeight;
        protected ImageData ImageData;


        public AIModule(int width, int height, ImageData imageData)
        {
            PixelWidth = width;
            PixelHeight = height;
            ImageData = imageData;
        }
        protected virtual List<List<byte>> AIAnnotations { get; set; }
        
        public virtual void CleanAnnotations()
        {
            AIAnnotations = null;
        }

        public abstract Task GetAIAnnotations(Stream PngStream, string TempPath, string FileName, 
            string redtext, string greentext, string bluetext, string ApparentMagnification, 
            bool endOfTask);

        public abstract Task GetAIAnnotations(int AnnotationWidth, int AnnotationHeight, int TifIndex, string TempPath, string FileName,
            string redtext, string greentext, string bluetext, string ApparentMagnification,
            bool endOfTask);

        public double AICalculationTime { get; protected set; }
        public double AntibodyPercentage { get; protected set; } = double.NegativeInfinity;

        public async Task AntibodyPercentageAsync(int AnnotationWidth, int AnnotationHeight, int TifIndex, string redtext, string greentext, string bluetext, string FileDir,
            string ApparentMagnification,
            string RangeMin,
            string RangeMax,
            double Threshold,
            bool Parallel
            )
        {
            MemoryStream ms = ImageData.OriginalSizePngStream as MemoryStream;
            MemoryStream Maskms = ImageData.AnnotationMapPngStream as MemoryStream;
            using (HttpClientHandler antibodyPercentageHch = new HttpClientHandler
            {
                Proxy = null,
                UseProxy = false
            })
            using (var httpClient = new HttpClient(antibodyPercentageHch))
            using (var content = new MultipartFormDataContent())
            using (var annotationWidth = new StringContent(AnnotationWidth.ToString(), Encoding.UTF8))
            using (var annotationHeight = new StringContent(AnnotationHeight.ToString(), Encoding.UTF8))
            using (var tifIndex = new StringContent(TifIndex.ToString(), Encoding.UTF8))
            using (var annotation = HttpHelper.CreateFileContent(Maskms, "masks", "annotationImage.png", "image/png"))
            using (var fileDir = new StringContent(FileDir, Encoding.UTF8))
            using (var redinfo = new StringContent(redtext, Encoding.UTF8))
            using (var greeninfo = new StringContent(greentext, Encoding.UTF8))
            using (var blueinfo = new StringContent(bluetext, Encoding.UTF8))
            using (var ApparentMagnificationinfo = new StringContent(ApparentMagnification, Encoding.UTF8))
            using (var RangeMininfo = new StringContent(RangeMin, Encoding.UTF8))
            using (var RangeMaxinfo = new StringContent(RangeMax, Encoding.UTF8))
            using (var Thresholdinfo = new StringContent(Threshold.ToString(), Encoding.UTF8))
            {
                httpClient.Timeout = TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["JELLOX_ANTIBODY_PERCENTAGE_SERVER_TIMEOUT"], NumberFormatInfo.InvariantInfo));
                content.Add(annotationWidth, "annotationWidth");
                content.Add(annotationHeight, "annotationHeight");
                content.Add(tifIndex, "tifIndex");
                content.Add(annotation);
                content.Add(fileDir, "name_id");
                content.Add(redinfo, "red_channel");
                content.Add(greeninfo, "green_channel");
                content.Add(blueinfo, "blue_channel");
                content.Add(ApparentMagnificationinfo, "apparent_magnification");
                content.Add(RangeMininfo, "range_min");
                content.Add(RangeMaxinfo, "range_max");
                content.Add(Thresholdinfo, "threshold");
                HttpResponseMessage response;
                if (Parallel)
                    response = await httpClient.PostAsync(new Uri(ConfigurationManager.AppSettings["AI_ANTIBODY_PERCENTAGE_PARALLEL_SERVER_STRING"]), content).ConfigureAwait(true);
                else
                    response = await httpClient.PostAsync(new Uri(ConfigurationManager.AppSettings["AI_ANTIBODY_PERCENTAGE_SERVER_STRING"]), content).ConfigureAwait(true);
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                CellMask ReturnJsonObject = JsonConvert.DeserializeObject<CellMask[]>(result)[0];
                AntibodyPercentage = ReturnJsonObject.Percentage;
                AIAnnotations = ReturnJsonObject.CellArea;
                
                await Resource.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (ReturnJsonObject.CellArea != null)
                    {
                        DrawAIAnnotations("Antibody Area");
                    }
                    
                    
                }));
            }
        }

        public async Task PluginFunctionPost(FunctionData functionData)
        {
            MemoryStream ms = ImageData.OriginalSizePngStream as MemoryStream;
            MemoryStream Maskms = ImageData.AnnotationMapPngStream as MemoryStream;
            
            using (HttpClientHandler antibodyPercentageHch = new HttpClientHandler
            {
                Proxy = null,
                UseProxy = false
            })
            using (var httpClient = new HttpClient(antibodyPercentageHch))
            using (var content = new MultipartFormDataContent())

            using (var filecontent = HttpHelper.CreateFileContent(ms, "images", "Image.png", "image/png"))
            using (var Annotation = HttpHelper.CreateFileContent(Maskms, "masks", "annotationImage.png", "image/png"))
            using (var filenameonfo = new StringContent("a", Encoding.UTF8))
            using (var redinfo = new StringContent("b", Encoding.UTF8))
            using (var greeninfo = new StringContent("s", Encoding.UTF8))
            using (var blueinfo = new StringContent("e", Encoding.UTF8))
            using (var fileDir = new StringContent(ImageData.Dir, Encoding.UTF8))
            using (var tifIndex = new StringContent(ImageData.TifIndex.ToString(), Encoding.UTF8))
            using (var annotationWidth = new StringContent(ImageData.AnnotationWidth.ToString(), Encoding.UTF8))
            using (var annotationHeight = new StringContent(ImageData.AnnotationHeight.ToString(), Encoding.UTF8))
            {
                httpClient.Timeout = TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["JELLOX_ANTIBODY_PERCENTAGE_SERVER_TIMEOUT"], NumberFormatInfo.InvariantInfo));

                content.Add(fileDir, "name_id");
                if (functionData.IsSendImage)
                { 
                    content.Add(filecontent);
                    
                }
                if (functionData.IsSendMask)
                    content.Add(Annotation);


                content.Add(annotationWidth, "annotationWidth");
                content.Add(annotationHeight, "annotationHeight");
                content.Add(tifIndex, "tifIndex");
                content.Add(new StringContent(functionData.FunctionName, Encoding.UTF8), "FunctionName");
                foreach (var col in functionData.Content)
                {
                    if (col.parameterType == nameof(String))
                    {
                        content.Add(new StringContent(col.datas[0].value as string, Encoding.UTF8), col.parameterName);
                    }
                    else if (col.parameterType == "RangeInt32")
                    {
                        content.Add(new StringContent(col.range.Min.ToString(), Encoding.UTF8), col.parameterName + ".Min");
                        content.Add(new StringContent(col.range.Max.ToString(), Encoding.UTF8), col.parameterName + ".Max");
                        content.Add(new StringContent(col.range.From.ToString(), Encoding.UTF8), col.parameterName + ".From");
                        content.Add(new StringContent(col.range.To.ToString(), Encoding.UTF8), col.parameterName + ".To");
                    }
                    else if (col.parameterType == "Boolean")
                    {
                        content.Add(new StringContent(col.boolean.ToString(), Encoding.UTF8), col.parameterName);
                    }
                    else if (col.parameterType == "ComboBox")
                    {
                        for (int i = 0; i < col.datas.Count; i++)
                        {
                            content.Add(new StringContent(col.datas[i].value as string, Encoding.UTF8), col.parameterName + "."+col.datas[i].varName);
                        }
                    }
                    else if (col.parameterType == nameof(Slider) + "Int32")
                    {
                        if (col.auto)
                        {
                            col.range.From = -1;
                        }
                        content.Add(new StringContent(col.range.From.ToString(), Encoding.UTF8), col.parameterName);
                    }
                }
                var response = await httpClient.PostAsync(new Uri(functionData.PostUri), content).ConfigureAwait(true);
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                CellMask ReturnJsonObject = JsonConvert.DeserializeObject<CellMask[]>(result)[0];
                ImageData.ObjectCounting = ReturnJsonObject.Percentage;
                AIAnnotations = ReturnJsonObject.CellArea;

                await Resource.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (ReturnJsonObject.CellArea != null)
                    {
                        DrawAIAnnotations("Object Area");
                    }


                }));
            }
        }

        public string result { get; set; }

        protected virtual void DrawAIAnnotations()
        {
            int stride = PixelWidth * ((ImageData.ResizedImage.Format.BitsPerPixel) / 8);
            byte[] pixels = new byte[PixelHeight * stride];
            ImageData.AddAnnotationLayer("Advanced AIModel Result", ColorHelper.HIGHLIGHTER);
            ImageData.AnnotationLayers.Last().Layer.CopyPixels(pixels, stride, 0);
            var color = ColorHelper.HIGHLIGHTER;

            unsafe
            {
                Parallel.For(0, PixelHeight, y =>
                {
                    int yTmp = (int)(y * ImageData.AnnotationScale);

                    for (int x = 0; x < stride; x += 4)
                    {
                        int yXstride = x + y * stride;
                        if (AIAnnotations[yTmp][(int)(x * ImageData.AnnotationScale / 4)] != 0)
                        {
                            pixels[yXstride] = 0;
                            pixels[yXstride + 1] = 0;
                            pixels[yXstride + 2] = 0;
                            pixels[yXstride + 3] = color.A;
                        }

                    }
                });
            }

            ImageData.AnnotationLayers.Last().Layer.WritePixels(new Int32Rect(0, 0, PixelWidth, PixelHeight), pixels, stride, 0);
        }

        protected virtual void DrawAIAnnotations(string AnnotaionName)
        {
            int stride = PixelWidth * (32 / 8);
            byte[] pixels = new byte[PixelHeight * stride];
            ImageData.AddAnnotationLayer(AnnotaionName, ColorHelper.HIGHLIGHTER);
            ImageData.AnnotationLayers.Last().Layer.CopyPixels(pixels, stride, 0);
            var color = ColorHelper.HIGHLIGHTER;

            unsafe
            {
                Parallel.For(0, PixelHeight, y =>
                {
                    int yTmp = (int)(y * ImageData.AnnotationScale);

                    for (int x = 0; x < stride; x += 4)
                    {
                        int yXstride = x + y * stride;
                        if (AIAnnotations[yTmp][(int)(x * ImageData.AnnotationScale / 4)] != 0)
                        {
                            pixels[yXstride] = 0;
                            pixels[yXstride + 1] = 0;
                            pixels[yXstride + 2] = 0;
                            pixels[yXstride + 3] = color.A;
                        }

                    }
                });
            }

            ImageData.AnnotationLayers.Last().Layer.WritePixels(new Int32Rect(0, 0, PixelWidth, PixelHeight), pixels, stride, 0);

        }

        public virtual void DrawAIAnnotations(Helper.Image JsonAnnotation)
        {   
            int stride = PixelWidth * ((ImageData.ResizedImage.Format.BitsPerPixel) / 8);
            byte[] pixels = new byte[PixelHeight * stride];
            ImageData.AddAnnotationLayer(JsonAnnotation.imageName, ColorHelper.HIGHLIGHTER);
            ImageData.AnnotationLayers.Last().Layer.CopyPixels(pixels, stride, 0);
            Color color;
            if (JsonAnnotation.colorString != null)
            {
                var colorBrush = ColorHelper.XamlStringColor(JsonAnnotation.colorString);
                ImageData.AnnotationLayers.Last().AnnotationColorBrush = colorBrush;
                color = colorBrush.Color;
            }
            else
            {
                color = ColorHelper.HIGHLIGHTER;
            }            

            unsafe
            {
                Parallel.For(0, PixelHeight, y =>
                {
                    for (int x = 0; x < stride; x += 4)
                    {
                        int yXstride = x + y * stride;
                        if (JsonAnnotation.value[y][(int)(x / 4)] != 0)
                        {
                            pixels[yXstride] = 0;
                            pixels[yXstride + 1] = 0;
                            pixels[yXstride + 2] = 0;
                            pixels[yXstride + 3] = color.A;
                        }

                    }
                });
            }
            
            ImageData.AnnotationLayers.Last().Layer.WritePixels(new Int32Rect(0, 0, PixelWidth, PixelHeight), pixels, stride, 0);
            Resource.MainWindow.DrawContour();
        }
    }
}
