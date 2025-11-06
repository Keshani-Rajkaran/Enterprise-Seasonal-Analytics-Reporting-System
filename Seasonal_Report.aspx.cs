using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mckaylan_Motor_Engineering.Private_Pages
{
    public partial class Seasonal_Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) PopulateYears();
        }

        private void PopulateYears()
        {
            ddlYear.Items.Clear();
            for (int year = DateTime.Now.Year; year >= 2022; year--)
            {
                ddlYear.Items.Add(new ListItem(year.ToString(), year.ToString()));
            }
        }

        // --- STAGE 1: REQUEST ---
        protected void btnRequest_Click(object sender, EventArgs e)
        {
            pnlReportResults.Visible = false;
            List<string> selectedFocusAreas = new List<string>();
            foreach (ListItem item in cblReportFocus.Items)
            {
                if (item.Selected) selectedFocusAreas.Add(item.Text);
            }

            if (selectedFocusAreas.Count == 0) { ShowErrorAlert("Please select at least one Report Focus Area."); return; }
            if (ddlSeason.SelectedValue == "" || ddlYear.SelectedValue == "") { ShowErrorAlert("Please select Season and Fiscal Year."); return; }

            var dates = GetSeasonDates(ddlSeason.SelectedValue, int.Parse(ddlYear.SelectedValue));
            if (DateTime.Now < dates.Start) { ShowErrorAlert("Cannot report on future operational periods."); return; }

            lblConfirmSeason.Text = ddlSeason.SelectedItem.Text;
            lblConfirmYear.Text = ddlYear.SelectedValue;
            lblConfirmDates.Text = $"{dates.Start:dd MMM} to {dates.End:dd MMM yyyy}";
            lblConfirmFocus.Text = string.Join("<br/>+ ", selectedFocusAreas);
            lblConfirmCompare.Text = ddlComparison.SelectedItem.Text;
            pnlConfirmModal.Visible = true;
        }

        // --- STAGE 2: EXECUTE ---
        protected void btnFinalConfirm_Click(object sender, EventArgs e)
        {
            pnlConfirmModal.Visible = false;
            pnlReportResults.Visible = true;
            var dates = GetSeasonDates(ddlSeason.SelectedValue, int.Parse(ddlYear.SelectedValue));
            string comparisonMode = ddlComparison.SelectedValue;
            bool showCharts = chkShowCharts.Checked;

            lblReportTitle.Text = $"{"McKaylan Motor Engineering"} {"Seasonal Performance Report"}{ddlSeason.SelectedItem.Text} {ddlYear.SelectedValue} - Integrated Analysis";
            // NEW: ADDED GENERATION TIMESTAMP
            lblResultSeason.Text = $"{dates.Start:dd MMM yyyy} to {dates.End:dd MMM yyyy} | <em>Generated on: {DateTime.Now:dd MMM yyyy HH:mm}</em>";

            StringBuilder masterReport = new StringBuilder();

            if (cblReportFocus.Items.FindByValue("Load").Selected)
            {
                masterReport.Append(RenderSectionHeading("📊 Operational Volume Analysis"));
                masterReport.Append(GenerateOperationalVolumeReport(dates.Start, dates.End, comparisonMode, showCharts));
            }
            if (cblReportFocus.Items.FindByValue("Staff").Selected)
            {
                masterReport.Append(RenderSectionHeading("⚙️ Workforce Efficiency Analysis (Technicians)"));
                masterReport.Append(GenerateWorkforceEfficiencyReport(dates.Start, dates.End, comparisonMode, showCharts));
            }
            if (cblReportFocus.Items.FindByValue("Customer").Selected)
            {
                masterReport.Append(RenderSectionHeading("👥 Client Acquisition & Retention"));
                masterReport.Append(GenerateClientRetentionReport(dates.Start, dates.End, comparisonMode, showCharts));
            }

            lblResultFocus.Text = masterReport.ToString();
        }

        protected void btnCancel_Click(object sender, EventArgs e) { pnlConfirmModal.Visible = false; }

        // ========================================================
        // ENGINE 1: OPERATIONAL VOLUME
        // ========================================================
        private string GenerateOperationalVolumeReport(DateTime start, DateTime end, string compareMode, bool showChart)
        {
            StringBuilder html = new StringBuilder();
            int totalServiceUnits = GetValue<int>("SELECT COUNT(*) FROM Appointment WHERE Date >= @S AND Date <= @E", start, end);
            double grossHours = GetValue<double>("SELECT ISNULL(SUM(DATEDIFF(MINUTE, Start_Time, End_Time) / 60.0), 0) FROM Appointment WHERE Date >= @S AND Date <= @E", start, end);
            double throughput = totalServiceUnits > 0 ? grossHours / totalServiceUnits : 0;

            // NEW: DYNAMIC EXECUTIVE SUMMARY
            string summary = $"During this period, the workshop processed a total of <strong>{totalServiceUnits} service units</strong>, logging <strong>{grossHours:F0} operational hours</strong>. ";
            if (throughput < 2.5) summary += "The low average throughput time indicates a high volume of quick-turnaround jobs (e.g., routine maintenance). ";
            else if (throughput > 4.5) summary += "The high average throughput time suggests complex, labor-intensive repairs dominated the schedule. ";
            html.Append(RenderExecutiveSummary(summary));

            string volBadge = "", hoursBadge = "";
            if (compareMode == "LastYear")
            {
                DateTime pS = start.AddYears(-1), pE = end.AddYears(-1);
                volBadge = GenerateComparisonBadge(totalServiceUnits, GetValue<int>("SELECT COUNT(*) FROM Appointment WHERE Date >= @S AND Date <= @E", pS, pE));
                hoursBadge = GenerateComparisonBadge(grossHours, GetValue<double>("SELECT ISNULL(SUM(DATEDIFF(MINUTE, Start_Time, End_Time) / 60.0), 0) FROM Appointment WHERE Date >= @S AND Date <= @E", pS, pE));
            }

            html.Append("<div style='display:flex; gap:20px; margin-bottom:30px;'>");
            html.Append(RenderMetricCard("📦 Total Service Demand", totalServiceUnits.ToString("N0"), volBadge));
            html.Append(RenderMetricCard("⏱️ Gross Operational Hours", grossHours.ToString("N1") + "h", hoursBadge));
            html.Append(RenderMetricCard("⚡ Throughput Rate", throughput.ToString("F1") + " hrs/unit", "Avg time per service"));
            html.Append("</div>");

            if (showChart)
            {
                string query = "SELECT Format(Date, 'dd MMM') as Day, COUNT(*) as Count FROM Appointment WHERE Date >= @S AND Date <= @E GROUP BY Date ORDER BY Date";
                var data = GetChartData(query, start, end);
                html.Append(RenderChart("line", "volChart", "Daily Service Demand Trend", data.Labels, data.Values, "'#3498db'"));
            }

            return html.ToString();
        }

        // ========================================================
        // ENGINE 2: WORKFORCE EFFICIENCY
        // ========================================================
        private string GenerateWorkforceEfficiencyReport(DateTime start, DateTime end, string compareMode, bool showChart)
        {
            StringBuilder html = new StringBuilder();
            int totalTechPool = GetValueNoDates<int>("SELECT COUNT(*) FROM Staff WHERE Role = 'Technician'");
            int activeTechs = GetValue<int>("SELECT COUNT(DISTINCT Tech_ID) FROM Appointment WHERE Date >= @S AND Date <= @E", start, end);
            double totalHours = GetValue<double>("SELECT ISNULL(SUM(DATEDIFF(MINUTE, Start_Time, End_Time) / 60.0), 0) FROM Appointment WHERE Date >= @S AND Date <= @E", start, end);
            double avgTechLoad = activeTechs > 0 ? totalHours / activeTechs : 0;
            double engagementRate = totalTechPool > 0 ? ((double)activeTechs / totalTechPool) * 100 : 0;
            string topTech = GetStringValue(@"SELECT TOP 1 Tech_Name FROM Appointment WHERE Date >= @S AND Date <= @E GROUP BY Tech_Name ORDER BY SUM(DATEDIFF(MINUTE, Start_Time, End_Time)) DESC", start, end) ?? "N/A";

            // NEW: DYNAMIC EXECUTIVE SUMMARY
            string summary = $"Workforce engagement was at <strong>{engagementRate:F0}%</strong> ({activeTechs} of {totalTechPool} technicians utilized). ";
            if (engagementRate < 75) summary += "Consider investigating why a significant portion of the technical staff was unutilized during this peak season. ";
            summary += $"<strong>{topTech}</strong> was the highest output resource based on billable hours. ";
            html.Append(RenderExecutiveSummary(summary));

            string loadBadge = "";
            if (compareMode == "LastYear")
            {
                DateTime pS = start.AddYears(-1), pE = end.AddYears(-1);
                int pastActive = GetValue<int>("SELECT COUNT(DISTINCT Tech_ID) FROM Appointment WHERE Date >= @S AND Date <= @E", pS, pE);
                double pastHours = GetValue<double>("SELECT ISNULL(SUM(DATEDIFF(MINUTE, Start_Time, End_Time) / 60.0), 0) FROM Appointment WHERE Date >= @S AND Date <= @E", pS, pE);
                loadBadge = GenerateComparisonBadge(avgTechLoad, pastActive > 0 ? pastHours / pastActive : 0);
            }

            html.Append("<div style='display:flex; gap:20px; margin-bottom:30px;'>");
            html.Append(RenderMetricCard("👥 Technical Staff Engagement", $"{activeTechs}/{totalTechPool}", $"Utilization Rate: {engagementRate:F0}%"));
            html.Append(RenderMetricCard("⚙️ Avg Technician Load", avgTechLoad.ToString("F1") + "h", loadBadge));
            html.Append(RenderMetricCard("🏆 Top Output Technician", topTech, "Highest billable hours"));
            html.Append("</div>");

            if (showChart)
            {
                string query = "SELECT Tech_Name, SUM(DATEDIFF(MINUTE, Start_Time, End_Time) / 60.0) as Hours FROM Appointment WHERE Date >= @S AND Date <= @E GROUP BY Tech_Name ORDER BY Hours DESC";
                var data = GetChartData(query, start, end);
                html.Append(RenderChart("bar", "staffChart", "Total Billable Hours per Technician", data.Labels, data.Values, "'#2ecc71'"));
            }

            html.Append("<h4 style='color:#2c3e50; margin-top:30px; margin-bottom:15px;'>📋 Individual Technician Performance Breakdown</h4>");
            html.Append(RenderTechnicianTable(start, end));

            return html.ToString();
        }

        // ========================================================
        // ENGINE 3: CLIENT RETENTION
        // ========================================================
        private string GenerateClientRetentionReport(DateTime start, DateTime end, string compareMode, bool showChart)
        {
            StringBuilder html = new StringBuilder();
            int totalTraffic = GetValue<int>("SELECT COUNT(DISTINCT Cust_ID) FROM Appointment WHERE Date >= @S AND Date <= @E", start, end);
            int newAcquisitions = GetValue<int>("SELECT COUNT(*) FROM Customer WHERE JoinDate >= @S AND JoinDate <= @E", start, end);
            int returningClients = totalTraffic - newAcquisitions;
            if (returningClients < 0) returningClients = 0;
            double rate = totalTraffic > 0 ? ((double)newAcquisitions / totalTraffic) * 100 : 0;

            // NEW: DYNAMIC EXECUTIVE SUMMARY
            string summary = $"The business saw <strong>{totalTraffic} unique clients</strong> this season. The acquisition ratio is <strong>{rate:F1}%</strong> ({newAcquisitions} new clients). ";
            if (rate > 30) summary += "This indicates a strong growth phase with successful marketing reach. ";
            else if (rate < 15) summary += "Heavy reliance on repeat business. Consider campaigns to attract new clientele if growth is a target. ";
            html.Append(RenderExecutiveSummary(summary));

            string trafficBadge = "";
            if (compareMode == "LastYear")
            {
                trafficBadge = GenerateComparisonBadge(totalTraffic, GetValue<int>("SELECT COUNT(DISTINCT Cust_ID) FROM Appointment WHERE Date >= @S AND Date <= @E", start.AddYears(-1), end.AddYears(-1)));
            }

            html.Append("<div style='display:flex; gap:20px; margin-bottom:30px;'>");
            html.Append(RenderMetricCard("👣 Total Client Footfall", totalTraffic.ToString(), trafficBadge));
            html.Append(RenderMetricCard("🆕 New Acquisitions", newAcquisitions.ToString(), "Joined this season"));
            html.Append(RenderMetricCard("📈 Acquisition Ratio", rate.ToString("F1") + "%", "% of new traffic"));
            html.Append("</div>");

            if (showChart)
            {
                string labels = "'New Acquisitions', 'Returning Clients'";
                string values = $"{newAcquisitions}, {returningClients}";
                html.Append(RenderChart("doughnut", "clientChart", "Client Composition", labels, values, "'#3498db', '#95a5a6'"));
            }

            return html.ToString();
        }

        // ========================================================
        // HELPERS
        // ========================================================
        // NEW: Renders the grey "Executive Summary" box
        private string RenderExecutiveSummary(string text)
        {
            return $@"<div style='background-color:#f8f9fa; border-left: 4px solid #3498db; padding: 15px; margin-bottom: 25px; color: #2c3e50; line-height: 1.5;'>
                        <strong>📝 Executive Summary:</strong> {text}
                      </div>";
        }

        private string RenderChart(string type, string canvasId, string label, string labelsList, string dataList, string colors)
        {
            string bgColors = colors.Contains(",") ? $"[{colors}]" : $"'{colors.Replace("'", "")}'";
            return $@"<div style='background:#fff; padding:20px; border-radius:10px; box-shadow:0 2px 8px rgba(0,0,0,0.05); margin-bottom:30px; height:300px;'>
                        <canvas id='{canvasId}'></canvas>
                      </div>
                      <script>new Chart(document.getElementById('{canvasId}'), {{ type: '{type}', data: {{ labels: [{labelsList}], datasets: [{{ label: '{label}', data: [{dataList}], backgroundColor: {bgColors}, borderWidth: 1 }}] }}, options: {{ responsive: true, maintainAspectRatio: false, plugins: {{ legend: {{ position: 'bottom' }} }} }} }});</script>";
        }

        private (string Labels, string Values) GetChartData(string query, DateTime start, DateTime end)
        {
            List<string> labels = new List<string>();
            List<string> values = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WstGrp30ConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@S", start);
                    cmd.Parameters.AddWithValue("@E", end);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) { labels.Add($"'{reader[0].ToString()}'"); values.Add(reader[1].ToString().Replace(',', '.')); }
                    }
                }
            }
            return (string.Join(",", labels), string.Join(",", values));
        }

        private string RenderTechnicianTable(DateTime start, DateTime end)
        {
            StringBuilder tableHtml = new StringBuilder();
            tableHtml.Append("<table style='width:100%; border-collapse:collapse; margin-top:10px; background:#fff; box-shadow:0 1px 3px rgba(0,0,0,0.1);'><tr style='background:#2c3e50; color:white; text-align:left;'><th style='padding:12px;'>Technician Name</th><th style='padding:12px;'>Total Jobs</th><th style='padding:12px;'>Season Total Hours</th><th style='padding:12px;'>Avg Weekly Hours</th><th style='padding:12px;'>Avg Monthly Hours</th></tr>");
            double totalDays = (end - start).TotalDays;
            double weeksInSeason = Math.Max(1, totalDays / 7.0);
            double monthsInSeason = Math.Max(1, totalDays / 30.0);
            string query = @"SELECT Tech_Name, COUNT(*) as JobCount, SUM(DATEDIFF(MINUTE, Start_Time, End_Time) / 60.0) as TotalHours FROM Appointment WHERE Date >= @S AND Date <= @E GROUP BY Tech_Name ORDER BY TotalHours DESC";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WstGrp30ConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@S", start); cmd.Parameters.AddWithValue("@E", end); conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool altRow = false;
                        while (reader.Read())
                        {
                            string rowColor = altRow ? "#f8f9fa" : "#ffffff";
                            tableHtml.Append($"<tr style='background:{rowColor}; border-bottom:1px solid #eee;'><td style='padding:12px; font-weight:bold; color:#2c3e50;'>{reader["Tech_Name"]}</td><td style='padding:12px;'>{reader["JobCount"]}</td><td style='padding:12px; color:#2980b9; font-weight:bold;'>{Convert.ToDouble(reader["TotalHours"]):F1} h</td><td style='padding:12px;'>{(Convert.ToDouble(reader["TotalHours"]) / weeksInSeason):F1} h/week</td><td style='padding:12px;'>{(Convert.ToDouble(reader["TotalHours"]) / monthsInSeason):F1} h/month</td></tr>");
                            altRow = !altRow;
                        }
                    }
                }
            }
            tableHtml.Append("</table>");
            return tableHtml.ToString();
        }

        private string RenderSectionHeading(string title) { return $@"<h3 style='color:#2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; margin-top: 40px; margin-bottom: 20px;'>{title}</h3>"; }
        private T GetValue<T>(string query, DateTime start, DateTime end) { using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WstGrp30ConnectionString"].ConnectionString)) { using (SqlCommand cmd = new SqlCommand(query, conn)) { cmd.Parameters.AddWithValue("@S", start); cmd.Parameters.AddWithValue("@E", end); conn.Open(); object result = cmd.ExecuteScalar(); if (result == null || result == DBNull.Value) return default(T); return (T)Convert.ChangeType(result, typeof(T)); } } }
        private T GetValueNoDates<T>(string query) { using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WstGrp30ConnectionString"].ConnectionString)) { using (SqlCommand cmd = new SqlCommand(query, conn)) { conn.Open(); object result = cmd.ExecuteScalar(); if (result == null || result == DBNull.Value) return default(T); return (T)Convert.ChangeType(result, typeof(T)); } } }
        private string GetStringValue(string query, DateTime start, DateTime end) { using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["WstGrp30ConnectionString"].ConnectionString)) { using (SqlCommand cmd = new SqlCommand(query, conn)) { cmd.Parameters.AddWithValue("@S", start); cmd.Parameters.AddWithValue("@E", end); conn.Open(); return cmd.ExecuteScalar()?.ToString(); } } }
        private string GenerateComparisonBadge(double current, double past) { if (past == 0) return "<span style='color:#95a5a6; font-size:0.8em'>(No prior data)</span>"; double diff = (current - past) / past * 100; if (diff > 0.1) return $"<span style='color:#27ae60; background:#e8f8f5; padding:2px 6px; border-radius:8px; font-weight:bold; font-size:0.9em'>📈 {diff:F0}% UP</span>"; else if (diff < -0.1) return $"<span style='color:#e74c3c; background:#fdedec; padding:2px 6px; border-radius:8px; font-weight:bold; font-size:0.9em'>📉 {Math.Abs(diff):F0}% DOWN</span>"; else return "<span style='color:#f39c12; font-weight:bold; font-size:0.9em'>➖ Flat</span>"; }
        private string RenderMetricCard(string title, string mainValue, string subText) { return $@"<div style='flex:1; background:#fff; border:1px solid #e0e0e0; border-radius:10px; padding:20px; box-shadow:0 2px 8px rgba(0,0,0,0.05); min-width: 200px;'><div style='color:#7f8c8d; font-size:0.85em; text-transform:uppercase; letter-spacing:1px; margin-bottom:10px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;'>{title}</div><div style='color:#2c3e50; font-size:2.2em; font-weight:bold; margin-bottom:10px; line-height:1;'>{mainValue}</div><div style='font-size:0.9em; min-height:20px;'>{subText}</div></div>"; }
        private (DateTime Start, DateTime End) GetSeasonDates(string seasonCode, int year) { switch (seasonCode) { case "Easter": return (new DateTime(year, 3, 1), new DateTime(year, 4, 30)); case "Winter": return (new DateTime(year, 5, 1), new DateTime(year, 7, 31)); case "Spring": return (new DateTime(year, 8, 1), new DateTime(year, 10, 31)); case "BlackFriday": return (new DateTime(year, 11, 1), new DateTime(year, 11, 30)); case "HolidayRush": return (new DateTime(year, 12, 1), new DateTime(year, 12, 31)); default: return (DateTime.MinValue, DateTime.MinValue); } }
        private void ShowErrorAlert(string msg) { ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('{msg.Replace("'", "\'")}');", true); }
    }
}