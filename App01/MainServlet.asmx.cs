using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Configuration;
using log4net.Config;
using log4net;

namespace App01
{
    /// <summary>
    /// Summary description for MainServlet
    /// </summary>
    // [WebService(Namespace = "http://tempuri.org/")]
    // [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class MainServlet : System.Web.Services.WebService
    {
        private static ILog g_objLog = LogManager.GetLogger("LOGGER");

        static MainServlet()
        {
            XmlConfigurator.Configure();
        }

        [WebMethod]
        // [System.Web.Script.Services.ScriptMethod(UseHttpGet=true)]
        public void exec(string NAME)
        {
            Context.Response.Clear();
            if (String.IsNullOrEmpty(NAME))
            {
                this.warning(Context.Response, "NAME parameter missing");
                return;
            }
            DbConnection l_objDB = this.getDB();
            if (null == l_objDB)
            {
                this.warning(Context.Response, "DB connect failed");
                return;
            }
            
            this.info(Context.Response, "Search for " + NAME);
            g_objLog.Debug(String.Format(@"Looking for {0}", NAME));
            try
            {
                String l_strQuery = "SELECT CONCAT (cname2, ' ', cname1) AS cname FROM pt.employee WHERE UPPER(cname2) = UPPER('" + NAME + "')";
                DbCommand l_objSQL = l_objDB.CreateCommand();
                l_objSQL.CommandText = l_strQuery;
                using (DbDataReader l_objRS = l_objSQL.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
                {
                    Context.Response.Write("<table class=\"table\"><tr><th>Employee</th></tr>");
                    while (l_objRS.Read())
                    {
                        Context.Response.Write("<tr><td>");
                        Context.Response.Write(l_objRS.GetString(0));
                        Context.Response.Write("</td></tr>");
                    }
                    Context.Response.Write("</table>");
                }
            }
            catch (Exception e)
            {
                g_objLog.Error(e);
            }
            Context.Response.Headers.Set("X-XSS-Protection", "0");
        }

        protected void warning(HttpResponse theResponse, String theWarning)
        {
            theResponse.Write("<div class=\"bs-callout bs-callout-warning\">");
            theResponse.Write("<h4>Warning</h4>");
            theResponse.Write(theWarning);
            theResponse.Write("</div>");
        }

        protected void info(HttpResponse theResponse, String theInfo)
        {
            theResponse.Write("<div class=\"bs-callout bs-callout-INFO\">");
            theResponse.Write("<h4>Info</h4>");
            theResponse.Write(theInfo);
            theResponse.Write("</div>");
        }

        private DbConnection getDB()
        {
            DbConnection l_objRes = null;
            try
            {
                ConnectionStringSettings l_objSettings = ConfigurationManager.ConnectionStrings["PT"];
                DbProviderFactory l_objFactory = DbProviderFactories.GetFactory(l_objSettings.ProviderName);
                l_objRes = l_objFactory.CreateConnection();
                l_objRes.ConnectionString = l_objSettings.ConnectionString;
                l_objRes.Open();
            }
            catch (Exception e)
            {
                g_objLog.Error(e);
                l_objRes = null;
            }
            return l_objRes;
        }
    }
}
