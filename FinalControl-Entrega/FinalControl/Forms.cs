using FinalControl.Models;
using FinalControl.Services;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace FinalControl.Forms;

public abstract class AppForm : MaterialForm
{
    protected AppForm()
    {
        var skin = MaterialSkinManager.Instance;
        skin.AddFormToManage(this);
        skin.Theme = MaterialSkinManager.Themes.LIGHT;
        skin.ColorScheme = new ColorScheme(Primary.Teal600, Primary.Teal700, Primary.Teal200, Accent.Cyan200, TextShade.WHITE);
        StartPosition = FormStartPosition.CenterScreen;
    }

    protected async Task<bool> RunAsync(Func<Task> action, Logger logger)
    {
        try { await action(); return true; }
        catch (Exception exception) { logger.Error(exception); MessageBox.Show(exception.Message, "FinalControl", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
    }

    protected static MaterialButton Button(string text) => new() { Text = text, AutoSize = false, Width = 150, Height = 38, Margin = new Padding(6) };
}

public sealed class LoginForm(AuthenticationService auth, IServiceProvider provider, Logger logger) : AppForm
{
    private readonly MaterialTextBox2 _user = new() { Hint = "Usuario", Width = 320 };
    private readonly MaterialTextBox2 _password = new() { Hint = "Contraseña", Width = 320, UseSystemPasswordChar = true };

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e); Text = "FinalControl | Iniciar sesión"; Size = new Size(520, 500); MaximizeBox = false;
        var title = new Label { Text = "FINALCONTROL", AutoSize = true, Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = Color.FromArgb(0, 105, 92) };
        var subtitle = new Label { Text = "Inventario y ventas, en un solo lugar", AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = Color.DimGray, Padding = new Padding(0, 0, 0, 20) };
        var login = Button("INGRESAR"); login.Width = 200; login.Click += async (_, _) => await LoginAsync();
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(80, 70, 70, 30), WrapContents = false };
        panel.Controls.AddRange([title, subtitle, _user, _password, login, new Label { Text = "Demo: admin / Admin123*", AutoSize = true, ForeColor = Color.Gray, Padding = new Padding(8, 18, 0, 0) }]);
        Controls.Add(panel); AcceptButton = login;
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(_user.Text) || string.IsNullOrWhiteSpace(_password.Text)) { MessageBox.Show("Ingrese usuario y contraseña."); return; }
        try
        {
            var user = await auth.LoginAsync(_user.Text.Trim(), _password.Text);
            if (user is null) { MessageBox.Show("Usuario o contraseña inválidos.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            Hide(); using var scope = provider.CreateScope();
            using var form = ActivatorUtilities.CreateInstance<MainForm>(scope.ServiceProvider, user);
            form.ShowDialog(this); Show(); _password.Text = "";
        }
        catch (Exception exception) { logger.Error(exception); MessageBox.Show("No se pudo iniciar sesión. Revise el log."); }
    }
}

