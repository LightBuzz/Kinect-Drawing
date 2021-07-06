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
    /// Interaction logic for help_user_page.xaml
    /// </summary>
    public partial class help_user_page : Page
    {
        public help_user_page()
        {
            InitializeComponent();
        }

        private void choose_help(object sender, RoutedEventArgs e)
        {
            if (help_choice.SelectedItem == primitive)
            {
                // Console.WriteLine("do nothing");
                help_container.Content = new primitive_help();
            }
            else if (help_choice.SelectedItem == sketches)
            {
                //Console.WriteLine("bardo m4 hn3ml 7aga ");
                help_container.Content = new sketches_help();
            }
            else if (help_choice.SelectedItem == learn_draw)
            {
                Console.WriteLine("wreny b2a hrsm ezay ");
                help_container.Content = new how_to_draw();
            }
            else {
                MessageBox.Show("not current help selected", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }
    }
}
