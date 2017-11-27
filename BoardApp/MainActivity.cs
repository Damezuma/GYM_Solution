using Android.App;
using Android.Widget;
using Android.OS;
using Java.Lang;
using APILibaray;
using System.Collections.Generic;
using Android.Database;
using Android.Views;
using Android.Content;
using Android.Runtime;

namespace BoardApp
{
    /*
    public class ThreadListAsync : AsyncTask<Void, Void, ThreadListResponse>
    {
        public delegate void OnComplete(List<APILibaray.Thread> list);
        public OnComplete onComplete = null;
        public ThreadListAsync(OnComplete onComplete)
        {
            this.onComplete = onComplete;
        }

        protected override ThreadListResponse RunInBackground(params Void[] @params)
        {
            var api = new APILibaray.ThreadListAPI();
            api.ResponseType = ResponseType.Json;
            if(api.Call() == ResponseCode.Ok)
            {
                return (ThreadListResponse)api.GetResponse();
            }
            return null;
        }
        protected override void OnPostExecute(ThreadListResponse result)
        {
            if(this.onComplete != null)
            {
                this.onComplete(result.Get());
            }
            //showDialog("Downloaded " + result + " bytes");
        }
    }
    */
    [Activity(Label = "BoardApp")]
    public class MainActivity : Activity
    {
        private ListView threadListView;
        private class ThreadListAdapter : BaseAdapter<APILibaray.Thread>
        {
            private List<APILibaray.Thread> list;
            private Activity activity;
            public ThreadListAdapter(Activity activity, List<APILibaray.Thread> list)
            {
                this.list = list;
                this.activity = activity;
            }
            public override APILibaray.Thread this[int position] => list[position];
            public override int Count => list.Count;
            public override long GetItemId(int position) => position;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                
                View view = convertView; // re-use an existing view, if one is available
                if (view == null) // otherwise create a new one
                    view = activity.LayoutInflater.Inflate(Resource.Layout.thread_list_item, null);
                view.FindViewById<TextView>(Resource.Id.thread_list_item_subject).Text = list[position].Subject;
                view.FindViewById<TextView>(Resource.Id.thread_list_item_opener).Text = list[position].Opener.ToString();
                view.FindViewById<TextView>(Resource.Id.thread_list_item_recent_update_datetime).Text = list[position].RecentUpdateDatetime.ToString();
                return view;
            }
        }
        protected async override  void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Singletone.Instance.ThreadList == null)
            {
                StartActivity(typeof(SplashActivity));
                SetContentView(Resource.Layout.Main);
                this.threadListView =
                    FindViewById<ListView>(Resource.Id.main_view_thread_list);
                return;
            }

            SetContentView(Resource.Layout.Main);
            this.threadListView =
                FindViewById<ListView>(Resource.Id.main_view_thread_list);
            this.threadListView.ItemClick += ThreadListView_ItemClick;
            var progressDialog =
            new ProgressDialog(this);
            progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            progressDialog.SetMessage("스레드 목록을 가져오고 있습니다.");
            progressDialog.Show();
            var list = await ThreadList.Get(0, 25);
            Singletone.Instance.ThreadList = list;
            var adapter =
            new ThreadListAdapter(this, Singletone.Instance.ThreadList);
            this.threadListView.Adapter =
                adapter;
            progressDialog.Dismiss();
        }

        private void ThreadListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //throw new System.NotImplementedException();
            var item = Singletone.Instance.ThreadList[(int)e.Id];
            var intent = new Intent(this, typeof(ThreadViewActivity));
            intent.PutExtra("uid", item.Uid);
            intent.PutExtra("subject", item.Subject);
            StartActivity(intent);
            
        }

        protected override void OnResume()
        {
            base.OnResume();
            
        }
    }
}

