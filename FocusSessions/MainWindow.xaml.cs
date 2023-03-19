using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FocusSessions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DateTime focustimeend;
        TimeSpan focustimetimer;
        TimeSpan focustimePauseDuration;
        DateTime breaktimeend;
        TimeSpan breaktimetimer;
        Timer timer;
        bool focusTimerSubscribed;
        bool focusTimerHasEnded;
        string focusFileName, breakFileName;
        public MainWindow()
        {
            InitializeComponent();
            timer = new Timer(1000);
            timer.Elapsed += TimerElapsed;
            timer.Start();
            statusBarText.Text = "No sounds loaded yet.";
        }

        private void FocusSessionStartClick(object sender, RoutedEventArgs e)
        {
            var selectedFocusTime = ((ComboBoxItem)FocusTimeComboBox.SelectedValue).Content;
            focustimeend = DateTime.Now.AddMinutes(Int32.Parse(selectedFocusTime.ToString()));
            timer.Elapsed += FocusSessionUpdater;
            focusTimeStackPanel.Background = new SolidColorBrush(Colors.LightGreen);
            focusTimerSubscribed = true;
            focusTimerHasEnded = false;
            pauseTimerButton.IsEnabled = true;
            if(focusFileName == null && breakFileName != null)
            {
                statusBarText.Text = "No focus sound set. No sound will play when Focus Session ends.";
            }
            if(focusFileName == null && breakFileName == null)
            {
                statusBarText.Text = "No sounds set. No sounds will play when either Focus/Break ends.";
            }
            if(focusFileName != null && breakFileName == null)
            {
                statusBarText.Text = "No break sound set. No sound will play when break ends.";
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                CurrentTime.Content = DateTime.Now.ToString();
            });
        }

        private void FocusSessionUpdater(object sender, ElapsedEventArgs e)
        {
            focustimetimer = focustimeend - DateTime.Now;
            var textToDisplay = focustimetimer.ToString(@"hh\:mm\:ss");
            this.Dispatcher.Invoke(() =>
            {
                timerText.Text = textToDisplay;
            });
            if (focustimetimer < TimeSpan.Zero)
            {
                timer.Elapsed -= FocusSessionUpdater;
                focusTimerSubscribed = false;
                focusTimerHasEnded = true;
                if (focusFileName != null)
                {
                    SoundPlayer soundPlayer = new SoundPlayer(focusFileName);
                    soundPlayer.Load();
                    soundPlayer.Play();
                }
                this.Dispatcher.Invoke(() =>
                {
                    focusTimeStackPanel.Background = new SolidColorBrush(Colors.White);
                    breakTimeStackPanel.Background = new SolidColorBrush(Colors.LightGreen);
                    var selectedBreakTime = ((ComboBoxItem)BreakTimeComboBox.SelectedValue).Content;
                    breaktimeend = DateTime.Now.AddMinutes(Int32.Parse(selectedBreakTime.ToString()));
                });
                
                timer.Elapsed += BreakTimeUpdater;
            }
        }

        private void BreakTimeUpdater(object sender, ElapsedEventArgs e)
        {
            breaktimetimer = breaktimeend - DateTime.Now;
            var textToDisplay = breaktimetimer.ToString(@"hh\:mm\:ss");
            this.Dispatcher.Invoke(() =>
            {
                breakText.Text = textToDisplay;
            });
            if(breaktimetimer < TimeSpan.Zero)
            {
                timer.Elapsed -= BreakTimeUpdater;
                this.Dispatcher.Invoke(() =>
                {
                    var selectedFocusTime = ((ComboBoxItem)FocusTimeComboBox.SelectedValue).Content;
                    focustimeend = DateTime.Now.AddMinutes(Int32.Parse(selectedFocusTime.ToString()));
                });
                
                timer.Elapsed += FocusSessionUpdater;
                focusTimerHasEnded = false;
                focusTimerSubscribed = true;
                if(breakFileName != null)
                {
                    SoundPlayer soundPlayer = new SoundPlayer(breakFileName);
                    soundPlayer.Load();
                    soundPlayer.Play();
                }
                this.Dispatcher.Invoke(() =>
                {
                    focusTimeStackPanel.Background = new SolidColorBrush(Colors.LightGreen);
                    breakTimeStackPanel.Background = new SolidColorBrush(Colors.White);
                    pauseTimerButton.IsEnabled = true;
                    restartTimerButton.IsEnabled = false;
                });
            }
        }

        private void FocusSessionResumeClick(object sender, RoutedEventArgs e)
        {
            restartTimerButton.IsEnabled = false;
            pauseTimerButton.IsEnabled = true;
            if (!focusTimerHasEnded)
            {
                timer.Elapsed += FocusSessionUpdater;
                focusTimerSubscribed = true;
            }
            focustimeend = DateTime.Now.Add(focustimePauseDuration);
        }

        private void FocusSessionPauseClick(object sender, RoutedEventArgs e)
        {
            pauseTimerButton.IsEnabled = false;
            restartTimerButton.IsEnabled = true;
            if (focusTimerSubscribed)
            {
                timer.Elapsed -= FocusSessionUpdater;
                focusTimerSubscribed = false;
            }
            focustimePauseDuration = focustimeend - DateTime.Now;
        }

        private void FocusLoaderClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.Title = "Pick a focus sound: ";
            dialog.Filter = "Audio Files | *.wav; *.mp3";
            dialog.ShowDialog();
            focusFileName = dialog.FileName;
            if(focusFileName != null) 
            {
                statusBarText.Text = $"Focus Time End audio file: {focusFileName}";
            }
        }

        private void BreakLoaderClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.Title = "Pick a break sound: ";
            dialog.Filter = "Audio Files | *.wav; *.mp3";
            dialog.ShowDialog();
            breakFileName = dialog.FileName;
            if(breakFileName != null) 
            {
                statusBarText.Text = $"Break Time End audio file: {breakFileName}";
            }
        }

        private void soundSaverClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
