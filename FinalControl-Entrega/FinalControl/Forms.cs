using FinalControl.Models;
using FinalControl.Services;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Drawing2D;

namespace FinalControl.Forms;

public abstract class AppForm : MaterialForm
{
    protected static readonly Color Canvas = Color.FromArgb(244, 247, 251);
    protected static readonly Color Surface = Color.White;
    protected static readonly Color Ink = Color.FromArgb(29, 41, 57);
    protected static readonly Color Muted = Color.FromArgb(102, 112, 133);
    protected static readonly Color PrimaryColor = Color.FromArgb(21, 94, 117);
    protected static readonly Color BorderColor = Color.FromArgb(228, 231, 236);
    protected static readonly Color DangerColor = Color.FromArgb(217, 45, 32);

    protected AppForm()
    {
        var skin = MaterialSkinManager.Instance;
        skin.AddFormToManage(this);
        skin.Theme = MaterialSkinManager.Themes.LIGHT;
        skin.ColorScheme = new ColorScheme(Primary.Teal600, Primary.Teal700, Primary.Teal200, Accent.Cyan200, TextShade.WHITE);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Canvas;
        ForeColor = Ink;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        DoubleBuffered = true;
    }

    protected async Task<bool> RunAsync(Func<Task> action, Logger logger)
    {
        try { await action(); return true; }
        catch (Exception exception)
        {
            logger.Error(exception);
            var message = exception is InvalidOperationException
                ? exception.Message
                : $"Ocurrió un error inesperado. Consulte el registro en:\n{logger.FilePath}";
            MessageBox.Show(message, "FinalControl", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }

    protected static System.Windows.Forms.Button Button(string text, bool primary = true)
    {
        var button = new System.Windows.Forms.Button
        {
            Text = text,
            AutoSize = false,
            Width = 150,
            Height = 40,
            Margin = new Padding(6),
            Padding = new Padding(12, 0, 12, 0),
            BackColor = primary ? PrimaryColor : Surface,
            ForeColor = primary ? Color.White : Ink,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = BorderColor;
        var baseColor = button.BackColor;
        button.MouseEnter += (_, _) => button.BackColor = primary ? Color.FromArgb(16, 75, 94) : Color.FromArgb(239, 243, 248);
        button.MouseLeave += (_, _) => button.BackColor = baseColor;
        return button;
    }

    protected static Panel PageHeader(string section, string title, string subtitle)
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 94, BackColor = Surface, Padding = new Padding(28, 12, 24, 10) };
        var eyebrow = new Label { Text = section.ToUpperInvariant(), AutoSize = true, Location = new Point(28, 12), ForeColor = PrimaryColor, Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold) };
        var heading = new Label { Text = title, AutoSize = true, Location = new Point(28, 30), ForeColor = Ink, Font = new Font("Segoe UI", 20F, FontStyle.Bold) };
        var detail = new Label { Text = subtitle, AutoSize = true, Location = new Point(30, 66), ForeColor = Muted, Font = new Font("Segoe UI", 9F) };
        header.Controls.AddRange([eyebrow, heading, detail]);
        header.Paint += (_, e) => { using var pen = new Pen(BorderColor); e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1); };
        return header;
    }

    protected static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Color.FromArgb(240, 242, 245);
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight = 44;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252), ForeColor = Muted, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold), Padding = new Padding(12, 0, 4, 0), SelectionBackColor = Color.FromArgb(248, 250, 252), SelectionForeColor = Muted };
        grid.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Surface, ForeColor = Ink, Font = new Font("Segoe UI", 9F), Padding = new Padding(12, 5, 4, 5), SelectionBackColor = Color.FromArgb(232, 244, 247), SelectionForeColor = Ink };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(250, 251, 253), ForeColor = Ink, SelectionBackColor = Color.FromArgb(232, 244, 247), SelectionForeColor = Ink };
        grid.RowHeadersVisible = false;
        grid.RowTemplate.Height = 42;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AllowUserToAddRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.DataBindingComplete += (_, _) => FormatGridColumns(grid);
        FormatGridColumns(grid);
    }

    private static void FormatGridColumns(DataGridView grid)
    {
        var headings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Code"] = "Código", ["Name"] = "Nombre", ["Category"] = "Categoría",
            ["Price"] = "Precio", ["Stock"] = "Existencias", ["Estado"] = "Estado",
            ["Cantidad"] = "Cantidad", ["Subtotal"] = "Subtotal", ["ProductId"] = "Producto"
        };
        foreach (DataGridViewColumn column in grid.Columns)
        {
            if (column.Name is "Id" or "ProductId") { column.Visible = false; continue; }
            if (headings.TryGetValue(column.Name, out var heading)) column.HeaderText = heading;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }

    protected static void StyleFilter(MaterialTextBox2 input) => input.Font = new Font("Segoe UI", 9F);
}

