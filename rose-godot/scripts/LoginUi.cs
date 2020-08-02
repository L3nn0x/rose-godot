using Godot;
using System;

public class LoginUi : Node2D
{
    NetworkManager Manager;
    Button LoginButton;
    Label ConnectionStatus;
    LineEdit Username;
    LineEdit Password;
        
    public override void _Ready()
    {
        LoginButton = GetNode("Login") as Button;
        ConnectionStatus = GetNode("ConnectionStatus") as Label;
        Username = GetNode("Username") as LineEdit;
        Password = GetNode("Password") as LineEdit;
        Manager = GetNode("/root/NetworkManager") as NetworkManager;
        Manager.Connect("LoginReplySignal", this, "OnLoginReply");
    }

    public void _on_Login_pressed()
    {
        if (Username.Text != "" && Password.Text != "")
        {
            LoginButton.Disabled = true;
            ConnectionStatus.Text = "Connecting...";
            Manager.SendLoginRequest(Username.Text, Password.Text);
        }
    }

    public void OnLoginReply(Rose.Network.Packets.LoginReplyValue ret)
    {
        if (ret == Rose.Network.Packets.LoginReplyValue.OK)
        {
            ConnectionStatus.Text = "Connected!";
        }
        else
        {
            ConnectionStatus.Text = "Connection failed";
        }
    }
}
