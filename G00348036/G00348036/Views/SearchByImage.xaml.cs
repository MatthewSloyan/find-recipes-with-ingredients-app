﻿using Newtonsoft.Json;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace G00348036.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchByImage : ContentPage
	{
        string filePath = "";

		public SearchByImage ()
		{
			InitializeComponent ();
        }

        private void BtnTakePicture_Clicked(object sender, EventArgs e)
        {
            TakePictureAsync();
        }
        
        private void BtnSend_Clicked(object sender, EventArgs e)
        {
            SetUpAPI();
        }

        private async Task TakePictureAsync()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Sample",
                Name = "image.jpg"
            });

            if (file == null)
                return;

            await DisplayAlert("File Location", file.Path, "OK");

            PhotoImage.Source = Xamarin.Forms.ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                filePath = file.Path;
                // Set up the API call
                //SetUpAPI(file.Path);
                return stream;
            });
        }

        private void SetUpAPI()
        {
            // Convert the saved image to base64 format to send via http
            byte[] bytes = File.ReadAllBytes(filePath);
            string file = Convert.ToBase64String(bytes);
            string result = "";

            // Based on the JSON format and layout that google Vision requires build Json data using information.
            var obj = new
            {
                requests = new[] {
                    new  {
                         image = new { content  = file },
                         features = new[] {
                             new  { type = "OBJECT_LOCALIZATION", maxResults = 10}
                         }
                    }
                }
            };

            try
            {
                // To implement the HTTP request I found suitable code below but modified it and improved for my own needs.
                // https://stackoverflow.com/questions/9145667/how-to-post-json-to-a-server-using-c

                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://vision.googleapis.com/v1/images:annotate?key=AIzaSyAz2XlLIlDE4NHoCENCrliqW1Motsk8WHY");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    // Serialize the new object created above into JSON format for sending via HTTP
                    streamWriter.Write(JsonConvert.SerializeObject(obj));
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                // Recieve response from Google Vision API call
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    loadRecipesPage(result);
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Error");
            }
        }

        private void loadRecipesPage(string result)
        {
            System.Diagnostics.Debug.WriteLine(result);

            SearchByImageApiData results = JsonConvert.DeserializeObject<SearchByImageApiData>(result);

            List<Respons> response = new List<Respons>();
            response = results.responses;

            List<LocalizedObjectAnnotation> listOfDetectedIngredients = new List<LocalizedObjectAnnotation>();
            listOfDetectedIngredients = response[0].localizedObjectAnnotations;

            string dynamicString = "";
            //dynamicString = listOfDetectedIngredients[0].name;

            // Iterate through the returned api data to the limit specified by the user
            for (int i = 0; i < pckIngredients.SelectedIndex; i++)
            {
                dynamicString += listOfDetectedIngredients[i].name + "%2C";
            }       
            
            string URL = "https://spoonacular-recipe-food-nutrition-v1.p.rapidapi.com/recipes/findByIngredients?number=10&ranking=1&fillIngredients=true&ingredients=" + dynamicString;

            System.Diagnostics.Debug.WriteLine(URL);

            Navigation.PushAsync(new Recipes(URL, 1));
        }
    }
}