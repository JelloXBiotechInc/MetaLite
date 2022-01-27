using MetaLite_Viewer.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MetaLite_Viewer
{
    class HHImageData : ImageData
    {
        private List<Annotation> aiAnnotations;
        private List<Annotation> AiAnnotations
        {
            get { return aiAnnotations; }
            set
            {
                this.aiAnnotations = value;
                if (this.aiAnnotations != null)
                {
                    foreach (var annotation in this.aiAnnotations)
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
                    this.aiAnnotations.RemoveAll(anno => anno.X > PixelWidth || anno.Y > PixelHeight);
                }
            }
        }

        public HHImageData(FileStream stream) : base(stream)
        {
        }

        public override void DeleteAnnotations()
        {
            AiAnnotations = null;
            AnnotationBitmap.Clear(Colors.Transparent);
        }

        public override async Task GetAiAnnotations()
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
                    content.Add(file);
                    var response = await httpClient.PostAsync(new Uri(ConfigurationManager.AppSettings["HH_AI_ANNOTATING_SERVER_STRING"]), content).ConfigureAwait(true);
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    var annotationsString = JsonConvert.DeserializeObject<JObject>(result)["data"][0]["information"].ToString();
                    AiCalculationTime = (long.Parse(JsonConvert.DeserializeObject<JObject>(result)["data"][0]["timestamp_end"].ToString()) - long.Parse(JsonConvert.DeserializeObject<JObject>(result)["data"][0]["timestamp_start"].ToString())) / 1000.0;
                    var annotations = JsonConvert.DeserializeObject<List<Annotation>>(annotationsString);
                    AiAnnotations = annotations;
                }
                DrawAiAnnotations();
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void DrawAiAnnotations()
        {
            using (AnnotationBitmap.GetBitmapContext())
            {
                foreach (var annotation in AiAnnotations)
                {
                    AnnotationBitmap.FillRectangle(annotation.X, annotation.Y, annotation.X + annotation.Width, annotation.Y + annotation.Height, ColorHelper.GetColorByProbability(annotation.Softmax));
                }
            }
        }
    }
}
