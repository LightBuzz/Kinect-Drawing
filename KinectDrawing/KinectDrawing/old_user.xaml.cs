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

using System.IO;
using System.Text;

namespace KinectDrawing
{
    /// <summary>
    /// Interaction logic for old_user.xaml
    /// </summary>
    public partial class old_user : Page
    {
        static string IDs_file_path = @"C:\Users\khale\OneDrive\Kinect_Data\IDs\Main_IDs.txt";
        public int user_id_old_user = -1;
        string IDs_file_as_a_string = System.IO.File.ReadAllText(IDs_file_path);
        string drawing_hand;
        

        public old_user()
        {
            InitializeComponent();
        }

        private void search_id_Click(object sender, RoutedEventArgs e)
        {
            string IDs_file_as_a_string = System.IO.File.ReadAllText(IDs_file_path);
            scroll_text_box_ids.Text = IDs_file_as_a_string.Replace(';', '\n');

            
            

        }

        private void open_kinect_Click(object sender, RoutedEventArgs e)
        {
            //user_id is the textbox used in the GUI 
            user_id_old_user = Int32.Parse(user_id.Text);

            //to obtain the used hand from the file:
            int index_of_user_id = IDs_file_as_a_string.IndexOf(user_id_old_user.ToString());
            string IDs_file_substring = IDs_file_as_a_string.Substring(index_of_user_id);
            int hand_used_index = IDs_file_substring.IndexOf(',') + 1;
            drawing_hand = IDs_file_substring[hand_used_index].ToString();

            //Console.WriteLine(drawing_hand);
            //Console.WriteLine(index_of_user_id);



            Kinect_Draw kd = new Kinect_Draw(user_id_old_user, drawing_hand);
            kd.Show();
        }
    }
}
