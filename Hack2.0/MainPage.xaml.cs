using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Media.Capture;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Microsoft.ProjectOxford.Emotion;
//using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Common.Contract;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Hack2._0
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string APIKey = "08189c83993d4af69bf46002665ea084";
        EmotionServiceClient emotionserviceclient = new EmotionServiceClient(APIKey);
        CameraCaptureUI captureUI = new CameraCaptureUI();
        StorageFile photo;
        IRandomAccessStream imageStream;
        Emotion[] emotionresult;
        public MainPage()
        {
            this.InitializeComponent();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);
        }
        private async void btnTakePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
                if (photo == null)
                {
                    return;
                }
                else
                {
                    imageStream = await photo.OpenAsync(FileAccessMode.Read);
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(imageStream);
                    SoftwareBitmap softwarebitmap = await decoder.GetSoftwareBitmapAsync();
                    SoftwareBitmap softwarebitmapBGRB = SoftwareBitmap.Convert(softwarebitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    SoftwareBitmapSource bitmapsource = new SoftwareBitmapSource();
                    await bitmapsource.SetBitmapAsync(softwarebitmapBGRB);
                    imgEmotion.Source = bitmapsource;
                }
            }
            catch
            {
                //blEmotion.Text = "error taking Photo";
            }
        }
        private async void btnEmotions_Click(object sender, RoutedEventArgs e)
        {
            //MakeRequest("");

            emotionresult = await emotionserviceclient.RecognizeAsync(imageStream.AsStream());
            var res = emotionresult[0].Scores;

            var emotions = new Dictionary<string, float>();
            emotions.Add("Anger", res.Anger);
            emotions.Add("Contempt", res.Contempt);
            emotions.Add("Disgust", res.Disgust);
            emotions.Add("Fear", res.Fear);
            emotions.Add("Happiness", res.Happiness);
            emotions.Add("Neutral", res.Neutral);
            emotions.Add("Sadness", res.Sadness);
            emotions.Add("Suprise", res.Surprise);

            float max = 2;
            string total = string.Empty;

            foreach (var item in emotions)
            {
                if (max > item.Value)
                {
                    max = item.Value;
                    total = item.Key;
                }
            }

            txt_Result.Text = $"Result: {total}\nScore: {max}";

            var mainEmo = emotions.Max(); 
            //try
            //{
            //    emotionresult = await emotionserviceclient.RecognizeAsync(imageStream.AsStream());
            //    if (emotionresult != null)
            //    {
            //        Scores score = emotionresult[0].Scores;
            //        tblEmotion.Text = "Your Emotion are : \n" +
            //            "Happiness: " + score.Happiness + "\n" +
            //            "Sadness: " + score.Sadness + "\n" +
            //            "Surprise: " + score.Surprise + "\n" +
            //            "Neutral: " + score.Neutral + "\n" +
            //            "Anger: " + score.Anger + "\n" +
            //            "Contempt: " + score.Contempt + "\n" +
            //            "Disgust: " + score.Disgust + "\n" +
            //            "Fear: " + score.Fear + "\n";
            //    }
            //}
            //catch
            //{
            //    tblEmotion.Text = "Error Returning the emotion";
            //}
        }




        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        static async void MakeRequest(string imageFilePath)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid key.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "08189c83993d4af69bf46002665ea084"); // 

            // NOTE: You must use the same region in your REST call as you used to obtain your subscription keys.
            //   For example, if you obtained your subscription keys from westcentralus, replace "westus" in the 
            //   URI below with "westcentralus".
            string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            HttpResponseMessage response;
            string responseContent;

            // Request body. Try this sample with a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (var content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                responseContent = response.Content.ReadAsStringAsync().Result;
            }

            // A peek at the raw JSON response.
            Console.WriteLine(responseContent);

            // Processing the JSON into manageable objects.
            JToken rootToken = JArray.Parse(responseContent).First;

            // First token is always the faceRectangle identified by the API.
            JToken faceRectangleToken = rootToken.First;

            // Second token is all emotion scores.
            JToken scoresToken = rootToken.Last;

            

           
        }
    }
}