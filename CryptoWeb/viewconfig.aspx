<%@ Page Language="C#" %>

<%
    var settings = ConfigurationManager.AppSettings;
    foreach (var key in settings.AllKeys)
    {
        Response.Write(settings[key]);
    }
%>