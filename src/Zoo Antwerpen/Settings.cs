using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;


namespace Zoo_Antwerpen
{
    public class Settings
    {
        private const string LanguageKey = "Language"; //false= dutch, true=french
        
        private T RetrieveSetting<T>(string settingKey)
        {
            object settingValue;
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(settingKey, out settingValue))
                return (T)settingValue;
            return default(T);
        }

        public bool Language
        {
            get {
                
                return RetrieveSetting<bool>(LanguageKey);
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[LanguageKey] =value;

               
            }
        }

    }
}
