using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using APILibaray;
namespace BoardWinApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var list = await ThreadList.Get(0, 25);
            foreach (var it in list)
            {
                var s =
                    $"{it.Subject}({it.Opener.Nickname}):{it.RecentUpdateDatetime}";
                this.listBox1.Items.Add(s);
            }
        }
    }
}
