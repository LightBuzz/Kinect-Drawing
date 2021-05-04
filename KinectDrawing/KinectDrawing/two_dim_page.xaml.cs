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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectDrawing
{
    /// <summary>
    /// Interaction logic for two_dim_page.xaml
    /// </summary>
    public partial class two_dim_page : Page
    {
       
        public two_dim_page()
        {
            InitializeComponent();
        }

        private void primitive_shapes_fn(object sender, RoutedEventArgs e)
        {
            int selected1 = (App.Current as App).data_traveling1;
            string selected2 = (App.Current as App).data_traveling2;

            if (selected2 == "" || selected1 == 0)
            {
                MessageBox.Show("user not selected", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("user selected with id = " + selected1 + selected2, "Alert", MessageBoxButton.OK, MessageBoxImage.Information) ;
                Kinect_Draw kd = new Kinect_Draw(selected1,selected2);
                kd.Show();
            }

        }

        private void sketches_fn(object sender, RoutedEventArgs e)
        {
            int selected1 = (App.Current as App).data_traveling1;
            string selected2 = (App.Current as App).data_traveling2;

            if (selected2 == "" || selected1 == 0)
            {
                MessageBox.Show("user not selected", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("user selected with id = " + selected1 + selected2, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                Kinect_Draw kd = new Kinect_Draw(selected1, selected2);
                kd.Show();
            }

        }
        private void go_to_help_page(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("help_user_page.xaml", UriKind.Relative);
            this.NavigationService.Navigate(uri);

        }
    }
    
    }
