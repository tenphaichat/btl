using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.Utils;

namespace QLCuaHangSach.GUI;

/// <summary>Giao diện 1: Đăng nhập hệ thống và phân quyền theo vai trò.</summary>
public class frmLogin : Form
{
    private readonly AuthBUS _authBus = new AuthBUS();
    public TextBox txtUsername;
    public TextBox txtPassword;
    public CheckBox chkShowPass;
    public Button btnLogin;
    public Button btnExit;
    public Label lblMessage;

    public frmLogin()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Đăng nhập - Hệ thống Quản lý Cửa hàng Sách Đông Á";
        Size = new Size(760, 440);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;

        // Left Branding Panel
        var pnlLeft = new Panel
        {
            Dock = DockStyle.Left,
            Width = 310,
            BackColor = UITheme.Primary
        };
        var lblBrand = new Label
        {
            Text = "NHÀ SÁCH ĐÔNG Á\nBOOKSTORE ERP",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(24, 55),
            AutoSize = true
        };
        var lblDesc = new Label
        {
            Text = "Phần mềm Quản lý Cửa hàng Sách\nHọc phần: Lập trình .NET (IT3212)\nĐề tài 20 - Khoa CNTT (EAUT)\n\n" +
                   "• Quản lý danh mục Sách & Kho\n" +
                   "• Bán hàng & Thanh toán POS\n" +
                   "• Tích điểm Khách hàng thân thiết\n" +
                   "• Thống kê doanh thu & Lợi nhuận",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(219, 234, 254),
            Location = new Point(24, 130),
            Size = new Size(265, 220)
        };
        pnlLeft.Controls.Add(lblBrand);
        pnlLeft.Controls.Add(lblDesc);

        // Right Login Form Panel
        var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(36) };
        var lblHeader = new Label
        {
            Text = "ĐĂNG NHẬP HỆ THỐNG",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = UITheme.Primary,
            Location = new Point(350, 38),
            AutoSize = true
        };

        var lblUser = new Label
        {
            Text = "Tên đăng nhập:",
            Font = UITheme.FontBold,
            Location = new Point(350, 95),
            AutoSize = true
        };
        txtUsername = new TextBox
        {
            Font = new Font("Segoe UI", 11F),
            Location = new Point(350, 120),
            Width = 340
        };

        var lblPass = new Label
        {
            Text = "Mật khẩu:",
            Font = UITheme.FontBold,
            Location = new Point(350, 168),
            AutoSize = true
        };
        txtPassword = new TextBox
        {
            Font = new Font("Segoe UI", 11F),
            Location = new Point(350, 193),
            Width = 340,
            UseSystemPasswordChar = true
        };

        chkShowPass = new CheckBox
        {
            Text = "Hiển thị mật khẩu",
            Font = UITheme.FontRegular,
            Location = new Point(350, 230),
            AutoSize = true
        };
        chkShowPass.CheckedChanged += (s, e) => txtPassword.UseSystemPasswordChar = !chkShowPass.Checked;

        lblMessage = new Label
        {
            Text = "Nhập tài khoản được cấp. Tài khoản dùng thử: xem file README.md",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(350, 262),
            Size = new Size(360, 34)
        };

        btnLogin = UITheme.CreateButton("ĐĂNG NHẬP", UITheme.Primary, 190, 40);
        btnLogin.Location = new Point(350, 310);
        btnLogin.Click += BtnLogin_Click;

        btnExit = UITheme.CreateButton("THOÁT", Color.FromArgb(100, 116, 139), 130, 40);
        btnExit.Location = new Point(560, 310);
        btnExit.Click += (s, e) => Application.Exit();

        AcceptButton = btnLogin;

        Controls.Add(lblHeader);
        Controls.Add(lblUser);
        Controls.Add(txtUsername);
        Controls.Add(lblPass);
        Controls.Add(txtPassword);
        Controls.Add(chkShowPass);
        Controls.Add(lblMessage);
        Controls.Add(btnLogin);
        Controls.Add(btnExit);
        Controls.Add(pnlLeft);
    }

    // Chống dò mật khẩu (Lab 5): sai 3 lần liên tiếp thì khóa đăng nhập 30 giây.
    private const int SoLanSaiToiDa = 3;
    private const int SoGiayKhoa = 30;
    private int _soLanSai;
    private DateTime _khoaDen = DateTime.MinValue;

    private void BtnLogin_Click(object sender, EventArgs e)
    {
        if (DateTime.Now < _khoaDen)
        {
            int conLai = (int)Math.Ceiling((_khoaDen - DateTime.Now).TotalSeconds);
            lblMessage.ForeColor = UITheme.Danger;
            lblMessage.Text = $"Bạn đã nhập sai {SoLanSaiToiDa} lần. Vui lòng thử lại sau {conLai} giây.";
            return;
        }

        try
        {
            _authBus.Login(txtUsername.Text, txtPassword.Text);
            _soLanSai = 0;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblMessage.ForeColor = UITheme.Danger;
            lblMessage.Text = ErrorHandler.Message(ex);

            if (ex is BusinessException && !string.IsNullOrWhiteSpace(txtPassword.Text)
                && !string.IsNullOrWhiteSpace(txtUsername.Text) && ++_soLanSai >= SoLanSaiToiDa)
            {
                _soLanSai = 0;
                _khoaDen = DateTime.Now.AddSeconds(SoGiayKhoa);
                AuditBUS.Ghi("KHOA_DANG_NHAP", txtUsername.Text.Trim(), $"Sai {SoLanSaiToiDa} lần liên tiếp",
                             null, false, txtUsername.Text.Trim());
                lblMessage.Text = $"Sai {SoLanSaiToiDa} lần liên tiếp. Đăng nhập bị khóa {SoGiayKhoa} giây.";
            }
        }
    }
}
