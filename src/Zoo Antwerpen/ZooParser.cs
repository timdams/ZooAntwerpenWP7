using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace Zoo_Antwerpen
{

    public class ZooTable : INotifyPropertyChanged
    {
        private string _title = "";
        public string Title { get { return _title; } set { _title = value; NotifyPropertyChanged("Title"); } }
        public DateTime Date { get; set; }
        private ObservableCollection<ZooActivity> _activityList;
        public ObservableCollection<ZooActivity> ActivityList { get { return _activityList; } set { _activityList = value; NotifyPropertyChanged("ActivityList"); } }

        public ZooTable()
        {
           _activityList = new ObservableCollection<ZooActivity>();
        }

        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

    }
    public class ZooActivity:INotifyPropertyChanged
    {
        public string Time { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }

        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public enum Languages { Dutch, French };

    public  class ZooParser:INotifyPropertyChanged
    {
        private ZooTable _showSchedule = new ZooTable();
        private ZooTable _activitySchedule = new ZooTable();
        private ZooTable _continueSchedule = new ZooTable();
        public ZooTable ShowSchedule { get { return _showSchedule; } set { _showSchedule = value; NotifyPropertyChanged("ShowSchedule"); } }
        public ZooTable ActivitySchedule { get { return _activitySchedule; } set { _activitySchedule = value; NotifyPropertyChanged("ActivitySchedule"); } }
        public ZooTable ContinueSchedule { get { return _continueSchedule; } set { _continueSchedule = value; NotifyPropertyChanged("ContinueSchedule"); } }

        public ZooParser()
        {
            ShowSchedule = new ZooTable();
            ActivitySchedule = new ZooTable();
            ContinueSchedule = new ZooTable();
        }


        public event AsyncCompletedEventHandler ScheduleUpdated;
        public  void FillTotalScheduleAsync(Languages language, DateTime date)
        {
            try
            {
                var client = new WebClient();


                #region language string
                string languagestr = "NL";
                if (language == Languages.Dutch)
                    languagestr = "NL";
                else if (language == Languages.French)
                    languagestr = "FR";
                #endregion

                #region datetime string
                string datestr = date.Month + "/" + date.Day + "/" + date.Year;
                #endregion

                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);

                client.DownloadStringAsync(new Uri(@"http://www.zooantwerpen.be/?page=activities&date=" + datestr + "&lang=" + languagestr));
            }
            catch (Exception e)
            {
                throw;
            }

        }

         void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                var arr = (string)e.Result;
                if(e.Error==null)
                {
                //Stuk voor <div id = "overview"> weghalen
                var begin = new Regex("<div id = \"overview\">");
                if (begin.IsMatch(arr))//juiste tabel gevonden)
                {
                    ShowSchedule = new ZooTable();
                    ActivitySchedule = new ZooTable();
                    ContinueSchedule = new ZooTable();

                    arr = begin.Split(arr)[1];

                    //Eerste div weghalen
                    var end = new Regex("</div>");
                    arr = end.Split(arr)[0];

                    //tabels zoeken
                    var tabel = new Regex(@"<table", RegexOptions.IgnoreCase);
                    if (tabel.IsMatch(arr))
                    {
                        string[] tabels = tabel.Split(arr);

                        foreach (var tabeltoparse in tabels)
                        {
                            if (tabeltoparse.Contains("<th class=\"title\" >Activiteit</th>") || tabeltoparse.Contains("<th class=\"title\" >Activit&eacute; </th>"))
                                ActivitySchedule = ParseTabel(tabeltoparse);
                            else if (tabeltoparse.Contains("<th class=\"title\">Shows</th>"))
                                ShowSchedule = ParseTabel(tabeltoparse);
                            else if (tabeltoparse.Contains("<th class=\"title\">Doorlopende activiteiten</th>") || tabeltoparse.Contains("<th class=\"title\">Activit&eacute; continu</th>"))
                                ContinueSchedule = ParseTabel(tabeltoparse);
                        }
                    }

                    }

                }
                else
                {
                    MessageBox.Show(e.Error.Message);
                }

                if (ScheduleUpdated != null)
                {
                    ScheduleUpdated(this, e);
                }
                NotifyPropertyChanged("ShowSchedule");
                NotifyPropertyChanged("ActivitySchedule");
                NotifyPropertyChanged("ContinueSchedule");
                NotifyPropertyChanged("ActivityList");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured. Please ensure you have an active, working data connection.");
                if (ScheduleUpdated != null)
                {
                    ScheduleUpdated(this, e);
                }
            }
           

        }



        private  ZooTable ParseTabel(string tabel)
        {
            var result = new ZooTable();

            #region Parse title
            Regex title = new Regex("<th class=\"title\".?>");
            if (title.IsMatch(tabel))
            {
                string t = title.Split(tabel)[1];
                Regex th = new Regex("</th>");
                result.Title = th.Split(t)[0];
            }
            #endregion

            #region Parse activities
            Regex tr = new Regex("<tr>");
            string[] trs = tr.Split(tabel);
            //eerste 2 negeren (= header info)
            for (int i = 2; i < trs.Length; i++)
            {
                result.ActivityList.Add(ParseActivity(trs[i]));
            }

            #endregion
            return result;
            //Check of parseable table is

        }

        private  ZooActivity ParseActivity(string trs)
        {
            //Form "\r\n            <td>Zeeleeuwenshow</td>\r\n            <td>11.15</td>\r\n            <td>Aquaforum</td>\r\n        </tr>\r\n\r\n        "
            var td = new Regex("<td>");
            var tdend = new Regex("</td>");
            var infos = new string[3];

            infos = td.Split(trs);


            var result = new ZooActivity
                             {
                                 Description = tdend.Split(infos[1])[0],
                                 Time = tdend.Split(infos[2])[0],
                                 Location = tdend.Split(infos[3])[0]
                             };

            return result;

        }

        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
