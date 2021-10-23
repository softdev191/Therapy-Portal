using System.Web;
using System.Web.Optimization;

namespace TherapyCorner.Portal
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/commonscripts.js",
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery.tablesorter.min.js",
                        "~/Scripts/jquery.timepicker.min.js",
                        "~/Scripts/jquery-ui-timepicker-addon.min.js",
                        "~/Scripts/jquery-ui-sliderAccess.js",
                        "~/Scripts/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/counterscripts").Include(
                     "~/Scripts/counterloadingscripts.js"));

            bundles.Add(new ScriptBundle("~/bundles/claimsearch").Include(
         "~/Scripts/claimsearchresults.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/staffdetails").Include(
          "~/Scripts/staffdetailscripts.js",
          "~/Scripts/staffcredentialsscript.js"));

            bundles.Add(new ScriptBundle("~/bundles/createmessage").Include(
"~/Scripts/clientselectionscripts.js",
"~/Scripts/messagescripts.js"));

            bundles.Add(new ScriptBundle("~/bundles/calendar").Include(
"~/Scripts/calendarscripts.js",
"~/Scripts/moment.js",
"~/Scripts/fullcalendar.js"));

            bundles.Add(new ScriptBundle("~/bundles/createappointment").Include(
"~/Scripts/calendarscripts.js",
"~/Scripts/moment.js",
"~/Scripts/fullcalendar.js",
"~/Scripts/createappointmentscripts.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                                          "~/Content/themes/base/core.css",
                        "~/Content/themes/base/resizable.css",
                                  "~/Content/themes/base/button.css",
                        "~/Content/themes/base/dialog.css",
                                           "~/Content/themes/base/theme.css",
                     "~/Content/themes/base/datepicker.css",
                     "~/Content/themes/base/draggable.css",
                     "~/Content/themes/base/autocomplete.css",
                     "~/Content/themes/base/spinner.css",
                      "~/Content/themes/base/slider.css",
                     "~/Content/site.css",
                     "~/Content/jquery.timepicker.min.css",
                      "~/Content/jquery-ui-timepicker-addon.min.css"));

            bundles.Add(new StyleBundle("~/Content/tabs").Include(
"~/Content/themes/base/tabs.css",
          "~/Content/tabsuppl.css"));

            bundles.Add(new StyleBundle("~/Content/calendar").Include(
"~/Content/fullcalendar.min.css"));

            bundles.Add(new StyleBundle("~/Content/staffdetails").Include(
"~/Content/staffdetails.css",
"~/Content/credentialvalidationstyles.css"));

        }
    }
}
