using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace KinectDrawing
{
    /// <summary>
    /// Interaction logic for UserEntry.xaml
    /// </summary>
    /// 
    public partial class Kinect_Draw_Complex : Window
    {



        int user_id;
        string hand_used;
        int main_counter;
        int user_counter;

        bool left_lasso_gesture_handling;
        bool right_lasso_gesture_handling;

        const string DATA_PATH = @"C:\Users\khale\OneDrive\Kinect_Data\Data";

        private KinectSensor _sensor = null;
        private ColorFrameReader _colorReader = null;
        private BodyFrameReader _bodyReader = null;
        private IList<Body> _bodies = null;

        private int _width = 0;
        private int _height = 0;
        private byte[] _pixels = null;
        private WriteableBitmap _bitmap = null;


        public Kinect_Draw_Complex(int user_id_passed, string hand_used_passed)
        {

            InitializeComponent();

            left_lasso_gesture_handling = false;
            right_lasso_gesture_handling = false;

            this.user_id = user_id_passed;
            this.hand_used = hand_used_passed;




            //Indexing from (1)
            main_counter = 0;
            user_counter = 0;



            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _width = _sensor.ColorFrameSource.FrameDescription.Width;
                _height = _sensor.ColorFrameSource.FrameDescription.Height;

                _colorReader = _sensor.ColorFrameSource.OpenReader();
                _colorReader.FrameArrived += ColorReader_FrameArrived;

                _bodyReader = _sensor.BodyFrameSource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                _pixels = new byte[_width * _height * 4];
                _bitmap = new WriteableBitmap(_width, _height, 96.0, 96.0, PixelFormats.Bgra32, null);

                _bodies = new Body[_sensor.BodyFrameSource.BodyCount];

                camera.Source = _bitmap;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_colorReader != null)
            {
                _colorReader.Dispose();
            }

            if (_bodyReader != null)
            {
                _bodyReader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        private void ColorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.CopyConvertedFrameDataToArray(_pixels, ColorImageFormat.Bgra);

                    _bitmap.Lock();
                    Marshal.Copy(_pixels, 0, _bitmap.BackBuffer, _pixels.Length);
                    _bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
                    _bitmap.Unlock();
                }
            }
        }

        private Polyline generate_new_trail()
        {
            Polyline trail2 = new Polyline();
            trail2.Stroke = Brushes.Red;
            trail2.StrokeThickness = 5;
            BlurEffect blur = new BlurEffect();
            blur.Radius = 10;
            trail2.Effect = blur;
            return trail2;

        }

        private DrawingBrush generate_new_brush()
        {
            DrawingBrush db = new DrawingBrush();
            db.Width = 300;
            db.Height = 300;
            db.Name = "brush";

            return db;
        }

        private int check_last_user_counter(int user_id, string path)
        {
            string[] filePaths = Directory.GetFiles(path);

            int id_from_folder;
            bool id_found = false;
            int user_counter;
            int user_last_counter = -1;

            foreach (string f in filePaths)
            {
                //Obtain the user_id and user_counter from file path
                string file_name_with_ex = f.Substring(f.LastIndexOf('\\') + 1);
                string[] file_name_with_ex_arr = file_name_with_ex.Split('.');
                string file_name = file_name_with_ex_arr[0];
                string[] id_arr = file_name.Split('_');


                id_from_folder = Int32.Parse(id_arr[0]);
                user_counter = Int32.Parse(id_arr[1]);


                //obtain the max counter from the folder
                if (id_from_folder == user_id)
                {
                    if (user_counter > user_last_counter)
                        user_last_counter = user_counter;

                    id_found = true;
                }
            }

            if (id_found)
                return user_last_counter;
            else
                return 0;

        }


        //Async predicting 
        public async void RunProcessAsync(string filePath)
        {
            var script = @"C:\Users\khale\Source\Repos\Kinect-Drawing\Python_part\GP_2\Complex_script.py";
            var coord = filePath;
            var fileName = @"C:\Users\khale\AppData\Local\Programs\Python\Python37\python.exe";

            using (
            var process = new Process
            {
                StartInfo =
                    {
                        FileName = fileName, Arguments = $"\"{script}\" \"{coord}\"" ,
                        UseShellExecute = false, CreateNoWindow = true,
                        RedirectStandardOutput = true, RedirectStandardError = true
                    },
                EnableRaisingEvents = true
            }
            )

            {
                await RunProcessAsync(process).ConfigureAwait(false);
            }
        }
        private Task<int> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.Exited += (s, ea) => tcs.SetResult(process.ExitCode);
            process.OutputDataReceived += (s, ea) =>
            {
                if (ea.Data != null)
                    Application.Current.Dispatcher.Invoke(new Action(() => { shape_counter_l.Content = ea.Data; }));
            };
            process.ErrorDataReceived += (s, ea) => Console.WriteLine("ERR: " + ea.Data);



            bool started = process.Start();
            if (!started)
            {
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        //Sync predicting
        //void run_py_Csharp(string file_path)
        //{
        //    Console.WriteLine("Execute python process...");
        //    // 1) Create Process Info
        //    var psi = new ProcessStartInfo();
        //    psi.FileName = @"C:\Users\khale\AppData\Local\Programs\Python\Python37\python.exe";

        //    // 2) Provide script and arguments
        //    var script = @"C:\Users\khale\Source\Repos\Kinect-Drawing\Python_part\GP_2\Primitive_script.py";
        //    var coord = file_path;
        //    //var coord = @"C:\Users\khale\OneDrive\Desktop\GP_2\6_1.txt";


        //    psi.Arguments = $"\"{script}\" \"{coord}\"";

        //    // 3) Process configuration
        //    psi.UseShellExecute = false;
        //    psi.CreateNoWindow = true;
        //    psi.RedirectStandardOutput = true;
        //    psi.RedirectStandardError = true;

        //    // 4) Execute process and get output
        //    var errors = "";


        //    using (var process = Process.Start(psi))
        //    {
        //        errors = process.StandardError.ReadToEnd();
        //        python_results = process.StandardOutput.ReadToEnd();
        //    }

        //    //// 5) Display output
        //    //Console.WriteLine("ERRORS:");
        //    //Console.WriteLine(errors);
        //    //Console.WriteLine();
        //    //Console.WriteLine("Results:");
        //    Console.WriteLine("python_results: " + python_results);

        //    //Console.WriteLine();
        //}





        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {


            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(_bodies);

                    Body body = _bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (body != null)
                    {
                        Joint handRight = body.Joints[JointType.HandRight];
                        Joint handLeft = body.Joints[JointType.HandLeft];

                        //if (body.HandRightState == HandState.Closed || body.HandRightState == HandState.Open)

                        //Check the hand used for drawing and customize the project based on it.
                        if (hand_used == "R")
                        {


                            CameraSpacePoint handRightPosition = handRight.Position;
                            ColorSpacePoint handRightPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(handRightPosition);

                            float x = handRightPoint.X;
                            float y = handRightPoint.Y;

                            if (!float.IsInfinity(x) && !float.IsInfinity(y))
                            {



                                // DRAW!
                                if (body.HandRightState == HandState.Closed)
                                {
                                    left_lasso_gesture_handling = false;

                                    redraw_l.Content = "";

                                    trail.Points.Add(new Point { X = x, Y = y });
                                }


                                //Create a new trail to avoid connecting the last and first points issue
                                if (body.HandRightState == HandState.Open)
                                {


                                    Polyline trail2 = generate_new_trail();


                                    //Check if trail is not empty to avoid creating too many empty trials.
                                    if (trail.Points.Count() != 0)
                                    {
                                        trail = trail2;
                                        canvas.Children.Add(trail);
                                    }
                                }




                                //Save to a file
                                if (body.HandLeftState == HandState.Lasso)
                                {
                                    //This condition helps us in avoiding to enter this lasso event until another gesture is made 
                                    //that means that for each lasso gesture only one file will be saved
                                    if (left_lasso_gesture_handling == false)
                                    {

                                        // to show that you'll get an enumerable of rectangles.
                                        IEnumerable<Polyline> trails = canvas.Children.OfType<Polyline>();
                                        //In order to save the drawing into a file the drawing must be consist of only one ployline(for drawing primitive shapes) -but it's ok for sketches to be drawned using more than one polyline-
                                        //that's why we will check if the canvas has only 2 trails(the one containg the drawing and the other one is empty)
                                        //and we need  to check also that the second polyline is empty.
                                        bool empty_trail = false;
                                        foreach (var trial in trails)
                                        {
                                            //Console.WriteLine("-----> " + trail.Points.Count());
                                            if (trial.Points.Count == 0)
                                                empty_trail = true;
                                        }



                                        if (trails.Count() >= 2 && empty_trail)
                                        {
                                            //the main_counter will increase each time a file is created

                                            main_counter += 1;
                                            //After 25 Drawings each 5 for one of the 5 shapes the program will print DONE!
                                            //if (main_counter == 25)
                                            //    done_l.Content = "BRAVOO DONE!";

                                            shape_counter_l.Content = "Predicting....";

                                            left_lasso_gesture_handling = true;

                                            string shape_path = DATA_PATH;

                                            //Collecting 5  Shapes(5 drawings for each one)
                                            //user_counter is used to obtain the number of saved files in each shape folder for a certain user
                                            //  User is required to draw 5 drawings for each shape(5 shapes) and that's  the main_counter job

                                            //    if (main_counter < 6)
                                            //    {
                                            //        shape_path += "\\s1";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;

                                            //    }
                                            //    else if (main_counter > 5 && main_counter < 11)
                                            //    {
                                            //        shape_path += "\\s2";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;
                                            //        shape_l.Content = "Shape = S2";
                                            //    }
                                            //    else if (main_counter > 10 && main_counter < 16)
                                            //    {
                                            //        shape_path += "\\s3";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;
                                            //        shape_l.Content = "Shape = S3";
                                            //    }
                                            //    else if (main_counter > 15 && main_counter < 21)
                                            //    {
                                            //        shape_path += "\\s4";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;
                                            //        shape_l.Content = "Shape = S4";
                                            //    }
                                            //    else if (main_counter > 20 && main_counter < 26)
                                            //    {
                                            //        shape_path += "\\s5";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;
                                            //        shape_l.Content = "Shape = S5";

                                            //    }
                                            //    else
                                            //    {
                                            //        shape_path += "\\trash";
                                            //        shape_l.Content = "Shape = trash";
                                            //    }



                                            //Collecting only one shape:
                                            string shape_folder_name = "Trash";
                                            shape_path += "\\" + shape_folder_name;
                                            user_counter = check_last_user_counter(user_id, shape_path) + 1;



                                            string fileName = user_id + "_" + user_counter + ".txt";
                                            string filePath = shape_path + '\\' + fileName;
                                            Console.WriteLine(filePath);

                                            //Console.WriteLine("$ " + filePath);
                                            if (!File.Exists(filePath))
                                            {

                                                using (FileStream fs = File.Create(filePath))
                                                {

                                                    foreach (var trial in trails)
                                                    {




                                                        //Write the trials into the generated file
                                                        if (trial.Points.Count != 0)
                                                        {

                                                            Byte[] coordinates = new UTF8Encoding(true).GetBytes("Shape:\n" + trial.Points.ToString() + "\nEnd\n");
                                                            fs.Write(coordinates, 0, coordinates.Length);
                                                        }


                                                        //Console.WriteLine("___________________________________");

                                                    }


                                                    //Clear After writing into the file
                                                    canvas.Children.Clear();
                                                    brush = generate_new_brush();
                                                    canvas.Children.Add(brush);
                                                    trail = generate_new_trail();
                                                    canvas.Children.Add(trail);
                                                }

                                                //async Predicting part:
                                                //Send the file created to the python Script
                                                RunProcessAsync(filePath);

                                            }
                                            else
                                                Console.WriteLine("Error in storing");

                                        }
                                       
                                    }
                                }


                                //Clear
                                if (body.HandLeftState == HandState.Closed)
                                {


                                    canvas.Children.Clear();
                                    brush = generate_new_brush();
                                    canvas.Children.Add(brush);
                                    trail = generate_new_trail();
                                    canvas.Children.Add(trail);


                                }

                                //Changing color
                                if (body.HandRightState == HandState.Open)
                                {
                                    left_lasso_gesture_handling = false;

                                    if (trail.Stroke == Brushes.Red)
                                        trail.Stroke = Brushes.Green;
                                    else if (trail.Stroke == Brushes.Green)
                                        trail.Stroke = Brushes.Blue;
                                    else
                                        trail.Stroke = Brushes.Red;
                                }

                                Canvas.SetLeft(brush, x - brush.Width / 2.0);
                                Canvas.SetTop(brush, y - brush.Height);
                            }
                        }

                        //if (body.HandLeftState == HandState.Closed || body.HandLeftState == HandState.Open)
                        else if (hand_used == "L")
                        {

                            CameraSpacePoint handLeftPosition = handLeft.Position;
                            ColorSpacePoint handLeftPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(handLeftPosition);

                            float x = handLeftPoint.X;
                            float y = handLeftPoint.Y;

                            if (!float.IsInfinity(x) && !float.IsInfinity(y))
                            {
                                // DRAW!
                                if (body.HandLeftState == HandState.Closed)
                                {
                                    right_lasso_gesture_handling = false;

                                    redraw_l.Content = "";

                                    trail.Points.Add(new Point { X = x, Y = y });
                                }


                                //Create a new trail to avoid connecting the last and first points issue
                                if (body.HandLeftState == HandState.Open)
                                {

                                    Polyline trail2 = generate_new_trail();


                                    //Check if trail is not empty to avoid creating too many empty trials.
                                    if (trail.Points.Count() != 0)
                                    {
                                        trail = trail2;
                                        canvas.Children.Add(trail);
                                    }
                                }

                                //Save to a file
                                if (body.HandRightState == HandState.Lasso)
                                {
                                    //This condition helps us in avoiding to enter this lasso event until another gesture is made 
                                    //that means that for each lasso gesture only one file will be saved
                                    if (right_lasso_gesture_handling == false)
                                    {

                                        // to show that you'll get an enumerable of rectangles.
                                        IEnumerable<Polyline> trails = canvas.Children.OfType<Polyline>();
                                        //In order to save the drawing into a file the drawing must be consist of only one ployline(for drawing primitive shapes) -but it's ok for sketches to be drawned using more than one polyline-
                                        //that's why we will check if the canvas has only 2 trails(the one containg the drawing and the other one is empty)
                                        //and we need  to check also that the second polyline is empty.
                                        bool empty_trail = false;
                                        foreach (var trial in trails)
                                        {
                                            //Console.WriteLine("-----> " + trail.Points.Count());
                                            if (trial.Points.Count == 0)
                                                empty_trail = true;
                                        }



                                        if (trails.Count() >= 2 && empty_trail)
                                        {
                                            //the main_counter will increase each time a file is created

                                            main_counter += 1;
                                            //After 25 Drawings each 5 for one of the 5 shapes the program will print DONE!
                                            //if (main_counter == 25)
                                            //    done_l.Content = "BRAVOO DONE!";

                                            shape_counter_l.Content = "Predicting....";

                                            right_lasso_gesture_handling = true;

                                            string shape_path = DATA_PATH;

                                            //Collecting 5  Shapes(5 drawings for each one)
                                            //user_counter is used to obtain the number of saved files in each shape folder for a certain user
                                            //  User is required to draw 5 drawings for each shape(5 shapes) and that's  the main_counter job

                                            //    if (main_counter < 6)
                                            //    {
                                            //        shape_path += "\\s1";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;

                                            //    }
                                            //    else if (main_counter > 5 && main_counter < 11)
                                            //    {
                                            //        shape_path += "\\s2";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;
                                            //        shape_l.Content = "Shape = S2";
                                            //    }
                                            //    else if (main_counter > 10 && main_counter < 16)
                                            //    {
                                            //        shape_path += "\\s3";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;
                                            //        shape_l.Content = "Shape = S3";
                                            //    }
                                            //    else if (main_counter > 15 && main_counter < 21)
                                            //    {
                                            //        shape_path += "\\s4";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;
                                            //        shape_l.Content = "Shape = S4";
                                            //    }
                                            //    else if (main_counter > 20 && main_counter < 26)
                                            //    {
                                            //        shape_path += "\\s5";
                                            //        user_counter = check_last_user_counter(user_id, shape_path) + 1;
                                            //        shape_l.Content = "Shape = S5";

                                            //    }
                                            //    else
                                            //    {
                                            //        shape_path += "\\trash";
                                            //        shape_l.Content = "Shape = trash";
                                            //    }



                                            //Collecting only one shape:
                                            string shape_folder_name = "Trash";
                                            shape_path += "\\" + shape_folder_name;
                                            user_counter = check_last_user_counter(user_id, shape_path) + 1;



                                            string fileName = user_id + "_" + user_counter + ".txt";
                                            string filePath = shape_path + '\\' + fileName;
                                            Console.WriteLine(filePath);

                                            //Console.WriteLine("$ " + filePath);
                                            if (!File.Exists(filePath))
                                            {

                                                using (FileStream fs = File.Create(filePath))
                                                {

                                                    foreach (var trial in trails)
                                                    {




                                                        //Write the trials into the generated file
                                                        if (trial.Points.Count != 0)
                                                        {

                                                            Byte[] coordinates = new UTF8Encoding(true).GetBytes("Shape:\n" + trial.Points.ToString() + "\nEnd\n");
                                                            fs.Write(coordinates, 0, coordinates.Length);
                                                        }


                                                        //Console.WriteLine("___________________________________");

                                                    }


                                                    //Clear After writing into the file
                                                    canvas.Children.Clear();
                                                    brush = generate_new_brush();
                                                    canvas.Children.Add(brush);
                                                    trail = generate_new_trail();
                                                    canvas.Children.Add(trail);
                                                }

                                                //async Predicting part:
                                                //Send the file created to the python Script
                                                RunProcessAsync(filePath);

                                            }
                                            else
                                                Console.WriteLine("Error in storing");

                                        }
                                        
                                    }
                                }




                                //Clear
                                if (body.HandRightState == HandState.Closed)
                                {


                                    canvas.Children.Clear();
                                    brush = generate_new_brush();
                                    canvas.Children.Add(brush);
                                    trail = generate_new_trail();
                                    canvas.Children.Add(trail);


                                }

                                //Changing color
                                if (body.HandLeftState == HandState.Open)
                                {
                                    right_lasso_gesture_handling = false;

                                    if (trail.Stroke == Brushes.Red)
                                        trail.Stroke = Brushes.Green;
                                    else if (trail.Stroke == Brushes.Green)
                                        trail.Stroke = Brushes.Blue;
                                    else
                                        trail.Stroke = Brushes.Red;
                                }


                                Canvas.SetLeft(brush, x - brush.Width / 2.0);
                                Canvas.SetTop(brush, y - brush.Height);
                            }
                        }
                    }
                }
            }
        }


    }


}


