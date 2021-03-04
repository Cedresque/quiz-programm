﻿using System;
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
using MySql.Data.MySqlClient;

namespace WpfApplication1
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static SolidColorBrush blue = new SolidColorBrush(Colors.LightBlue);
        private static SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush green = new SolidColorBrush(Colors.Green);
        private static SolidColorBrush light_green = new SolidColorBrush(Colors.LightGreen);
        private static SolidColorBrush light_red = new SolidColorBrush(Colors.LightCoral);
        private static SolidColorBrush white = new SolidColorBrush(Colors.White);
        private static SolidColorBrush black = new SolidColorBrush(Colors.Black);
        private string m_strMySQLConnectionString = "server=localhost;userid=root;password=secret;database=quiz_test";
        private static int question_buffer_size = 7;
        private string[] question_buffer = new string[question_buffer_size];
        private int score = 0;
        private int correct = 3;
        private int question_count = 0;
        private int current_question = 0;
        private int[] question_id_bucket;

        public MainWindow()
        {
            InitializeComponent();
            initGame();
     
        }

        private void initDatabase()
        {
            try
            {
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                string[] data = System.IO.File.ReadAllLines(directory + "/user.txt");
                m_strMySQLConnectionString = "server=localhost;userid=" + data[0] + ";password=" + data[1] + ";database=quiz_test";
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void initGame()
        {
            initField();
            getCount();
            question_count = Math.Min(question_count, 10);
            generateQuestions();
            loadNextQuestion();
        }

        private void displayFinish()
        {
            string finish_text = "Du bist fertig!\n Du hast " + score + " Punkte erreicht!";
            Finish.Text = finish_text;
            Finish.Visibility = Visibility.Visible;
            Restart.Visibility = Visibility.Visible;
            Continue.Visibility = Visibility.Hidden;
        }

        private void generateQuestions()
        {
            question_id_bucket = new int[question_count];
            for (int i = 0; i < question_count; i++)
            {
                question_id_bucket[i] = i+1;
            }
        }

        private void getCount()
        {
            string command = "select count(id) from fragen";
            using (var sqlconn = new MySqlConnection(m_strMySQLConnectionString))
            using (var cmd = new MySqlCommand(command, sqlconn))
            {
                try
                {
                    sqlconn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        rd.Read();
                        question_count = rd.GetInt32(0);
                        rd.Close();
                    }
                    sqlconn.Close();
                }
                catch (MySqlException e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }
        
        private void getQuestion(int question_id)
        {
            string command = "select * from fragen where id="+question_id.ToString();
            using (var sqlconn = new MySqlConnection(m_strMySQLConnectionString))
            using (var cmd = new MySqlCommand(command, sqlconn))
            {
                try
                {
                    sqlconn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        rd.Read();
                        for (int i = 0; i < question_buffer_size; i++)
                        {
                            question_buffer[i] = rd.GetString(i);
                        }

                        rd.Close();
                    }
                    sqlconn.Close();
                }
                catch (MySqlException e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        private Button selectButton(int button_id)
        {
            switch (button_id)
            {
                case 1: return Antwort_A;
                case 2: return Antwort_B;
                case 3: return Antwort_C;
                case 4: return Antwort_D;
                default: return null;
            }
        }

       private void Antwort_A_Click(object sender, RoutedEventArgs e)
        {
            Antwort_Clicked(1);
        }

        private void Antwort_B_Click(object sender, RoutedEventArgs e)
        {
            Antwort_Clicked(2);
        }

        private void Antwort_C_Click(object sender, RoutedEventArgs e)
        {
            Antwort_Clicked(3);
        }

        private void Antwort_D_Click(object sender, RoutedEventArgs e)
        {
            Antwort_Clicked(4);
        }

        private int Antwort_Clicked(int answer)
        {
            if (!isAnswered())
            {
                Continue.Visibility = Visibility.Visible;
                if (isCorrect(answer))
                {
                    Background = light_green;
                    selectButton(answer).Background = green;
                    score += 100;
                    Score.Content = "Score: " + score.ToString();
                    return 0;
                }
                else
                {
                    Background = light_red;
                    selectButton(answer).Background = red;
                    selectButton(correct).Background = green; 
                    return 1;
                }
            }
            else
            {
                return 2;
            }
        }

        private bool isCorrect(int number) {
            if(number == correct)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private bool isAnswered()
        {
            if (Continue.Visibility == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            loadNextQuestion();
        }

        private void initField()
        {
            Button[] alle_Btn = { Antwort_A, Antwort_B, Antwort_C, Antwort_D };
            foreach (Button btn in alle_Btn)
            {
                btn.FontSize = 18;
                btn.Background = blue;
                btn.BorderBrush = black;
            }
            Frage.FontSize = 18;
            Frage.Background = blue;
            Frage.BorderBrush = black;
            Frage.HorizontalContentAlignment = HorizontalAlignment.Center;
            Frage.VerticalContentAlignment = VerticalAlignment.Center;
            Frage.IsReadOnly = true;
            Score.Content = "Score: " + score.ToString();
            Continue.Visibility = Visibility.Hidden;
            Continue.Content = "Fortfahren";
            Finish.Visibility = Visibility.Hidden;
            Finish.FontSize = 40;
            Finish.Background = blue;
            Finish.BorderBrush = black;
            Finish.IsReadOnly = true;
            Finish.HorizontalContentAlignment = HorizontalAlignment.Center;
            Finish.VerticalContentAlignment = VerticalAlignment.Center;
            Restart.Visibility = Visibility.Hidden;
            Restart.Content = "Neue Runde";
            current_question = 0;
            score = 0;
        }

        private void loadNextQuestion()
        {
            if (current_question < question_count)
            {
                getQuestion(question_id_bucket[current_question]);

                Background = white;

                correct = Convert.ToInt32(question_buffer[5]);

                for (int button_id = 1; button_id < 5; button_id++)
                {
                    selectButton(button_id).Background = blue;
                    selectButton(button_id).Content = question_buffer[button_id];
                }

                Frage.Background = blue;
                Frage.Text = question_buffer[6];

                Continue.Visibility = Visibility.Hidden;
                current_question += 1;
            }else
            {
                displayFinish();
            }
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            initGame();
        }
    }
}