internal sealed class GradientPanel : Panel
{
    public GradientPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (ClientRectangle.Width > 0 && ClientRectangle.Height > 0)
        {
            using var brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(16, 42, 67), Color.FromArgb(21, 94, 117), 35F);
            e.Graphics.FillRectangle(brush, ClientRectangle);
            using var glow = new SolidBrush(Color.FromArgb(25, 255, 255, 255));
            e.Graphics.FillEllipse(glow, ClientSize.Width - 170, -85, 250, 250);
            e.Graphics.FillEllipse(glow, -110, ClientSize.Height - 125, 220, 220);
        }
        base.OnPaint(e);
    }
}

public sealed class LoginForm(AuthenticationService auth, IServiceProvider provider, Logger logger) : AppForm
{
    private readonly MaterialTextBox2 _user = new() { Hint = "Usuario", Width = 320 };
    private readonly MaterialTextBox2 _password = new() { Hint = "Contraseña", Width = 320, UseSystemPasswordChar = true };
    private readonly CheckBox _showPassword = new() { Text = "Mostrar contraseña", AutoSize = true, ForeColor = Muted, Margin = new Padding(2, 6, 0, 14), Cursor = Cursors.Hand };

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e); Text = "FinalControl | Iniciar sesión"; Size = new Size(850, 590); MinimumSize = new Size(820, 560); MaximizeBox = false; MinimizeBox = false;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Surface };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        var brand = new GradientPanel { Dock = DockStyle.Fill, Padding = new Padding(38) };
        var brandTop = new Label { Text = "FINALCONTROL", AutoSize = true, Location = new Point(38, 48), Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 242, 235), BackColor = Color.Transparent };
        var brandTitle = new Label { Text = "Todo bajo\ncontrol.", AutoSize = true, Location = new Point(38, 148), Font = new Font("Segoe UI", 34F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent };
        var brandText = new Label { Text = "Administra tu inventario,\nregistra ventas y consulta\ntus resultados desde un\nsolo lugar.", AutoSize = true, Location = new Point(42, 270), Font = new Font("Segoe UI", 12F), ForeColor = Color.FromArgb(220, 232, 241), BackColor = Color.Transparent };
        var brandFoot = new Label { Text = "INVENTARIO  ·  VENTAS  ·  REPORTES", AutoSize = true, Location = new Point(40, 490), Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 242, 235), BackColor = Color.Transparent };
        brand.Controls.AddRange([brandTop, brandTitle, brandText, brandFoot]);

        var formPanel = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(46, 30, 36, 26) };
        var content = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Surface, Padding = new Padding(0, 20, 0, 0) };
        var welcome = new Label { Text = "Bienvenido de nuevo", AutoSize = true, ForeColor = Ink, Font = new Font("Segoe UI", 23F, FontStyle.Bold), Margin = new Padding(0, 0, 0, 4) };
        var subtitle = new Label { Text = "Ingresa tus datos para continuar", AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 10F), Margin = new Padding(2, 0, 0, 24) };
        _user.Width = 330; _password.Width = 330; StyleFilter(_user); StyleFilter(_password);
        _showPassword.CheckedChanged += (_, _) => _password.UseSystemPasswordChar = !_showPassword.Checked;
        var login = Button("INICIAR SESIÓN"); login.Width = 330; login.Height = 48; login.Margin = new Padding(0, 6, 0, 14); login.Click += async (_, _) => await LoginAsync();
        var demo = new Label { Text = "ACCESO DE DEMOSTRACIÓN\nUsuario: admin     Contraseña: Admin123*", AutoSize = true, ForeColor = Muted, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(14), Font = new Font("Segoe UI", 8.5F), Margin = new Padding(0, 10, 0, 0) };
        content.Controls.AddRange([welcome, subtitle, _user, _password, _showPassword, login, demo]);
        formPanel.Controls.Add(content);
        layout.Controls.Add(brand, 0, 0); layout.Controls.Add(formPanel, 1, 0); Controls.Add(layout);
        AcceptButton = login;
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
        catch (Exception exception)
        {
            logger.Error(exception);
            MessageBox.Show($"No se pudo iniciar sesión. Revise el registro en:\n{logger.FilePath}");
        }
    }
}

