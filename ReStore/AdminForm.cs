using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Globalization;

namespace ReStore
{
    public class AdminForm : Form
    {
        private Label label;
        private DataGridView grid;
        private Button btnMakeAdmin;
        private Button btnDeleteUser;
        private TextBox textBoxSearch;
        private Button btnSearch;
        private ReStore.Dao.KullaniciDao dao = new ReStore.Dao.KullaniciDao();
        public AdminForm()
        {
            InitializeComponents();
        }

        private void OpenCouponManager()
        {
            var dlg = new Form { Text = "Kupon Yönetimi", ClientSize = new Size(700, 480), StartPosition = FormStartPosition.CenterParent };
            var grid = new DataGridView
            {
                Location = new Point(20, 50),
                Size = new Size(640, 260),
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "Id", Width = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kod", DataPropertyName = "Code", Width = 180 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İndirim %", DataPropertyName = "Percent", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tutar", DataPropertyName = "Amount", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Son Kullanma", DataPropertyName = "ExpiresAt", Width = 120 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Aktif", DataPropertyName = "Active", Width = 60 });

            var couponDao = new ReStore.Dao.CouponDao();
            void LoadCoupons(string? filter = null)
            {
                var list = couponDao.GetAll();
                if (!string.IsNullOrWhiteSpace(filter)) list = list.Where(c => (c.Code ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                grid.DataSource = null;
                grid.DataSource = list;
            }

            var lblFilter = new Label { Text = "Filtre (kod):", Location = new Point(20, 20), Size = new Size(80, 22) };
            var txtFilter = new TextBox { Location = new Point(100, 18), Size = new Size(160, 22) };
            var btnFilter = new Button { Text = "Uygula", Location = new Point(270, 16), Size = new Size(80, 26) };
            btnFilter.Click += (s, e) => LoadCoupons(txtFilter.Text?.Trim());

            var btnAdd = new Button { Text = "Yeni Kupon Ekle", Location = new Point(20, 330), Size = new Size(120, 28) };
            btnAdd.Click += (s, e) =>
            {
                var addDlg = new Form { Text = "Kupon Ekle", ClientSize = new Size(460, 320), StartPosition = FormStartPosition.CenterParent };
                var txtCode = new TextBox { Location = new Point(160, 20), Width = 260 };
                var lblCode = new Label { Text = "Kod:", Location = new Point(20, 22), Size = new Size(120, 24) };
                var lblPercent = new Label { Text = "Yüzde:", Location = new Point(20, 62), Size = new Size(120, 24) };
                var txtPercent = new TextBox { Location = new Point(160, 60), Width = 260 };
                var lblExpires = new Label { Text = "Son Kullanma (ops):", Location = new Point(20, 102), Size = new Size(120, 24) };
                var dtExpires = new DateTimePicker { Location = new Point(160, 100), Width = 260, Format = DateTimePickerFormat.Short };
                // default 30 day option
                var chkDefault30 = new CheckBox { Text = "Varsayılan 30 gün (oluşturulduktan sonra)", Location = new Point(160, 132), Checked = true, AutoSize = true };
                var chkActive = new CheckBox { Text = "Aktif", Location = new Point(160, 160), Checked = true };
                var btnSave = new Button { Text = "Kaydet", Location = new Point(160, 200), Size = new Size(100, 28) };

                chkDefault30.CheckedChanged += (cs, ce) =>
                {
                    // if default selected, disable manual date selection
                    dtExpires.Enabled = !chkDefault30.Checked;
                };

                // initialize dtExpires to 30 days ahead but disabled because default is checked
                dtExpires.Value = DateTime.Now.Date.AddDays(30);
                dtExpires.Enabled = false;

                btnSave.Click += (se, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtCode.Text)) { MessageBox.Show("Kod girin."); return; }
                    decimal? percent = null;
                    if (decimal.TryParse(txtPercent.Text, out decimal p)) percent = p;
                    DateTime? exp = null;
                    if (chkDefault30.Checked)
                    {
                        exp = DateTime.Now.Date.AddDays(30);
                    }
                    else
                    {
                        if (dtExpires.Value.Date > DateTime.Now.Date) exp = dtExpires.Value.Date;
                    }
                    var c = new ReStore.Models.Coupon { Code = txtCode.Text.Trim(), Percent = percent, Amount = null, ExpiresAt = exp, Active = chkActive.Checked };
                    var ok2 = couponDao.AddCoupon(c);
                    if (ok2) { MessageBox.Show("Kupon eklendi."); addDlg.Close(); LoadCoupons(txtFilter.Text?.Trim()); }
                    else MessageBox.Show("Eklenemedi.");
                };

                addDlg.Controls.AddRange(new Control[] { lblCode, txtCode, lblPercent, txtPercent, lblExpires, dtExpires, chkDefault30, chkActive, btnSave });
                addDlg.ShowDialog();
            };

            var btnEdit = new Button { Text = "Düzenle", Location = new Point(160, 330), Size = new Size(120, 28) };
            btnEdit.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("Kupon seçin."); return; }
                var c = grid.SelectedRows[0].DataBoundItem as ReStore.Models.Coupon;
                if (c == null) return;
                var ed = new Form { Text = "Kupon Düzenle", ClientSize = new Size(420, 260), StartPosition = FormStartPosition.CenterParent };
                var lblCode = new Label { Text = "Kod:", Location = new Point(20, 22), Size = new Size(120, 24) };
                var txtCode = new TextBox { Location = new Point(160, 20), Width = 220, Text = c.Code };
                var lblPercent = new Label { Text = "Yüzde:", Location = new Point(20, 62), Size = new Size(120, 24) };
                var txtPercent = new TextBox { Location = new Point(160, 60), Width = 220, Text = c.Percent?.ToString() ?? string.Empty };
                var lblExpires = new Label { Text = "Son Kullanma (ops):", Location = new Point(20, 102), Size = new Size(120, 24) };
                var dtExpires = new DateTimePicker { Location = new Point(160, 100), Width = 220, Format = DateTimePickerFormat.Short };
                if (c.ExpiresAt.HasValue) dtExpires.Value = c.ExpiresAt.Value;
                var chkActive = new CheckBox { Text = "Aktif", Location = new Point(160, 140), Checked = c.Active };
                var btnSave = new Button { Text = "Güncelle", Location = new Point(160, 180), Size = new Size(100, 28) };
                btnSave.Click += (se, ev) =>
                {
                    // validate code
                    var newCode = txtCode.Text?.Trim() ?? string.Empty;
                    if (string.IsNullOrEmpty(newCode)) { MessageBox.Show("Kod girin."); return; }
                    var exists = couponDao.GetByCode(newCode);
                    if (exists != null && exists.Id != c.Id)
                    {
                        MessageBox.Show("Bu kod zaten mevcut. Lütfen farklı bir kod girin.");
                        return;
                    }

                    decimal? percent = null;
                    if (decimal.TryParse(txtPercent.Text, out decimal p)) percent = p;
                    DateTime? exp = null;
                    // if user set a date in future, use it
                    if (dtExpires.Value.Date > DateTime.Now.Date) exp = dtExpires.Value.Date;
                    c.Code = newCode;
                    c.Percent = percent;
                    c.Amount = null;
                    c.ExpiresAt = exp;
                    c.Active = chkActive.Checked;
                    var ok2 = couponDao.UpdateCoupon(c);
                    if (ok2) { MessageBox.Show("Güncellendi."); ed.Close(); LoadCoupons(txtFilter.Text?.Trim()); }
                    else MessageBox.Show("Güncellenemedi.");
                };
                ed.Controls.AddRange(new Control[] { lblCode, txtCode, lblPercent, txtPercent, lblExpires, dtExpires, chkActive, btnSave });
                ed.ShowDialog();
            };

