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
        Grid CurrentRectangle { get; set; }
        Grid HiddenRectangle { get; set; }
        CurrentWeather CurrentWeather { get; set; }
        ForecastBy3h ForecastBy3h { get; set; }
        WeatherClient.WeatherClient weatherClient { get; set; }

        DateTime lastUpdate = DateTime.MinValue;


        public MainWindow()
        {
            InitializeComponent();

            CurrentRectangle = grid1;
            HiddenRectangle = grid2;

            //this.WindowTitle = "Storyboards Example";
            //StackPanel myStackPanel = new StackPanel();
            //myStackPanel.Margin = new Thickness(20);

            //Rectangle myRectangle = new Rectangle();
            //myRectangle.Name = "MyRectangle";

            //// Create a name scope for the page.
            //NameScope.SetNameScope(this, new NameScope());

            //this.RegisterName(myRectangle.Name, myRectangle);
            //myRectangle.Width = 100;
            //myRectangle.Height = 100;
            //SolidColorBrush mySolidColorBrush = new SolidColorBrush(Colors.Blue);
            //this.RegisterName("MySolidColorBrush", mySolidColorBrush);
            //myRectangle.Fill = mySolidColorBrush;

            //DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            //myDoubleAnimation.From = -20;
            //myDoubleAnimation.To = 10;
            //myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            //Storyboard.SetTargetName(myDoubleAnimation, myRectangle.Name);
            //Storyboard.SetTargetProperty(myDoubleAnimation,
            //    new PropertyPath(Rectangle.WidthProperty));

            //ColorAnimation myColorAnimation = new ColorAnimation();
            //myColorAnimation.From = Colors.Blue;
            //myColorAnimation.To = Colors.Red;
            //myColorAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            //Storyboard.SetTargetName(myColorAnimation, "MySolidColorBrush");
            //Storyboard.SetTargetProperty(myColorAnimation,
            //    new PropertyPath(SolidColorBrush.ColorProperty));
            //Storyboard myStoryboard = new Storyboard();
            //myStoryboard.Children.Add(myDoubleAnimation);
            //myStoryboard.Children.Add(myColorAnimation);

            //myRectangle.MouseEnter += delegate (object sender, MouseEventArgs e)
            //{
            //    myStoryboard.Begin(this);
            //};

            //myStackPanel.Children.Add(myRectangle);
            //this.Content = myStackPanel;



            //var tt = new TranslateTransform();
            //rect2.RenderTransform = tt;

            weatherClient = new WeatherClient.WeatherClient();
        }

        private void Button_Forward_click(object sender, RoutedEventArgs e)
        {
            //DoubleAnimation transformAnimation = new DoubleAnimation();
            //transformAnimation.From = this.ActualWidth;
            //transformAnimation.To = 0;
            //transformAnimation.Duration = new Duration(TimeSpan.FromSeconds(2));
            //transformAnimation.Completed += (obj, arg) =>
            //{
            //    SwapRectangles();
            //};
            //Storyboard.SetTargetName(transformAnimation, HiddenRectangle.Name);
            //Storyboard.SetTargetProperty(transformAnimation,
            //    new PropertyPath("RenderTransform.Children[3].X"));

            //Storyboard myStoryboard = new Storyboard();
            //myStoryboard.Children.Add(transformAnimation);
            //myStoryboard.Begin(this);
            //////////////

            MatrixAnimationUsingPath transformMatrix = new MatrixAnimationUsingPath();
            transformMatrix.PathGeometry = new PathGeometry();
            transformMatrix.PathGeometry.AddGeometry(new PathGeometry(new []{ new PathFigure(new Point(0,0),new  []{ new LineSegment(new Point(50,0),true)},false)}));
            transformMatrix.Duration = new Duration(TimeSpan.FromSeconds(2));
            transformMatrix.Completed += (obj, arg) =>
            {
                //SwapRectangles();
            };

            Storyboard.SetTargetName(transformMatrix, HiddenRectangle.Name);
            Storyboard.SetTargetProperty(transformMatrix,
                new PropertyPath("RenderTransform.Matrix"));

            Storyboard myStoryboard = new Storyboard();
            myStoryboard.Children.Add(transformMatrix);
            myStoryboard.Begin(this);

            //HiddenRectangle.RenderTransform.SetValue(MatrixTransform.MatrixProperty, new Matrix(1,0,0,1,50,0));
        }

        private void Button_Backward_click(object sender, RoutedEventArgs e)
        {
            CreateCurrentWeatherPanel();
        }

        private void SwapRectangles()
        {
            var rect1 = CurrentRectangle;
            CurrentRectangle = HiddenRectangle;
            HiddenRectangle = rect1;
        }


        private void CreateElements()
        {
            Grid currentWeatherGrid = new Grid();
            Label l1 = new Label();
            //HorizontalAlignment = "Left" Margin = "235,35,0,0" VerticalAlignment = "Top" Width = "96"
            l1.Content = "Currentweather";
            l1.VerticalAlignment = VerticalAlignment.Top;
            l1.HorizontalAlignment = HorizontalAlignment.Left;
            currentWeatherGrid.Children.Add(l1);
            MainGrid.Children.Add(currentWeatherGrid);
        }

        private void CreateCurrentWeatherPanel()
        {
            Grid currentWeatherGrid = new Grid();
            DockPanel dp = new DockPanel();
            StackPanel sp = new StackPanel();

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

            currentWeatherGrid.Children.Add(dp);
            ContainerGrid.Children.Add(currentWeatherGrid);
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

        private void CreateForecastWeatherPanels()
        {

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
        }

        private void ReDrawPanels()
        {
            ContainerGrid.Children.Clear();
            CreateCurrentWeatherPanel();
        }
    }
}