public sealed class MainForm(IServiceProvider provider, User user, ExchangeRateService rate, DashboardService dashboard, Logger logger) : AppForm
{
    private readonly Label _clock = ValueLabel();
    private readonly Label _rate = ValueLabel();
    private readonly Label _products = ValueLabel();
    private readonly Label _units = ValueLabel();
    private readonly Label _sales = ValueLabel();
    private readonly List<System.Windows.Forms.Button> _navigation = [];
    private System.Windows.Forms.Timer? _timer;

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e); Text = $"FinalControl | {user.FullName}"; Size = new Size(1320, 850); MinimumSize = new Size(1120, 720); WindowState = FormWindowState.Maximized;

        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Canvas };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 232)); shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var sidebar = BuildSidebar();
        var workspace = new Panel { Dock = DockStyle.Fill, BackColor = Canvas };
        var topbar = BuildTopBar();
        var body = new Panel { Dock = DockStyle.Fill, BackColor = Canvas, AutoScroll = true };
        var content = BuildDashboardContent();
        body.Controls.Add(content);
        body.Resize += (_, _) => ResizeDashboardContent(body, content);
        ResizeDashboardContent(body, content);
        workspace.Controls.Add(body); workspace.Controls.Add(topbar);
        shell.Controls.Add(sidebar, 0, 0); shell.Controls.Add(workspace, 1, 0); Controls.Add(shell);

        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += (_, _) => _clock.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy  ·  HH:mm");
        _clock.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy  ·  HH:mm");
        _timer.Start();
        await LoadDashboardAsync();
        _ = LoadRateAsync();
    }

    private Panel BuildSidebar()
    {
        var sidebar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(16, 24, 40) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = sidebar.BackColor };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        var brand = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22, 23, 12, 10), BackColor = sidebar.BackColor };
        brand.Controls.Add(new Label { Text = "FINALCONTROL", AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold), Location = new Point(20, 22) });
        brand.Controls.Add(new Label { Text = "GESTIÓN COMERCIAL", AutoSize = true, ForeColor = Color.FromArgb(152, 166, 186), Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold), Location = new Point(22, 54) });
        var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(12, 12, 12, 10), BackColor = sidebar.BackColor };
        AddNavigationItem(nav, "  Resumen", () => { SelectNavigation(0); return LoadDashboardAsync(); }, true);
        AddNavigationItem(nav, "  Productos", () => { SelectNavigation(1); Open<ProductForm>(); return Task.CompletedTask; });
        AddNavigationItem(nav, "  Categorías", () => { SelectNavigation(2); Open<CategoryForm>(); return Task.CompletedTask; });
        AddNavigationItem(nav, "  Nueva venta", () => { SelectNavigation(3); Open<SaleForm>(); return Task.CompletedTask; });
        AddNavigationItem(nav, "  Reporte de inventario", async () => { SelectNavigation(4); await GenerateReportAsync(); SelectNavigation(0); });
        var account = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 17, 12, 10), BackColor = Color.FromArgb(22, 34, 52) };
        account.Controls.Add(new Label { Text = user.FullName, AutoSize = false, Width = 190, Height = 24, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), Location = new Point(18, 17) });
        account.Controls.Add(new Label { Text = "Sesión activa  ·  Cerrar sesión", AutoSize = true, ForeColor = Color.FromArgb(152, 166, 186), Font = new Font("Segoe UI", 8F), Location = new Point(18, 43), Cursor = Cursors.Hand });
        account.Controls[1].Click += (_, _) => Close();
        layout.Controls.Add(brand, 0, 0); layout.Controls.Add(nav, 0, 1); layout.Controls.Add(account, 0, 2); sidebar.Controls.Add(layout);
        return sidebar;
    }

    private void AddNavigationItem(FlowLayoutPanel nav, string text, Func<Task> action, bool selected = false)
    {
        var button = new System.Windows.Forms.Button { Text = text, Width = 190, Height = 46, Margin = new Padding(0, 3, 0, 3), Padding = new Padding(10, 0, 8, 0), TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = selected ? Color.White : Color.FromArgb(190, 203, 218), BackColor = selected ? Color.FromArgb(40, 61, 81) : Color.FromArgb(16, 24, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, UseVisualStyleBackColor = false, Tag = _navigation.Count };
        button.FlatAppearance.BorderSize = 0;
        button.MouseEnter += (_, _) => { if ((int)button.Tag != ActiveNavigation) button.BackColor = Color.FromArgb(31, 45, 64); };
        button.MouseLeave += (_, _) => { if ((int)button.Tag != ActiveNavigation) button.BackColor = Color.FromArgb(16, 24, 40); };
        button.Click += async (_, _) => await action();
        _navigation.Add(button); nav.Controls.Add(button);
        if (selected) ActiveNavigation = (int)button.Tag;
    }

    private int ActiveNavigation { get; set; } = -1;
    private void SelectNavigation(int index)
    {
        ActiveNavigation = index;
        for (var i = 0; i < _navigation.Count; i++)
        {
            var selected = i == index;
            _navigation[i].BackColor = selected ? Color.FromArgb(40, 61, 81) : Color.FromArgb(16, 24, 40);
            _navigation[i].ForeColor = selected ? Color.White : Color.FromArgb(190, 203, 218);
        }
    }

    private Panel BuildTopBar()
    {
        var topbar = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Surface, Padding = new Padding(26, 12, 20, 10) };
        var section = new Label { Text = "ESCRITORIO", AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold), Location = new Point(28, 27) };
        _clock.Font = new Font("Segoe UI", 9F); _clock.ForeColor = Muted; _clock.Anchor = AnchorStyles.Top | AnchorStyles.Right; _clock.Location = new Point(550, 27);
        var refresh = Button("↻  Actualizar", false); refresh.Width = 132; refresh.Height = 38; refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right; refresh.Location = new Point(890, 17); refresh.Click += async (_, _) => await LoadDashboardAsync();
        topbar.Controls.AddRange([section, _clock, refresh]);
        topbar.Resize += (_, _) => { refresh.Left = topbar.ClientSize.Width - refresh.Width - 20; _clock.Left = Math.Max(section.Right + 30, refresh.Left - _clock.Width - 22); };
        return topbar;
    }

    private FlowLayoutPanel BuildDashboardContent()
    {
        var content = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 580, Width = 900, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(30, 27, 30, 20), BackColor = Canvas };
        var welcome = new Label { Text = $"Hola, {user.FullName}", AutoSize = true, Font = new Font("Segoe UI", 25F, FontStyle.Bold), ForeColor = Ink, Margin = new Padding(0, 0, 0, 2) };
        var subtitle = new Label { Text = "Aquí tienes el resumen de la operación de hoy.", AutoSize = true, Font = new Font("Segoe UI", 10F), ForeColor = Muted, Margin = new Padding(2, 0, 0, 22) };
        var cards = new TableLayoutPanel { Height = 142, Width = 840, ColumnCount = 3, RowCount = 1, BackColor = Canvas, Margin = new Padding(0, 0, 0, 22), Padding = new Padding(0) };
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        cards.Controls.Add(MetricCard("PRODUCTOS ACTIVOS", _products, "Artículos disponibles", Color.FromArgb(21, 94, 117), () => Open<ProductForm>()), 0, 0);
        cards.Controls.Add(MetricCard("UNIDADES EN STOCK", _units, "Existencias registradas", Color.FromArgb(21, 112, 239), () => Open<ProductForm>()), 1, 0);
        cards.Controls.Add(MetricCard("VENTAS DE HOY", _sales, "Transacciones realizadas", Color.FromArgb(127, 86, 217), () => Open<SaleForm>()), 2, 0);

        var actions = new Panel { Height = 126, Width = 840, BackColor = Surface, Margin = new Padding(0, 0, 0, 18), Padding = new Padding(20, 15, 16, 12) };
        actions.Paint += (_, e) => { using var pen = new Pen(BorderColor); e.Graphics.DrawRectangle(pen, 0, 0, actions.Width - 1, actions.Height - 1); };
        actions.Controls.Add(new Label { Text = "Acciones rápidas", AutoSize = true, ForeColor = Ink, Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold), Location = new Point(20, 14) });
        var shortcuts = new FlowLayoutPanel { Location = new Point(13, 49), Height = 54, Width = 800, WrapContents = false, BackColor = Surface };
        var sale = Button("＋  Registrar venta"); sale.Width = 176; sale.Click += (_, _) => Open<SaleForm>();
        var inventory = Button("▤  Ver inventario", false); inventory.Width = 166; inventory.Click += (_, _) => Open<ProductForm>();
        var categories = Button("▦  Categorías", false); categories.Width = 144; categories.Click += (_, _) => Open<CategoryForm>();
        shortcuts.Controls.AddRange([sale, inventory, categories]); actions.Controls.Add(shortcuts);

        var status = new Panel { Height = 104, Width = 840, BackColor = Surface, Padding = new Padding(20, 14, 16, 10) };
        status.Paint += (_, e) => { using var pen = new Pen(BorderColor); e.Graphics.DrawRectangle(pen, 0, 0, status.Width - 1, status.Height - 1); };
        status.Controls.Add(new Label { Text = "ESTADO DE LA SESIÓN", AutoSize = true, Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold), ForeColor = PrimaryColor, Location = new Point(20, 14) });
        _rate.Location = new Point(20, 45); _rate.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold); _rate.ForeColor = Ink;
        var state = new Label { Text = "●  Sesión activa", AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(3, 152, 85), Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(690, 47) };
        status.Controls.AddRange([_rate, state]);

        content.Controls.AddRange([welcome, subtitle, cards, actions, status]);
        content.Resize += (_, _) => ResizeDashboardContent(content.Parent as Panel, content);
        return content;
    }

    private Panel MetricCard(string heading, Label value, string caption, Color accent, Action click)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(0, 0, 14, 0), Cursor = Cursors.Hand };
        card.Paint += (_, e) => { using var pen = new Pen(BorderColor); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
        var stripe = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };
        var title = new Label { Text = heading, AutoSize = true, Location = new Point(20, 16), ForeColor = Muted, Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold), Cursor = Cursors.Hand };
        value.AutoSize = true; value.Location = new Point(20, 41); value.ForeColor = Ink; value.Font = new Font("Segoe UI", 25F, FontStyle.Bold); value.Cursor = Cursors.Hand;
        var detail = new Label { Text = caption, AutoSize = true, Location = new Point(21, 103), ForeColor = Muted, Font = new Font("Segoe UI", 8.5F), Cursor = Cursors.Hand };
        card.Controls.AddRange([stripe, title, value, detail]);
        void Hover(bool active)
        {
            card.BackColor = active ? Color.FromArgb(250, 252, 254) : Surface;
            foreach (Control child in card.Controls) child.BackColor = card.BackColor;
            card.BackColor = active ? Color.FromArgb(250, 252, 254) : Surface;
            stripe.BackColor = active ? Color.FromArgb(Math.Min(255, accent.R + 15), Math.Min(255, accent.G + 15), Math.Min(255, accent.B + 15)) : accent;
        }
        card.Click += (_, _) => click(); card.MouseEnter += (_, _) => Hover(true); card.MouseLeave += (_, _) => Hover(false);
        foreach (Control child in card.Controls) { child.Click += (_, _) => click(); child.MouseEnter += (_, _) => Hover(true); child.MouseLeave += (_, _) => Hover(false); }
        return card;
    }

    private static void ResizeDashboardContent(Panel? body, FlowLayoutPanel content)
    {
        if (body is null || body.ClientSize.Width <= 0) return;
        content.Width = Math.Max(700, body.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        if (content.Controls.Count > 2 && content.Controls[2] is TableLayoutPanel cards)
        {
            cards.Width = content.ClientSize.Width - content.Padding.Horizontal;
            var cardWidth = Math.Max(180, (cards.Width - 60) / 3);
            foreach (Control card in cards.Controls) card.Width = cardWidth;
        }
        foreach (var index in new[] { 3, 4 })
            if (content.Controls.Count > index) content.Controls[index].Width = content.ClientSize.Width - content.Padding.Horizontal;
    }

    private static Label ValueLabel() => new() { AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Ink };
    private async Task LoadDashboardAsync()
    {
        var result = await RunAsync(async () => { var summary = await dashboard.GetSummaryAsync(); _products.Text = summary.Products.ToString(); _units.Text = summary.Units.ToString("N0"); _sales.Text = summary.SalesToday.ToString(); }, logger);
        if (!result) _products.Text = _units.Text = _sales.Text = "-";
    }
    private async Task LoadRateAsync()
    {
        var value = await rate.GetAsync();
        if (!IsDisposed && !Disposing)
        {
            _rate.Text = value;
        }
    }
    private void Open<T>() where T : Form { using var scope = provider.CreateScope(); using var form = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider, user); form.ShowDialog(this); SelectNavigation(0); _ = LoadDashboardAsync(); }
    private async Task GenerateReportAsync()
    {
        await RunAsync(async () => { using var scope = provider.CreateScope(); var file = await scope.ServiceProvider.GetRequiredService<ReportService>().InventoryPdfAsync(); MessageBox.Show($"Reporte generado correctamente en:\n{file}", "FinalControl"); }, logger);
    }
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        base.OnFormClosed(e);
    }
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
        base.OnLoad(e); Text = $"FinalControl | Productos | {user.FullName}"; Size = new Size(1180, 760); MinimumSize = new Size(1000, 660);
        _code.Width = 120; _name.Width = 180; _category.Width = 150; _price.Width = 95; _stock.Width = 80;
        var save = Button("Guardar producto"); save.Width = 166; save.Click += async (_, _) => await SaveAsync(); var fresh = Button("Nuevo", false); fresh.Width = 110; fresh.Click += (_, _) => ClearForm(); var toggle = Button("Activar / desactivar", false); toggle.Width = 180; toggle.Click += async (_, _) => await ToggleAsync();
        StyleFilter(_search);
        _search.TextChanged += (_, _) => BindGrid(); _showInactive.CheckedChanged += async (_, _) => await RefreshAsync(); _grid.SelectionChanged += (_, _) => LoadSelected();
        _category.FlatStyle = FlatStyle.Flat;
        var filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 66, Padding = new Padding(20, 10, 18, 8), BackColor = Surface, WrapContents = false };
        filter.Controls.AddRange([_search, _showInactive, new Label { Text = "Selecciona una fila para editar sus datos", AutoSize = true, ForeColor = Muted, Padding = new Padding(8, 14, 0, 0) }]);
        var editor = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 132, Padding = new Padding(18, 12, 12, 8), BackColor = Color.FromArgb(248, 250, 252), WrapContents = true };
        editor.Controls.AddRange([_code, _name, new Label { Text = "Categoría", AutoSize = true, ForeColor = Muted, Padding = new Padding(5, 12, 0, 0) }, _category, new Label { Text = "Precio", AutoSize = true, ForeColor = Muted, Padding = new Padding(5, 12, 0, 0) }, _price, new Label { Text = "Stock", AutoSize = true, ForeColor = Muted, Padding = new Padding(5, 12, 0, 0) }, _stock, save, fresh, toggle]);
        editor.SetFlowBreak(_stock, true);
        editor.Paint += (_, e) => { using var pen = new Pen(BorderColor); e.Graphics.DrawLine(pen, 0, editor.Height - 1, editor.Width, editor.Height - 1); };
        StyleGrid(_grid);
        _grid.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex < 0) return;
            if (_grid.Columns[e.ColumnIndex].Name == "Stock" && e.Value is int stock && stock <= 5)
            {
                e.CellStyle.ForeColor = stock == 0 ? DangerColor : Color.FromArgb(181, 71, 8);
                e.CellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            }
            else if (_grid.Columns[e.ColumnIndex].Name == "Estado" && Equals(e.Value, "Activo"))
                e.CellStyle.ForeColor = Color.FromArgb(3, 152, 85);
        };
        Controls.Add(_grid); Controls.Add(editor); Controls.Add(filter); Controls.Add(PageHeader("Catálogo", "Productos", "Controla precios, existencias y disponibilidad de cada artículo."));
        await RunAsync(async () =>
        {
            await LoadCategoriesAsync();
            await RefreshAsync();
        }, logger);
    }
    private static DataGridView Grid() => new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private async Task LoadCategoriesAsync() { _categories = await categories.GetAsync(); _category.DataSource = _categories; _category.DisplayMember = "Name"; _category.ValueMember = "Id"; }
    private async Task RefreshAsync() { _items = await products.GetAsync(_showInactive.Checked); BindGrid(); }
    private void BindGrid()
    {
        var term = _search.Text.Trim(); var rows = _items.Where(product => string.IsNullOrWhiteSpace(term) || product.Code.Contains(term, StringComparison.OrdinalIgnoreCase) || product.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).Select(product => new { product.Id, product.Code, product.Name, Category = product.Category?.Name, product.Price, product.Stock, Estado = product.Active ? "Activo" : "Inactivo" }).ToList();
        _grid.DataSource = rows; if (_grid.Columns["Id"] is { } idColumn) idColumn.Visible = false;
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
    private readonly MaterialTextBox2 _name = new() { Hint = "Nombre de categoría", Width = 300 };
    private readonly MaterialTextBox2 _search = new() { Hint = "Buscar categoría", Width = 210 };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private List<Category> _items = []; private int _editingId;
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e); Text = $"FinalControl | Categorías | {user.FullName}"; Size = new Size(1080, 680); MinimumSize = new Size(980, 580);
        StyleFilter(_name); StyleFilter(_search); _name.Width = 230;
        var save = Button("Guardar categoría"); save.Width = 146; save.Click += async (_, _) => await SaveAsync();
        var fresh = Button("Nueva", false); fresh.Width = 86; fresh.Click += (_, _) => ClearForm();
        var delete = Button("Eliminar", false); delete.Width = 94; delete.ForeColor = DangerColor; delete.Click += async (_, _) => await DeleteAsync();
        _search.TextChanged += (_, _) => BindGrid();
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 76, Padding = new Padding(18, 10, 14, 8), BackColor = Surface, WrapContents = false };
        top.Controls.AddRange([_search, _name, save, fresh, delete]);
        StyleGrid(_grid);
        _grid.SelectionChanged += (_, _) => LoadSelected(); Controls.Add(_grid); Controls.Add(top); Controls.Add(PageHeader("Organización", "Categorías", "Mantén el catálogo ordenado y encuentra categorías al instante."));
        await RunAsync(RefreshAsync, logger);
    }
    private async Task RefreshAsync() { _items = await categories.GetAsync(); BindGrid(); }
    private void BindGrid() { var term = _search.Text.Trim(); _grid.DataSource = _items.Where(category => string.IsNullOrWhiteSpace(term) || category.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).Select(category => new { category.Id, category.Name }).ToList(); }
    private void LoadSelected() { if (_grid.CurrentRow?.Cells["Id"].Value is not int id) return; var category = _items.FirstOrDefault(item => item.Id == id); if (category is null) return; _editingId = id; _name.Text = category.Name; }
    private void ClearForm() { _editingId = 0; _name.Text = ""; _grid.ClearSelection(); _name.Focus(); }
    private async Task SaveAsync() { var category = _editingId == 0 ? new Category() : _items.First(item => item.Id == _editingId); category.Name = _name.Text.Trim(); if (await RunAsync(() => categories.SaveAsync(category), logger)) { ClearForm(); await RefreshAsync(); } }
    private async Task DeleteAsync() { if (_editingId == 0) { MessageBox.Show("Seleccione una categoría."); return; } var category = _items.First(item => item.Id == _editingId); if (MessageBox.Show($"¿Eliminar '{category.Name}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes && await RunAsync(() => categories.DeleteAsync(category), logger)) { ClearForm(); await RefreshAsync(); } }
}

public sealed class SaleForm(ProductService products, SaleService sales, User user, Logger logger) : AppForm
{
    private readonly ComboBox _product = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240, FlatStyle = FlatStyle.Flat };
    private readonly NumericUpDown _quantity = new() { Minimum = 1, Maximum = 999, Width = 70, Font = new Font("Segoe UI", 10F), TextAlign = HorizontalAlignment.Center };
    private readonly DataGridView _lines = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly Label _total = new() { AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = PrimaryColor, Padding = new Padding(12) };
    private readonly Label _cartCount = new() { AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Muted, Padding = new Padding(12, 20, 0, 0) };
    private readonly List<SaleLine> _cart = []; private List<Product> _items = [];
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e); Text = "FinalControl | Nueva venta"; Size = new Size(1180, 760); MinimumSize = new Size(1080, 650);
        var add = Button("＋  Agregar al carrito"); add.Width = 155; add.Click += (_, _) => AddLine();
        var remove = Button("Quitar seleccionada", false); remove.Width = 150; remove.ForeColor = DangerColor; remove.Click += (_, _) => RemoveLine();
        var confirm = Button("Confirmar venta"); confirm.Width = 155; confirm.Click += async (_, _) => await ConfirmAsync();
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 82, Padding = new Padding(18, 15, 12, 8), BackColor = Surface, WrapContents = false };
        top.Controls.Add(new Label { Text = "PRODUCTO", AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold), Padding = new Padding(2, 14, 0, 0) });
        top.Controls.Add(_product); top.Controls.Add(new Label { Text = "CANT.", AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold), Padding = new Padding(4, 14, 0, 0) });
        top.Controls.AddRange([_quantity, add, remove, confirm]);
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 76, BackColor = Surface, Padding = new Padding(14, 3, 18, 3) };
        _total.Anchor = AnchorStyles.Top | AnchorStyles.Right; _cartCount.Location = new Point(24, 18);
        footer.Controls.AddRange([_cartCount, _total]); footer.Resize += (_, _) => _total.Left = footer.ClientSize.Width - _total.Width - 22;
        StyleGrid(_lines);
        Controls.Add(_lines); Controls.Add(footer); Controls.Add(top); Controls.Add(PageHeader("Punto de venta", "Nueva venta", "Agrega productos al carrito y revisa el total antes de confirmar."));
        await RunAsync(async () =>
        {
            _items = (await products.GetAsync(false)).Where(product => product.Stock > 0).ToList();
            _product.DataSource = _items;
            _product.DisplayMember = "Name";
            _product.ValueMember = "Id";
            RefreshCart();
        }, logger);
    }
    private void AddLine()
    {
        if (_product.SelectedItem is not Product product) return; var quantity = (int)_quantity.Value; var current = _cart.FirstOrDefault(line => line.ProductId == product.Id); var alreadyAdded = current?.Quantity ?? 0; if (quantity + alreadyAdded > product.Stock) { MessageBox.Show("La cantidad supera el stock disponible."); return; } if (current is not null) { _cart.Remove(current); _cart.Add(current with { Quantity = current.Quantity + quantity }); } else _cart.Add(new SaleLine(product.Id, quantity)); RefreshCart();
    }
    private void RemoveLine() { if (_lines.CurrentRow?.Cells["ProductId"].Value is int id) { var line = _cart.FirstOrDefault(item => item.ProductId == id); if (line is not null) _cart.Remove(line); RefreshCart(); } }
    private void RefreshCart()
    {
        _lines.DataSource = _cart.Join(_items, line => line.ProductId, product => product.Id, (line, product) => new { ProductId = product.Id, product.Code, product.Name, Cantidad = line.Quantity, Precio = product.Price, Subtotal = line.Quantity * product.Price }).ToList();
        _total.Text = $"Total: C$ {_cart.Join(_items, line => line.ProductId, product => product.Id, (line, product) => line.Quantity * product.Price).Sum():N2}";
        if (_total.Parent is { } footer) _total.Left = footer.ClientSize.Width - _total.Width - 22;
        var units = _cart.Sum(line => line.Quantity);
        _cartCount.Text = $"{_cart.Count} artículo(s)  ·  {units} unidad(es)";
    }
    private async Task ConfirmAsync()
    {
        if (_cart.Count == 0) { MessageBox.Show("Agregue al menos un producto antes de confirmar la venta.", "Carrito vacío", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        await RunAsync(async () => { var sale = await sales.CreateAsync(user.Id, _cart); MessageBox.Show($"Venta #{sale.Id} registrada. Total: C$ {sale.Total:N2}", "Venta completada"); _cart.Clear(); _items = (await products.GetAsync(false)).Where(product => product.Stock > 0).ToList(); _product.DataSource = _items; RefreshCart(); }, logger);
    }
}
