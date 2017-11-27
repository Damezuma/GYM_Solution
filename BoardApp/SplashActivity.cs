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
using APILibaray;

namespace BoardApp
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory =true)]
    public class SplashActivity : Activity
    {
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //SetContentView(Resource.Layout.Slash);
            //var progressDialog =
            //    new ProgressDialog(this);
            //progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            //progressDialog.SetMessage("스레드 목록을 가져오고 있습니다.");
            //progressDialog.Show();

            // Set our view from the "main" layout resource
            var list = await ThreadList.Get(0, 25);
            Singletone.Instance.ThreadList = list;
            AlertDialog dialog = null;
            dialog=
            new AlertDialog.Builder(this)
           .SetTitle("로그인")
           .SetView(Resource.Layout.LoginDialog)
           .SetPositiveButton("로그인", (EventHandler<DialogClickEventArgs>)null)
            .SetNegativeButton("건너뛰기", (EventHandler<DialogClickEventArgs>)null).Show();
            dialog.GetButton((int)DialogButtonType.Positive).Click +=async delegate
            {
                var email = dialog.FindViewById<EditText>(Resource.Id.LoginDialog_email).Text;
                var password = dialog.FindViewById<EditText>(Resource.Id.LoginDialog_password).Text;
                var token = await APILibaray.Token.Get(email, password);
                if(token == null)
                {
                    
                    new AlertDialog.Builder(this)
                    .SetPositiveButton(Android.Resource.String.Ok, delegate { })
                    .SetMessage("로그인에 실패하였습니다")
                    .Show();
                    return;
                }
                Toast.MakeText(this, "로그인에 성공하였습니다.", ToastLength.Long);
                Singletone.Instance.Token = token;
                dialog.Dismiss();
                StartActivity(new Intent(this, typeof(MainActivity)));
            };
            dialog.GetButton((int)DialogButtonType.Negative).Click += delegate
            {
                dialog.Dismiss();
                StartActivity(new Intent(this, typeof(MainActivity)));
            };
            //progressDialog.Dismiss();

            // Create your application here
        }
    }
}