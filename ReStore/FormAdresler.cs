using ReStore.Dao;
using ReStore.Models;
using System.Data;

namespace ReStore
{
    public class FormAdresler : Form
    {
        private readonly int _kullaniciId;
        private readonly AddressDao _addressDao;

        private DataGridView dgvAdresler = null!;
        private TextBox txtBaslik = null!;
        private TextBox txtAdresDetay = null!;
        private ComboBox cmbSehir = null!;
        private ComboBox cmbIlce = null!;
        private CheckBox chkVarsayilan = null!;
        private Button btnEkle = null!;
        private Button btnGuncelle = null!;
        private Button btnSil = null!;
        private int _selectedAdresId;

        public FormAdresler(int kullaniciId)
        {
            _kullaniciId = kullaniciId;
            _addressDao = new AddressDao();
            InitializeComponents();
            LoadAddresses();
        }

        private void InitializeComponents()
        {
            Text = "Adreslerim";
            Size = new Size(900, 560);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.WhiteSmoke;

            dgvAdresler = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 260,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };
            dgvAdresler.SelectionChanged += DgvAdresler_SelectionChanged;

            var pnlForm = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };

            var lblBaslik = new Label { Text = "Başlık", Location = new Point(20, 20), AutoSize = true };
            txtBaslik = new TextBox { Location = new Point(140, 18), Width = 220 };

            var lblSehir = new Label { Text = "Şehir", Location = new Point(20, 60), AutoSize = true };
            cmbSehir = new ComboBox { Location = new Point(140, 58), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSehir.Items.AddRange(new object[] { "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Diğer" });
            cmbSehir.SelectedIndex = 0;

            var lblIlce = new Label { Text = "İlçe", Location = new Point(20, 100), AutoSize = true };
            cmbIlce = new ComboBox { Location = new Point(140, 98), Width = 220, DropDownStyle = ComboBoxStyle.DropDown };

            var lblAdres = new Label { Text = "Adres Detay", Location = new Point(20, 140), AutoSize = true };
            txtAdresDetay = new TextBox { Location = new Point(140, 138), Width = 420, Height = 100, Multiline = true, ScrollBars = ScrollBars.Vertical };

            chkVarsayilan = new CheckBox { Text = "Varsayılan adres yap", Location = new Point(140, 250), AutoSize = true };

            btnEkle = new Button { Text = "Ekle", Location = new Point(140, 290), Size = new Size(110, 34), BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGuncelle = new Button { Text = "Güncelle", Location = new Point(260, 290), Size = new Size(110, 34), BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSil = new Button { Text = "Sil", Location = new Point(380, 290), Size = new Size(110, 34), BackColor = Color.Firebrick, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnEkle.Click += (_, _) => AddAddress();
            btnGuncelle.Click += (_, _) => UpdateAddress();
            btnSil.Click += (_, _) => DeleteAddress();

            pnlForm.Controls.AddRange(new Control[] { lblBaslik, txtBaslik, lblSehir, cmbSehir, lblIlce, cmbIlce, lblAdres, txtAdresDetay, chkVarsayilan, btnEkle, btnGuncelle, btnSil });

            Controls.Add(pnlForm);
            Controls.Add(dgvAdresler);
        }

        private void LoadAddresses()
        {
            var addresses = _addressDao.GetByKullaniciId(_kullaniciId);
            var table = new DataTable();
            table.Columns.Add("AdresID", typeof(int));
            table.Columns.Add("Başlık", typeof(string));
            table.Columns.Add("Şehir", typeof(string));
            table.Columns.Add("İlçe", typeof(string));
            table.Columns.Add("Adres", typeof(string));
            table.Columns.Add("Varsayılan", typeof(bool));

            foreach (var a in addresses)
            {
                table.Rows.Add(a.AdresID, a.Baslik, a.Sehir, a.Ilce, a.AdresDetay, a.Varsayilan);
            }

            dgvAdresler.DataSource = table;
            if (dgvAdresler.Columns["AdresID"] != null)
            {
                dgvAdresler.Columns["AdresID"]!.Visible = false;
            }
        }

        private void DgvAdresler_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvAdresler.SelectedRows.Count == 0)
            {
                return;
            }

            var row = dgvAdresler.SelectedRows[0];
            _selectedAdresId = Convert.ToInt32(row.Cells["AdresID"].Value);
            txtBaslik.Text = row.Cells["Başlık"].Value?.ToString() ?? string.Empty;
            cmbSehir.Text = row.Cells["Şehir"].Value?.ToString() ?? string.Empty;
            cmbIlce.Text = row.Cells["İlçe"].Value?.ToString() ?? string.Empty;
            txtAdresDetay.Text = row.Cells["Adres"].Value?.ToString() ?? string.Empty;
            chkVarsayilan.Checked = Convert.ToBoolean(row.Cells["Varsayılan"].Value);
        }

        private void AddAddress()
        {
            if (!ValidateInputs())
            {
                return;
            }

            _addressDao.Add(new Adres
            {
                KullaniciID = _kullaniciId,
                Baslik = txtBaslik.Text.Trim(),
                Sehir = cmbSehir.Text.Trim(),
                Ilce = cmbIlce.Text.Trim(),
                AdresDetay = txtAdresDetay.Text.Trim(),
                Varsayilan = chkVarsayilan.Checked
            });

            LoadAddresses();
            ClearForm();
        }

        private void UpdateAddress()
        {
            if (_selectedAdresId <= 0 || !ValidateInputs())
            {
                return;
            }

            _addressDao.Update(new Adres
            {
                AdresID = _selectedAdresId,
                KullaniciID = _kullaniciId,
                Baslik = txtBaslik.Text.Trim(),
                Sehir = cmbSehir.Text.Trim(),
                Ilce = cmbIlce.Text.Trim(),
                AdresDetay = txtAdresDetay.Text.Trim(),
                Varsayilan = chkVarsayilan.Checked
            });

            LoadAddresses();
        }

        private void DeleteAddress()
        {
            if (_selectedAdresId <= 0)
            {
                MessageBox.Show("Silmek için bir adres seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Seçili adres silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            _addressDao.Delete(_selectedAdresId, _kullaniciId);
            LoadAddresses();
            ClearForm();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtBaslik.Text) || string.IsNullOrWhiteSpace(txtAdresDetay.Text) ||
                string.IsNullOrWhiteSpace(cmbSehir.Text) || string.IsNullOrWhiteSpace(cmbIlce.Text))
            {
                MessageBox.Show("Tüm alanları doldurun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            _selectedAdresId = 0;
            txtBaslik.Clear();
            txtAdresDetay.Clear();
            cmbSehir.SelectedIndex = 0;
            cmbIlce.Text = string.Empty;
            chkVarsayilan.Checked = false;
        }
    }
}
