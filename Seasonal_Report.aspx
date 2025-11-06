<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Seasonal_Report.aspx.cs" Inherits="Mckaylan_Motor_Engineering.Private_Pages.Seasonal_Report" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Seasonal Performance Report</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <style type="text/css">
        body { font-family: 'Segoe UI', Arial, sans-serif; background-color: #f5f7fa; margin: 0; padding: 20px; }
        .report-container { max-width: 1000px; margin: 0 auto; background-color: #fff; padding: 30px; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }
        .header-section { text-align: center; margin-bottom: 30px; border-bottom: 2px solid #FA8C1E; padding-bottom: 20px; }
        .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 25px; margin-bottom: 30px; background-color: #f8f9fa; padding: 25px; border-radius: 8px; }
        .control-label { font-weight: bold; color: #2c3e50; display: block; margin-bottom: 8px; }
        .styled-dropdown { width: 100%; padding: 10px; border: 1px solid #ced4da; border-radius: 5px; font-size: 16px; }
        .btn-generate { width: 100%; padding: 15px; background-color: #FA8C1E; color: white; border: none; border-radius: 8px; font-size: 18px; font-weight: bold; cursor: pointer; transition: background-color 0.3s; }
        .btn-generate:hover { background-color: #e67e22; }
        .action-bar { display: flex; justify-content: flex-end; margin-bottom: 20px; padding-top: 20px; border-top: 1px solid #eee; }
        .btn-print { background-color: #2c3e50; color: white; padding: 10px 25px; border: none; border-radius: 5px; cursor: pointer; font-size: 16px; }

        /* MODAL STYLES */
        .modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.5); z-index: 1000; display: flex; justify-content: center; align-items: center; }
        .modal-box { background-color: white; padding: 30px; border-radius: 12px; box-shadow: 0 5px 25px rgba(0,0,0,0.3); width: 509px; max-width: 90%; }
        .modal-header { font-size: 22px; font-weight: bold; color: #2c3e50; margin-bottom: 15px; border-bottom: 2px solid #FA8C1E; padding-bottom: 10px; }
        .modal-body { background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin-bottom: 20px; font-size: 16px; line-height: 1.6; }
        .modal-actions { display: flex; justify-content: flex-end; gap: 15px; }
        .btn-confirm { background-color: #27ae60; color: white; border: none; padding: 12px 25px; border-radius: 6px; font-weight: bold; cursor: pointer; }
        .btn-cancel { background-color: #95a5a6; color: white; border: none; padding: 12px 20px; border-radius: 6px; cursor: pointer; }

        /* Styled Checkbox List for Multi-Select */
        .checkbox-group { border: 1px solid #ced4da; border-radius: 5px; padding: 10px; background-color: #fff; max-height: 150px; overflow-y: auto; }
        .checkbox-group table { width: 100%; }
        .checkbox-group td { padding: 5px; }
        .checkbox-group label { margin-left: 8px; color: #2c3e50; cursor: pointer; }

        @media print {
            body { background-color: white; padding: 0; }
            .report-container { box-shadow: none; max-width: 100%; }
            .no-print { display: none !important; }
        }
    </style>
    <script type="text/javascript">
        function printReport() { window.print(); }
    </script>
</head>
    
<body>
    <form id="form1" runat="server">
        <div class="report-container">
            <div class="header-section">
              
                <h1 style="color: #333; margin: 0;">McKaylan Motor Engineering C.C</h1>
                <h2 style="color: #FA8C1E; margin-top: 10px;">Seasonal Performance Report</h2>
            </div>

            <div class="no-print">
                <div class="form-grid">
                    <div>
                        <asp:Label ID="lblSeason" runat="server" Text="1. Select Business Season" CssClass="control-label"></asp:Label>
                        <asp:DropDownList ID="ddlSeason" runat="server" CssClass="styled-dropdown">
                            <asp:ListItem Text="-- Select Season --" Value="" />
                            <asp:ListItem Value="Easter">🐰 Easter Travel (Mar-Apr)</asp:ListItem>
                            <asp:ListItem Value="Winter">❄️ Winter Maintenance (May-Jul)</asp:ListItem>
                            <asp:ListItem Value="Spring">🌻 Spring Ready (Aug-Oct)</asp:ListItem>
                            <asp:ListItem Value="BlackFriday">🏷️ Black Friday (Nov)</asp:ListItem>
                            <asp:ListItem Value="HolidayRush">🎄 Holiday Rush (Dec)</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div>
                        <asp:Label ID="lblYear" runat="server" Text="2. Select Year" CssClass="control-label"></asp:Label>
                        <asp:DropDownList ID="ddlYear" runat="server" CssClass="styled-dropdown"></asp:DropDownList>
                    </div>
                    <div>
                        <asp:Label ID="lblFocus" runat="server" Text="3. Report Focus Area(s)" CssClass="control-label"></asp:Label>
                        <div class="checkbox-group">
                            <asp:CheckBoxList ID="cblReportFocus" runat="server" RepeatLayout="Table" CellSpacing="0" CellPadding="0">
                                <asp:ListItem Text="📊 Operational Volume (Demand)" Value="Load" Selected="True" />
                                <asp:ListItem Text="⚙️ Workforce Efficiency (Staff)" Value="Staff" />
                                <asp:ListItem Text="👥 Client Acquisition & Retention" Value="Customer" />
                            </asp:CheckBoxList>
                        </div>
                        <div style="font-size: 0.85em; color: #7f8c8d; margin-top: 5px;">
                            * Tick multiple boxes to generate a combined report.
                        </div>
                    </div>
                    <div>
                        <asp:Label ID="lblComparison" runat="server" Text="4. Compare Against" CssClass="control-label"></asp:Label>
                        <asp:DropDownList ID="ddlComparison" runat="server" CssClass="styled-dropdown">
                            <asp:ListItem Text="-- No Comparison --" Value="None" />
                            <asp:ListItem Text="Same Season Last Year (YoY)" Value="LastYear" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div style="margin-bottom: 25px;">
                    <label style="display: flex; align-items: center; cursor: pointer; font-size: 16px;">
                        <asp:CheckBox ID="chkShowCharts" runat="server" />
                        <span style="margin-left: 10px;">Include Visual Performance Charts 📈</span>
                    </label>
                </div>
                <asp:Button ID="btnGenerate" runat="server" Text="REVIEW & GENERATE REPORT" CssClass="btn-generate" OnClick="btnRequest_Click" />
            </div>

            <asp:Panel ID="pnlConfirmModal" runat="server" CssClass="modal-overlay" Visible="false">
                <div class="modal-box">
                    <div class="modal-header">⚠️ Confirm Report Details</div>
                    <div class="modal-body">
                        Please confirm parameters before generating this report:
                        <ul style="margin-top: 10px;">
                            <li><strong>Season:</strong> <asp:Label ID="lblConfirmSeason" runat="server" /> (<asp:Label ID="lblConfirmYear" runat="server" />)</li>
                            <li><strong>Date Range:</strong> <asp:Label ID="lblConfirmDates" runat="server" /></li>
                            <li><strong>Focus Area(s):</strong> <br/><asp:Label ID="lblConfirmFocus" runat="server" style="margin-left: 15px; display: block; margin-top: 5px;" /></li>
                            <li><strong>Comparison:</strong> <asp:Label ID="lblConfirmCompare" runat="server" ForeColor="#E67E22" Font-Bold="true" /></li>
                        </ul>
                    </div>
                    <div class="modal-actions">
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-cancel" OnClick="btnCancel_Click" CausesValidation="false" />
                        <asp:Button ID="btnFinalConfirm" runat="server" Text="✅ CONFIRM & RUN" CssClass="btn-confirm" OnClick="btnFinalConfirm_Click" />
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlReportResults" runat="server" Visible="false" style="margin-top: 40px;">
                <div class="action-bar no-print">
                    <button type="button" class="btn-print" onclick="printReport()">🖨️ Print / Save as PDF</button>
                </div>
                <div id="printableArea">
                    <h3 style="color: #2c3e50; border-bottom: 1px solid #eee; padding-bottom: 10px;">
                        RESULTS: <asp:Label ID="lblReportTitle" runat="server"></asp:Label>
                    </h3>
                    <div style="padding: 20px; background: #f9f9f9; border-radius: 8px; margin-top: 20px;">
                        <p><strong>Reporting Period:</strong> <asp:Label ID="lblResultSeason" runat="server"></asp:Label></p>
                        <asp:Label ID="lblResultFocus" runat="server"></asp:Label>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </form>
</body>
</html>