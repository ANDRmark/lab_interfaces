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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WeatherInformer.WeatherClient.Models.Location;

namespace WeatherInformer
{
    /// <summary>
    /// Interaction logic for LocationSelector.xaml
    /// </summary>
    public partial class LocationSelector : Window
    {
        Action<long> locationselected;
        List<Location> data;
        public LocationSelector(Action<long> locationselected, List<Location> data)
        {
            InitializeComponent();
            this.locationselected = locationselected;
            this.data = data;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(this.SearchResults.SelectedItem != null)
            {
                locationselected(((Location)this.SearchResults.SelectedItem).id);
            }
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string name = NameSearchTextBox.Text;
            string country = CountrySearchTextBox.Text;
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(country))
            {
                return;
            }

            List<Location> filtered;

            filtered = this.data.Where(l => l.country.ToLower().Contains(country)).Where(l => l.name.ToLower().Contains(name)).ToList();

            if (filtered.Count < 1000)
            {
                this.SearchResults.ItemsSource = filtered;
            }
        }
    }
}
