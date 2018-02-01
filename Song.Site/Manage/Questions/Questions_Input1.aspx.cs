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
using System.Data.OleDb;

using WeiSha.Common;

using Song.ServiceInterfaces;
using Song.Entities;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Song.Site.Manage.Questions
{
    public partial class Questions_Input1 : Extend.CustomPage
    {
        //��������
        int type=WeiSha.Common.Request.QueryString["type"].Int32 ?? 0;
        //����get�����Ŀγ�id
        int couid_get = WeiSha.Common.Request.QueryString["couid"].Int32 ?? 0;
        //��ǰ����
        Song.Entities.Organization org = null;
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        protected void ExcelInput1_OnInput(object sender, EventArgs e)
        {
            org = Business.Do<IOrganization>().OrganCurrent();
            //�������е�����
            DataTable dt = ExcelInput1.SheetDataTable;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    //���������е������ݿ�
                    _inputData(dt.Rows[i]);
                }
                catch
                {
                    //����������������з��ظ��ؼ�
                    ExcelInput1.AddError(dt.Rows[i]);
                }
            }
        }

        

        #region ��������
        /// <summary>
        /// ��ĳһ�����ݼ��뵽���ݿ�
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="dl"></param>
        private void _inputData(DataRow dr)
        {
            Song.Entities.Questions obj = new Song.Entities.Questions();
            obj.Qus_IsUse = true;
            obj.Qus_Type = this.type;
            //��ȷ��
            int correct = 0;
            foreach (KeyValuePair<String, String> rel in ExcelInput1.DataRelation)
            {               
                //Excel���е�ֵ
                string column = dr[rel.Key].ToString();
                //���ݿ��ֶε�����
                string field = rel.Value;
                if (field == "Qus_ID")
                {
                    if (string.IsNullOrEmpty(column) || column.Trim() == "") continue;
                    int ques = Convert.ToInt32(column);
                    Song.Entities.Questions isHavObj = Business.Do<IQuestions>().QuesSingle(ques);
                    if (isHavObj != null) obj = isHavObj;
                }
                //����Ѷȡ�רҵ�����⽲��
                if (field == "Qus_Title")
                {
                    if (column == string.Empty || column.Trim() == "") return;
                    obj.Qus_Title = column;
                    obj.Qus_Title = tranTxt(obj.Qus_Title);
                }
                if (field == "Qus_Diff") obj.Qus_Diff = Convert.ToInt16(column);
                if (field == "Sbj_Name")
                {
                    Song.Entities.Subject subject = Business.Do<ISubject>().SubjectBatchAdd(org.Org_ID, column);
                    if (subject != null)
                    {
                        obj.Sbj_Name = subject.Sbj_Name;
                        obj.Sbj_ID = subject.Sbj_ID;
                    }
                }
                if (field == "Cou_Name")
                {
                    Song.Entities.Course course = Business.Do<ICourse>().CourseBatchAdd(org.Org_ID, obj.Sbj_ID, column);
                    if (course != null) obj.Cou_ID = course.Cou_ID;
                }
                if (field == "Ol_Name")
                {
                    Song.Entities.Outline outline = Business.Do<IOutline>().OutlineBatchAdd(org.Org_ID, obj.Sbj_ID, obj.Cou_ID, column);
                    if (outline != null) obj.Ol_ID = outline.Ol_ID;
                }
                if (field == "Qus_Explain") obj.Qus_Explain = column;
                //Ψһֵ����ȷ�𰸣�����
                obj.Qus_UID = WeiSha.Common.Request.UniqueID();
                if (field == "Ans_IsCorrect")
                {
                    if (new Regex(@"^\d+$", RegexOptions.Multiline).Match(column).Success)
                        correct = column == string.Empty ? 0 : Convert.ToInt32(column);
                }
            }
            //�ٱ���һ�飬ȡ��
            List<Song.Entities.QuesAnswer> ansItem = new List<QuesAnswer>();
            foreach (KeyValuePair<String, String> rel in ExcelInput1.DataRelation)
            {                
                //���ݿ��ֶε�����
                string field = rel.Value;
                Match match = new Regex(@"(Ans_Context)(\d+)").Match(field);
                if (match.Success)
                {
                    //Excel���е�ֵ
                    string column = dr[rel.Key].ToString();
                    if (column == string.Empty || column.Trim() == "") continue;
                    int index = Convert.ToInt16(match.Groups[2].Value);
                    Song.Entities.QuesAnswer ans = new Song.Entities.QuesAnswer();
                    ans.Ans_Context = column;
                    ans.Ans_IsCorrect = index == correct;
                    ans.Qus_UID = obj.Qus_UID;
                    ansItem.Add(ans);
                }
            }
            //�ж��Ƿ��д�
            string error = "";
            if (ansItem.Count < 1) error = "ȱ�ٴ�ѡ��";
            if (correct < 1 || correct > ansItem.Count) 
                error = string.Format("��ȷ�𰸵����ò���ȷ����{0}����ѡ���������Ϊ{1}", ansItem.Count, correct);
            obj.Qus_IsError = error != "";
            obj.Qus_ErrorInfo = error;
            if (obj.Sbj_ID == 0) throw new Exception("��ǰ��������רҵ��������");
            if (obj.Cou_ID == 0) throw new Exception("��ǰ�������ڿγ̲�������");
            //if (obj.Ol_ID == 0) throw new Exception("��ǰ���������½ڲ�������");
            if (org != null) obj.Org_ID = org.Org_ID;
            Business.Do<IQuestions>().QuesInput(obj, ansItem); 
        }
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        private string tranTxt(string txt)
        {
            txt = txt.Replace("(", "��");
            txt = txt.Replace(")", "��");
            txt = txt.Replace("��", "�� ");
            return txt;
        }
        #endregion
      
    }
}