public sealed class MainForm(IServiceProvider provider, User user, ExchangeRateService rate, DashboardService dashboard, Logger logger) : AppForm
{
    private readonly Label _clock = ValueLabel();
    private readonly Label _rate = ValueLabel();
    private readonly Label _products = ValueLabel();
    private readonly Label _units = ValueLabel();
    private readonly Label _sales = ValueLabel();
    private System.Windows.Forms.Timer? _timer;

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e); Text = $"FinalControl | Sesión: {user.FullName}"; Size = new Size(1060, 690);
        var products = Button("PRODUCTOS"); products.Click += (_, _) => Open<ProductForm>();
        var categories = Button("CATEGORÍAS"); categories.Click += (_, _) => Open<CategoryForm>();
        var sales = Button("NUEVA VENTA"); sales.Click += (_, _) => Open<SaleForm>();
        var report = Button("REPORTE PDF"); report.Click += async (_, _) => await GenerateReportAsync();
        var refresh = Button("ACTUALIZAR"); refresh.Click += async (_, _) => await LoadDashboardAsync();
        var nav = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(25, 12, 15, 10), BackColor = Color.White };
        nav.Controls.AddRange([products, categories, sales, report, refresh]);
        var welcome = new Label { Text = $"Hola, {user.FullName}", AutoSize = true, Font = new Font("Segoe UI", 25, FontStyle.Bold), ForeColor = Color.FromArgb(0, 105, 92) };
        var subtitle = new Label { Text = "Panel de control operativo", AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = Color.DimGray, Padding = new Padding(2, 0, 0, 22) };
        var cards = new FlowLayoutPanel { Width = 880, Height = 124, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        cards.Controls.AddRange([Card("PRODUCTOS ACTIVOS", _products, Color.FromArgb(0, 121, 107)), Card("UNIDADES EN STOCK", _units, Color.FromArgb(2, 136, 209)), Card("VENTAS HOY", _sales, Color.FromArgb(123, 31, 162))]);
        var status = new Panel { Width = 850, Height = 115, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 22, 0, 0) };
        var statusTitle = new Label { Text = "ESTADO DEL SISTEMA", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(0, 121, 107), Location = new Point(20, 16) };
        _clock.Location = new Point(20, 42); _rate.Location = new Point(20, 70); status.Controls.AddRange([statusTitle, _clock, _rate]);
        var content = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(48, 34, 48, 25), BackColor = Color.FromArgb(246, 249, 249), WrapContents = false };
        content.Controls.AddRange([welcome, subtitle, cards, status]); Controls.Add(content); Controls.Add(nav);
        _timer = new System.Windows.Forms.Timer { Interval = 1000 }; _timer.Tick += (_, _) => _clock.Text = $"Fecha y hora: {DateTime.Now:F}"; _timer.Start();
        await LoadDashboardAsync(); _ = LoadRateAsync();
    }

    private static Label ValueLabel() => new() { AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(55, 71, 79) };
    private static Panel Card(string heading, Label value, Color color)
    {
        var panel = new Panel { Width = 260, Height = 105, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 18, 0) };
        var label = new Label { Text = heading, AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = color, Location = new Point(18, 16) };
        value.Location = new Point(18, 43); panel.Controls.AddRange([label, value]); return panel;
    }
    private async Task LoadDashboardAsync()
    {
        var result = await RunAsync(async () => { var summary = await dashboard.GetSummaryAsync(); _products.Text = summary.Products.ToString(); _units.Text = summary.Units.ToString("N0"); _sales.Text = summary.SalesToday.ToString(); }, logger);
        if (!result) _products.Text = _units.Text = _sales.Text = "-";
    }
    private async Task LoadRateAsync() => _rate.Text = await rate.GetAsync();
    private void Open<T>() where T : Form { using var scope = provider.CreateScope(); using var form = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider, user); form.ShowDialog(this); _ = LoadDashboardAsync(); }
    private async Task GenerateReportAsync()
    {
        await RunAsync(async () => { using var scope = provider.CreateScope(); var file = await scope.ServiceProvider.GetRequiredService<ReportService>().InventoryPdfAsync(); MessageBox.Show($"Reporte generado correctamente en:\n{file}", "FinalControl"); }, logger);
    }
    protected override void OnFormClosed(FormClosedEventArgs e) { _timer?.Stop(); base.OnFormClosed(e); }
}

