using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace Zoo_Antwerpen
{
    public partial class MainPage : PhoneApplicationPage
    {
        public ZooParser FullSchedule = new ZooParser();
        public DateTime SelectedDate;

        private Settings AppSettings;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            AppSettings = (App.Current as App).AppSettings;

            pivotActivity.DataContext = FullSchedule.ActivitySchedule;
            pivotShows.DataContext = FullSchedule.ShowSchedule;
            pivotContinue.DataContext = FullSchedule.ContinueSchedule;
            
            SelectedDate = DateTime.Now;

            if (AppSettings.Language || AppSettings.Language == null)
            {
                languageselector.IsChecked = true;
                languageselector.Content = "Français";
                pivotActivity.Header = "Activité";
                pivotContinue.Header = "Continu";
                pivotShows.Header = "Shows";
                pivotSettings.Header = "Configuration";
                titleApp.Title = "Zoo d'Anvers";
                txbDateTxb.Text = "Choisir la date";
                disclaimtitletxb.Text = "Avertissement";
                disclaimtxb.Text =
                    "Cette application n'est pas officielle et n'est pas approuvé par le Zoo d'Anvers ou d'une de ses filiales. Toutes les images, données, etc appartiennent à leurs propriétaires respectifs. Pour des questions ou des plaintes sur la violation du copyright etc, s'il vous plaît contactez-moi dams.tim @ telenet.be";
            }
            UpdateSchedule();
        }

        private void UpdateSchedule()
        {
            try
            {
                progressbar.Visibility = System.Windows.Visibility.Visible;
                progressbar.IsIndeterminate = true;
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    FullSchedule.ScheduleUpdated +=
                        new System.ComponentModel.AsyncCompletedEventHandler(fullSchedule_ScheduleUpdated);
                    if (languageselector.IsChecked != null && languageselector.IsChecked.Value)
                        FullSchedule.FillTotalScheduleAsync(Languages.French, SelectedDate);
                    else
                    {
                        FullSchedule.FillTotalScheduleAsync(Languages.Dutch, SelectedDate);
                        languageselector.IsChecked = false;
                    }
                }
                else
                {
                    progressbar.Visibility = System.Windows.Visibility.Collapsed;
                    progressbar.IsIndeterminate = false;
                    MessageBox.Show("No data network connection is available.");
                    //Todo: load cached results
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occured. Please ensure you have an active, working data connection.");
                progressbar.Visibility = System.Windows.Visibility.Collapsed;
                progressbar.IsIndeterminate = false;
            }

        }

        void fullSchedule_ScheduleUpdated(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            progressbar.Visibility = System.Windows.Visibility.Collapsed;
            progressbar.IsIndeterminate = false;
            LbActivity.ItemsSource = FullSchedule.ActivitySchedule.ActivityList;
            LbShows.ItemsSource = FullSchedule.ShowSchedule.ActivityList;
            LbContinue.ItemsSource = FullSchedule.ContinueSchedule.ActivityList;



        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            // http://forums.create.msdn.com/forums/t/87002.aspx 
                languageselector.Content = "Français";
                pivotActivity.Header = "Activité";
                pivotContinue.Header = "Continu";
                pivotShows.Header = "Shows";
                pivotSettings.Header = "Configuration";
                titleApp.Title = "Zoo d'Anvers";
                txbDateTxb.Text = "Choisir la date";
            disclaimtitletxb.Text = "Avertissement";
            disclaimtxb.Text =
                "Cette application n'est pas officielle et n'est pas approuvé par le Zoo d'Anvers ou d'une de ses filiales. Toutes les images, données, etc appartiennent à leurs propriétaires respectifs. Pour des questions ou des plaintes sur la violation du copyright etc, s'il vous plaît contactez-moi dams.tim @ telenet.be";
                AppSettings.Language = true;
                UpdateSchedule(); 
        }

        private void languageselector_Unchecked(object sender, RoutedEventArgs e)
        {
            languageselector.Content = "Nederlands";
            pivotActivity.Header = "Activiteiten";
            pivotContinue.Header = "Doorlopend";
            pivotShows.Header = "Shows";
            pivotSettings.Header = "Instellingen";
            titleApp.Title = "Zoo Antwerpen";
            txbDateTxb.Text = "Kies datum";
            disclaimtitletxb.Text = "Disclaimer";
            disclaimtxb.Text =
                "Deze toepassing is niet officieel en niet onderschreven door de Zoo Antwerpen of een van haar filialen. Alle beelden, gegevens, etc. behoren tot de respectievelijke eigenaars. Voor vragen of klachten over schending van het auteursrecht, enz., neem dan contact met mij op dams.tim @ telenet.be";
            AppSettings.Language = false;
            UpdateSchedule(); 
        }

        private void DatePicker_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            if (selDatePick.Value != null) 
            {SelectedDate = (DateTime)selDatePick.Value;UpdateSchedule();}
            else
            {
                MessageBox.Show("No date or wrong date selected.");
            }
            
        }
    }
}