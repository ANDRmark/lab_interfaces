using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeatherInformer.WeatherClient.Models.CurrentWeather;
using WeatherInformer.WeatherClient.Models.Forecast5days3hours;

namespace WeatherInformer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Grid CurrentPanel { get; set; }
        Grid HiddenRectangle { get; set; }

        List<StackPanel> panels { get; set; } = new List<StackPanel>();
        CurrentWeather CurrentWeather { get; set; }
        ForecastBy3h ForecastBy3h { get; set; }
        WeatherClient.WeatherClient weatherClient { get; set; } = new WeatherClient.WeatherClient();
        DateTime lastUpdate = DateTime.MinValue;
        int currentPanelNumber { get; set; } = 0;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Forward_click(object sender, RoutedEventArgs e)
        {
            if (panels.Count > 0 && currentPanelNumber + 1 <= panels.Count - 1)
            {
                currentPanelNumber += 1;
                var currentPanel = panels[currentPanelNumber];
                var pannelToHide = panels[currentPanelNumber - 1];
                ContainerGrid.Children.Add(currentPanel);


                MatrixAnimationUsingPath transformMatrix1 = CreatematrixAnimationTransform(ContainerGrid.ActualWidth,0);
                MatrixAnimationUsingPath transformMatrix2 = CreatematrixAnimationTransform(0, -ContainerGrid.ActualWidth);
                transformMatrix2.Completed += (obj, arg) =>
                {
                    ContainerGrid.Children.RemoveAt(0);
                };

                StartAnimation(currentPanel, pannelToHide, transformMatrix1, transformMatrix2);
            }
        }

        MatrixAnimationUsingPath CreatematrixAnimationTransform(double startXOffset, double endXOffset)
        {
            MatrixAnimationUsingPath transformMatrix = new MatrixAnimationUsingPath();
            transformMatrix.PathGeometry = new PathGeometry();
            transformMatrix.PathGeometry.AddGeometry(new PathGeometry(new[] { new PathFigure(new Point(startXOffset, 0), new[] { new LineSegment(new Point(endXOffset, 0), true) }, false) }));
            transformMatrix.Duration = new Duration(TimeSpan.FromSeconds(0.15));
            return transformMatrix;
        }

        private void Button_Backward_click(object sender, RoutedEventArgs e)
        {
            if(panels.Count > 0 && currentPanelNumber-1 >= 0)
            {
                currentPanelNumber -= 1;
                var currentPanel = panels[currentPanelNumber];
                var pannelToHide = panels[currentPanelNumber + 1];
                ContainerGrid.Children.Add(currentPanel);


                MatrixAnimationUsingPath transformMatrix1 = CreatematrixAnimationTransform(-ContainerGrid.ActualWidth, 0);
                MatrixAnimationUsingPath transformMatrix2 = CreatematrixAnimationTransform(0, ContainerGrid.ActualWidth);
                transformMatrix2.Completed += (obj, arg) =>
                {
                    ContainerGrid.Children.RemoveAt(0);
                };

                StartAnimation(currentPanel, pannelToHide, transformMatrix1, transformMatrix2);
            }
        }

        private void StartAnimation(StackPanel currentPanel, StackPanel pannelToHide, MatrixAnimationUsingPath transformMatrix1, MatrixAnimationUsingPath transformMatrix2)
        {
            Storyboard.SetTarget(transformMatrix1, currentPanel);
            Storyboard.SetTargetProperty(transformMatrix1,
                new PropertyPath("RenderTransform.Matrix"));

            Storyboard.SetTarget(transformMatrix2, pannelToHide);
            Storyboard.SetTargetProperty(transformMatrix2,
                new PropertyPath("RenderTransform.Matrix"));

            Storyboard myStoryboard = new Storyboard();
            myStoryboard.Children.Add(transformMatrix1);
            myStoryboard.Children.Add(transformMatrix2);
            myStoryboard.Begin(this);
        }

        private void SwapRectangles()
        {
            var rect1 = CurrentPanel;
            CurrentPanel = HiddenRectangle;
            HiddenRectangle = rect1;
        }


        private void CreateCurrentWeatherPanel()
        {
            if (CurrentWeather == null) return;

            StackPanel currentWeatherPanel = new StackPanel();
            DockPanel dp = new DockPanel();
            StackPanel sp = new StackPanel();

            Label TitleLabel = new Label();
            TitleLabel.Content = "Now";
            TitleLabel.VerticalAlignment = VerticalAlignment.Top;
            TitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            TitleLabel.Margin = new Thickness(10, 0, 10, 0);
            TitleLabel.FontSize = 20;
            currentWeatherPanel.Children.Add(TitleLabel);

            var labelsTexts = new List<string>
            {
                "Country: " + CurrentWeather?.sys?.country,
                "City: " + CurrentWeather?.name,
                "Weather: " + CurrentWeather?.weather?[0]?.main + "; " + CurrentWeather?.weather?[0]?.description,
                "Temperature: " + CurrentWeather?.main?.temp + "°C",
                "Pressure: " + CurrentWeather?.main?.pressure + "mm",
                "Humidity: " + CurrentWeather?.main?.humidity + "%"
            };

            if (CurrentWeather?.wind?.speed != null)
            {
                labelsTexts.Add("Wind speed: " + CurrentWeather?.wind?.speed + "km/h");
            }

            if (CurrentWeather?.wind?.deg != null)
            {
                labelsTexts.Add("Wind direction: " + CurrentWeather?.wind?.deg + "°");
            }

            if (CurrentWeather?.clouds?.all != null)
            {
                labelsTexts.Add("Clouds: " + CurrentWeather?.clouds?.all + "%");
            }

            foreach (var labelText in labelsTexts)
            {
                Label label = CreateLabel(labelText, new Thickness(10, 0, 10, 0));
                sp.Children.Add(label);
            }
            dp.Children.Add(sp);

            if(CurrentWeather?.weather?[0]?.icon != null)
            {
                Image img = new Image();
                img.Source = new BitmapImage(new Uri(@"http://openweathermap.org/img/w/"+ CurrentWeather?.weather?[0]?.icon + ".png"));
                img.MaxWidth = 50;
                img.MinWidth = 50;
                img.VerticalAlignment = VerticalAlignment.Top;
                img.HorizontalAlignment = HorizontalAlignment.Left;
                dp.Children.Add(img);
            }

            currentWeatherPanel.Children.Add(dp);
            panels.Add(currentWeatherPanel);
        }

        private Label CreateLabel(string text, Thickness margin)
        {
            Label Label = new Label();
            Label.Content = text;
            Label.VerticalAlignment = VerticalAlignment.Top;
            Label.HorizontalAlignment = HorizontalAlignment.Left;
            Label.Margin = margin;
            return Label;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private void CreateForecastWeatherPanels()
        {
            if (ForecastBy3h?.list == null ) return;

            foreach(var forecast in ForecastBy3h?.list)
            {
                StackPanel forecastPanel = new StackPanel();
                DockPanel dp = new DockPanel();
                StackPanel sp = new StackPanel();

                Label TitleLabel = new Label();
                TitleLabel.Content = "Forecast for " + UnixTimeStampToDateTime(forecast.dt).ToShortDateString() +"  " + UnixTimeStampToDateTime(forecast.dt).ToShortTimeString();
                TitleLabel.VerticalAlignment = VerticalAlignment.Top;
                TitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
                TitleLabel.Margin = new Thickness(10,0,10,0);
                TitleLabel.FontSize = 20;
                forecastPanel.Children.Add(TitleLabel);

                var labelsTexts = new List<string>
            {
                "Country: " + ForecastBy3h?.city?.country,
                "City: " + ForecastBy3h?.city?.name,
                "Weather: " + forecast?.weather?[0]?.main + "; " + forecast?.weather?[0]?.description,
                "Temperature: " + forecast?.main?.temp + "°C",
                "Pressure: " + forecast?.main?.pressure + "mm",
                "Humidity: " + forecast?.main?.humidity + "%"
            };

                if (forecast?.wind?.speed != null)
                {
                    labelsTexts.Add("Wind speed: " + forecast?.wind?.speed + "km/h");
                }

                if (forecast?.wind?.deg != null)
                {
                    labelsTexts.Add("Wind direction: " + forecast?.wind?.deg + "°");
                }

                if (forecast?.clouds?.all != null)
                {
                    labelsTexts.Add("Clouds: " + forecast?.clouds?.all + "%");
                }

                foreach (var labelText in labelsTexts)
                {
                    Label label = CreateLabel(labelText, new Thickness(10, 0, 10, 0));
                    sp.Children.Add(label);
                }
                dp.Children.Add(sp);

                if (forecast?.weather?[0]?.icon != null)
                {
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(@"http://openweathermap.org/img/w/" + forecast?.weather?[0]?.icon + ".png"));
                    img.MaxWidth = 50;
                    img.MinWidth = 50;
                    img.VerticalAlignment = VerticalAlignment.Top;
                    img.HorizontalAlignment = HorizontalAlignment.Left;
                    dp.Children.Add(img);
                }

                forecastPanel.Children.Add(dp);
                panels.Add(forecastPanel);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private async Task Refresh()
        {
            if ((DateTime.Now - lastUpdate) > new TimeSpan(0, 0, 5))
            {
                await GetNewData();
                ReDrawPanels();
                LastUpdatedLabel.Content = "Last updated: " + DateTime.Now;
                lastUpdate = DateTime.Now;
            }
        }


        private async Task GetNewData()
        {
            var location = "Kyiv,ua";
            this.CurrentWeather = await weatherClient.GetCurrentWeatherAsync(location);
            this.ForecastBy3h = await weatherClient.GetForecastAsync(location,5);
        }

        private void ReDrawPanels()
        {
            ContainerGrid.Children.Clear();
            panels.Clear();
            CreateCurrentWeatherPanel();
            CreateForecastWeatherPanels();
            currentPanelNumber = 0;
            ContainerGrid.Children.Add(panels[currentPanelNumber]);
        }
    }
}
