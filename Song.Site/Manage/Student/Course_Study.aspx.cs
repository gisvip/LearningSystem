using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using WeiSha.Common;
using Song.ServiceInterfaces;
using Song.Entities;
using WeiSha.WebControl;
using System.Collections.Generic;

namespace Song.Site.Manage.Student
{
    public partial class Course_Study : Extend.CustomPage
    {
        private string _uppath = "Course";
        Song.Entities.Organization org;
        //ѧϰ��¼��datatable
        DataTable dtLog = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            Song.Entities.Accounts st = this.Master.Account;
            if (st == null) return;
            org = Business.Do<IOrganization>().OrganCurrent();
            dtLog = Business.Do<IStudent>().StudentStudyCourseLog(org.Org_ID, this.Master.Account.Ac_ID);
            if (!this.IsPostBack)
            {  
                BindData(null, null);
            }
        }
        /// <summary>
        /// ���б�
        /// </summary>
        protected void BindData(object sender, EventArgs e)
        {
            //��ǰѧ���Ŀγ�
            Song.Entities.Accounts st = this.Master.Account;
            if (st == null) return;
            //����Ŀγ�(�������õģ�
            List<Song.Entities.Course> cous = Business.Do<ICourse>().CourseForStudent(st.Ac_ID, null, 1,null,-1);
            foreach (Song.Entities.Course c in cous)
            {
                //�γ�ͼƬ
                if (!string.IsNullOrEmpty(c.Cou_LogoSmall) && c.Cou_LogoSmall.Trim() != "")
                    c.Cou_LogoSmall = Upload.Get[_uppath].Virtual + c.Cou_LogoSmall;                    
                c.Cou_IsStudy = true;
            }
            rptCourse.DataSource = cous;
            rptCourse.DataBind();
            plNoCourse.Visible = cous.Count < 1;           
        }
        /// <summary>
        /// ��ȡ�γ̵Ĺ�����Ϣ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected string getBuyInfo(object id)
        {
            int couid = 0;
            int.TryParse(id.ToString(), out couid);
            Student_Course sc= Business.Do<ICourse>().StudyCourse(Extend.LoginState.Accounts.CurrentUser.Ac_ID, couid);
            if (sc == null) return "";
            if (sc.Stc_IsFree) return "��ѣ������ڣ�";
            if (sc.Stc_IsTry) return "����";
            return sc.Stc_StartTime.ToString("yyyy��MM��dd��") + " - " + sc.Stc_EndTime.ToString("yyyy��MM��dd��");
        }
        /// <summary>
        /// ȡ���γ�ѧϰ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbSelected_Click(object sender, EventArgs e)
        {
            LinkButton lb = (LinkButton)sender;            
            int couid = Convert.ToInt32(lb.CommandArgument);    //�γ�id            
            Song.Entities.Accounts st = this.Master.Account;     //��ǰѧ��       
            //ȡ��
            Business.Do<ICourse>().DelteCourseBuy(st.Ac_ID, couid);
            //���ص�ǰ��
            this.Reload();
        }
        /// <summary>
        /// �����ۼ�ѧϰʱ��
        /// </summary>
        /// <param name="studyTime"></param>
        /// <returns></returns>
        protected string CaleStudyTime(string studyTime)
        {
            int num = 0;
            int.TryParse(studyTime, out num);
            if (num < 60) return num + "����";
            //�������
            num = num / 60;
            int ss = num % 60;
            if (num < 60) return num + "����";
            //����Сʱ
            int hh = num / 60;
            int mm = num % 60;
            return string.Format("{0}Сʱ{1}����", hh, mm);
        }
        /// <summary>
        /// ��ȡ�ۼ�ѧϰʱ��
        /// </summary>
        /// <param name="studyTime"></param>
        /// <returns></returns>
        protected string GetstudyTime(string couid)
        {
            string studyTime = "0";
            foreach (DataRow dr in dtLog.Rows)
            {
                if (dr["Cou_ID"].ToString() == couid)
                {
                    studyTime = dr["studyTime"].ToString();
                }
            }
            return CaleStudyTime(studyTime);
        }
        /// <summary>
        /// ��ȡ���ѧϰʱ��
        /// </summary>
        /// <param name="couid"></param>
        /// <returns></returns>
        protected string GetLastTime(string couid)
        {
            DateTime? lastTime = null;
            foreach (DataRow dr in dtLog.Rows)
            {
                if (dr["Cou_ID"].ToString() == couid)
                {
                    lastTime = Convert.ToDateTime(dr["LastTime"]);
                }
            }
            if (lastTime == null) return "����û��ѧϰ��";
            return ((DateTime)lastTime).ToString();
        }
        /// <summary>
        /// ��ȡѧԱѧϰ�Ŀγ̼�¼
        /// </summary>
        /// <param name="couidstr"></param>
        /// <returns></returns>
        protected Song.Entities.Student_Course GetStc(string couidstr)
        {
            int couid = 0;
            int.TryParse(couidstr, out couid);
            return Business.Do<ICourse>().StudyCourse(this.Master.Account.Ac_ID, couid);
        }
    }
}