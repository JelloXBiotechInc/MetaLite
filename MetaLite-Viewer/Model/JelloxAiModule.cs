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
using System.ComponentModel;
using Windows.UI.Notifications;
using System.Windows.Threading;

namespace MetaLite_Viewer
{
    class JelloxAIModule : AIModule
    {

        protected override List<List<byte>> AIAnnotations { get; set; }

        public JelloxAIModule(int width, int height, ImageData imageData) : base(width, height, imageData)
        {
            ModuleName = "";
        }

        private string ModuleName { get; set; }

        public JelloxAIModule(int width, int height, ImageData imageData, string moduleName) : base(width, height, imageData)
        {
            ModuleName = moduleName;
        }

        public override void CleanAnnotations()
        {
            AIAnnotations = null;
        }

        public override async Task GetAIAnnotations(Stream PngStream, string TempPath, string FileName,
            string redtext, string greentext, string bluetext, string ApparentMagnification,
            bool EndOfTask)
        {
            
        }

        public override async Task GetAIAnnotations(int AnnotationWidth, int AnnotationHeight, int TifIndex, string TempPath, string FileDir,
            string redtext, string greentext, string bluetext, string ApparentMagnification,
            bool EndOfTask)
        {
            try
            {
                using (HttpClientHandler tumorPredictionHch = new HttpClientHandler
                {
                    Proxy = null,
                    UseProxy = false
                })
                using (var httpClient = new HttpClient(tumorPredictionHch))
                using (var content = new MultipartFormDataContent())
                using (var annotationWidth = new StringContent(AnnotationWidth.ToString(), Encoding.UTF8))
                using (var annotationHeight = new StringContent(AnnotationHeight.ToString(), Encoding.UTF8))
                using (var tifIndex = new StringContent(TifIndex.ToString(), Encoding.UTF8))
                using (var redinfo = new StringContent(redtext, Encoding.UTF8))
                using (var greeninfo = new StringContent(greentext, Encoding.UTF8))
                using (var blueinfo = new StringContent(bluetext, Encoding.UTF8))
                using (var ApparentMagnificationinfo = new StringContent(ApparentMagnification, Encoding.UTF8))
                using (var fileDir = new StringContent(FileDir, Encoding.UTF8))
                using (var tempfileinfo = new StringContent(TempPath, Encoding.UTF8))
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["JELLOX_AI_ANNOTATING_SERVER_TIMEOUT"], NumberFormatInfo.InvariantInfo));
                    content.Add(annotationWidth, "annotationWidth");
                    content.Add(annotationHeight, "annotationHeight");
                    content.Add(tifIndex, "tifIndex");
                    content.Add(redinfo, "red_channel");
                    content.Add(greeninfo, "green_channel");
                    content.Add(blueinfo, "blue_channel");
                    content.Add(ApparentMagnificationinfo, "apparent_magnification");
                    content.Add(fileDir, "name_id");
                    content.Add(tempfileinfo, "temp_path");

                    var response = await httpClient.PostAsync(new Uri(ConfigurationManager.AppSettings[ModuleName]), content).ConfigureAwait(true);
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                    var tmpJson = JsonConvert.DeserializeObject<List<List<List<byte>>>>(result)[0];
                    if (tmpJson[0].Count < 1)
                    {
                        Resource.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MetaLite_Viewer.Subwindow.messagebox.Show("AI inference error, please try restart MetaLite analyzer", "Exception occure", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                        return;
                    }
                    AIAnnotations = tmpJson;
                   
                    await Resource.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DrawAIAnnotations();

                    }));

                    if (EndOfTask)
                    {
                        Resource.MainWindow.Dispatcher.BeginInvoke(new Action(() => { Resource.MainWindow.ResetProgressBar(); }));
                        Resource.MainWindow.statusTextBlock.Text = "Finish";
                    }

                    AICalculationTime = 0;
                }
            }
            catch (Exception)
            {

            }
        }


        protected override void DrawAIAnnotations()
        {
            int stride = PixelWidth * 4;
            byte[] pixels = new byte[PixelHeight * stride];
            ImageData.AddAnnotationLayer("Advanced AIModel Result", ColorHelper.HIGHLIGHTER);
            ImageData.AnnotationLayers.Last().Layer.CopyPixels(pixels, stride, 0);
            var color = ColorHelper.HIGHLIGHTER;
            
            unsafe
            {
                Parallel.For(0, PixelHeight, y =>
                {
                    int yTmp = y;

                    for (int x = 0; x < stride; x += 4)
                    {
                        int yXstride = x + y * stride;
                        if (AIAnnotations[yTmp][(int)(x / 4)] != 0)
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
    }
}
