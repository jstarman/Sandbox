<%@ Page Language="C#" %>
<%
Response.Write(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
%>
<br/>
<br/>
<%
foreach (string var in Request.ServerVariables)
{
  Response.Write(var + " " + Request[var] + "<br>");
}
%>