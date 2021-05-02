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
    /// Interaction logic for new_user.xaml
    /// </summary>
    public partial class new_user : Page
    {
        static string IDs_file_path = @"E:\New folder\e\cs\4th year\graduation project\Kinect_Data\IDs\Main_IDs.txt";
        public int user_id_new_user = -1;
        string IDs_file_as_a_string = System.IO.File.ReadAllText(IDs_file_path);
        public new_user()
        {
            InitializeComponent();
        }


        


private void submit_b_newUser_Click(object sender, RoutedEventArgs e)
        {
            //Enter the user data

            string name = user_name.Text;

            string drawing_hand;
            if (Left_h.IsChecked == true)
                drawing_hand = "L";
            else 
                drawing_hand = "R";


            string data_record;
            if (IDs_file_as_a_string.Length == 0)
            {
                user_id_new_user = 0;
                data_record = ";" + user_id_new_user + ":(" + name + "," + drawing_hand + ")";
                System.Console.WriteLine("Your ID is: " + user_id_new_user);
            }
            else
            {
                //Search for the last id to give user the next one

                //Data records are separated by ';'  
                int last_id_index = IDs_file_as_a_string.LastIndexOf(';') + 1;

                //Subtract the ID char from the ascii of '0' to obtain its int value
                int last_id = IDs_file_as_a_string[last_id_index] - '0';
                user_id_new_user = last_id + 1;
                System.Console.WriteLine("Your ID is: " + user_id_new_user);
                data_record = ";" + user_id_new_user + ":(" + name + "," + drawing_hand + ")";
            }

            using (StreamWriter sw = File.AppendText(IDs_file_path))
            {
                //Append data info to the main file
                sw.Write(data_record);
            }


            //Send the user_id_new_user to Kinect window. 
            Kinect_Draw newWindow = new Kinect_Draw(user_id_new_user, drawing_hand);
            newWindow.Show();

            


        }
    }
}
