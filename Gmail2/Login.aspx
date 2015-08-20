<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Gmail2.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Login with: 
        <asp:DropDownList ID="dropdown" runat="server" OnSelectedIndexChanged="dropdown_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
    </div>
    </form>
</body>
</html>
