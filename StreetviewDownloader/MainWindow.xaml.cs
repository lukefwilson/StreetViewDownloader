﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace StreetviewDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<int, string> keyboardPanoLinkList = new Dictionary<int, string>();

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }


        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (panoIdTextBox.IsFocused)
            {
                return;
            }

            switch (e.Key){
                case Key.NumPad1:
                case Key.D1:
                    thumbnailKeyPressed(1);
                    break;
                case Key.NumPad2:
                case Key.D2:
                    thumbnailKeyPressed(2);
                    break;
                case Key.NumPad3:
                case Key.D3:
                    thumbnailKeyPressed(3);
                    break;
                case Key.NumPad4:
                case Key.D4:
                    thumbnailKeyPressed(4);
                    break;
                case Key.NumPad5:
                case Key.D5:
                    thumbnailKeyPressed(5);
                    break;
                case Key.NumPad6:
                case Key.D6:
                    thumbnailKeyPressed(6);
                    break;
            }
        }

        private void thumbnailKeyPressed(int key)
        {
            if (keyboardPanoLinkList.ContainsKey(key))
            {
                string newPanoId = string.Empty;
                keyboardPanoLinkList.TryGetValue(key, out newPanoId);
                panoIdTextBox.Text = newPanoId;
                DownloadBigPano(newPanoId);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            locationLabel.Content = "Loading...";
            string panoId = panoIdTextBox.Text;

            //This should be in a thread
            DownloadBigPano(panoId);

            
            //Thread downloaderThread = new Thread(() => DownloadBigPano(panoId));
            //downloaderThread.Start();
        }

        private void DownloadBigPano(string panoId)
        {
            //Download the pano
            string cachePathBase = @"C:\StreetSwoop\cache\";

            //Set up downloader...
            Downloader.Downloader imageDownloader = new Downloader.Downloader(cachePathBase);

            //Download XML or Load from cache
            if (!System.IO.File.Exists(cachePathBase + panoId + @"\" + panoId + ".xml"))
            {
                if (!System.IO.Directory.Exists(cachePathBase + panoId))
                {
                    System.IO.Directory.CreateDirectory(cachePathBase + panoId);
                }
                imageDownloader.Download("http://cbk0.google.com/cbk?output=xml&panoid=" + panoId, cachePathBase + panoId + @"\" + panoId + ".xml");
            }

            XmlSerializer serializer = new XmlSerializer(typeof(panorama));
            StreamReader reader = new StreamReader(cachePathBase + panoId + @"\" + panoId + ".xml");
            panorama panoObject = (panorama)serializer.Deserialize(reader);
            reader.Close();

            if (panoObject.data_properties == null)
            {
                locationLabel.Content = "Error: Panorama has invalid XML data...";
                return;
            }
            else
            {
                DisplayThumbnails(panoObject);
            }

            locationLabel.Content = GetPanoramaLocationText(panoObject);

            int zoomLevel = 3; //This can be edited
            if (panoObject.data_properties.image_width < 3330 || zoomLevel <= 3)
            {
                zoomLevel = 3; //We're dealing with a low res panorama here
            }

            System.Drawing.Image image = imageDownloader.GetFullImage(panoId.ToString(), zoomLevel);
            mainImage.Source = new BitmapImage(new Uri(@"C:\StreetSwoop\cache\" + panoId + @"\3Complete.jpg", UriKind.RelativeOrAbsolute));

            //mainImage.Source = ImageConverter(image);
        }

        private string GetPanoramaLocationText(panorama panoObject)
        {
            string location = string.Empty;
            
            string country = panoObject.data_properties.country ?? string.Empty;
            string region = panoObject.data_properties.region ?? string.Empty;
            string text = panoObject.data_properties.text ?? string.Empty;
            string copyright = panoObject.data_properties.copyright ?? string.Empty;

            location = text + ", " + region + ", " + country + " " + copyright;

            return location;
        }

        private void DisplayThumbnails(panorama panoObject)
        {
            // Clear previous items
            thumbnails.Children.Clear();
            keyboardPanoLinkList.Clear();

            //Set up downloader...
            string cachePathBase = @"C:\StreetSwoop\cache\";
            Downloader.Downloader imageDownloader = new Downloader.Downloader(cachePathBase);

            int thumbnailKey = 1;

            // Get all the thumbnails
            foreach (var annotation in panoObject.annotation_properties)
            {
                var button = new System.Windows.Controls.Button();
                var thumbImg = new System.Windows.Controls.Image();

                var thumbnail = imageDownloader.GetThumbnail(annotation.pano_id);
                thumbImg.Source = ImageConverter(thumbnail);
                thumbImg.Width = 100;
                //Img.Height = thumbnail.Height;

                button.Content = thumbImg;
                button.Click += (object sender, RoutedEventArgs e) =>
                {
                    panoIdTextBox.Text = annotation.pano_id;
                    DownloadBigPano(annotation.pano_id);
                };
                thumbnails.Children.Add(button);

                keyboardPanoLinkList.Add(thumbnailKey++, annotation.pano_id);
            }


        }



        private ImageSource ImageConverter(System.Drawing.Image inputImage) {
            MemoryStream ms = new MemoryStream();
            inputImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            return bi;
        }

        public static string zero(int number)
        {
            if (number < 10)
            {
                return "0" + number;
            }

            return number.ToString();
        }

        public static string fiveZero(int number)
        {
            if (number < 10)
            {
                return "0000" + number;
            }
            if (number < 100)
            {
                return "000" + number;
            }
            if (number < 1000)
            {
                return "00" + number;
            }
            if (number < 10000)
            {
                return "0" + number;
            }

            return number.ToString();
        }

        public static string RemoveInvalidFilePathCharacters(string filename, string replaceChar)
        {
            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(filename, replaceChar);
        }

        public static int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        public static decimal mod(decimal x, decimal m)
        {
            decimal r = x % m;
            return r < 0 ? r + m : r;
        }

    }
}
