using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQLEditor
{
    public partial class Form1 : Form
    {
        static bool flag = false;
        public Form1()
        {
            InitializeComponent();
        }

        public async void WaitSomeTime(int sec)
        {
            if (sec < 1) return;
            DateTime _desired = DateTime.Now.AddSeconds(sec);
            while (DateTime.Now < _desired)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

    private void button2_Click(object sender, EventArgs e)
        {
            flag = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            flag = false;
            textBox7.Text = "";
            textBox6.Text = "";
            textBox8.Text = textBox1.Text;
            int starting_id = Int32.Parse(textBox1.Text);
            int record_n = Int32.Parse(textBox3.Text);
            int count = Int32.Parse(textBox2.Text);
            int com_wait = Int32.Parse(textBox4.Text);
            string set_cols =
                "[AssignmentCode2]=[AssignmentCode], " +
                "[StudentCode2]=[StudentCode], " +
                "[LecturerCode2]=[LecturerCode], " +
                "[CourseCode2]=[CourseCode], " +
                "[Exception2]=[Exception], " +
                "[senderIP2]=[senderIP], " +
                "[facultyCode2]=[facultyCode] , " +
                "[facultyName2]=[facultyName] ," +
                "[deptCode2]=[deptCode] ," +
                "[deptName2]=[deptName] ," +
                "[courseCategory2]=[courseCategory] ," +
                "[courseName2]=[courseName] , " +
                "[MoodleAssignPageNo2]=[MoodleAssignPageNo] ," +
                "[GroupMembers2]=[GroupMembers] ";


            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServer"].ToString());
            con.Open();
            SqlTransaction transaction = con.BeginTransaction();
            string stmt = "SELECT COUNT([Id]) FROM [dbo].[Documents]  WHERE [Id] >" + (starting_id - 1);
            int total_records = 0;
            try
            {
                using (SqlCommand cmdCount = new SqlCommand(stmt, con, transaction))
                {
                    total_records = (int)cmdCount.ExecuteScalar();
                    listBox1.Items.Insert(0, "Total records are " + total_records);
                }
            }
            catch (Exception ex)
            {
                listBox1.Items.Insert(0, ex.ToString());
            }

            try
            {
                if (total_records > 0)
                {
                    int row = 1;
                    int affected_count = 0;
                    int remain_count = count;
                    for (int i = 0; i < (int)(count / record_n) + 1; i++)
                    {
                        if (record_n > remain_count)
                        {
                            record_n = remain_count;
                        }
                        string qry = "UPDATE TOP(" + record_n + ") [dbo].[Documents] SET " + set_cols +
                        " WHERE [Id] >" + (starting_id - 1);
                        //con.Open();
                        SqlCommand oCmd = new SqlCommand(qry, con, transaction);
                        var num = oCmd.ExecuteNonQuery();
                        //con.Close();
                        if (num < 1)
                        {
                            break;
                        }
                        //con.Open();
                        oCmd = new SqlCommand("SELECT TOP(" + record_n + ") [Id] FROM [dbo].[Documents]  WHERE [Id] >" + (starting_id - 1), con, transaction);
                        using (SqlDataReader oReader = oCmd.ExecuteReader())
                        {
                            List<string> ids = new List<string>();
                            while (oReader.Read())
                            {
                                ids.Add(oReader["Id"].ToString());
                                listBox1.Items.Insert(0, "Row " + (row++) + ", Id " + oReader["Id"].ToString());
                            }
                            if (ids.Count > 0)
                            {
                                textBox7.Text = ids[ids.Count - 1];
                                starting_id = Int32.Parse(ids[ids.Count - 1]) + 1;
                            }

                        }
                        transaction.Commit();
                        
                        affected_count += num;
                        textBox6.Text = affected_count.ToString();
                        Application.DoEvents();

                        if (flag)
                        {
                            listBox1.Items.Insert(0, "We are done for " + affected_count);
                            listBox1.Items.Insert(0, "We are stopping...");
                            break;
                        }
                        remain_count -= num;
                        listBox1.Items.Insert(0, "We are done for " + affected_count);
                        listBox1.Items.Insert(0,"We are waiting...");
                        WaitSomeTime(Int32.Parse(textBox4.Text));
                        transaction = con.BeginTransaction();
                    }
                }
                listBox1.Items.Insert(0, "All done!");
                MessageBox.Show("All done!");
            }
            catch (Exception ex)
            {
                listBox1.Items.Insert(0, ex.ToString());
                transaction.Rollback();
            }
            finally
            {
                con.Close();
            }
        }
    }
}
