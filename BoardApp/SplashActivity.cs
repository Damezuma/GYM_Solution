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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if(Singletone.Instance.Token != null)
            {
                StartActivity(new Intent(this, typeof(MainActivity)));
                return;
            }
            AlertDialog dialog = null;
            dialog=
            new AlertDialog.Builder(this)
           .SetTitle("로그인")
           .SetView(Resource.Layout.LoginDialog)
           .SetCancelable(false)
           .SetPositiveButton("로그인", (EventHandler<DialogClickEventArgs>)null)
            .SetNegativeButton("건너뛰기", (EventHandler<DialogClickEventArgs>)null).Show();
            dialog.GetButton((int)DialogButtonType.Positive).Click +=async delegate
            {
                var b = new ProgressDialog(this);
                b.SetMessage("로그인 중…");
                b.Show();

                var email = dialog.FindViewById<EditText>(Resource.Id.LoginDialog_email).Text;
                var password = dialog.FindViewById<EditText>(Resource.Id.LoginDialog_password).Text;
                var token = await APILibaray.Token.Get(email, password);

                b.Dismiss();
                if (token == null)
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
        }
    }
}