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
using System.Threading.Tasks;

namespace BoardApp
{
    [Activity(Label = "스레드 목록")]
    public class MainActivity : Activity
    {
        private ListView threadListView = null;
        private List<APILibaray.Thread> list = null;
        private ThreadListAdapter adapter = null;
        private Task<List<APILibaray.Thread>> task = null;
        private class ThreadListAdapter : BaseAdapter<APILibaray.Thread>
        {
            private List<APILibaray.Thread> list;
            private Activity activity;
            public ThreadListAdapter(Activity activity, List<APILibaray.Thread> list)
            {
                this.list = list;
                this.activity = activity;
            }
            public void SetList(List<APILibaray.Thread> list)
            {
                this.list = list;
                NotifyDataSetChanged();
            }
            public override APILibaray.Thread this[int position] =>(list != null)? list[position]: null;
            public override int Count => (list != null) ? list.Count : 0;
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
        protected override  void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            this.threadListView =
                FindViewById<ListView>(Resource.Id.main_view_thread_list);
            this.threadListView.ItemClick += ThreadListView_ItemClick;
            this.threadListView.ItemLongClick += ThreadListView_ItemLongClick;
           
        }
        protected override void OnStop()
        {
            base.OnStop();
            if(task != null)
            {
                task.Dispose();
                task = null;
            }
        }
        private void ThreadListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var thread = this.list[e.Position];
            AlertDialog dialog = null;
            //throw new NotImplementedException();
            var handler = new System.EventHandler<DialogClickEventArgs>(async delegate (object obj, DialogClickEventArgs args) {
                if (args.Which == 0)
                {
                    
                    var res = await thread.Delete(Singletone.Instance.Token);
                    if (res)
                    {
                        await LoadThreadList();
                        //adapter.NotifyDataSetInvalidated();

                    }
                    else
                    {
                        new AlertDialog.Builder(this).SetMessage("권한이 없습니다.").SetPositiveButton("확인", delegate { }).Show();
                    }
                }
                else
                {
                    dialog.Dismiss();
                }
            });

            dialog = new AlertDialog.Builder(this).SetTitle($"{thread.Subject} by {thread.Opener}를…").SetItems(new string[] { "삭제" ,"취소"}, handler).Show();
        }
        
        private async Task LoadThreadList()
        {
            var progressDialog =
            new ProgressDialog(this);
            progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            progressDialog.SetMessage("스레드 목록을 가져오고 있습니다.");
            progressDialog.SetCancelable(false);
            progressDialog.Show();
            if (Singletone.Instance.ThreadList == null)
            {
                Singletone.Instance.ThreadList = new ThreadList();
            }
            if(task != null)
            {
                try
                {
                    task.Dispose();
                }
                catch(Exception e)
                {
                    e.PrintStackTrace();
                }
            }
            task = Singletone.Instance.ThreadList.Get(0, 25);
            list = await task;
            task = null;
            if (adapter == null)
            {
                adapter = new ThreadListAdapter(this, list);
                this.threadListView.Adapter = adapter;
            }
            else
            {
                adapter.SetList(list);
            }
            progressDialog.Dismiss();
        }
        private void ThreadListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //throw new System.NotImplementedException();
            var item = list[(int)e.Id];
            var intent = new Intent(this, typeof(ThreadViewActivity));
            intent.PutExtra("uid", item.Uid);
            intent.PutExtra("subject", item.Subject);
            StartActivity(intent);
        }

        protected async override void OnResume()
        {
            base.OnResume();
            await LoadThreadList();
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater inflater = this.MenuInflater;
            inflater.Inflate(Resource.Menu.menu, menu);
            //menu.FindItem(Resource.Id.ActionBar_createThread).
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle presses on the action bar items
            switch (item.ItemId)
            {
                case Resource.Id.ActionBar_createThread:
                    //openSearch();
                    StartActivity(typeof(ThreadOpenActivity));
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}

