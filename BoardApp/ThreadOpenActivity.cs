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
    [Activity(Label = "ThreadOpenActivity")]
    public class ThreadOpenActivity : Activity
    {
        private EditText subject;
        private EditText tags;
        private EditText content;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ThreadOpenActivity);
            subject = FindViewById<EditText>(Resource.Id.ThreadOpenActivity_subject);
            tags = FindViewById<EditText>(Resource.Id.ThreadOpenActivity_tags);
            content = FindViewById<EditText>(Resource.Id.ThreadOpenActivity_comment);
            // Create your application here
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater inflater = this.MenuInflater;
            inflater.Inflate(Resource.Menu.ThreadOpenActivity_menu, menu);
            //menu.FindItem(Resource.Id.ActionBar_createThread).
            return base.OnCreateOptionsMenu(menu);
        }
        public async void Upload()
        {
            var thread = new APILibaray.Thread();
            thread.Subject = subject.Text;

            var progressDialog = new ProgressDialog(this);
            progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            progressDialog.SetMessage("업로드 중입니다.");
            progressDialog.SetCancelable(false);
            progressDialog.Show();
            var task = thread.Upload(Singletone.Instance.Token, content.Text, tags.Text);
            var result = await task;
            progressDialog.Dismiss();
            if (!result)
            {
                new AlertDialog.Builder(this)
                    .SetMessage("권한이 없습니다.")
                    .SetNeutralButton("확인",delegate { Finish(); })
                    .Show();
                
            }
            else
            {
                Finish();
            }
            
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle presses on the action bar items
            switch (item.ItemId)
            {
                case Resource.Id.ActionBar_commit:
                    //openSearch();
                    Upload();
                    return true;
                case Resource.Id.ActionBar_cancel:
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}