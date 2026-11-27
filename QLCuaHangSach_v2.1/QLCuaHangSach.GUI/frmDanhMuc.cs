using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QLCuaHangSach.BUS;
using QLCuaHangSach.DTO;

namespace QLCuaHangSach.GUI;

/// <summary>
/// Giao diện 4: Quản lý Danh mục Thể loại, Tác giả, Nhà xuất bản, Nhà cung cấp.
/// Cả 4 danh mục đều có đủ Thêm - Sửa - Xóa; chọn một dòng trên lưới để nạp lên ô nhập.
/// </summary>
public class frmDanhMuc : Form
{
    private readonly DanhMucBUS _bus = new DanhMucBUS();
    private readonly List<Action> _lamMoiCacTab = new List<Action>();

    public frmDanhMuc()
    {
        InitializeComponent();
        LoadAllTabs();
    }

    private void InitializeComponent()
    {
        Text = "Quản lý Danh mục Dùng chung (Thể loại - Tác giả - NXB - Nhà cung cấp)";
        Size = new Size(1000, 620);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UITheme.BgLight;

        var pnlHeader = UITheme.CreateHeaderBanner(
            "QUẢN LÝ DANH MỤC THỂ LOẠI, TÁC GIẢ, NXB & NHÀ CUNG CẤP",
            "Chuẩn hóa dữ liệu danh mục phục vụ tra cứu và nhập/xuất sách");

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = UITheme.FontBold, Padding = new Point(14, 6) };

        tabs.TabPages.Add(TaoTab("1. Thể loại Sách",
            new[] { "Mã thể loại", "Tên thể loại", "Mô tả" },
            _bus.GetTheLoai,
            (v, isNew) => _bus.LuuTheLoai(v[0], v[1], v[2], isNew),
            _bus.XoaTheLoai));

        tabs.TabPages.Add(TaoTab("2. Tác giả",
            new[] { "Mã tác giả", "Tên tác giả", "Quốc tịch" },
            _bus.GetTacGia,
            (v, isNew) => _bus.LuuTacGia(v[0], v[1], v[2], isNew),
            _bus.XoaTacGia));

        tabs.TabPages.Add(TaoTab("3. Nhà xuất bản",
            new[] { "Mã NXB", "Tên nhà xuất bản", "Địa chỉ" },
            _bus.GetNhaXuatBan,
            (v, isNew) => _bus.LuuNhaXuatBan(v[0], v[1], v[2], isNew),
            _bus.XoaNhaXuatBan));

        tabs.TabPages.Add(TaoTab("4. Nhà cung cấp",
            new[] { "Mã NCC", "Tên nhà cung cấp", "Địa chỉ", "Điện thoại", "Email" },
            _bus.GetNhaCungCap,
            (v, isNew) => _bus.LuuNhaCungCap(new NhaCungCap
            {
                MaNCC = v[0].Trim(),
                TenNCC = v[1].Trim(),
                DiaChi = v[2],
                SoDienThoai = v[3],
                Email = v[4]
            }, isNew),
            _bus.XoaNhaCungCap));

        Controls.Add(tabs);
        Controls.Add(pnlHeader);
    }

    /// <summary>
    /// Dựng một tab CRUD: các ô nhập (theo nhan), lưới dữ liệu và 4 nút Thêm/Sửa/Xóa/Nhập mới.
    /// Cột đầu tiên của lưới luôn là mã và khớp với ô nhập đầu tiên.
    /// </summary>
    private TabPage TaoTab(string tieuDe, string[] nhan, Func<DataTable> taiDuLieu,
                           Func<string[], bool, bool> luu, Func<string, bool> xoa)
    {
        var tab = new TabPage(tieuDe);
        var pnl = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White };
        var dgv = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(dgv);

        var txt = new TextBox[nhan.Length];
        int x = 15;
        for (int i = 0; i < nhan.Length; i++)
        {
            int rong = i == 0 ? 100 : (nhan.Length > 3 ? 150 : 230);
            pnl.Controls.Add(new Label { Text = nhan[i] + ":", Location = new Point(x, 10), AutoSize = true, Font = UITheme.FontRegular });
            txt[i] = new TextBox { Location = new Point(x, 30), Width = rong, Font = UITheme.FontRegular };
            pnl.Controls.Add(txt[i]);
            x += rong + 15;
        }

        Action lamMoi = () =>
        {
            try { dgv.DataSource = taiDuLieu(); }
            catch (Exception ex) { ErrorHandler.Show(this, ex); }
        };
        _lamMoiCacTab.Add(lamMoi);

        string[] DocO() => txt.Select(t => t.Text).ToArray();

        void ThucHien(bool isNew)
        {
            try
            {
                luu(DocO(), isNew);
                lamMoi();
                MessageBox.Show(this, isNew ? "Đã thêm mới." : "Đã cập nhật.", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ErrorHandler.Show(this, ex); }
        }

        var btnThem = UITheme.CreateButton("Thêm mới", UITheme.Success, 110, 30);
        btnThem.Location = new Point(15, 62);
        btnThem.Click += (s, e) => ThucHien(true);

        var btnSua = UITheme.CreateButton("Cập nhật", UITheme.Accent, 110, 30);
        btnSua.Location = new Point(135, 62);
        btnSua.Click += (s, e) => ThucHien(false);

        var btnXoa = UITheme.CreateButton("Xóa", UITheme.Danger, 90, 30);
        btnXoa.Location = new Point(255, 62);
        btnXoa.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txt[0].Text)) return;
            if (MessageBox.Show(this, $"Xóa \"{txt[0].Text}\"?", "Xác nhận xóa",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                xoa(txt[0].Text.Trim());
                foreach (TextBox t in txt) t.Clear();
                lamMoi();
            }
            catch (Exception ex) { ErrorHandler.Show(this, ex); }
        };

        var btnMoi = UITheme.CreateButton("Nhập mới", UITheme.PrimaryDark, 100, 30);
        btnMoi.Location = new Point(355, 62);
        btnMoi.Click += (s, e) => { foreach (TextBox t in txt) t.Clear(); txt[0].Focus(); };

        pnl.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnMoi });

        // Chọn dòng trên lưới -> nạp lên các ô nhập
        dgv.SelectionChanged += (s, e) =>
        {
            if (dgv.CurrentRow == null) return;
            for (int i = 0; i < txt.Length && i < dgv.Columns.Count; i++)
                txt[i].Text = dgv.CurrentRow.Cells[i].Value?.ToString() ?? "";
        };

        tab.Controls.Add(dgv);
        tab.Controls.Add(pnl);
        return tab;
    }

    private void LoadAllTabs()
    {
        foreach (Action lamMoi in _lamMoiCacTab) lamMoi();
    }
}
