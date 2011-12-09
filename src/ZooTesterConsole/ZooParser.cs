using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace ZooTesterConsole
{
    public class ZooSchedule : List<ZooTable>
    {
    }
    public class ZooTable
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public List<ZooActivity> ActivityList = new List<ZooActivity>();


    }
    public class ZooActivity
    {
        public string Time { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }

    public enum Languages { Dutch, French };

    public static class ZooParser
    {
        public static ZooSchedule GetTotalSchedule(Languages language, DateTime date)
        {
            ZooSchedule totalSchedule = new ZooSchedule();

            using (WebClient client = new WebClient())
            {

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
                string arr = client.DownloadString(@"http://www.zooantwerpen.be/?page=activities&date=" + datestr + "&lang=" + languagestr);

                //Stuk voor <div id = "overview"> weghalen
                Regex begin = new Regex("<div id = \"overview\">");
                if (begin.IsMatch(arr))//juiste tabel gevonden)
                {
                    arr = begin.Split(arr)[1];

                    //Eerste div weghalen
                    Regex end = new Regex("</div>");
                    arr = end.Split(arr)[0];

                    //tabels zoeken
                    Regex tabel = new Regex(@"<table", RegexOptions.IgnoreCase);
                    if (tabel.IsMatch(arr))
                    {
                        string[] tabels = tabel.Split(arr);

                        for (int i = 1; i < tabels.Length; i++)
                        {
                            totalSchedule.Add(ParseTabel(tabels[i]));
                        }

                    }

                }

                return totalSchedule;

            }

        }



        private static ZooTable ParseTabel(string tabel)
        {
            ZooTable result = new ZooTable();

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

        private static ZooActivity ParseActivity(string trs)
        {
            //Form "\r\n            <td>Zeeleeuwenshow</td>\r\n            <td>11.15</td>\r\n            <td>Aquaforum</td>\r\n        </tr>\r\n\r\n        "
            Regex td = new Regex("<td>");
            Regex tdend = new Regex("</td>");
            string[] infos = new string[3];

            infos = td.Split(trs);


            ZooActivity result = new ZooActivity();
            result.Description = tdend.Split(infos[1])[0];
            result.Time = tdend.Split(infos[2])[0];
            result.Location = tdend.Split(infos[3])[0];

            return result;

        }
    }
}
