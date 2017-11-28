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
using Android.Graphics;
using Android.Text;

namespace BoardApp
{
    class CommentListAdapter : BaseAdapter<APILibaray.Comment>
    {
        private List<APILibaray.Comment> list;
        
        private Activity activity;
        private Dictionary<String, Bitmap> gravatar;
        public CommentListAdapter(Activity activity, List<APILibaray.Comment> list, Dictionary<String, Bitmap> gravatar)
        {
            this.list = list;
            this.activity = activity;
            this.gravatar = gravatar;
        }
        public void SetList(List<APILibaray.Comment> list)
        {
            this.list = list;
            
        }
        public override APILibaray.Comment this[int position] => list[position];
        public override int Count => list.Count;
        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            
            View view = convertView; // re-use an existing view, if one is available
            if (view == null) // otherwise create a new one
                view = activity.LayoutInflater.Inflate(Resource.Layout.CommentListItemView, null);
            view.FindViewById<TextView>(Resource.Id.CommentListItemView_content).Text = list[position].Content;
            view.FindViewById<TextView>(Resource.Id.CommentListItemView_writer).Text = list[position].User.ToString();
            view.FindViewById<TextView>(Resource.Id.CommentListItemView_writtenDatetime).Text = list[position].write_datetime.ToString();
            
            if (gravatar.ContainsKey(list[position].User.Email))
            {
                view.FindViewById<ImageView>(Resource.Id.CommentListItemView_gravater).SetImageBitmap(gravatar[list[position].User.Email]);
            }
            return view;
        }
    }
    [Activity(Label = "스레드 생성")]
    class ThreadViewActivity : Activity
    {
        List<APILibaray.Comment> comments;
        APILibaray.CommentList commentList;
        ListView commentListView;
        Dictionary<String, Bitmap> gravatar;
        Button sendButton;
        EditText commentEdit;
        CommentListAdapter adapter;
        uint uid;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ThreadViewActivity);
            commentListView = FindViewById<ListView>(Resource.Id.ThreadViewCommentList);
            sendButton = FindViewById<Button>(Resource.Id.ThreadView_commentSender);
            commentEdit = FindViewById<EditText>(Resource.Id.ThreadView_commentInput);
            if(Singletone.Instance.Token == null)
            {
                sendButton.Enabled = false;
            }
            else
            {
                sendButton.Click += SendButton_Click;
            }
            
            var intent = this.Intent;
            uid = (uint)intent.GetLongExtra("uid", 0);
            var title = intent.GetStringExtra("subject");
            if(uid == 0)
            {
                Finish();
            }
            Title = title;

            gravatar = new Dictionary<string, Bitmap>();
            comments = new List<APILibaray.Comment>();
            adapter = new CommentListAdapter(this, comments, gravatar);
            commentListView.Adapter = adapter;
            commentListView.ItemLongClick += CommentListView_ItemLongClick;
            LoadComments();
            
            // commentListView.Adapter.notifyDataSetInvalidated();
        }

        private void CommentListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            //throw new NotImplementedException();
            var handler = new EventHandler<DialogClickEventArgs>(async delegate (object obj, DialogClickEventArgs args) {
                if(args.Which == 0)
                {
                    var comment = this.comments[e.Position];
                    var res = await comment.Delete(Singletone.Instance.Token);
                    if(res)
                    {
                        await LoadComments();
                        //adapter.NotifyDataSetInvalidated();
                        commentListView.SetSelectionFromTop(comments.Count, commentListView.MaxScrollAmount);
                    }
                    else
                    {
                        new AlertDialog.Builder(this).SetMessage("권한이 없습니다.").SetPositiveButton("확인", delegate { }).Show();
                    }
                }
            });

            new AlertDialog.Builder(this).SetItems(new String[] { "삭제", "취소" },  handler).Show();
        }

        private async System.Threading.Tasks.Task LoadComments()
        {
            var progressDialog =
            new ProgressDialog(this);
            progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            progressDialog.SetMessage("스레드를 가져오고 있습니다.");
            progressDialog.SetCancelable(false);
            progressDialog.Show();

            commentList =new APILibaray.CommentList(new APILibaray.Thread() { Uid = (uint)uid });
            comments = await commentList.Get();
            adapter.SetList(comments);
            progressDialog.Dismiss();

            var tasks =
                new Dictionary<string, System.Threading.Tasks.Task<Bitmap>>();

            foreach (var it in comments)
            {
                String email = it.User.Email;
                if (tasks.Keys.Contains(it.User.Email))
                {
                    continue;
                }
                var t = System.Threading.Tasks.Task<Bitmap>.Factory.StartNew(() =>
                {
                    String hash;
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(email);
                        byte[] hashBytes = md5.ComputeHash(inputBytes);

                        // Convert the byte array to hexadecimal string
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < hashBytes.Length; i++)
                        {
                            sb.Append(hashBytes[i].ToString("x2"));
                        }
                        hash = sb.ToString();
                    }
                    var url = new Java.Net.URL($"https://www.gravatar.com/avatar/{hash}?s=128");
                    return Android.Graphics.BitmapFactory.DecodeStream(url.OpenStream());
                });
                tasks.Add(email, t);
            }
            //gravatar.Clear();
            foreach (var pair in tasks)
            {
                if(!gravatar.ContainsKey(pair.Key))
                {
                    var bitmap = await pair.Value;
                    gravatar.Add(pair.Key, bitmap);
                }
            }
            adapter.NotifyDataSetChanged();
            
        }
        private async void SendButton_Click(object sender, EventArgs e)
        {
            String content = commentEdit.Text;
            var a = await APILibaray.Comment.Upload(Singletone.Instance.Token, uid, content);
            if(a)
            {
                commentEdit.Text = "";
                await LoadComments();
                //adapter.NotifyDataSetInvalidated();
                commentListView.SetSelectionFromTop(comments.Count, commentListView.MaxScrollAmount);
            }
        }
    }
}