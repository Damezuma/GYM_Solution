using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BoardApp
{
    public class Singletone
    {
        private static Singletone instance = null;
        public static Singletone Instance{
            get{
                lock (typeof(Singletone))
                {
                    if(instance == null)
                    {
                        instance = new Singletone();
                    }
                }
                return instance;
            }
        }
        public List<APILibaray.Thread> ThreadList { get; set; }
        public String Token { get; set; }
    }

}