            var btnAssign = new Button { Text = "Kullanıcıya Ata", Location = new Point(300, 330), Size = new Size(120, 28) };
            var btnAssignAll = new Button { Text = "Tüm Kullanıcılara Ata", Location = new Point(300, 370), Size = new Size(140, 28) };
            var cmbUsers = new ComboBox { Location = new Point(440, 330), Size = new Size(220, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            // populate users
            try
            {
                var users = new ReStore.Dao.KullaniciDao().GetAllUsers();
                foreach (var u in users) cmbUsers.Items.Add(u.KullaniciEmail);
                if (cmbUsers.Items.Count > 0) cmbUsers.SelectedIndex = 0;
            }
            catch { }
            btnAssign.Click += (s, e) =>
            {
                // try to use current cell/row if SelectedRows is empty (happens if SelectionMode issues)
                ReStore.Models.Coupon? c = null;
                if (grid.SelectedRows.Count > 0)
                {
                    c = grid.SelectedRows[0].DataBoundItem as ReStore.Models.Coupon;
                }
                else if (grid.CurrentRow != null)
                {
                    c = grid.CurrentRow.DataBoundItem as ReStore.Models.Coupon;
                }

                if (c == null) { MessageBox.Show("Kupon seçin."); return; }
                if (cmbUsers.SelectedItem == null) { MessageBox.Show("Kullanıcı seçin."); return; }
                var mail = cmbUsers.SelectedItem.ToString() ?? string.Empty;
                var ok2 = couponDao.AssignCouponToUser(mail, c.Code);
                MessageBox.Show(ok2 ? "Kupon kullanıcıya atandı." : "Atama başarısız.");
            };

            btnAssignAll.Click += (s, e) =>
            {
                ReStore.Models.Coupon? c = null;
                if (grid.SelectedRows.Count > 0) c = grid.SelectedRows[0].DataBoundItem as ReStore.Models.Coupon;
                else if (grid.CurrentRow != null) c = grid.CurrentRow.DataBoundItem as ReStore.Models.Coupon;
                if (c == null) { MessageBox.Show("Kupon seçin."); return; }
                var resAll = MessageBox.Show($"'{c.Code}' kuponunu tüm kullanıcılara atamak istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resAll != DialogResult.Yes) return;
                var okAll = couponDao.AssignCouponToAllUsers(c.Code);
                MessageBox.Show(okAll ? "Kupon tüm kullanıcılara atandı." : "Toplu atama başarısız.");
            };

            var btnDelete = new Button { Text = "Seçiliyi Sil", Location = new Point(440, 370), Size = new Size(120, 28) };
            btnDelete.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("Kupon seçin."); return; }
                var c = grid.SelectedRows[0].DataBoundItem as ReStore.Models.Coupon;
                if (c == null) return;
                var ok = couponDao.DeleteCouponByCode(c.Code);
                if (ok) { MessageBox.Show("Silindi."); LoadCoupons(txtFilter.Text?.Trim()); }
                else MessageBox.Show("Silinemedi.");
            };

            var btnExport = new Button { Text = "CSV Dışa Aktar", Location = new Point(20, 410), Size = new Size(140, 28) };
            btnExport.Click += (s, e) =>
            {
                try
                {
                    var list = grid.DataSource as System.Collections.IEnumerable;
                    var coupons = new System.Collections.Generic.List<ReStore.Models.Coupon>();
                    if (list != null)
                    {
                        foreach (var it in list)
                        {
                            if (it is ReStore.Models.Coupon cc) coupons.Add(cc);
                        }
                    }
                    else
                    {
                        coupons = couponDao.GetAll();
                    }

                    var sfd = new SaveFileDialog { Filter = "CSV dosyası|*.csv", FileName = "coupons.csv" };
                    if (sfd.ShowDialog() != DialogResult.OK) return;
                    var sb = new StringBuilder();
                    sb.AppendLine("Code,Percent,Amount,ExpiresAt,Active");
                    foreach (var c in coupons)
                    {
                        var code = c.Code?.Replace("\"", "\"\"") ?? string.Empty;
                        var percent = c.Percent.HasValue ? c.Percent.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                        var amount = c.Amount.HasValue ? c.Amount.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                        var exp = c.ExpiresAt.HasValue ? c.ExpiresAt.Value.ToString("o", CultureInfo.InvariantCulture) : string.Empty;
                        var active = c.Active ? "1" : "0";
                        sb.AppendLine($"\"{code}\",{percent},{amount},{exp},{active}");
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("CSV dışa aktarıldı.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("CSV dışa aktarılırken hata: " + ex.Message);
                }
            };

            var btnImport = new Button { Text = "CSV İçe Aktar", Location = new Point(180, 410), Size = new Size(140, 28) };
            btnImport.Click += (s, e) =>
            {
                try
                {
                    var ofd = new OpenFileDialog { Filter = "CSV dosyası|*.csv" };
                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    var lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                    int added = 0;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i].Trim();
                        if (i == 0 && line.IndexOf("Code", StringComparison.OrdinalIgnoreCase) >= 0) continue; // header
                        if (string.IsNullOrEmpty(line)) continue;
                        // simple CSV parse: split by comma, remove quotes
                        var parts = line.Split(',');
                        if (parts.Length < 5) continue;
                        string code = parts[0].Trim().Trim('"');
                        decimal? percent = null;
                        if (decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal p)) percent = p;
                        decimal? amount = null;
                        if (decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal a)) amount = a;
                        DateTime? exp = null;
                        if (DateTime.TryParse(parts[3], CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime dt)) exp = dt;
                        bool active = parts[4].Trim() == "1" || parts[4].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                        var c = new ReStore.Models.Coupon { Code = code, Percent = percent, Amount = amount, ExpiresAt = exp, Active = active };
                        var ok = couponDao.AddCoupon(c);
                        if (ok) added++;
                    }
                    LoadCoupons(txtFilter.Text?.Trim());
                    MessageBox.Show($"İçe aktarma tamamlandı. Yeni eklenen: {added}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("CSV içe aktarılırken hata: " + ex.Message);
                }
            };

