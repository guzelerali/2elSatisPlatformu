using ReStore.Dao;
using ReStore.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ReStore
{
    public class UserPanel : Form
    {
        private PictureBox picProfileField;
        private TabControl tabs;
        private TabPage tabHome, tabFav, tabRestore, tabCart, tabOrders, tabAccount, tabSupport;
        private ListView listHome;
        private ListView listFav;
        private ListView listCart;
        private Button btnAddProduct;
        private TextBox txtTitle, txtDesc, txtPrice;
        private TextBox txtPhone;
        private Label lblUser;
        
        private ReStore.Dao.KullaniciDao kullaniciDao = new ReStore.Dao.KullaniciDao();


        private TextBox textBoxSearchHome;
        private Button btnSearchHome;
        private ComboBox cbCategoryFilter;
        private ComboBox cbGenderFilter;
        private ComboBox cbBrandFilter;
        private ComboBox cbSizeFilter;
        private ComboBox cbNewProductCategory;
        private ComboBox cbNewProductGender;
        private ComboBox cbNewProductBrand;
        private ComboBox cbNewProductSize;
        private DataGridView dgvOrdersHistory;
        private DataGridView dgvOrderDetailsHistory;

        
        private List<Product> products = new List<Product>();
        private List<Product> favorites = new List<Product>();
        private List<Product> cart = new List<Product>();
        private ReStore.Dao.ProductDao productDao = new ReStore.Dao.ProductDao();
        private ReStore.Dao.FavoriteDao favoriteDao = new ReStore.Dao.FavoriteDao();
        private ReStore.Dao.CouponDao couponDao = new ReStore.Dao.CouponDao();
        private ReStore.Dao.OrderDao orderDao = new ReStore.Dao.OrderDao();
        private ReStore.Dao.AddressDao addressDao = new ReStore.Dao.AddressDao();
        private ReStore.Models.Coupon? appliedCoupon;


        private readonly CommentDao commentDao = new CommentDao();

        private string currentUserEmail = null!;
        private string currentUserName = null!;
        private string? currentUserPhone;
        private int currentUserId;

        public UserPanel(int userId, string userEmail, string userName, string? userPhone = null)
        {
            currentUserId = userId;
            currentUserEmail = userEmail;
            currentUserName = userName;
            currentUserPhone = userPhone;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Kullanıcı Paneli";
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.WhiteSmoke;

            lblUser = new Label
            {
                Text = $"Hoş geldiniz, {currentUserName}",
                Location = new Point(16, 12),
                Size = new Size(760, 28),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            tabs = new TabControl { Location = new Point(8, 48), Size = new Size(784, 480), Font = new Font("Segoe UI", 10F) };
            tabHome = new TabPage("Anasayfa");
            tabFav = new TabPage("Favoriler");
            tabRestore = new TabPage("Restore +");
            tabCart = new TabPage("Sepetim");
            tabOrders = new TabPage("Sipariş Geçmişim");
            tabAccount = new TabPage("Hesabım");
            tabSupport = new TabPage("Müşteri Hizmetleri");

            
            var pnlHome = new Panel { Dock = DockStyle.Fill };

            var pnlHomeTop = new Panel { Dock = DockStyle.Top, Height = 72, Padding = new Padding(8) };
            textBoxSearchHome = new TextBox
            {
                PlaceholderText = "Ürün başlığı, açıklama, satıcı, marka ara",
                Location = new Point(8, 6),
                Size = new Size(420, 24)
            };
            textBoxSearchHome.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    btnSearchHome.PerformClick();
                }
            };

            btnSearchHome = new Button
            {
                Text = "Ara",
                Location = new Point(436, 4),
                Size = new Size(80, 28)
            };
            btnSearchHome.Click += (s, e) =>
            {
                var q = textBoxSearchHome.Text?.Trim();
                ApplyHomeFilters(string.IsNullOrWhiteSpace(q) ? null : q);
            };

            var btnClearHomeSearch = new Button
            {
                Text = "Temizle",
                Location = new Point(520, 4),
                Size = new Size(80, 28)
            };
            btnClearHomeSearch.Click += (s, e) =>
            {
                textBoxSearchHome.Text = string.Empty;
                if (cbCategoryFilter != null) cbCategoryFilter.SelectedIndex = 0;
                if (cbGenderFilter != null) cbGenderFilter.SelectedIndex = 0;
                if (cbBrandFilter != null) cbBrandFilter.SelectedIndex = 0;
                if (cbSizeFilter != null) cbSizeFilter.SelectedIndex = 0;
                ApplyHomeFilters(null);
            };

            cbCategoryFilter = new ComboBox { Location = new Point(608, 4), Size = new Size(160, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cbCategoryFilter.Items.AddRange(new object[] { "Tümü", "Tişört", "Sweat", "Pantolon" });
            cbCategoryFilter.SelectedIndex = 0;
            cbCategoryFilter.SelectedIndexChanged += (s, e) =>
            {
                var q = textBoxSearchHome.Text?.Trim();
                ApplyHomeFilters(string.IsNullOrWhiteSpace(q) ? null : q);
            };
            cbGenderFilter = new ComboBox { Location = new Point(8, 40), Size = new Size(140, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cbGenderFilter.Items.AddRange(new object[] { "Cinsiyet: Tümü", "Kadın", "Erkek", "Unisex" });
            cbGenderFilter.SelectedIndex = 0;
            cbGenderFilter.SelectedIndexChanged += (s, e) => ApplyHomeFilters(string.IsNullOrWhiteSpace(textBoxSearchHome.Text?.Trim()) ? null : textBoxSearchHome.Text.Trim());

            cbBrandFilter = new ComboBox { Location = new Point(156, 40), Size = new Size(180, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cbBrandFilter.Items.AddRange(new object[] { "Marka: Tümü", "Nike", "Adidas", "Puma", "LC Waikiki", "Mavi", "Koton", "Defacto", "Diğer" });
            cbBrandFilter.SelectedIndex = 0;
            cbBrandFilter.SelectedIndexChanged += (s, e) => ApplyHomeFilters(string.IsNullOrWhiteSpace(textBoxSearchHome.Text?.Trim()) ? null : textBoxSearchHome.Text.Trim());

            cbSizeFilter = new ComboBox { Location = new Point(344, 40), Size = new Size(120, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cbSizeFilter.Items.AddRange(new object[] { "Beden: Tümü", "XS", "S", "M", "L", "XL", "XXL", "34", "36", "38", "40", "42", "44" });
            cbSizeFilter.SelectedIndex = 0;
            cbSizeFilter.SelectedIndexChanged += (s, e) => ApplyHomeFilters(string.IsNullOrWhiteSpace(textBoxSearchHome.Text?.Trim()) ? null : textBoxSearchHome.Text.Trim());

            var pnlQuickFilters = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(8, 4, 8, 4),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            Button CreateQuickFilterButton(string text, string? category)
            {
                var btn = new Button { Text = text, Size = new Size(100, 28) };
                btn.Click += (s, e) =>
                {
                    if (cbCategoryFilter != null)
                    {
                        cbCategoryFilter.SelectedItem = category is null ? "Tümü" : category;
                    }
                    var q = textBoxSearchHome.Text?.Trim();
                    var search = string.IsNullOrWhiteSpace(q) ? null : q;
                    ApplyHomeFilters(search, category);
                };
                return btn;
            }

            var btnAll = CreateQuickFilterButton("Tum Urunler", null);
            var btnTshirt = CreateQuickFilterButton("Tshirt", "Tişört");
            var btnSweat = CreateQuickFilterButton("Sweat", "Sweat");
            var btnPantolon = CreateQuickFilterButton("Pantolon", "Pantolon");
            pnlQuickFilters.Controls.AddRange(new Control[] { btnAll, btnTshirt, btnSweat, btnPantolon });

            pnlHomeTop.Controls.AddRange(new Control[] { textBoxSearchHome, btnSearchHome, btnClearHomeSearch, cbCategoryFilter, cbGenderFilter, cbBrandFilter, cbSizeFilter });

            listHome = new ListView
            {
                View = View.LargeIcon,
                Dock = DockStyle.Fill,
                MultiSelect = false,
                Activation = ItemActivation.OneClick
            };
            var il = new ImageList { ImageSize = new Size(120, 120), ColorDepth = ColorDepth.Depth32Bit };
            listHome.LargeImageList = il;
            listHome.ItemActivate += ListHome_ItemActivate;

            pnlHome.Controls.Add(listHome);
            pnlHome.Controls.Add(pnlQuickFilters);
            pnlHome.Controls.Add(pnlHomeTop);
            tabHome.Controls.Add(pnlHome);

            listFav = new ListView { View = View.Details, Dock = DockStyle.Fill, MultiSelect = false };
            listFav.Columns.Add("Başlık", 300);
            listFav.Columns.Add("Fiyat", 100);
            listFav.Columns.Add("Satıcı", 300);
            listFav.DoubleClick += ListFav_DoubleClick;

            var pnlFavTop = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
            var btnRemoveFavorite = new Button { Text = "Seçiliyi Favorilerden Kaldır", Location = new Point(8, 6), Size = new Size(200, 28) };
            btnRemoveFavorite.Click += (s, e) =>
            {
                if (listFav.SelectedItems.Count == 0) { MessageBox.Show("Lütfen favorilerden kaldırmak için bir ürün seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var it = listFav.SelectedItems[0];
                var p = it.Tag as Product;
                if (p == null) return;
                var confirm = MessageBox.Show($"\"{p.Title}\" ürünü favorilerinizden kaldırmak istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
                var ok = favoriteDao.RemoveFavorite(currentUserEmail, p.Id);
                if (ok)
                {
                    var existing = favorites.FirstOrDefault(x => x.Id == p.Id);
                    if (existing != null) favorites.Remove(existing);
                    RefreshFavoritesList();
                    MessageBox.Show("Ürün favorilerden kaldırıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Favoriden kaldırma sırasında hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var btnRefreshFav = new Button { Text = "Yenile", Location = new Point(220, 6), Size = new Size(100, 28) };
            btnRefreshFav.Click += (s, e) => LoadFavoritesFromDb();

            pnlFavTop.Controls.AddRange(new Control[] { btnRemoveFavorite, btnRefreshFav });
            tabFav.Controls.Add(listFav);
            tabFav.Controls.Add(pnlFavTop);

            var pnlRestore = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var lblTitle = new Label { Text = "Ürün Başlığı:", Location = new Point(10, 14), AutoSize = true };
            txtTitle = new TextBox { Location = new Point(110, 12), Width = 420, TextAlign = HorizontalAlignment.Center };
            var lblDesc = new Label { Text = "Açıklama:", Location = new Point(10, 50), AutoSize = true };
            txtDesc = new TextBox { Location = new Point(110, 48), Width = 420, Height = 100, Multiline = true, ScrollBars = ScrollBars.Vertical };
            var lblPrice = new Label { Text = "Fiyat (TL):", Location = new Point(10, 162), AutoSize = true };
            txtPrice = new TextBox { Location = new Point(110, 160), Width = 120 };

            var lblImage = new Label { Text = "Fotoğraf:", Location = new Point(10, 200), AutoSize = true };
            var txtImagePath = new TextBox { Location = new Point(110, 198), Width = 320, ReadOnly = true };
            var lblCategory = new Label { Text = "Kategori:", Location = new Point(10, 236), AutoSize = true };
            cbNewProductCategory = new ComboBox { Location = new Point(110, 234), Size = new Size(200, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cbNewProductCategory.Items.AddRange(new object[] { "Tişört", "Sweat", "Pantolon" });
            cbNewProductCategory.SelectedIndex = 0;
            var lblGender = new Label { Text = "Cinsiyet:", Location = new Point(10, 270), AutoSize = true };
            cbNewProductGender = new ComboBox { Location = new Point(110, 268), Size = new Size(200, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cbNewProductGender.Items.AddRange(new object[] { "Kadın", "Erkek", "Unisex" });
            cbNewProductGender.SelectedIndex = 0;
            var lblBrand = new Label { Text = "Marka:", Location = new Point(10, 304), AutoSize = true };
            cbNewProductBrand = new ComboBox { Location = new Point(110, 302), Size = new Size(200, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cbNewProductBrand.Items.AddRange(new object[] { "Nike", "Adidas", "Puma", "LC Waikiki", "Mavi", "Koton", "Defacto", "Diğer" });
            cbNewProductBrand.SelectedIndex = 0;
            var lblSize = new Label { Text = "Beden:", Location = new Point(10, 338), AutoSize = true };
            cbNewProductSize = new ComboBox { Location = new Point(110, 336), Size = new Size(200, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cbNewProductSize.Items.AddRange(new object[] { "XS", "S", "M", "L", "XL", "XXL", "34", "36", "38", "40", "42", "44" });
            cbNewProductSize.SelectedIndex = 0;
            var btnBrowse = new Button { Text = "Gözat", Location = new Point(440, 196), Size = new Size(80, 26) };
            btnBrowse.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog();
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtImagePath.Text = dlg.FileName;
            };

            btnAddProduct = new Button { Text = "Satışa Koy", Location = new Point(110, 376), Size = new Size(120, 30) };
            btnAddProduct.Click += (s, e) => BtnAddProductWithImage_Click(s, e, txtImagePath.Text);

            var lblInfo = new Label { Text = "Not: Ürünler geçici olarak uygulama belleğine eklenir.", Location = new Point(250, 165), AutoSize = true, ForeColor = Color.DarkGray };

            pnlRestore.Controls.AddRange(new Control[] { lblTitle, txtTitle, lblDesc, txtDesc, lblPrice, txtPrice, lblImage, txtImagePath, btnBrowse, lblCategory, cbNewProductCategory, lblGender, cbNewProductGender, lblBrand, cbNewProductBrand, lblSize, cbNewProductSize, btnAddProduct, lblInfo });
            tabRestore.Controls.Add(pnlRestore);

            // Cart
            listCart = new ListView { View = View.Details, Dock = DockStyle.Top, Height = 360 };
            listCart.Columns.Add("Başlık", 300);
            listCart.Columns.Add("Fiyat", 100);
            listCart.Columns.Add("Satıcı", 300);

            var pnlCartBottom = new Panel { Dock = DockStyle.Fill };
            var btnRemoveFromCart = new Button { Text = "Seçiliyi Kaldır", Location = new Point(10, 6), Size = new Size(120, 30) };
            btnRemoveFromCart.Click += (s, e) =>
            {
                if (listCart.SelectedItems.Count == 0) { MessageBox.Show("Lütfen sepetten bir ürün seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var it = listCart.SelectedItems[0];
                var p = it.Tag as Product;
                if (p != null)
                {
                    var toRemove = cart.FirstOrDefault(x => x.Id == p.Id);
                    if (toRemove != null) cart.Remove(toRemove);
                    RefreshCartList();
                }
            };

            var btnCheckout = new Button { Text = "Satın Al", Location = new Point(140, 6), Size = new Size(140, 30), TextAlign = ContentAlignment.MiddleCenter };
            btnCheckout.Click += (s, e) =>
            {
                if (cart.Count == 0) { MessageBox.Show("Sepetiniz boş.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                decimal total = cart.Sum(x => x.Price);
                if (appliedCoupon != null)
                {
                    if (appliedCoupon.Percent.HasValue) total = total * (1 - appliedCoupon.Percent.Value / 100m);
                    else if (appliedCoupon.Amount.HasValue) total = Math.Max(0, total - appliedCoupon.Amount.Value);
                }

                var res = MessageBox.Show($"Toplam: {total:C}. Alışverişi tamamlamak istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    var items = cart.Select(c2 => new ReStore.Models.OrderItem
                    {
                        ProductId = c2.Id,
                        Quantity = 1,
                        UnitPrice = c2.Price,
                        UrunAdi = c2.Title,
                        Fiyat = c2.Price
                    }).ToList();
                    ShowPaymentDialog(total, items);
                }
            };

            var lblCartTotal = new Label { Text = "Toplam: 0 TL", Location = new Point(300, 12), AutoSize = true };
            var txtCoupon = new TextBox { Location = new Point(460, 10), Width = 140 };
            var btnApplyCoupon = new Button { Text = "Kupon Uygula", Location = new Point(610, 8), Size = new Size(120, 28) };
            btnApplyCoupon.Click += (s, e) =>
            {
                var code = txtCoupon.Text?.Trim();
                if (string.IsNullOrEmpty(code)) { MessageBox.Show("Kupon kodu girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var c = couponDao.GetByCode(code);
                if (c == null) { MessageBox.Show("Kupon bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                if (!c.Active) { MessageBox.Show("Kupon aktif değil.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                if (c.ExpiresAt.HasValue && c.ExpiresAt.Value < DateTime.Now) { MessageBox.Show("Kupon süresi dolmuş.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                appliedCoupon = c;
                MessageBox.Show("Kupon uygulandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshCartList();
            };

            pnlCartBottom.Controls.AddRange(new Control[] { btnRemoveFromCart, btnCheckout, lblCartTotal, txtCoupon, btnApplyCoupon });

            tabCart.Controls.Add(pnlCartBottom);
            tabCart.Controls.Add(listCart);

            var splitOrders = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 230
            };

            dgvOrdersHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };
            dgvOrdersHistory.Columns.Add("SiparisID", "Sipariş No");
            dgvOrdersHistory.Columns.Add("Tarih", "Tarih");
            dgvOrdersHistory.Columns.Add("ToplamFiyat", "Toplam Fiyat");
            dgvOrdersHistory.Columns.Add("AdresBaslik", "Adres");
            dgvOrdersHistory.SelectionChanged += (_, _) => FillSelectedOrderDetails();

            dgvOrderDetailsHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };
            dgvOrderDetailsHistory.Columns.Add("UrunAdi", "Ürün Adı");
            dgvOrderDetailsHistory.Columns.Add("Fiyat", "Fiyat");

            var pnlOrdersTop = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
            var btnRefreshOrders = new Button { Text = "Geçmişi Yenile", Size = new Size(140, 28), Location = new Point(8, 8) };
            btnRefreshOrders.Click += (_, _) => LoadOrderHistoryToGrid();
            pnlOrdersTop.Controls.Add(btnRefreshOrders);

            splitOrders.Panel1.Controls.Add(dgvOrdersHistory);
            splitOrders.Panel2.Controls.Add(dgvOrderDetailsHistory);
            tabOrders.Controls.Add(splitOrders);
            tabOrders.Controls.Add(pnlOrdersTop);

            var pnlAccount = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var table = new TableLayoutPanel
            {
                Location = new Point(40, 20),
                Size = new Size(420, 160),
                ColumnCount = 2,
                RowCount = 3,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                BackColor = Color.Transparent
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            var lblNameTitle = new Label { Text = "Ad:", Anchor = AnchorStyles.Right, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            var lblEmailTitle = new Label { Text = "E-posta:", Anchor = AnchorStyles.Right, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            var lblPhoneTitle = new Label { Text = "Telefon:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

            var lblAccInfoName2 = new Label { Name = "lblAccInfoName", Text = currentUserName, Anchor = AnchorStyles.Left, AutoSize = false, Font = new Font("Segoe UI", 10F), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var lblAccInfoMail2 = new Label { Name = "lblAccInfoMail", Text = currentUserEmail, Anchor = AnchorStyles.Left, AutoSize = false, Font = new Font("Segoe UI", 10F), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };

            txtPhone = new TextBox { Text = "", Anchor = AnchorStyles.Left, Width = 200, Font = new Font("Segoe UI", 10F) };
            if (!string.IsNullOrEmpty(currentUserPhone)) txtPhone.Text = currentUserPhone;
            var btnSavePhone2 = new Button { Text = "Kaydet", Size = new Size(90, 26), BackColor = Color.DarkRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnSavePhone2.Click += (s, e) =>
            {
                var ok = kullaniciDao.UpdateTelefon(currentUserEmail, txtPhone.Text?.Trim() ?? string.Empty);
                if (ok) MessageBox.Show("Telefon bilgisi güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else MessageBox.Show("Telefon güncellenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            var phonePanel = new TableLayoutPanel { Dock = DockStyle.Fill, Height = 44, ColumnCount = 2, RowCount = 1, Padding = new Padding(0) };
            phonePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            phonePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            phonePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            txtPhone.Height = 26;
            txtPhone.Width = 200;
            txtPhone.Dock = DockStyle.Left;
            txtPhone.Margin = new Padding(0, 9, 0, 0);
            btnSavePhone2.Size = new Size(90, 26);
            btnSavePhone2.Margin = new Padding(8, 9, 0, 0);
            btnSavePhone2.FlatStyle = FlatStyle.Flat;
            btnSavePhone2.Anchor = AnchorStyles.Left;
            phonePanel.Controls.Add(txtPhone, 0, 0);
            phonePanel.Controls.Add(btnSavePhone2, 1, 0);

            table.Controls.Add(lblNameTitle, 0, 0);
            table.Controls.Add(lblAccInfoName2, 1, 0);
            table.Controls.Add(lblEmailTitle, 0, 1);
            table.Controls.Add(lblAccInfoMail2, 1, 1);
            table.Controls.Add(lblPhoneTitle, 0, 2);
            table.Controls.Add(phonePanel, 1, 2);

            var fl = new FlowLayoutPanel { Location = new Point(40, 200), Size = new Size(640, 48), FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 6, 0, 0) };
            Size actionBtnSize = new Size(150, 32);
            Action<Button> styleActionBtn = (b) =>
            {
                b.Size = actionBtnSize;
                b.BackColor = Color.SteelBlue;
                b.ForeColor = Color.White;
                b.FlatStyle = FlatStyle.Flat;
                b.TextAlign = ContentAlignment.MiddleCenter;
                b.Margin = new Padding(8, 0, 0, 0);
                b.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            };

            var btnMyFavorites = new Button { Text = "Benim Favorilerim" }; styleActionBtn(btnMyFavorites);
            btnMyFavorites.Click += (s, e) => tabs.SelectedTab = tabFav;
            var btnMyCoupons = new Button { Text = "İndirim Kuponlarım" }; styleActionBtn(btnMyCoupons);
            btnMyCoupons.Click += BtnMyCoupons_Click;
            var btnEditProfile = new Button { Text = "Profil Düzenle" }; styleActionBtn(btnEditProfile);
            btnEditProfile.Click += BtnEditProfile_Click;
            var btnAddresses = new Button { Text = "Adreslerim" }; styleActionBtn(btnAddresses);
            btnAddresses.Click += BtnAddresses_Click;
            var btnOrderHistory = new Button { Text = "Sipariş Geçmişi" }; styleActionBtn(btnOrderHistory);
            btnOrderHistory.Click += BtnOrderHistory_Click;

            fl.Controls.AddRange(new Control[] { btnMyFavorites, btnMyCoupons, btnEditProfile, btnAddresses, btnOrderHistory });

            picProfileField = new PictureBox
            {
                Location = new Point(480, 20),
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            var btnUploadPic = new Button { Text = "Resim Yükle", Location = new Point(480, 150), Size = new Size(120, 28), BackColor = Color.DarkCyan, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUploadPic.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog();
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var ok = kullaniciDao.UpdateProfilResmi(currentUserEmail, dlg.FileName);
                    if (ok)
                    {
                        try
                        {
                            var img = CreateCroppedImage(dlg.FileName, picProfileField.Size);
                            if (picProfileField.Image != null) { picProfileField.Image.Dispose(); picProfileField.Image = null; }
                            picProfileField.Image = img;
                        }
                        catch { }
                        MessageBox.Show("Profil resmi güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Profil resmi güncellenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            pnlAccount.Controls.Add(table);
            pnlAccount.Controls.Add(fl);
            pnlAccount.Controls.Add(picProfileField);
            pnlAccount.Controls.Add(btnUploadPic);
            tabAccount.Controls.Add(pnlAccount);

            var pnlSupport = new Panel { Dock = DockStyle.Fill };
            var lblSupport = new Label { Text = "Canlı destek için aşağıdaki butona tıklayın veya destek@restore.com adresine e-posta atın.", Dock = DockStyle.Top, Height = 48, TextAlign = ContentAlignment.MiddleLeft };
            var btnLive = new Button { Text = "Canlı Destek (Chat)", Size = new Size(180, 36), Location = new Point(12, 64), BackColor = Color.DarkCyan, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLive.Click += (s, e) =>
            {
                try
                {
                    var url = "https://example.com/livechat"; 
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch
                {
                    MessageBox.Show("Canlı destek açılamıyor. Lütfen destek@restore.com adresine e-posta gönderin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            pnlSupport.Controls.Add(lblSupport);
            pnlSupport.Controls.Add(btnLive);
            tabSupport.Controls.Add(pnlSupport);

            tabs.TabPages.AddRange(new TabPage[] { tabHome, tabFav, tabRestore, tabCart, tabOrders, tabAccount, tabSupport });

            this.Controls.Add(lblUser);
            this.Controls.Add(tabs);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.LightGray, Padding = new Padding(8) };
            var btnLogout = new Button { Text = "Çıkış Yap", Location = new Point(12, 12), Size = new Size(110, 32), BackColor = Color.DarkRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += (s, e) => { this.Close(); };
            var btnHelp = new Button { Text = "Yardım", Location = new Point(132, 12), Size = new Size(110, 32), BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnHelp.Click += (s, e) => MessageBox.Show("Destek: destek@restore.com", "Yardım", MessageBoxButtons.OK, MessageBoxIcon.Information);
            pnlBottom.Controls.AddRange(new Control[] { btnLogout, btnHelp });
            this.Controls.Add(pnlBottom);

            LoadProductsFromDb();
            LoadFavoritesFromDb();
            RefreshHomeList();
            LoadUserInfo();
            LoadOrderHistoryToGrid();
        }

        private void LoadFavoritesFromDb()
        {
            try
            {
                var favIds = favoriteDao.GetFavoritesForUser(currentUserEmail);
                favorites = products.Where(p => favIds.Contains(p.Id)).ToList();
                RefreshFavoritesList();
            }
            catch
            {
                favorites = new List<Product>();
            }
        }

        private void RefreshFavoritesList()
        {
            listFav.Items.Clear();
            foreach (var p in favorites)
            {
                var it = new ListViewItem(new[] { p.Title, p.Price.ToString("C"), p.SellerEmail }) { Tag = p };
                listFav.Items.Add(it);
            }
            RefreshCartList();
        }

        private void RefreshCartList()
        {
            listCart.Items.Clear();
            foreach (var p in cart)
            {
                var it = new ListViewItem(new[] { p.Title, p.Price.ToString("C"), p.SellerEmail }) { Tag = p };
                listCart.Items.Add(it);
            }
            foreach (Control ctl in tabCart.Controls)
            {
                if (ctl is Panel pnl)
                {
                    foreach (Control c in pnl.Controls)
                    {
                        if (c is Label lbl && lbl.Text.StartsWith("Toplam:"))
                        {
                            lbl.Text = $"Toplam: {cart.Sum(x => x.Price):C}";
                        }
                    }
                }
            }
        }

        private void LoadUserInfo()
        {
            try
            {
                var users = kullaniciDao.GetAllUsers();
                var me = users.FirstOrDefault(u => string.Equals(u.KullaniciEmail, currentUserEmail, StringComparison.OrdinalIgnoreCase));
                if (me != null)
                {
                    txtPhone.Text = me.Telefon ?? string.Empty;
                    var lblName = this.Controls.Find("lblAccInfoName", true).FirstOrDefault() as Label;
                    var lblMail = this.Controls.Find("lblAccInfoMail", true).FirstOrDefault() as Label;
                    if (lblName != null) lblName.Text = me.KullaniciAd;
                    if (lblMail != null) lblMail.Text = me.KullaniciEmail;
                    if (!string.IsNullOrEmpty(me.ProfilResmi) && System.IO.File.Exists(me.ProfilResmi))
                    {
                        try
                        {
                            var img = CreateCroppedImage(me.ProfilResmi, picProfileField.Size);
                            if (picProfileField.Image != null) { picProfileField.Image.Dispose(); picProfileField.Image = null; }
                            picProfileField.Image = img;
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                
            }

        }

        private Image CreateCroppedImage(string path, Size targetSize)
        {
            using var src = Image.FromFile(path);
            int srcW = src.Width; int srcH = src.Height;
            float targetRatio = (float)targetSize.Width / targetSize.Height;
            float srcRatio = (float)srcW / srcH;

            Rectangle srcRect;
            if (srcRatio > targetRatio)
            {
              
                int newW = (int)(srcH * targetRatio);
                int x = (srcW - newW) / 2;
                srcRect = new Rectangle(x, 0, newW, srcH);
            }
            else
            {
                int newH = (int)(srcW / targetRatio);
                int y = (srcH - newH) / 2;
                srcRect = new Rectangle(0, y, srcW, newH);
            }

            var dest = new Bitmap(targetSize.Width, targetSize.Height);
            dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
            using var g = Graphics.FromImage(dest);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(src, new Rectangle(0, 0, targetSize.Width, targetSize.Height), srcRect, GraphicsUnit.Pixel);
            return dest;
        }

        private void ListHome_ItemActivate(object? sender, EventArgs e)
        {
            if (listHome.SelectedItems.Count == 0) return;
            var it = listHome.SelectedItems[0];
            var p = it.Tag as Product;
            if (p == null) return;

            ShowProductDetails(p);
        }

        private void ListFav_DoubleClick(object? sender, EventArgs e)
        {
            if (listFav.SelectedItems.Count == 0) return;
            var it = listFav.SelectedItems[0];
            var p = it.Tag as Product;
            if (p == null) return;

            ShowProductDetails(p);
        }

        private static Panel BuildProductCommentCard(string author, DateTime createdAt, string body, int cardWidth)
        {
            const int padX = 14;
            const int padY = 11;
            int innerW = Math.Max(100, cardWidth - padX * 2);

            var card = new Panel
            {
                Width = cardWidth,
                BackColor = Color.White,
                Cursor = Cursors.Default
            };
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, card, new object[] { true });
            }
            catch { /* eski çalışma zamanları */ }

            string datePart = createdAt != default && createdAt != DateTime.MinValue
                ? createdAt.ToString("d MMMM yyyy · HH:mm", new CultureInfo("tr-TR"))
                : "";

            var lblMeta = new Label
            {
                AutoSize = false,
                Width = innerW,
                Location = new Point(padX, padY),
                Font = new Font("Segoe UI", 9.25f, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 34, 39),
                Cursor = Cursors.Default,
                Text = string.IsNullOrEmpty(datePart) ? author : $"{author}    ·    {datePart}"
            };
            int metaH = TextRenderer.MeasureText(lblMeta.Text, lblMeta.Font, new Size(innerW, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
            lblMeta.Height = Math.Max(18, metaH);

            var lblBody = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(innerW, 0),
                Location = new Point(padX, lblMeta.Bottom + 7),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(75, 80, 90),
                Cursor = Cursors.Default,
                Text = string.IsNullOrEmpty(body) ? "\u00a0" : body
            };
            int bodyH = TextRenderer.MeasureText(lblBody.Text, lblBody.Font, new Size(lblBody.MaximumSize.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
            lblBody.Height = Math.Max(16, bodyH);

            card.Controls.Add(lblMeta);
            card.Controls.Add(lblBody);
            card.Height = lblBody.Bottom + padY;

            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(232, 234, 238));
                e.Graphics.DrawLine(pen, padX, card.Height - 1, card.Width - padX, card.Height - 1);
            };

            return card;
        }

        private void ShowProductDetails(Product p)
        {
            var dlg = new Form
            {
                Text = p.Title,
                ClientSize = new Size(650, 720),
                StartPosition = FormStartPosition.CenterParent,
                MinimumSize = new Size(640, 680)
            };
            var pic = new PictureBox { Location = new Point(12, 12), Size = new Size(256, 256), SizeMode = PictureBoxSizeMode.Zoom };
            if (!string.IsNullOrEmpty(p.ImagePath) && System.IO.File.Exists(p.ImagePath))
            {
                try { pic.Image = Image.FromFile(p.ImagePath); } catch { }
            }
            var lblTitle = new Label { Text = p.Title, Location = new Point(280, 12), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            var lblPrice = new Label { Text = p.Price.ToString("C"), Location = new Point(280, 46), AutoSize = true };
            var lblAttrs = new Label
            {
                Text = $"Kategori: {p.Category} | Cinsiyet: {p.Gender} | Marka: {p.Brand} | Beden: {p.Size}",
                Location = new Point(280, 70),
                Size = new Size(300, 40)
            };
            var txtDesc = new TextBox { Text = p.Description, Location = new Point(280, 112), Width = 300, Height = 124, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
            bool isFav = favorites.Any(x => x.Id == p.Id);

            string boskalp = System.IO.Path.Combine(Application.StartupPath, "Assets", "boskalp.png");
            string dolukalp = System.IO.Path.Combine(Application.StartupPath, "Assets", "dolukalp.png");

            Image imgBos = Image.FromFile(boskalp);
            Image imgDolu = Image.FromFile(dolukalp);

            PictureBox btnFav = new PictureBox
            {
                Location = new Point(280, 248),
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };

            btnFav.Image = isFav ? imgDolu : imgBos;

            btnFav.Click += (s, e) =>
            {
                bool currentlyFav = favorites.Any(x => x.Id == p.Id);

                if (!currentlyFav)
                {
                    var ok = favoriteDao.AddFavorite(currentUserEmail, p.Id);

                    if (ok)
                    {
                        favorites.Add(p);
                        btnFav.Image = imgDolu;
                    }
                }
                else
                {
                    var ok = favoriteDao.RemoveFavorite(currentUserEmail, p.Id);

                    if (ok)
                    {
                        var existing = favorites.FirstOrDefault(x => x.Id == p.Id);
                        if (existing != null) favorites.Remove(existing);

                        btnFav.Image = imgBos;
                    }
                }
            };

            dlg.FormClosed += (s, e) =>
            {
                imgBos.Dispose();
                imgDolu.Dispose();
            };
            var btnAddCart = new Button { Text = "Sepete Ekle", Location = new Point(410, 248), Size = new Size(120, 30) };
            btnAddCart.Click += (s, e) =>
            {
                cart.Add(p);
                RefreshCartList();
                MessageBox.Show("Sepete eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var lblComments = new Label { Text = "Yorumlar", Location = new Point(12, 298), AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(32, 34, 39) };
            var lblCommentsSub = new Label
            {
                Text = "Bu ürün hakkında düşünceler",
                Location = new Point(12, 320),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = Color.FromArgb(120, 125, 135)
            };
            var pnlCommentsHost = new Panel
            {
                Location = new Point(12, 342),
                Size = new Size(616, 168),
                AutoScroll = true,
                BackColor = Color.FromArgb(246, 247, 249),
                BorderStyle = BorderStyle.None,
                Padding = new Padding(0, 6, 0, 6)
            };

            var lblYorumYap = new Label { Text = "Yorum yaz", Location = new Point(12, 518), AutoSize = true, Font = new Font("Segoe UI", 9.25f, FontStyle.Bold), ForeColor = Color.FromArgb(32, 34, 39) };
            var txtNewComment = new TextBox
            {
                Location = new Point(12, 542),
                Size = new Size(500, 28),
                MaxLength = 2000,
                Font = new Font("Segoe UI", 9.25f),
                BorderStyle = BorderStyle.FixedSingle
            };
            var btnAddComment = new Button
            {
                Text = "Yayınla",
                Location = new Point(520, 540),
                Size = new Size(108, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 35, 24),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddComment.FlatAppearance.BorderSize = 0;

            void LoadComments()
            {
                pnlCommentsHost.Controls.Clear();
                try
                {
                    var comments = commentDao.GetCommentsByProduct(p.Id);
                    int cardW = Math.Max(200, pnlCommentsHost.ClientSize.Width - 16);
                    if (comments.Count == 0)
                    {
                        var empty = new Label
                        {
                            Text = "Henüz yorum yok. İlk yorumu siz yazın.",
                            Location = new Point(18, 24),
                            AutoSize = true,
                            Font = new Font("Segoe UI", 9.25f, FontStyle.Italic),
                            ForeColor = Color.FromArgb(130, 135, 145),
                            Cursor = Cursors.Default
                        };
                        pnlCommentsHost.Controls.Add(empty);
                        return;
                    }
                    int y = 6;
                    foreach (var c in comments)
                    {
                        var who = string.IsNullOrWhiteSpace(c.UserName) ? "Kullanıcı" : c.UserName;
                        var body = c.Text ?? "";
                        var card = BuildProductCommentCard(who, c.CreatedAt, body, cardW);
                        card.Location = new Point(8, y);
                        card.Tag = "comment";
                        pnlCommentsHost.Controls.Add(card);
                        y = card.Bottom + 6;
                    }
                }
                catch (Exception ex)
                {
                    var err = new Label
                    {
                        Text = "Yorumlar yüklenemedi: " + ex.Message,
                        Location = new Point(14, 20),
                        Size = new Size(pnlCommentsHost.Width - 28, 60),
                        ForeColor = Color.DarkRed,
                        Font = new Font("Segoe UI", 9f)
                    };
                    pnlCommentsHost.Controls.Add(err);
                }
            }
            LoadComments();

            btnAddComment.Click += (s, e) =>
            {
                string text = txtNewComment.Text.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("Yorum boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var comment = new Comment
                {
                    ProductId = p.Id,
                    UserId = currentUserId,
                    UserName = currentUserName ?? currentUserEmail,
                    Text = text,
                    CreatedAt = DateTime.Now
                };
                try
                {
                    if (!commentDao.AddComment(comment, out var errMsg))
                    {
                        MessageBox.Show(string.IsNullOrEmpty(errMsg) ? "Yorum eklenemedi." : errMsg, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    txtNewComment.Clear();
                    LoadComments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Yorum kaydedilemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            dlg.Controls.AddRange(new Control[]
            {
                pic,
                lblTitle,
                lblPrice,
                lblAttrs,
                txtDesc,
                btnFav,
                btnAddCart,
                lblComments,
                lblCommentsSub,
                pnlCommentsHost,
                lblYorumYap,
                txtNewComment,
                btnAddComment
            });

            dlg.ShowDialog();
        }

        private void BtnMyCoupons_Click(object? sender, EventArgs e)
        {
            var dlg = new Form { Text = "Kuponlarım", ClientSize = new Size(420, 280), StartPosition = FormStartPosition.CenterParent };
            var lst = new ListBox { Dock = DockStyle.Top, Height = 160 };

            void LoadUserCoupons()
            {
                lst.Items.Clear();
                try
                {
                    var my = couponDao.GetCouponsForUser(currentUserEmail);
                    if (my == null || my.Count == 0)
                    {
                        lst.Items.Add("Kupon yok.");
                        return;
                    }

                    foreach (var c in my)
                    {
                        var desc = c.Code + " - " + (c.Percent.HasValue ? c.Percent + "%" : (c.Amount.HasValue ? c.Amount.Value.ToString("C") : ""));
                        if (c.ExpiresAt.HasValue) desc += " (Son: " + c.ExpiresAt.Value.ToShortDateString() + ")";
                        lst.Items.Add(desc);
                    }
                }
                catch
                {
                    lst.Items.Add("Kupon yüklenemedi.");
                }
            }

            var pnl = new Panel { Dock = DockStyle.Fill };
            var lbl = new Label { Text = "Ödeme sırasında kupon kodunu girerek indirim uygulayabilirsiniz.", Dock = DockStyle.Top, Height = 24 };
            var btnRefresh = new Button { Text = "Yenile", Location = new Point(12, 170), Size = new Size(100, 28) };
            btnRefresh.Click += (s, e) => LoadUserCoupons();
            var btnCopy = new Button { Text = "Kodu Kopyala", Location = new Point(130, 170), Size = new Size(120, 28) };
            btnCopy.Click += (s, e) =>
            {
                if (lst.SelectedItem == null) { MessageBox.Show("Lütfen bir kupon seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var text = lst.SelectedItem.ToString() ?? string.Empty;
                var code = text.Split(' ')[0];
                try { Clipboard.SetText(code); MessageBox.Show("Kupon kodu panoya kopyalandı: " + code, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information); } catch { }
            };

            pnl.Controls.Add(lbl);
            pnl.Controls.Add(btnRefresh);
            pnl.Controls.Add(btnCopy);
            dlg.Controls.AddRange(new Control[] { lst, pnl });

            LoadUserCoupons();
            dlg.ShowDialog();
        }

        private void BtnEditProfile_Click(object? sender, EventArgs e)
        {
            var dlg = new Form { Text = "Profil Düzenle", ClientSize = new Size(420, 260), StartPosition = FormStartPosition.CenterParent };
            var lblName = new Label { Text = "Ad:", Location = new Point(12, 12), AutoSize = false, Size = new Size(80, 24), TextAlign = ContentAlignment.MiddleLeft };
            var txtName = new TextBox { Location = new Point(100, 10), Width = 300, Text = currentUserName, TextAlign = HorizontalAlignment.Center };
            var lblOld = new Label { Text = "Eski Şifre:", Location = new Point(12, 52), AutoSize = false, Size = new Size(80, 24), TextAlign = ContentAlignment.MiddleLeft };
            var txtOld = new TextBox { Location = new Point(100, 50), Width = 300, PasswordChar = '*', TextAlign = HorizontalAlignment.Center };
            var lblNew = new Label { Text = "Yeni Şifre:", Location = new Point(12, 92), AutoSize = false, Size = new Size(80, 24), TextAlign = ContentAlignment.MiddleLeft };
            var txtNew = new TextBox { Location = new Point(100, 90), Width = 300, PasswordChar = '*', TextAlign = HorizontalAlignment.Center };
            var btnSave = new Button { Text = "Kaydet", Location = new Point(160, 140), Size = new Size(100, 28), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnSave.Click += (s, ev) =>
            {
                bool nameOk = true, passOk = true;
                if (txtName.Text != currentUserName)
                {
                    var trimmedName = txtName.Text?.Trim() ?? string.Empty;
                    nameOk = kullaniciDao.UpdateName(currentUserEmail, trimmedName);
                    if (nameOk) currentUserName = trimmedName;
                }
                if (!string.IsNullOrWhiteSpace(txtNew.Text))
                {
                    passOk = kullaniciDao.UpdatePassword(currentUserEmail, txtOld.Text, txtNew.Text);
                }

                if (nameOk && passOk)
                {
                    MessageBox.Show("Profil güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlg.Close();
                }
                else
                {
                    MessageBox.Show("Güncelleme başarısız.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            dlg.Controls.AddRange(new Control[] { lblName, txtName, lblOld, txtOld, lblNew, txtNew, btnSave });
            dlg.ShowDialog();
        }

        private void BtnOrderHistory_Click(object? sender, EventArgs e)
        {
            tabs.SelectedTab = tabOrders;
            LoadOrderHistoryToGrid();
        }

        private void BtnAddresses_Click(object? sender, EventArgs e)
        {
            using var frm = new FormAdresler(currentUserId);
            frm.ShowDialog(this);
        }

        private void LoadProductsFromDb()
        {
            try
            {
                products = productDao.GetAllProducts();
            }
            catch
            {
                products = new List<Product>();
            }
        }

        private void ApplyHomeFilters(string? search = null, string? categoryOverride = null)
        {
            var category = categoryOverride;
            if (string.IsNullOrWhiteSpace(category))
            {
                category = cbCategoryFilter?.SelectedItem?.ToString();
                if (string.Equals(category, "Tümü", StringComparison.OrdinalIgnoreCase)) category = null;
            }

            var gender = cbGenderFilter?.SelectedItem?.ToString();
            if (string.Equals(gender, "Cinsiyet: Tümü", StringComparison.OrdinalIgnoreCase)) gender = null;

            var brand = cbBrandFilter?.SelectedItem?.ToString();
            if (string.Equals(brand, "Marka: Tümü", StringComparison.OrdinalIgnoreCase)) brand = null;

            var size = cbSizeFilter?.SelectedItem?.ToString();
            if (string.Equals(size, "Beden: Tümü", StringComparison.OrdinalIgnoreCase)) size = null;

            RefreshHomeList(search, category, gender, brand, size);
        }

        private void RefreshHomeList(string? filter = null, string? category = null, string? gender = null, string? brand = null, string? size = null)
        {
            listHome.Items.Clear();
            var il = listHome.LargeImageList!;
            il.Images.Clear();
            int idx = 0;

            IEnumerable<Product> items = products;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                items = items.Where(p =>
                    (p.Title ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || (p.Description ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || (p.SellerEmail ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || (p.Brand ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || (p.Gender ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || (p.Size ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                items = items.Where(p => string.Equals(p.Category ?? string.Empty, category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(gender))
            {
                items = items.Where(p => string.Equals(p.Gender ?? string.Empty, gender, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(brand))
            {
                items = items.Where(p => string.Equals(p.Brand ?? string.Empty, brand, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(size))
            {
                items = items.Where(p => string.Equals(p.Size ?? string.Empty, size, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            foreach (var p in items)
            {
                if (!string.IsNullOrEmpty(p.ImagePath) && System.IO.File.Exists(p.ImagePath))
                {
                    try
                    {
                        il.Images.Add(Image.FromFile(p.ImagePath));
                    }
                    catch { il.Images.Add(new Bitmap(120, 120)); }
                }
                else
                {
                    il.Images.Add(new Bitmap(120, 120));
                }

                var it = new ListViewItem(p.Title) { Tag = p, ImageIndex = idx };
                it.SubItems.Add(p.Price.ToString("C"));
                it.Name = p.Id.ToString();
                listHome.Items.Add(it);
                idx++;
            }
        }

        private void BtnAddProduct_Click(object? sender, EventArgs e)
        {
        }

        private void BtnAddProductWithImage_Click(object? sender, EventArgs e, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Başlık ve fiyat girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Geçersiz fiyat.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var prod = new Product
            {
                Id = products.Count + 1,
                Title = txtTitle.Text.Trim(),
                Description = txtDesc.Text.Trim(),
                Price = price,
                SellerEmail = currentUserEmail,
                ImagePath = string.IsNullOrWhiteSpace(imagePath) ? string.Empty : imagePath,
                Category = cbNewProductCategory?.SelectedItem?.ToString() ?? string.Empty,
                Gender = cbNewProductGender?.SelectedItem?.ToString() ?? string.Empty,
                Brand = cbNewProductBrand?.SelectedItem?.ToString() ?? string.Empty,
                Size = cbNewProductSize?.SelectedItem?.ToString() ?? string.Empty
            };
           
            bool ok = productDao.AddProduct(prod);
            if (ok)
            {
                LoadProductsFromDb();
                RefreshHomeList();
            }

            txtTitle.Text = txtDesc.Text = txtPrice.Text = string.Empty;
            if (cbNewProductCategory != null) cbNewProductCategory.SelectedIndex = 0;
            if (cbNewProductGender != null) cbNewProductGender.SelectedIndex = 0;
            if (cbNewProductBrand != null) cbNewProductBrand.SelectedIndex = 0;
            if (cbNewProductSize != null) cbNewProductSize.SelectedIndex = 0;
            MessageBox.Show("Ürün satışa eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowPaymentDialog(decimal total, List<ReStore.Models.OrderItem> items)
        {
            var dlg = new Form { Text = "Ödeme", ClientSize = new Size(460, 360), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
            var lblInfo = new Label { Text = $"Ödenecek Tutar: {total:C}", Location = new Point(12, 12), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            var lblAddress = new Label { Text = "Teslimat Adresi:", Location = new Point(12, 44), Size = new Size(140, 22) };
            var cmbAddress = new ComboBox { Location = new Point(160, 42), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            var addresses = addressDao.GetByKullaniciId(currentUserId);
            foreach (var address in addresses)
            {
                cmbAddress.Items.Add(new AddressComboItem
                {
                    AdresID = address.AdresID,
                    Label = $"{address.Baslik} - {address.Sehir}/{address.Ilce}"
                });
            }
            if (cmbAddress.Items.Count > 0)
            {
                var defaultIndex = addresses.FindIndex(a => a.Varsayilan);
                cmbAddress.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
            }

            var lblCardName = new Label { Text = "Kart Sahibinin Adı:", Location = new Point(12, 82), Size = new Size(140, 22) };
            var txtCardName = new TextBox { Location = new Point(160, 80), Width = 280 };

            var lblCardNumber = new Label { Text = "Kart Numarası:", Location = new Point(12, 118), Size = new Size(140, 22) };
            var txtCardNumber = new TextBox { Location = new Point(160, 116), Width = 280, MaxLength = 19, PlaceholderText = "Sadece rakamlar" };

            var lblExpiry = new Label { Text = "Son Kullanma (MM/YY):", Location = new Point(12, 154), Size = new Size(140, 22) };
            var txtExpiry = new TextBox { Location = new Point(160, 152), Width = 100, PlaceholderText = "MM/YY", MaxLength = 5 };

            var lblCvc = new Label { Text = "CVC:", Location = new Point(12, 190), Size = new Size(140, 22) };
            var txtCvc = new TextBox { Location = new Point(160, 188), Width = 60, MaxLength = 4, PasswordChar = '*' };

            var btnPay = new Button { Text = "ÖDE", Location = new Point(220, 250), Size = new Size(100, 34), BackColor = Color.DarkGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnCancel = new Button { Text = "İptal", Location = new Point(340, 250), Size = new Size(100, 34) };

            btnCancel.Click += (s, e) => dlg.Close();

            btnPay.Click += (s, e) =>
            {
                var name = txtCardName.Text?.Trim();
                var number = (txtCardNumber.Text ?? "").Replace(" ", "").Trim();
                var exp = txtExpiry.Text?.Trim();
                var cvc = txtCvc.Text?.Trim();
                var selectedAddress = cmbAddress.SelectedItem as AddressComboItem;

                if (selectedAddress == null)
                {
                    MessageBox.Show("Siparişi tamamlamak için bir adres seçmelisiniz.", "Adres Gerekli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Kart sahibinin adını girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(number) || !number.All(char.IsDigit) || number.Length < 13 || number.Length > 19)
                {
                    MessageBox.Show("Geçerli bir kart numarası girin (13-19 rakam).", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(exp) || exp.Length != 5 || exp[2] != '/')
                {
                    MessageBox.Show("Son kullanma tarihini MM/YY formatında girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(exp.Substring(0, 2), out int mm) || mm < 1 || mm > 12)
                {
                    MessageBox.Show("Geçerli bir ay girin (01-12).", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(cvc) || !cvc.All(char.IsDigit) || (cvc.Length != 3 && cvc.Length != 4))
                {
                    MessageBox.Show("Geçerli bir CVC girin (3 veya 4 rakam).", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var successDlg = new Form { Text = "Bilgi", ClientSize = new Size(360, 160), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
                var lblDone = new Label { Text = "Siparişiniz oluşturulmuştur!", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
                var btnOk = new Button { Text = "Tamam", Size = new Size(100, 30), Location = new Point((successDlg.ClientSize.Width - 100) / 2, 100) };
                btnOk.Click += (se2, ev2) => successDlg.Close();
                successDlg.Controls.Add(lblDone);
                successDlg.Controls.Add(btnOk);

              
                bool orderOk = false;
                try
                {
                    if (appliedCoupon != null)
                        orderOk = orderDao.CreateOrder(currentUserId, selectedAddress.AdresID, items, total);
                    else
                        orderOk = orderDao.CreateOrder(currentUserId, selectedAddress.AdresID, items);

                    if (orderOk)
                    {
                        cart.Clear();
                        appliedCoupon = null;
                        RefreshCartList();
                        LoadOrderHistoryToGrid();
                    }
                }
                catch
                {
                }

                successDlg.ShowDialog();
                dlg.Close();
            };

            dlg.Controls.AddRange(new Control[] { lblInfo, lblAddress, cmbAddress, lblCardName, txtCardName, lblCardNumber, txtCardNumber, lblExpiry, txtExpiry, lblCvc, txtCvc, btnPay, btnCancel });
            dlg.ShowDialog();
        }

        private void LoadOrderHistoryToGrid()
        {
            if (dgvOrdersHistory == null || dgvOrderDetailsHistory == null)
            {
                return;
            }

            dgvOrdersHistory.Rows.Clear();
            dgvOrderDetailsHistory.Rows.Clear();
            var orders = orderDao.GetOrdersByUser(currentUserId);
            foreach (var order in orders)
            {
                var idx = dgvOrdersHistory.Rows.Add(order.SiparisID, order.Tarih.ToString("g"), order.ToplamFiyat.ToString("C"), order.AdresBaslik);
                dgvOrdersHistory.Rows[idx].Tag = order;
            }
        }

        private void FillSelectedOrderDetails()
        {
            if (dgvOrdersHistory == null || dgvOrderDetailsHistory == null)
            {
                return;
            }

            dgvOrderDetailsHistory.Rows.Clear();
            if (dgvOrdersHistory.SelectedRows.Count == 0)
            {
                return;
            }

            var order = dgvOrdersHistory.SelectedRows[0].Tag as Order;
            if (order == null)
            {
                return;
            }

            foreach (var item in order.Items)
            {
                dgvOrderDetailsHistory.Rows.Add(item.UrunAdi, item.Fiyat.ToString("C"));
            }
        }

        private class AddressComboItem
        {
            public int AdresID { get; set; }
            public string Label { get; set; } = string.Empty;

            public override string ToString() => Label;
        }
    }
}