public sealed class ProductForm(ProductService products, CategoryService categories, User user, Logger logger) : AppForm
{
    private readonly MaterialTextBox2 _search = new() { Hint = "Buscar por código o nombre", Width = 260 };
    private readonly MaterialTextBox2 _code = new() { Hint = "Código", Width = 140 };
    private readonly MaterialTextBox2 _name = new() { Hint = "Nombre", Width = 220 };
    private readonly ComboBox _category = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
    private readonly NumericUpDown _price = new() { DecimalPlaces = 2, Maximum = 999999, Width = 105 };
    private readonly NumericUpDown _stock = new() { Maximum = 999999, Width = 90 };
    private readonly CheckBox _showInactive = new() { Text = "Mostrar inactivos", AutoSize = true, Margin = new Padding(12) };
    private readonly DataGridView _grid = Grid();
    private List<Product> _items = []; private List<Category> _categories = []; private int _editingId;

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e); Text = "FinalControl | Productos"; Size = new Size(1100, 680);
        var save = Button("GUARDAR"); save.Click += async (_, _) => await SaveAsync(); var fresh = Button("NUEVO"); fresh.Click += (_, _) => ClearForm(); var toggle = Button("ACTIVAR / DESACTIVAR"); toggle.Width = 190; toggle.Click += async (_, _) => await ToggleAsync();
        _search.TextChanged += (_, _) => BindGrid(); _showInactive.CheckedChanged += async (_, _) => await RefreshAsync(); _grid.SelectionChanged += (_, _) => LoadSelected();
        var filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(14), BackColor = Color.White }; filter.Controls.AddRange([_search, _showInactive]);
        var editor = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 125, Padding = new Padding(14), BackColor = Color.FromArgb(246, 249, 249) };
        editor.Controls.AddRange([_code, _name, new Label { Text = "Categoría", AutoSize = true, Padding = new Padding(5, 8, 0, 0) }, _category, new Label { Text = "Precio", AutoSize = true, Padding = new Padding(5, 8, 0, 0) }, _price, new Label { Text = "Stock", AutoSize = true, Padding = new Padding(5, 8, 0, 0) }, _stock, save, fresh, toggle]);
        Controls.Add(_grid); Controls.Add(editor); Controls.Add(filter); await LoadCategoriesAsync(); await RefreshAsync();
    }
    private static DataGridView Grid() => new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private async Task LoadCategoriesAsync() { _categories = await categories.GetAsync(); _category.DataSource = _categories; _category.DisplayMember = "Name"; _category.ValueMember = "Id"; }
    private async Task RefreshAsync() { _items = await products.GetAsync(_showInactive.Checked); BindGrid(); }
    private void BindGrid()
    {
        var term = _search.Text.Trim(); var rows = _items.Where(product => string.IsNullOrWhiteSpace(term) || product.Code.Contains(term, StringComparison.OrdinalIgnoreCase) || product.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).Select(product => new { product.Id, product.Code, product.Name, Category = product.Category?.Name, product.Price, product.Stock, Estado = product.Active ? "Activo" : "Inactivo" }).ToList();
        _grid.DataSource = rows; if (_grid.Columns.Contains("Id")) _grid.Columns["Id"].Visible = false;
    }
    private void LoadSelected()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is not int id) return; var product = _items.FirstOrDefault(item => item.Id == id); if (product is null) return;
        _editingId = product.Id; _code.Text = product.Code; _name.Text = product.Name; _price.Value = product.Price; _stock.Value = product.Stock; _category.SelectedValue = product.CategoryId;
    }
    private void ClearForm() { _editingId = 0; _code.Text = _name.Text = ""; _price.Value = _stock.Value = 0; if (_categories.Count > 0) _category.SelectedIndex = 0; _grid.ClearSelection(); _code.Focus(); }
    private async Task SaveAsync()
    {
        var product = _editingId == 0 ? new Product() : _items.First(item => item.Id == _editingId);
        product.Code = _code.Text.Trim(); product.Name = _name.Text.Trim(); product.Price = _price.Value; product.Stock = (int)_stock.Value; product.CategoryId = _category.SelectedValue is int id ? id : 0; product.Active = true;
        if (await RunAsync(() => products.SaveAsync(product), logger)) { ClearForm(); await RefreshAsync(); }
    }
    private async Task ToggleAsync()
    {
        if (_editingId == 0) { MessageBox.Show("Seleccione un producto."); return; }
        var product = _items.First(item => item.Id == _editingId); if (await RunAsync(() => products.SetActiveAsync(product, !product.Active), logger)) { ClearForm(); await RefreshAsync(); }
    }
}