            dlg.Controls.AddRange(new Control[] { lblFilter, txtFilter, btnFilter, grid, btnAdd, btnEdit, btnAssign, cmbUsers, btnAssignAll, btnDelete, btnExport, btnImport });
            LoadCoupons();
            // ensure no pre-selection and allow user to select rows reliably
            grid.ClearSelection();
            dlg.ShowDialog();
        }

        private void InitializeComponents()
        {
            this.Text = "Admin Paneli";
            this.ClientSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            label = new Label
            {
                Text = "Yönetici paneline hoş geldiniz.",
                Location = new Point(20, 20),
                Size = new Size(560, 40),
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };

            grid = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(560, 240),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false
            };

            textBoxSearch = new TextBox
            {
                PlaceholderText = "İsim veya e-posta ara",
                Location = new Point(20, 60),
                Size = new Size(360, 22)
            };

            btnSearch = new Button
            {
                Text = "Ara",
                Location = new Point(392, 58),
                Size = new Size(80, 26)
            };
            btnSearch.Click += BtnSearch_Click;

            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "Id", Width = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ad", DataPropertyName = "KullaniciAd", Width = 200 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "E-Posta", DataPropertyName = "KullaniciEmail", Width = 220 });
            grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Admin", DataPropertyName = "IsAdmin", Width = 50 });

