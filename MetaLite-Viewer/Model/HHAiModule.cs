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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace MetaLite_Viewer
{
    class HHAIModule : AIModule
    {
        private List<Annotation> aIAnnotations;
        private List<Annotation> AIAnnotations
        {
            get { return aIAnnotations; }
            set
            {
                this.aIAnnotations = value;
                if (this.aIAnnotations != null)
                {
                    foreach (var annotation in this.aIAnnotations)
                    {
                        if (annotation.X + annotation.Width > PixelWidth)
                        {
                            annotation.Width = (PixelWidth - (annotation.X + annotation.Width)) > 0 ? annotation.Width : PixelWidth - annotation.X - 1;
                        }
                        if (annotation.Y + annotation.Height > PixelHeight)
                        {
                            annotation.Height = (PixelHeight - (annotation.Y + annotation.Height)) > 0 ? annotation.Height : PixelHeight - annotation.Y - 1;
                        }
                    }
                    this.aIAnnotations.RemoveAll(anno => anno.X > PixelWidth || anno.Y > PixelHeight);
                }
            }
        }

        public HHAIModule(int width, int height, ImageData imageData) : base(width, height, imageData)
        {
        }

        public override void CleanAnnotations()
        {
            AIAnnotations = null;
        }

        public override async Task GetAIAnnotations(Stream PngStream, string TempPath, string FileName,
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
                using (var file = HttpHelper.CreateFileContent(PngStream as MemoryStream, "target", "originalImage.png", "image/png"))
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["HH_AI_ANNOTATING_SERVER_TIMEOUT"], NumberFormatInfo.InvariantInfo));
                    content.Add(file);
                    var response = await httpClient.PostAsync(new Uri(ConfigurationManager.AppSettings["HH_AI_ANNOTATING_SERVER_STRING"]), content).ConfigureAwait(true);
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    
                    var annotationsString = JsonConvert.DeserializeObject<JObject>(result)["data"][0]["information"].ToString();
                    
                    AICalculationTime = (long.Parse(JsonConvert.DeserializeObject<JObject>(result)["data"][0]["timestamp_end"].ToString(), NumberFormatInfo.InvariantInfo) - long.Parse(JsonConvert.DeserializeObject<JObject>(result)["data"][0]["timestamp_start"].ToString(), NumberFormatInfo.InvariantInfo)) / 1000.0;
                    
                    var annotations = JsonConvert.DeserializeObject<List<Annotation>>(annotationsString);
                    AIAnnotations = annotations;
                }
                await Resource.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    DrawAIAnnotations();
                    if (EndOfTask)
                    {
                        Resource.MainWindow.ResetProgressBar();
                        Resource.MainWindow.statusTextBlock.Text = "Finish";
                    }
                }));
                
            }
            catch (Exception)
            {

                throw;
            }
        }
        public override async Task GetAIAnnotations(int AnnotationWidth, int AnnotationHeight, int TifIndex, string TempPath, string FileName,
            string redtext, string greentext, string bluetext, string ApparentMagnification,
            bool EndOfTask)
        { }
                
        protected override void DrawAIAnnotations()
        {
            Resource.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                ImageData.AddAnnotationLayer("Basic AIModel Result", ColorHelper.HIGHLIGHTER);
                using (ImageData.AnnotationLayers.Last().Layer.GetBitmapContext())
                {
                    foreach (var annotation in AIAnnotations)
                    {
                        ImageData.AnnotationLayers.Last().Layer.FillRectangle(annotation.X, annotation.Y, annotation.X + annotation.Width, annotation.Y + annotation.Height, ColorHelper.HIGHLIGHTER);
                    }

                }
            }));
        }
    }
}