public sealed class CategoryForm(CategoryService categories, User user, Logger logger) : AppForm
{
    private readonly MaterialTextBox2 _name = new() { Hint = "Nombre de categoría", Width = 300 }; private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill }; private List<Category> _items = []; private int _editingId;
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e); Text = "FinalControl | Categorías"; Size = new Size(700, 500); var save = Button("GUARDAR"); save.Click += async (_, _) => await SaveAsync(); var fresh = Button("NUEVO"); fresh.Click += (_, _) => ClearForm(); var delete = Button("ELIMINAR"); delete.Click += async (_, _) => await DeleteAsync(); var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(14), BackColor = Color.FromArgb(246, 249, 249) }; top.Controls.AddRange([_name, save, fresh, delete]); _grid.SelectionChanged += (_, _) => LoadSelected(); Controls.Add(_grid); Controls.Add(top); await RefreshAsync();
    }
    private async Task RefreshAsync() { _items = await categories.GetAsync(); _grid.DataSource = _items.Select(category => new { category.Id, category.Name }).ToList(); if (_grid.Columns.Contains("Id")) _grid.Columns["Id"].Visible = false; }
    private void LoadSelected() { if (_grid.CurrentRow?.Cells["Id"].Value is not int id) return; var category = _items.FirstOrDefault(item => item.Id == id); if (category is null) return; _editingId = id; _name.Text = category.Name; }
    private void ClearForm() { _editingId = 0; _name.Text = ""; _grid.ClearSelection(); _name.Focus(); }
    private async Task SaveAsync() { var category = _editingId == 0 ? new Category() : _items.First(item => item.Id == _editingId); category.Name = _name.Text.Trim(); if (await RunAsync(() => categories.SaveAsync(category), logger)) { ClearForm(); await RefreshAsync(); } }
    private async Task DeleteAsync() { if (_editingId == 0) { MessageBox.Show("Seleccione una categoría."); return; } var category = _items.First(item => item.Id == _editingId); if (MessageBox.Show($"¿Eliminar '{category.Name}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes && await RunAsync(() => categories.DeleteAsync(category), logger)) { ClearForm(); await RefreshAsync(); } }
}

public sealed class SaleForm(ProductService products, SaleService sales, User user, Logger logger) : AppForm
{
    private readonly ComboBox _product = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 310 }; private readonly NumericUpDown _quantity = new() { Minimum = 1, Maximum = 999, Width = 90 }; private readonly DataGridView _lines = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill }; private readonly Label _total = new() { AutoSize = true, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(0, 105, 92), Padding = new Padding(12) }; private readonly List<SaleLine> _cart = []; private List<Product> _items = [];
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e); Text = "FinalControl | Nueva venta"; Size = new Size(850, 580); var add = Button("AGREGAR"); add.Click += (_, _) => AddLine(); var remove = Button("QUITAR LÍNEA"); remove.Click += (_, _) => RemoveLine(); var confirm = Button("CONFIRMAR VENTA"); confirm.Width = 180; confirm.Click += async (_, _) => await ConfirmAsync(); var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 74, Padding = new Padding(14), BackColor = Color.FromArgb(246, 249, 249) }; top.Controls.AddRange([_product, _quantity, add, remove, confirm]); var footer = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White }; footer.Controls.Add(_total); Controls.Add(_lines); Controls.Add(footer); Controls.Add(top); _items = (await products.GetAsync(false)).Where(product => product.Stock > 0).ToList(); _product.DataSource = _items; _product.DisplayMember = "Name"; _product.ValueMember = "Id"; RefreshCart();
    }
    private void AddLine()
    {
        if (_product.SelectedItem is not Product product) return; var quantity = (int)_quantity.Value; var current = _cart.FirstOrDefault(line => line.ProductId == product.Id); var alreadyAdded = current?.Quantity ?? 0; if (quantity + alreadyAdded > product.Stock) { MessageBox.Show("La cantidad supera el stock disponible."); return; } if (current is not null) { _cart.Remove(current); _cart.Add(current with { Quantity = current.Quantity + quantity }); } else _cart.Add(new SaleLine(product.Id, quantity)); RefreshCart();
    }
    private void RemoveLine() { if (_lines.CurrentRow?.Cells["ProductId"].Value is int id) { var line = _cart.FirstOrDefault(item => item.ProductId == id); if (line is not null) _cart.Remove(line); RefreshCart(); } }
    private void RefreshCart() { _lines.DataSource = _cart.Join(_items, line => line.ProductId, product => product.Id, (line, product) => new { ProductId = product.Id, product.Code, product.Name, Cantidad = line.Quantity, Precio = product.Price, Subtotal = line.Quantity * product.Price }).ToList(); if (_lines.Columns.Contains("ProductId")) _lines.Columns["ProductId"].Visible = false; _total.Text = $"Total: C$ {_cart.Join(_items, line => line.ProductId, product => product.Id, (line, product) => line.Quantity * product.Price).Sum():N2}"; }
    private async Task ConfirmAsync() { if (await RunAsync(async () => { var sale = await sales.CreateAsync(user.Id, _cart); MessageBox.Show($"Venta #{sale.Id} registrada. Total: C$ {sale.Total:N2}", "Venta completada"); _cart.Clear(); _items = (await products.GetAsync(false)).Where(product => product.Stock > 0).ToList(); _product.DataSource = _items; RefreshCart(); }, logger)) { } }
}