            btnMakeAdmin = new Button
            {
                Text = "Admin Yap",
                Location = new Point(20, 330),
                Size = new Size(120, 30)
            };
            btnMakeAdmin.Click += BtnMakeAdmin_Click;

            btnDeleteUser = new Button
            {
                Text = "Kullanıcı Sil",
                Location = new Point(150, 330),
                Size = new Size(120, 30)
            };
            btnDeleteUser.Click += BtnDeleteUser_Click;

            var btnManageCoupons = new Button
            {
                Text = "Kupon Yönetimi",
                Location = new Point(280, 330),
                Size = new Size(120, 30)
            };
            btnManageCoupons.Click += (s, e) => OpenCouponManager();

            this.Controls.Add(textBoxSearch);
            this.Controls.Add(btnSearch);
            this.Controls.Add(grid);
            this.Controls.Add(btnMakeAdmin);
            this.Controls.Add(btnDeleteUser);
            this.Controls.Add(btnManageCoupons);

            this.Controls.Add(label);
            this.Load += AdminForm_Load;
        }

        private void AdminForm_Load(object? sender, EventArgs e)
        {
            var users = dao.GetAllUsers();
            grid.DataSource = users;
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            string? q = textBoxSearch.Text?.Trim();
            var all = dao.GetAllUsers();
            if (string.IsNullOrEmpty(q))
            {
                grid.DataSource = all;
                return;
            }

            var filtered = all.Where(u => (u.KullaniciAd ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                         || (u.KullaniciEmail ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                              .ToList();

            grid.DataSource = filtered;
        }

        private void BtnToggleAdmin_Click(object? sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir kullanıcı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = grid.SelectedRows[0];
            var user = row.DataBoundItem as ReStore.Models.Kullanici;
            if (user == null) return;

            // kept for compatibility but not used
        }

        private void BtnMakeAdmin_Click(object? sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir kullanıcı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = grid.SelectedRows[0];
            var user = row.DataBoundItem as ReStore.Models.Kullanici;
            if (user == null) return;

            if (user.IsAdmin)
            {
                MessageBox.Show("Seçili kullanıcı zaten admin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool ok = dao.SetAdmin(user.KullaniciEmail, true);
            if (ok)
            {
                MessageBox.Show("Kullanıcı admin yapıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                grid.DataSource = dao.GetAllUsers();
            }
            else
            {
                MessageBox.Show("İşlem başarısız.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteUser_Click(object? sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir kullanıcı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = grid.SelectedRows[0];
            var user = row.DataBoundItem as ReStore.Models.Kullanici;
            if (user == null) return;

            var res = MessageBox.Show($"{user.KullaniciEmail} kullanıcısını silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            bool ok = dao.DeleteUserByEmail(user.KullaniciEmail);
            if (ok)
            {
                MessageBox.Show("Kullanıcı silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                grid.DataSource = dao.GetAllUsers();
            }
            else
            {
                MessageBox.Show("Silme işlemi başarısız.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
