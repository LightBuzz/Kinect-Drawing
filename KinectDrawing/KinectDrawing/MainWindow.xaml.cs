
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
using System.Windows.Navigation;


namespace KinectDrawing
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }




        private void Main_f_Navigated(object sender, NavigationEventArgs e)
        {

        }




        private void new_Click(object sender, RoutedEventArgs e)
        {

            Main_f.Content = new new_user();

            //Button source = e.Source as Button;
            //if (source != null)
            //{
            //    source.Visibility = Visibility.Hidden;
            //}


        }

        private void old_click(object sender, RoutedEventArgs e)
        {

            Main_f.Content = new old_user();

            //Button source = e.Source as Button;
            //if (source != null)
            //{
            //    source.Visibility = Visibility.Hidden;
            //}


        }

        private void two_dim(object sender, RoutedEventArgs e)
        {

            Main_f.Content = new two_dim_page();

            //Button source = e.Source as Button;
            //if (source != null)
            //{
            //    source.Visibility = Visibility.Hidden;
            //}


        }
        private void three_dim(object sender, RoutedEventArgs e)
        {

            Main_f.Content = new three_dim_page();

            //Button source = e.Source as Button;
            //if (source != null)
            //{
            //    source.Visibility = Visibility.Hidden;
            //}


        }
    }
}